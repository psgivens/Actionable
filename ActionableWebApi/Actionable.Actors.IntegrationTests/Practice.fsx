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
 "actionable.title","Doing things"]
|> Map.ofList
|> ActionItemCommand.Create
|> (ActionItemModule.handle ActionItemState.DoesNotExist)

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

unsubscribeAll actionable debugger
