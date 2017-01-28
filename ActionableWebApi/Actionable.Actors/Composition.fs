module Actionable.Actors.Composition

open Akka.Actor
open Akka.FSharp

open Actionable.Actors.Infrastructure
open Actionable.Actors.SampleAgents
open Actionable.Domain.Infrastructure.Envelope

open Actionable.Actors.Persistance
open Actionable.Actors.Aggregates

let system = Configuration.load () |> System.create "ActionableSystem"
let subject<'TMessage> name =
    subject<'TMessage> system name 

//// Listeners     
//let eventSubject = subject<Envelope<'TEvent>> "events" 
//let invalidCommandSubject = subject<System.Exception> "invalidCommands"

open Actionable.Domain.ActionItemModule

let persisterListener = subject<ActionItemEvent> "persisterListener" 
let persisterErrorListener = subject<System.Exception> "persisterErrorListener"
let persistor = 
    PersistingAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
        persisterListener,
        persisterErrorListener,
        ActionItemState.DoesNotExist,
        Actionable.Domain.Persistance.EventSourcing.EF.GenericEventStore(), 
        buildState, 
        Actionable.Domain.Persistance.EventSourcing.EF.persistActionItem)
    |> spawn system "Persister"

let aggregateListener = subject<ActionItemEvent> "aggregateListener" 
let aggregateErrorListener = subject<System.Exception> "aggregateErrorListener"
let actionItemHandler = 
    AggregateAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
        aggregateListener,
        aggregateErrorListener,
        ActionItemState.DoesNotExist,
        Actionable.Domain.Persistance.EventSourcing.EF.GenericEventStore(), 
        buildState, 
        handle)
    |> spawn system "actionItemHandler"

let debugger = spawn system "debugger" <| actorOf (fun msg ->
    printfn "Message: %A" msg)

aggregateListener <! Subscribe debugger
aggregateListener <! Subscribe persistor

open Actionable.Domain.Infrastructure

let DoX () = 
    actionItemHandler <!
        envelopWithDefaults 
            (UserId <| "")
            (DeviceId "devid")
            (TransId.create ()) 
            (StreamId.create ()) 
            (Version 0s) 
            ActionItemCommand.Delete
    printfn "done"


