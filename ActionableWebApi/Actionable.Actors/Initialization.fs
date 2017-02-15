module Actionable.Actors.Initialization

open Akka.Actor
open Akka.FSharp

open Actionable.Actors.Infrastructure
open Actionable.Domain.Infrastructure.Envelope

open Actionable.Actors.Persistance
open Actionable.Actors.Aggregates

open Actionable.Domain
open Actionable.Domain.ActionItemModule
open Actionable.Domain.SessionNotificationsModule
open Actionable.Domain.Infrastructure

type ActionableActors 
        (system:ActorSystem, 
         actionItemEventStore:IEventStore<Envelope<ActionItemEvent>>, 
         notificationEventStore:IEventStore<Envelope<SessionNotificationsEvent>>,
         persist:UserId -> StreamId -> ActionItemState -> Async<unit>) as actors =
    do actors.StartPersister () 
    do actors.StartActionItemAggregator ()
    do actors.StartSessionNotificationsAggregator ()

    [<DefaultValue>] val mutable actionItemPersisterEventBroadcaster :IActorRef
    [<DefaultValue>] val mutable actionItemPersisterErrorBroadcaster :IActorRef
    [<DefaultValue>] val mutable actionItemPersistanceProcessor :IActorRef
    [<DefaultValue>] val mutable actionItemEventBroadcaster :IActorRef
    [<DefaultValue>] val mutable actionItemErrorBroadcaster :IActorRef
    [<DefaultValue>] val mutable actionItemAggregateProcessor :IActorRef
    [<DefaultValue>] val mutable sessionNotificationsEventBroadcaster :IActorRef
    [<DefaultValue>] val mutable sessionNotificationsErrorBroadcaster :IActorRef
    [<DefaultValue>] val mutable sessionNotificationsAggregateProcessor :IActorRef
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
                persist)
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
        this.sessionNotificationsEventBroadcaster <- subject system "sessionNotificationsEventBroadcaster" 
        this.sessionNotificationsErrorBroadcaster <- subject system "sessionNotificationsErrorBroadcaster"
        this.sessionNotificationsAggregateProcessor <- 
            AggregateAgent<SessionNotificationsState, SessionNotificationsCommand, SessionNotificationsEvent>.Create (
                this.sessionNotificationsEventBroadcaster,
                this.sessionNotificationsErrorBroadcaster,
                SessionNotificationsState.DoesNotExist,                
                notificationEventStore,
                SessionNotificationsModule.buildState, 
                SessionNotificationsModule.handle)
            |> spawn system "sessionNotificationsAggregateProcessor"
        this.actionItemToSessionTranlator <-     
            spawn system "actionItemToSessionTranslator" 
                <| actorOf (fun (envelope:Envelope<ActionItemEvent>) ->
                    envelope 
                    |> repackage (fun actionItemEvent ->  
                        "Event to Command Notification" 
                        |> SessionNotificationsCommand.AppendMessage )
                    |> this.sessionNotificationsAggregateProcessor.Tell
                )
                

