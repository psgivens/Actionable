#load "./References.fsx"

open Actionable.Actors.Infrastructure
open Actionable.Domain
open Actionable.Domain.ActionItemModule 
open Actionable.Domain.Infrastructure

["actionable.title","Doing things";
 "actionable.title","Doing things"]
|> Map.ofList
|> ActionItemCommand.Create
|> (ActionItemModule.handle ActionItemState.DoesNotExist)

NewtonsoftHack.resolveNewtonsoft ()

#load "./InMemoryPersistance.fs"
open InMemoryPersistance     
open Akka
open Akka.FSharp

let suffix = "1"
let system = Configuration.defaultConfig () |> System.create ("ActionableSystem" + suffix)

let actionable = 
    Actionable.Actors.Composition.composeSystem 
        (system, MemoryStore (), persist)

open Akka
open Akka.Actor
open Akka.FSharp


//system.Stop actionable.actionItemAggregateActor
//system.Stop actionable.actionItemEventListener
//system.Stop actionable.invalidActionItemMessageListener

//actionable.StartAggregator ()

let debugger = spawn system "debugger" <| actorOf2 (fun mailbox msg ->
    let path = mailbox.Sender().Path.Name
    printfn "==== Path: %s ====" path
    printfn "Message: %A" msg)

actionable.actionItemEventListener <! Subscribe debugger
actionable.invalidActionItemMessageListener <! Subscribe debugger
actionable.actionItemPersisterListener <! Subscribe debugger
actionable.actionItemPrsisterErrorListener <! Subscribe debugger

let title = "Hoobada Da Jubada Jistaliee"
let description = "hiplity fublin"
let description' = "hiplity dw mitibly fublin"
let streamId = StreamId.create ()
actionable.actionItemAggregateActor 
    <! envelopWithDefaults 
        (UserId.box "sampleuserid")
        (TransId.create ())
        (streamId) 
        (Version.box 0s) 
        (ActionItemCommand.Create 
            <| (["actionable.title",title;
                    "actionable.description", description] |> Map.ofList))

actionable.actionItemAggregateActor 
    <! envelopWithDefaults 
        (UserId.box "sampleuserid")
        (TransId.create ())
        (streamId) 
        (Version.box 0s) 
        (ActionItemCommand.Update
            <| (["actionable.title",title;
                    "actionable.description", description] |> Map.ofList))

actionable.actionItemEventListener <! Unsubscribe debugger
actionable.invalidActionItemMessageListener <! Unsubscribe debugger
actionable.actionItemPersisterListener <! Unsubscribe debugger
actionable.actionItemPrsisterErrorListener <! Unsubscribe debugger
