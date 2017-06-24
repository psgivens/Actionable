module Actionable.Actors.Composition

open Akka.Actor
open Akka.FSharp

open Actionable.Actors.Infrastructure
open Actionable.Domain.Infrastructure.Envelope

open Actionable.Actors.Initialization

open Actionable.Domain.ActionItemModule
open Actionable.Domain.UserNotificationsModule
open Actionable.Domain.Infrastructure

NewtonsoftHack.resolveNewtonsoft ()

let debugger system name = 
    actorOf2 (fun mailbox msg ->
        let path = mailbox.Sender().Path.Name
        printfn "==== Path: %s ====" path
        printfn "Message: %A" msg)
    |> spawn system ("debugger" + name)  
    |> Subscribe

let composeSystem 
       (system:ActorSystem, 
        actionItemEventStore:IEventStore<Envelope<ActionItemEvent>>, 
        notificationsEventStore:IEventStore<Envelope<UserNotificationsEvent>>, 
        persistItem:UserId -> StreamId -> ActionItemState -> Async<unit>,
        persistUserNotifications:UserId -> StreamId -> UserNotificationsState -> Async<unit>) =
    let actionable = ActionableActors (system, actionItemEventStore, notificationsEventStore, persistItem, persistUserNotifications)
    actionable.actionItemEventBroadcaster <! Subscribe actionable.actionItemPersistanceProcessor
    //actionable.actionItemPersisterEventBroadcaster <! Subscribe actionable.actionItemToSessionTranlator
    actionable.actionItemEventBroadcaster <! Subscribe actionable.actionItemToSessionTranlator
    actionable.actionItemToSessionTranlator <! Subscribe actionable.userNotificationsAggregateProcessor
    actionable.userNotificationsEventBroadcaster <! Subscribe actionable.userNotificationsPersistanceProcessor
    
    actionable.actionItemToSessionTranlator <! debugger system "translator"
    actionable.userNotificationsAggregateProcessor <! debugger system "aggregate"
    actionable.userNotificationsEventBroadcaster <! debugger system "event"
    actionable.userNotificationsErrorBroadcaster <! debugger system "error"
    
    actionable