module Actionable.Actors.IntegrationTests.ActionsActorsTests

open Xunit

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

let createWaiter name = 
    use signal = new System.Threading.AutoResetEvent false    
    let waitForsignal seconds =     
        System.TimeSpan.FromSeconds seconds 
        |> signal.WaitOne 
        |> Assert.True
    actorOf (fun msg -> signal.Set () |> ignore) 
    |> spawn system name, waitForsignal
    
[<Fact>]
let ``Create, retrieve, update, and delete an item`` () =
    
    let waiter, waitForSignal = createWaiter "crudWaiter"
    actionable.actionItemPersisterEventBroadcaster <! Subscribe waiter

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

    waitForSignal 60.0

    let results = fetch "sampleuserid"
    match results |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
        with
            | None -> failwith <| sprintf "item '%s' was not found" title
            | Some item -> 
                let ident = item.Id
                Assert.True (item.Fields.["actionable.description"] = description)

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
                                         
                waitForSignal 60.0

                let results' = fetch "sampleuserid"

                match results' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
                    with
                    | None -> failwith "Could not find item"
                    | Some (item') -> 
                        Assert.Equal (ident, item'.Id)
                        Assert.Equal (description', item'.Fields.["actionable.description"])


