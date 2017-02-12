module Actionable.Actors.Composition

open Akka.Actor
open Akka.FSharp

open Actionable.Actors.Infrastructure
open Actionable.Domain.Infrastructure.Envelope

open Actionable.Actors.Initialization

open Actionable.Domain.ActionItemModule
open Actionable.Domain.Infrastructure

NewtonsoftHack.resolveNewtonsoft ()


let composeSystem (system:Akka.Actor.ActorSystem, store:IEventStore<Envelope<Actionable.Domain.ActionItemModule.ActionItemEvent>>, persist:UserId -> StreamId -> ActionItemState -> Async<unit>) =
    let actionable = ActionableActors (system, store, persist)
    actionable.actionItemEventBroadcaster <! Subscribe actionable.actionItemPersistanceProcessor
    actionable