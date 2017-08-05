module Actionable.Actors.Initialization

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
         persistUserNotifications:UserId -> StreamId -> UserNotificationsState -> unit) as actors =
    
    let _actionItemPersisterEventBroadcaster = subject system "actionItemPersisterEventBroadcaster" 
    let _actionItemPersisterErrorBroadcaster = subject system "actionItemPersisterErrorBroadcaster"
    let _actionItemEventBroadcaster = subject system "actionItemEventBroadcaster" 
    let _actionItemErrorBroadcaster = subject system "actionItemErrorBroadcaster"
    let _userNotificationsPersisterEventBroadcaster = subject system "userNotificationsPersisterEventBroadcaster"
    let _userNotificationsPersisterErrorBroadcaster = subject system "userNotificationsPersisterErrorBroadcaster"    
    let _userNotificationsEventBroadcaster = subject system "userNotificationsEventBroadcaster"
    let _userNotificationsErrorBroadcaster = subject system "userNotificationsErrorBroadcaster"

    let mutable _actionItemPersistanceProcessor :IActorRef = null
    let mutable _actionItemAggregateProcessor :IActorRef = null
    let mutable _userNotificationsPersistanceProcessor :IActorRef = null
    let mutable _userNotificationsAggregateProcessor :IActorRef = null

    let mutable _actionItemBroadcaster :IActorRef = null

    do actors.StartActionItemPersister () 
    do actors.StartUserNotificationsPersister ()
    do actors.StartActionItemAggregator ()
    do actors.StartSessionNotificationsAggregator ()

    member this.ActionItemPersisterEventBroadcaster with get () = _actionItemPersisterEventBroadcaster 
    member this.ActionItemPersisterErrorBroadcaster with get () = _actionItemPersisterErrorBroadcaster     
    member this.ActionItemPersistanceProcessor with get () = _actionItemPersistanceProcessor

    member this.ActionItemEventBroadcaster with get () = _actionItemEventBroadcaster
    member this.ActionItemErrorBroadcaster with get () = _actionItemErrorBroadcaster
    member this.ActionItemAggregateProcessor with get () = _actionItemAggregateProcessor 
    
    member this.UserNotificationsPersisterEventBroadcaster with get () = _userNotificationsPersisterEventBroadcaster 
    member this.UserNotificationsPersisterErrorBroadcaster with get () = _userNotificationsPersisterErrorBroadcaster 
    member this.UserNotificationsPersistanceProcessor with get () = _userNotificationsPersistanceProcessor 
    

    member this.UserNotificationsEventBroadcaster with get () = _userNotificationsEventBroadcaster 
    member this.UserNotificationsErrorBroadcaster with get () = _userNotificationsErrorBroadcaster    
    member this.UserNotificationsAggregateProcessor with get () = _userNotificationsAggregateProcessor
    
    member this.ActionItemBroadcaster with get () = _actionItemBroadcaster
    
    member this.StartActionItemPersister () = 
        _actionItemPersistanceProcessor <-
            PersistingActor<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                _actionItemPersisterEventBroadcaster,
                _actionItemPersisterErrorBroadcaster,
                ActionItemState.DoesNotExist,
                actionItemEventStore,
                ActionItemModule.buildState,
                persistItems)
            |> spawn system "actionItemPersistanceProcessor"
        _actionItemEventBroadcaster <! Subscribe _actionItemPersistanceProcessor

    member this.StartUserNotificationsPersister () = 
        _userNotificationsPersistanceProcessor <-
            PersistingActor<UserNotificationsState, UserNotificationsCommand, UserNotificationsEvent>.Create (
                _userNotificationsPersisterEventBroadcaster,
                _userNotificationsPersisterErrorBroadcaster,
                UserNotificationsState.DoesNotExist,
                notificationEventStore,
                UserNotificationsModule.buildState,
                persistUserNotifications)
            |> spawn system "userNotificationsPersistanceProcessor"
        _userNotificationsEventBroadcaster <! Subscribe _userNotificationsPersistanceProcessor

    member this.StartActionItemAggregator () = 
        _actionItemAggregateProcessor <- 
            AggregateAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                _actionItemEventBroadcaster,
                _actionItemErrorBroadcaster,
                ActionItemState.DoesNotExist,
                actionItemEventStore,
                ActionItemModule.buildState, 
                ActionItemModule.handle)
            |> spawn system "actionItemAggregateProcessor"
    
    member this.StartSessionNotificationsAggregator () = 
        _userNotificationsAggregateProcessor <- 
            AggregateAgent<UserNotificationsState, UserNotificationsCommand, UserNotificationsEvent>.Create (
                _userNotificationsEventBroadcaster,
                _userNotificationsErrorBroadcaster,
                UserNotificationsState.DoesNotExist,                
                notificationEventStore,
                UserNotificationsModule.buildState, 
                UserNotificationsModule.handle)
            |> spawn system "sessionNotificationsAggregateProcessor"
        
        _actionItemBroadcaster <-     
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

//                    async {
//                        let! streamId = getUserNotificationStreamId envelope.UserId
//
//                        return
//                            envelope
//                            |> repackage streamId (fun actionItemEvent ->  
//                                (UserId.unbox envelope.UserId, 0, clientCmd)
//                                |> UserNotificationsCommand.AppendMessage)
//                    } |!> _userNotificationsAggregateProcessor
                    
                    return! loop () }
                loop ())
        
//        _actionItemBroadcaster <! Subscribe _userNotificationsAggregateProcessor

