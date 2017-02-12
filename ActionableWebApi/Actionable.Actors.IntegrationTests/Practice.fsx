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

let actionable = 
    Actionable.Actors.Composition.composeSystem 
        (MemoryStore (), persist)

open Akka
open Akka.Actor
open Akka.FSharp

let title = "Hoobada Da Jubada Jistaliee"
let description = "hiplity fublin"
let description' = "hiplity dw mitibly fublin"
let streamId = StreamId.create ()
actionable.ActionItemAggregateActor 
    <! envelopWithDefaults 
        (UserId.box "sampleuserid")
        (TransId.create ())
        (streamId) 
        (Version.box 0s) 
        (ActionItemCommand.Create 
            <| (["actionable.title",title;
                    "actionable.description", description] |> Map.ofList))






