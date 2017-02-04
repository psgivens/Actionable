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

let actionItemPersisterListener = subject<Envelope<ActionItemEvent>> "actionItemPersisterListener" 
let actionItemPrsisterErrorListener = subject<System.Exception> "actionItemPrsisterErrorListener"
let actionItemPersistanceActor = 
    PersistingAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
        actionItemPersisterListener,
        actionItemPrsisterErrorListener,
        ActionItemState.DoesNotExist,
        Actionable.Domain.Persistance.EventSourcing.EF.GenericEventStore(), 
        buildState, 
        Actionable.Domain.Persistance.EventSourcing.EF.persistActionItem)
    |> spawn system "actionItemPersistanceActor"

let actionItemEventListener = subject<Envelope<ActionItemEvent>> "actionItemEventListener" 
let invalidActionItemMessageListener = subject<System.Exception> "invalidActionItemMessageListener"
let actionItemAggregateActor = 
    AggregateAgent<ActionItemState, ActionItemCommand, ActionItemEvent>.Create (
        actionItemEventListener,
        invalidActionItemMessageListener,
        ActionItemState.DoesNotExist,
        Actionable.Domain.Persistance.EventSourcing.EF.GenericEventStore(), 
        buildState, 
        handle)
    |> spawn system "actionItemAggregateActor"


