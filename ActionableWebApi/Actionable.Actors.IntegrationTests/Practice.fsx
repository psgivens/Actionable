#load "./ScriptSetup.fsx"

open Actionable.Actors.Infrastructure
open Actionable.Domain
open Actionable.Domain.ActionItemModule 
open Actionable.Domain.Infrastructure

open InMemoryPersistance     

open Akka
open Akka.Actor
open Akka.FSharp

open ActorsScript

subscribeAll actionable debugger

["actionable.title","Doing things";
 "actionable.description","Doing things is an important activity"]
|> Map.ofList
|> ActionItemCommand.Create
|> ActionItemModule.handle ActionItemState.DoesNotExist

let state' =
    ActionItemState.State(
        {   Fields=
                ["actionable.status", "0"
                 "actionable.title","Doing things"
                 "actionable.description","Doing things is an important activity"]
                |> Map.ofList}) 

["actionable.title","Doing things"
 "actionable.description","Doing things has become a less important"]
|> Map.ofList
|> ActionItemCommand.Update
|> ActionItemModule.handle state'
|> ActionItemModule.evolveState state'

open Actionable.Domain
open Actionable.Domain.SessionNotificationsModule

"Session is being bootstrapped"
|> SessionNotificationsCommand.AppendMessage 
|> SessionNotificationsModule.handle SessionNotificationsState.DoesNotExist
|> SessionNotificationsModule.evolveState SessionNotificationsState.DoesNotExist

let title = "Hoobada Da Jubada Jistaliee"
let description = "hiplity fublin"
let description' = "hiplity dw mitibly fublin"
let streamId = StreamId.create ()
actionable.actionItemAggregateProcessor
    <! envelopWithDefaults 
        (UserId.box "sampleuserid")
        (TransId.create ())
        (streamId) 
        (Version.box 0s)         
        (["actionable.title",title;
          "actionable.description", description] 
         |> Map.ofList
         |> ActionItemCommand.Create)

actionable.actionItemAggregateProcessor
    <! envelopWithDefaults 
        (UserId.box "sampleuserid")
        (TransId.create ())
        (streamId) 
        (Version.box 0s) 
        (["actionable.title",title;
          "actionable.description", description'] 
         |> Map.ofList
         |> ActionItemCommand.Update)

actionable.actionItemAggregateProcessor
    <! envelopWithDefaults 
        (UserId.box "sampleuserid")
        (TransId.create ())
        (streamId) 
        (Version.box 0s) 
        (ActionItemCommand.Complete)

actionable.actionItemAggregateProcessor
    <! envelopWithDefaults 
        (UserId.box "sampleuserid")
        (TransId.create ())
        (streamId) 
        (Version.box 0s) 
        (ActionItemCommand.Delete)

unsubscribeAll actionable debugger
