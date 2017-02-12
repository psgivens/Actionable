module Actionable.Actors.Initialization

open Akka.Actor
open Akka.FSharp

open Actionable.Actors.Infrastructure
open Actionable.Domain.Infrastructure.Envelope

open Actionable.Actors.Persistance
open Actionable.Actors.Aggregates

let system = Configuration.defaultConfig () |> System.create "ActionableSystem"
let subject<'TMessage> name =
    subject<'TMessage> system name 

open Actionable.Domain.ActionItemModule
open Actionable.Domain.Infrastructure

type ActionableActors (store:IEventStore<Envelope<Actionable.Domain.ActionItemModule.ActionItemEvent>>, persist:UserId -> StreamId -> ActionItemState -> Async<unit>) =
    let actionItemPersisterListener = subject<Envelope<ActionItemEvent>> "actionItemPersisterListener" 
    let actionItemPrsisterErrorListener = subject<System.Exception> "actionItemPrsisterErrorListener"
    let actionItemPersistanceActor = 
        PersistingActor<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
            actionItemPersisterListener,
            actionItemPrsisterErrorListener,
            ActionItemState.DoesNotExist,
            store,
            buildState,
            persist)
        |> spawn system "actionItemPersistanceActor"

    let actionItemEventListener = subject<Envelope<ActionItemEvent>> "actionItemEventListener" 
    let invalidActionItemMessageListener = subject<System.Exception> "invalidActionItemMessageListener"
    let actionItemAggregateActor = 
        AggregateAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
            actionItemEventListener,
            invalidActionItemMessageListener,
            ActionItemState.DoesNotExist,
            store,
            buildState, 
            handle)
        |> spawn system "actionItemAggregateActor"
    member this.ActionItemPersisterListener      = actionItemPersisterListener 
    member this.ActionItemPrsisterErrorListener  = actionItemPrsisterErrorListener 
    member this.ActionItemPersistanceActor       = actionItemPersistanceActor 
    member this.ActionItemEventListener          = actionItemEventListener 
    member this.InvalidActionItemMessageListener = invalidActionItemMessageListener
    member this.ActionItemAggregateActor         = actionItemAggregateActor  


