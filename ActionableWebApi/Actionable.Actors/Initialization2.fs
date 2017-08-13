module Actionable.Actors.Initialization2

open Akka.Actor
open Akka.FSharp
open Akka.FSharp.Actors

open Actionable.Actors.Infrastructure
open Actionable.Domain.Infrastructure.Envelope

open Actionable.Actors.Persistance
open Actionable.Actors.Aggregates

open Actionable.Domain
open Actionable.Domain.ActionItemModule
open Actionable.Domain.UserNotificationsModule
open Actionable.Domain.Infrastructure
open Actionable.Domain.ClientCommands

[<AllowNullLiteral>]
type ActionableActors 
        (system:ActorSystem, 
         actionItemEventStore:IEventStore<Envelope<ActionItemEvent>>, 
         notificationEventStore:IEventStore<Envelope<UserNotificationsEvent>>,
         getUserNotificationStreamId:UserId -> StreamId,
         persistItems:UserId -> StreamId -> ActionItemState -> unit,
         persistUserNotifications:UserId -> StreamId -> UserNotificationsState -> unit) =
        
    let _userNotificationsPersisterEventBroadcaster, _userNotificationsPersisterEventBroadcaster' = 
        subject system "userNotificationsPersisterEventBroadcaster"
    let _userNotificationsPersisterErrorBroadcaster, _userNotificationsPersisterErrorBroadcaster' = 
        subject system "userNotificationsPersisterErrorBroadcaster"    
    let _userNotificationsPersistanceProcessor =
            PersistingActor<UserNotificationsState, UserNotificationsCommand, UserNotificationsEvent>.Create (
                _userNotificationsPersisterEventBroadcaster',
                _userNotificationsPersisterErrorBroadcaster',
                UserNotificationsState.DoesNotExist,
                notificationEventStore,
                UserNotificationsModule.buildState,
                persistUserNotifications)
            |> spawn system "userNotificationsPersistanceProcessor"

    let _userNotificationsErrorBroadcaster, _userNotificationsErrorBroadcaster' = 
        subject system "userNotificationsErrorBroadcaster" 
    let _userNotificationsAggregateProcessor =
            AggregateAgent<UserNotificationsState, UserNotificationsCommand, UserNotificationsEvent>.Create (
                _userNotificationsPersistanceProcessor,
                _userNotificationsErrorBroadcaster',
                UserNotificationsState.DoesNotExist,                
                notificationEventStore,
                UserNotificationsModule.buildState, 
                UserNotificationsModule.handle)
            |> spawn system "sessionNotificationsAggregateProcessor"
        
    let _actionItemBroadcaster =
            spawn system "actionItemNotifyer" (fun (mailbox:Actor<Envelope<ActionItemEvent>>) ->
                let rec loop () = actor {
                    let! envelope = mailbox.Receive ()
                    let clientCmd = serializeClientCommand { 
                        Actionable.Domain.ClientCommands.ActionItemUpdated.Id = envelope.StreamId |> StreamId.unbox }
                    let streamId = getUserNotificationStreamId envelope.UserId
                    let cmd = 
                        envelope
                        |> repackage streamId (fun actionItemEvent ->  
                            (UserId.unbox envelope.UserId, 0, clientCmd)
                            |> UserNotificationsCommand.AppendMessage)
                    cmd |> _userNotificationsAggregateProcessor.Tell

                    return! loop () }
                loop ())
    
    let _actionItemPersisterErrorBroadcaster, _actionItemPersisterErrorBroadcaster' = 
        subject system "actionItemPersisterErrorBroadcaster"
    let _actionItemPersistanceProcessor =
            PersistingActor<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                _actionItemBroadcaster,
                _actionItemPersisterErrorBroadcaster',
                ActionItemState.DoesNotExist,
                actionItemEventStore,
                ActionItemModule.buildState,
                persistItems)
            |> spawn system "actionItemPersistanceProcessor"

    let _actionItemErrorBroadcaster, _actionItemErrorBroadcaster' = 
        subject system "actionItemErrorBroadcaster"   
    let _actionItemAggregateProcessor =
            AggregateAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                _actionItemPersistanceProcessor,
                _actionItemErrorBroadcaster',
                ActionItemState.DoesNotExist,
                actionItemEventStore,
                ActionItemModule.buildState, 
                ActionItemModule.handle)
            |> spawn system "actionItemAggregateProcessor"


    member this.ActionItemAggregateProcessor with get () = _actionItemAggregateProcessor     
    member this.UserNotificationsPersisterEventBroadcaster with get () = _userNotificationsPersisterEventBroadcaster 
    