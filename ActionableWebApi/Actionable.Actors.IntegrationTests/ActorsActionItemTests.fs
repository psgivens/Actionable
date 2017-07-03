module Actors_ActionItems

open Xunit

open Akka
open Akka.Actor
open Akka.FSharp

open Actionable.Domain.Infrastructure
open Actionable.Domain
open Actionable.Domain.ActionItemModule
open Actionable.Domain.UserNotificationsModule

open Actionable.Actors 
open Actionable.Actors.Initialization
open Actionable.Actors.Composition

open InMemoryPersistance
open Actionable.Actors.Composition

let system = Configuration.defaultConfig () |> System.create (sprintf "%s-%A" "ActionableSystem" (System.Guid.NewGuid ()))
let actionable = 
    composeSystem 
        (system, 
         MemoryStore<ActionItemEvent> (), 
         MemoryStore<UserNotificationsEvent> (),
         persistActionItem,
         persistUserNotification
         )
          // Actionable.Domain.Persistance.EventSourcing.EF.persistActionItem)

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
    actionable.ActionItemPersisterEventBroadcaster <! Subscribe waiter.Actor

    let title = "Hoobada Da Jubada Jistaliee"
    let description = "hiplity fublin"
    let description' = "hiplity dw mitibly fublin"
    let streamId = StreamId.create ()
    actionable.ActionItemAggregateProcessor 
        <! envelopWithDefaults 
            (UserId.box "sampleuserid")
            (TransId.create ())
            (streamId) 
            (Version.box 0s) 
            (("sampleuserid", StreamId.unbox streamId, 
              ["actionable.title",title;
                "actionable.description", description] 
                |> Map.ofList)
             |> ActionItemCommand.Create)

    waiter.Wait 60.0

    let results = fetchActionItems "sampleuserid"
    let item = 
        match results |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
            with
                | None -> failwith <| sprintf "item '%s' was not found" title
                | Some item -> item
    
    Assert.Equal (item.Fields.["actionable.description"], description)

    actionable.ActionItemAggregateProcessor 
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
    let results' = fetchActionItems "sampleuserid"
    let item' = 
        match results' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
            with
            | None -> failwith "Could not find item"
            | Some item' -> item'

    Assert.Equal (item.Id, item'.Id)
    Assert.Equal (item'.Fields.["actionable.description"], description')

    actionable.ActionItemAggregateProcessor 
        <! envelopWithDefaults 
            (UserId.box "sampleuserid")
            (TransId.create ())
            (streamId) 
            (Version.box 1s) 
            (ActionItemCommand.Delete)
                
    waiter.Wait 60.0
    let results'' = fetchActionItems "sampleuserid"
    let item''' = results'' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
    
    Assert.Equal (item''', None)


