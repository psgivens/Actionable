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

    [<DefaultValue>] val mutable actionItemPersisterListener: IActorRef
    [<DefaultValue>] val mutable actionItemPrsisterErrorListener :IActorRef
    [<DefaultValue>] val mutable actionItemPersistanceActor :IActorRef
    [<DefaultValue>] val mutable actionItemEventListener :IActorRef
    [<DefaultValue>] val mutable invalidActionItemMessageListener :IActorRef
    [<DefaultValue>] val mutable actionItemAggregateActor :IActorRef
    
    member this.StartPersister () = 
        this.actionItemPersisterListener <- subject system "actionItemPersisterListener" 
        this.actionItemPrsisterErrorListener <- subject system "actionItemPrsisterErrorListener"
        this.actionItemPersistanceActor <-
            PersistingActor<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                this.actionItemPersisterListener,
                this.actionItemPrsisterErrorListener,
                ActionItemState.DoesNotExist,
                store,
                buildState,
                persist)
            |> spawn system "actionItemPersistanceActor"

    member this.StartAggregator () = 
        this.actionItemEventListener <- subject system "actionItemEventListener" 
        this.invalidActionItemMessageListener <- subject system "invalidActionItemMessageListener"
        this.actionItemAggregateActor <- 
            AggregateAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
                this.actionItemEventListener,
                this.invalidActionItemMessageListener,
                ActionItemState.DoesNotExist,
                store,
                buildState, 
                handle)
            |> spawn system "actionItemAggregateActor"

//    member this.ActionItemPersisterListener      = this.actionItemPersisterListener 
//    member this.ActionItemPrsisterErrorListener  = this.actionItemPrsisterErrorListener 
//    member this.ActionItemPersistanceActor       = this.actionItemPersistanceActor 
//    member this.ActionItemEventListener          = this.actionItemEventListener 
//    member this.InvalidActionItemMessageListener = this.invalidActionItemMessageListener
//    member this.ActionItemAggregateActor         = this.actionItemAggregateActor  


