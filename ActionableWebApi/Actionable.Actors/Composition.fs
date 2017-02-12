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
//    let debugger = spawn system "debugger" <| actorOf (fun msg ->
//        printfn "Message: %A" msg)
    let actionable = ActionableActors (system, store, persist)
//    actionable.actionItemEventListener <! Subscribe debugger
    actionable.actionItemEventListener <! Subscribe actionable.actionItemPersistanceActor
    actionable

//// TODO: Remove this sample method
//let DoX () = 
//    let actionable = composeSystem (Actionable.Domain.Persistance.EventSourcing.EF.GenericEventStore())
//    actionable.actionItemAggregateActor <!
//        envelopWithDefaults 
//            (UserId.box <| "")
//            (TransId.create ()) 
//            (StreamId.create ()) 
//            (Version.box 0s) 
//            ActionItemCommand.Delete
//    printfn "done"


