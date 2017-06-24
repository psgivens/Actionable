module Actionable.Actors.Initialization

open Akka.Actor
open Akka.FSharp

open Actionable.Actors.Infrastructure
open Actionable.Domain.Infrastructure.Envelope

open Actionable.Actors.Persistance
open Actionable.Actors.Aggregates

open Actionable.Domain
open Actionable.Domain.ActionItemModule
open Actionable.Domain.UserNotificationsModule
open Actionable.Domain.Infrastructure

type ActionableActors 
        (system:ActorSystem, 
         actionItemEventStore:IEventStore<Envelope<ActionItemEvent>>, 
         notificationEventStore:IEventStore<Envelope<UserNotificationsEvent>>,
         persistItems:UserId -> StreamId -> ActionItemState -> Async<unit>) as actors =
    do actors.StartPersister () 
    do actors.StartActionItemAggregator ()
    do actors.StartSessionNotificationsAggregator ()

    [<DefaultValue>] val mutable actionItemPersisterEventBroadcaster :IActorRef
    [<DefaultValue>] val mutable actionItemPersisterErrorBroadcaster :IActorRef
    [<DefaultValue>] val mutable actionItemPersistanceProcessor :IActorRef

    [<DefaultValue>] val mutable actionItemEventBroadcaster :IActorRef
    [<DefaultValue>] val mutable actionItemErrorBroadcaster :IActorRef
    [<DefaultValue>] val mutable actionItemAggregateProcessor :IActorRef
    
    [<DefaultValue>] val mutable userNotificationsPersisterEventBroadcaster :IActorRef
    [<DefaultValue>] val mutable userNotificationsPersisterErrorBroadcaster :IActorRef
    [<DefaultValue>] val mutable userNotificationsPersistanceProcessor :IActorRef

    [<DefaultValue>] val mutable userNotificationsEventBroadcaster :IActorRef
    [<DefaultValue>] val mutable userNotificationsErrorBroadcaster :IActorRef
    [<DefaultValue>] val mutable userNotificationsAggregateProcessor :IActorRef
    
    [<DefaultValue>] val mutable actionItemToSessionTranlator :IActorRef
    
    member this.StartPersister () = 
        this.actionItemPersisterEventBroadcaster <- subject system "actionItemPersisterEventBroadcaster" 
        this.actionItemPersisterErrorBroadcaster <- subject system "actionItemPersisterErrorBroadcaster"
        this.actionItemPersistanceProcessor <-
            PersistingActor<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                this.actionItemPersisterEventBroadcaster,
                this.actionItemPersisterErrorBroadcaster,
                ActionItemState.DoesNotExist,
                actionItemEventStore,
                ActionItemModule.buildState,
                persistItems)
            |> spawn system "actionItemPersistanceProcessor"

    member this.StartActionItemAggregator () = 
        this.actionItemEventBroadcaster <- subject system "actionItemEventBroadcaster" 
        this.actionItemErrorBroadcaster <- subject system "actionItemErrorBroadcaster"
        this.actionItemAggregateProcessor <- 
            AggregateAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                this.actionItemEventBroadcaster,
                this.actionItemErrorBroadcaster,
                ActionItemState.DoesNotExist,
                actionItemEventStore,
                ActionItemModule.buildState, 
                ActionItemModule.handle)
            |> spawn system "actionItemAggregateProcessor"
    
    member this.StartSessionNotificationsAggregator () = 
        this.userNotificationsEventBroadcaster <- subject system "sessionNotificationsEventBroadcaster" 
        this.userNotificationsErrorBroadcaster <- subject system "sessionNotificationsErrorBroadcaster"
        this.userNotificationsAggregateProcessor <- 
            AggregateAgent<UserNotificationsState, UserNotificationsCommand, UserNotificationsEvent>.Create (
                this.userNotificationsEventBroadcaster,
                this.userNotificationsErrorBroadcaster,
                UserNotificationsState.DoesNotExist,                
                notificationEventStore,
                UserNotificationsModule.buildState, 
                UserNotificationsModule.handle)
            |> spawn system "sessionNotificationsAggregateProcessor"
        this.actionItemToSessionTranlator <-     
            spawn system "actionItemToSessionTranslator" 
                <| actorOf (fun (envelope:Envelope<ActionItemEvent>) ->
                    envelope 
                    |> repackage (fun actionItemEvent ->  
                        "Event to Command Notification" 
                        |> UserNotificationsCommand.AppendMessage )
                    |> this.userNotificationsAggregateProcessor.Tell
                )
                

