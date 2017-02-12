module Actionable.Actors.Initialization

open Akka.Actor
open Akka.FSharp

open Actionable.Actors.Infrastructure
open Actionable.Domain.Infrastructure.Envelope

open Actionable.Actors.Persistance
open Actionable.Actors.Aggregates

open Actionable.Domain.ActionItemModule
open Actionable.Domain.Infrastructure

type ActionableActors (system:Akka.Actor.ActorSystem, store:IEventStore<Envelope<Actionable.Domain.ActionItemModule.ActionItemEvent>>, persist:UserId -> StreamId -> ActionItemState -> Async<unit>) as actors =
    do actors.StartPersister () 
    do actors.StartAggregator ()

    [<DefaultValue>] val mutable actionItemPersisterEventBroadcaster: IActorRef
    [<DefaultValue>] val mutable actionItemPersisterErrorBroadcaster :IActorRef
    [<DefaultValue>] val mutable actionItemPersistanceProcessor :IActorRef
    [<DefaultValue>] val mutable actionItemEventBroadcaster :IActorRef
    [<DefaultValue>] val mutable actionItemErrorBroadcaster :IActorRef
    [<DefaultValue>] val mutable actionItemAggregateProcessor :IActorRef
    
    member this.StartPersister () = 
        this.actionItemPersisterEventBroadcaster <- subject system "actionItemPersisterEventBroadcaster" 
        this.actionItemPersisterErrorBroadcaster <- subject system "actionItemPersisterErrorBroadcaster"
        this.actionItemPersistanceProcessor <-
            PersistingActor<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                this.actionItemPersisterEventBroadcaster,
                this.actionItemPersisterErrorBroadcaster,
                ActionItemState.DoesNotExist,
                store,
                buildState,
                persist)
            |> spawn system "actionItemPersistanceProcessor"

    member this.StartAggregator () = 
        this.actionItemEventBroadcaster <- subject system "actionItemEventBroadcaster" 
        this.actionItemErrorBroadcaster <- subject system "actionItemErrorBroadcaster"
        this.actionItemAggregateProcessor <- 
            AggregateAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                this.actionItemEventBroadcaster,
                this.actionItemErrorBroadcaster,
                ActionItemState.DoesNotExist,
                store,
                buildState, 
                handle)
            |> spawn system "actionItemAggregateProcessor"
    