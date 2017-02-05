﻿module Actionable.Actors.IntegrationTests.ActionsActorsTests

open Xunit

open Akka
open Akka.Actor
open Akka.FSharp

open Actionable.Domain.Infrastructure
//open Actionable.Domain
open Actionable.Domain.ActionItemModule

open Actionable.Actors 
open Actionable.Actors.Initialization
open Actionable.Actors.Composition

[<Fact>]
let ``Simple first test`` () =
    DoX ()
    actionItemAggregateActor <!
        envelopWithDefaults 
            (UserId.box "")
//            (DeviceId.box "devid")
            (TransId.create ()) 
            (StreamId.create ()) 
            (Version.box 0s) 
            ActionItemCommand.Delete

    System.Threading.Thread.Sleep 10000
    printfn "done"
    Assert.True true
//    true |> should equal true


open Actionable.Actors.Infrastructure

[<Fact>]
let ``Create an item, retrieve it, update it, and delete it`` () =

    composeSystem ()

    // TODO: make the signal waiter more generic
    use signal = new System.Threading.AutoResetEvent false
    let waiter = spawn system "testsignalwaiter" <| actorOf (fun msg ->
        signal.Set () |> ignore)
    actionItemEventListener <! Subscribe waiter

    let title = "Hoobada Da Jubada Jistaliee"
    let description = "hiplity fublin"
    let description' = "hiplity dw mitibly fublin"
    let streamId = StreamId.create ()
    actionItemAggregateActor 
    <! envelopWithDefaults 
        (UserId.box "sampleuserid")
//        (DeviceId.box "sampledeviceid")
        (TransId.create ())
        (streamId) 
        (Version.box 0s) 
        (ActionItemCommand.Create 
            <| (["actionable.title",title;
                 "actionable.description", description] |> Map.ofList))

    System.TimeSpan.FromSeconds 10.0 
    |> signal.WaitOne 
    |> Assert.True

    let results = Actionable.Domain.Persistance.EventSourcing.EF.fetchActionItems "sampleuserid"
    match results |> List.tryFind (fun r -> r.Fields.["actionable.title"] = title)
        with
            | None -> failwith <| sprintf "item '%s' was not found" title
            | Some item -> 
                let ident = item.Id
                Assert.True (item.Fields.["actionable.description"] = description)

                actionItemAggregateActor 
                <! envelopWithDefaults 
                    (UserId.box "sampleuserid")
            //        (DeviceId.box "sampledeviceid")
                    (TransId.create ())
                    (streamId) 
                    (Version.box 1s) 
                    (ActionItemCommand.Update 
                        <| (["actionable.title",title;
                             "actionable.description", description'] |> Map.ofList))

                System.Threading.Thread.Sleep 10000

                let results' = Actionable.Domain.Persistance.EventSourcing.EF.fetchActionItems "sampleuserid"

                match results' |> List.tryFind (fun r -> r.Fields.["actionable.title"] = title)
                    with
                    | None -> failwith "Could not find item"
                    | Some (item') -> 
                        Assert.Equal (ident, item'.Id)
                        Assert.Equal (description', item'.Fields.["actionable.description"])

    Assert.True true
    
type Class1() = 
    member this.X = "F#"