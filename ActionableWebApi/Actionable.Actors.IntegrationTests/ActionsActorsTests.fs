﻿module Actionable.Actors.IntegrationTests.ActionsActorsTests

open Xunit
open FsUnit
open FsUnit.Xunit

open Akka
open Akka.Actor
open Akka.FSharp

open Actionable.Domain.Infrastructure
open Actionable.Domain
open Actionable.Domain.ActionItemModule
open Actionable.Domain.SessionNotificationsModule

open Actionable.Actors 
open Actionable.Actors.Initialization
open Actionable.Actors.Composition

open InMemoryPersistance

let system = Configuration.defaultConfig () |> System.create "ActionableSystem"
let actionable = 
    composeSystem 
        (system, 
         MemoryStore<ActionItemEvent> (), 
         MemoryStore<SessionNotificationsEvent> (),
         persist) // Actionable.Domain.Persistance.EventSourcing.EF.persistActionItem)

open Actionable.Actors.Infrastructure

type SignalWaiter (name, system) = 
    let signal = new System.Threading.AutoResetEvent false    

    let actor = 
        actorOf (fun msg -> signal.Set () |> ignore) 
        |> spawn system name

    member this.Actor 
        with get () = actor

    member this.Wait seconds = 
        System.TimeSpan.FromSeconds seconds 
        |> signal.WaitOne 
        |> Assert.True
        
    interface System.IDisposable  with 
        member x.Dispose() = signal.Dispose ()
    
[<Fact>]
let ``Create, retrieve, update, and delete an item`` () =
  
    use waiter = new SignalWaiter ("crudWaiter", system)
    //let waiter, waitForSignal, disgnal = createWaiter "crudWaiter"
    //use disposeSignal = disgnal

    actionable.actionItemPersisterEventBroadcaster <! Subscribe (waiter.Actor)

    let title = "Hoobada Da Jubada Jistaliee"
    let description = "hiplity fublin"
    let description' = "hiplity dw mitibly fublin"
    let streamId = StreamId.create ()
    actionable.actionItemAggregateProcessor 
        <! envelopWithDefaults 
            (UserId.box "sampleuserid")
            (TransId.create ())
            (streamId) 
            (Version.box 0s) 
            (["actionable.title",title;
              "actionable.description", description] 
             |> Map.ofList
             |> ActionItemCommand.Create)

    waiter.Wait 60.0

    let results = fetch "sampleuserid"
    let item = 
        match results |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
            with
                | None -> failwith <| sprintf "item '%s' was not found" title
                | Some item -> item
    
    item.Fields.["actionable.description"] |> should equal description

    actionable.actionItemAggregateProcessor 
        <! envelopWithDefaults 
            (UserId.box "sampleuserid")
            (TransId.create ())
            (streamId) 
            (Version.box 1s) 
            (["actionable.title",title;
                "actionable.description", description'] 
                |> Map.ofList
                |> ActionItemCommand.Update)

    waiter.Wait 60.0
    let results' = fetch "sampleuserid"
    let item' = 
        match results' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
            with
            | None -> failwith "Could not find item"
            | Some item' -> item'

    item.Id |> should equal item'.Id
    item'.Fields.["actionable.description"] |> should equal description'

    actionable.actionItemAggregateProcessor 
        <! envelopWithDefaults 
            (UserId.box "sampleuserid")
            (TransId.create ())
            (streamId) 
            (Version.box 1s) 
            (ActionItemCommand.Delete)
                
    waiter.Wait 60.0
    let results'' = fetch "sampleuserid"
    let item''' = results'' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
    item''' |> should equal None


