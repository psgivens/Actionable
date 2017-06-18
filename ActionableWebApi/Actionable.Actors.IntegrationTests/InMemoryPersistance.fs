module InMemoryPersistance

open Actionable.Domain.Infrastructure
open Actionable.Domain
open Actionable.Domain.ActionItemModule

open Actionable.Actors 
open Actionable.Actors.Initialization
open Actionable.Actors.Composition

type MemoryStore<'TEventType> () =
    let mutable itemsMap = Map.empty<StreamId, Envelope<'TEventType> list> 
    interface IEventStore<Envelope<'TEventType>> with                    
        member this.GetEvents (streamId:StreamId) =
            match itemsMap |> Map.tryFind streamId with
            | None -> []
            | Some(events) -> events
        member this.AppendEventAsync (streamId:StreamId) (envelope:Envelope<'TEventType>) =
            async { 
                let items = 
                    match itemsMap |> Map.tryFind streamId with
                    | None -> [envelope]
                    | Some(list) -> list@[envelope]
                itemsMap <- itemsMap |> Map.add streamId items
                do! async.Zero ()
            }

open Actionable.Domain.Persistance.EventSourcing.EF
let mutable actionItems = Map.empty<System.Guid,ActionItem>
let persist userId (StreamId.Id(streamId)) state = 
    async {
        match state with
        | DoesNotExist -> 
            actionItems <- actionItems.Remove streamId
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