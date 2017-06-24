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

let composeSystem 
       (system:ActorSystem, 
        actionItemEventStore:IEventStore<Envelope<ActionItemEvent>>, 
        notificationsEventStore:IEventStore<Envelope<UserNotificationsEvent>>, 
        persistItem:UserId -> StreamId -> ActionItemState -> Async<unit>) =
    let actionable = ActionableActors (system, actionItemEventStore, notificationsEventStore, persistItem)
    actionable.actionItemEventBroadcaster <! Subscribe actionable.actionItemPersistanceProcessor
    actionable.actionItemPersisterEventBroadcaster <! Subscribe actionable.actionItemToSessionTranlator
    actionable