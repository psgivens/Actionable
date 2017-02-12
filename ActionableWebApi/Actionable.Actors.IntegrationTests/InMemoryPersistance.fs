module InMemoryPersistance

open Actionable.Domain.Infrastructure
open Actionable.Domain
open Actionable.Domain.ActionItemModule

open Actionable.Actors 
open Actionable.Actors.Initialization
open Actionable.Actors.Composition

type MemoryStore () =
    let mutable collection = System.Collections.Generic.List<Envelope<ActionItemEvent>>()
    interface IEventStore<Envelope<ActionItemModule.ActionItemEvent>> with        
        member this.GetEvents (streamId:StreamId) =
            collection |> List.ofSeq
        member this.AppendEventAsync (streamId:StreamId) (envelope:Envelope<ActionItemModule.ActionItemEvent>) =
            async { 
                collection.Add(envelope);
                do! Async.Sleep 2000
                do! async.Zero ()
            }

open Actionable.Domain.Persistance.EventSourcing.EF
let mutable actionItems = Map.empty<System.Guid,ActionItem>
let persist userId (StreamId.Id(streamId)) state = 
    async {
        match state with
        | DoesNotExist -> ()
        | State(item) ->
            actionItems <- actionItems.Add(streamId, item)
        do! async.Zero ()
    }

let fetch userId :ActionItemReadModel list= 
    actionItems 
    |> Map.map (fun key value ->
        {
            ActionItemReadModel.UserId = userId
            ActionItemReadModel.Fields = value.Fields
            ActionItemReadModel.Id = key
        })
    |> Map.toList
    |> List.map snd