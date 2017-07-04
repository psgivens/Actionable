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
        getUserNotificationStreamId:UserId -> StreamId,
        persistItem:UserId -> StreamId -> ActionItemState -> Async<unit>,
        persistUserNotifications:UserId -> StreamId -> UserNotificationsState -> Async<unit>) =
    let actionable = ActionableActors (system, actionItemEventStore, notificationsEventStore, getUserNotificationStreamId, persistItem, persistUserNotifications)

    //actionable.ActionItemEventBroadcaster <! Subscribe actionable.actionItemPersistanceProcessor
    actionable.ActionItemEventBroadcaster <! Subscribe actionable.ActionItemBroadcaster    

    //actionable.UserNotificationsEventBroadcaster <! Subscribe actionable.userNotificationsPersistanceProcessor
        
    actionable.UserNotificationsEventBroadcaster <! debugger system "event"
    actionable.UserNotificationsErrorBroadcaster <! debugger system "error"
    
    actionable