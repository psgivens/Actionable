module ActorsScript
#load "./References.fsx"
#load "./InMemoryPersistance.fs"

//NewtonsoftHack.resolveNewtonsoft ()


open Actionable.Actors.Infrastructure
open Actionable.Domain
open Actionable.Domain.ActionItemModule 
open Actionable.Domain.SessionNotificationsModule
open Actionable.Domain.Infrastructure

open InMemoryPersistance     

open Akka
open Akka.Actor
open Akka.FSharp

let createSystem suffix = 
    let system = 
        Configuration.defaultConfig () 
        |> System.create ("ActionableSystem" + suffix)
    let actionable = 
        Actionable.Actors.Composition.composeSystem 
            (system, 
             MemoryStore<ActionItemEvent> (), 
             MemoryStore<SessionNotificationsEvent> (),
             persist)
    (system, actionable)

let stopAggregate (system:Akka.Actor.ActorSystem) (actionable:Actionable.Actors.Initialization.ActionableActors) = 
     system.Stop actionable.actionItemAggregateProcessor
     system.Stop actionable.actionItemErrorBroadcaster
     system.Stop actionable.actionItemEventBroadcaster

let system, actionable = createSystem "script"

let debugger = spawn system "debugger" <| actorOf2 (fun mailbox msg ->
    let path = mailbox.Sender().Path.Name
    printfn "==== Path: %s ====" path
    printfn "Message: %A" msg)

let subscribeAll (actionable:Actionable.Actors.Initialization.ActionableActors) debugger = 
    actionable.actionItemEventBroadcaster <! Subscribe debugger
    actionable.actionItemErrorBroadcaster <! Subscribe debugger
    actionable.actionItemPersisterErrorBroadcaster <! Subscribe debugger
    actionable.actionItemPersisterEventBroadcaster <! Subscribe debugger
    actionable.sessionNotificationsEventBroadcaster <! Subscribe debugger

let unsubscribeAll (actionable:Actionable.Actors.Initialization.ActionableActors) debugger = 
    actionable.actionItemEventBroadcaster <! Unsubscribe debugger
    actionable.actionItemErrorBroadcaster <! Unsubscribe debugger
    actionable.actionItemPersisterErrorBroadcaster <! Unsubscribe debugger
    actionable.actionItemPersisterEventBroadcaster <! Unsubscribe debugger
    actionable.sessionNotificationsEventBroadcaster <! Unsubscribe debugger
