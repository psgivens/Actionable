module Actionable.Actors.Composition

open Akka.Actor
open Akka.FSharp

open Actionable.Actors.Infrastructure
open Actionable.Domain.Infrastructure.Envelope

open Actionable.Actors.Initialization

open Actionable.Domain.ActionItemModule
open Actionable.Domain.Infrastructure

let debugger = spawn system "debugger" <| actorOf (fun msg ->
    printfn "Message: %A" msg)

let composeSystem () = 
    actionItemEventListener <! Subscribe debugger
    actionItemEventListener <! Subscribe actionItemPersistanceActor

// TODO: Remove this sample method
let DoX () = 
    actionItemAggregateActor <!
        envelopWithDefaults 
            (UserId.box <| "")
//            (DeviceId.box "devid")
            (TransId.create ()) 
            (StreamId.create ()) 
            (Version.box 0s) 
            ActionItemCommand.Delete
    printfn "done"


