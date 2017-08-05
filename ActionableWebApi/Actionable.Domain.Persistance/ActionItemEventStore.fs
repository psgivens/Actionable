module Actionable.Domain.Persistance.EventSourcing.ActionItemEF

open Actionable.Domain.Persistance.Core
open Actionable.Domain.Infrastructure
open Actionable.Domain.ActionItemModule
open Newtonsoft.Json
open Actionable.Data

open Actionable.Domain.ActionItemsQueryResponse

type Actionable.Data.ActionableDbContext with 
    member this.GetActionItemEvents<'a> (StreamId.Id (aggregateId):StreamId) :seq<Envelope<'a>>= 
        query {
            for event in this.ActionItemEvents do
            where (event.StreamId = aggregateId)
            select event
        } |> Seq.map (fun event ->
                {
                    Id = event.Id
                    UserId = UserId.box event.UserId
                    StreamId = StreamId.box aggregateId
                    TransactionId = TransId.box event.TransactionId
                    Version = Version.box (event.Version)
                    Created = event.TimeStamp
                    Item = (JsonConvert.DeserializeObject<'a> event.Event)
                }
            )

type ActionItemEventStore () =
    interface IEventStore<Envelope<ActionItemEvent>> with
        member this.GetEvents (streamId:StreamId) =
            use context = new ActionableDbContext ()
            context.GetActionItemEvents<ActionItemEvent> streamId 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (streamId:StreamId) (envelope:Envelope<ActionItemEvent>) =
            try
                use context = new ActionableDbContext ()
                context.ActionItemEvents.Add (
                    ActionItemEnvelopeEntity (  Id = envelope.Id,
                                                StreamId = StreamId.unbox envelope.StreamId,
                                                UserId = UserId.unbox envelope.UserId,
                                                TransactionId = TransId.unbox envelope.TransactionId,
                                                Version = Version.unbox envelope.Version,
                                                TimeStamp = envelope.Created,
                                                Event = JsonConvert.SerializeObject(envelope.Item)
                                                )) |> ignore         
                context.SaveChanges () |> ignore
                
            with
                | ex -> System.Diagnostics.Debugger.Break () 

let persistActionItem (UserId.Val(userId):UserId) (StreamId.Id (streamId):StreamId) state = 
    try
        use context = new ActionableDbContext ()
        let entity = context.TaskInstances.Find(streamId) 

        match state with
        | DoesNotExist -> 
            match entity with
            | null -> ()
            | entity ->
                context.TaskInstances.Remove (entity) |> ignore
                context.SaveChanges () |> ignore

        | State (item) -> 
                
            let typeDef = query {
                for taskType in context.TaskTypeDefinitions do
                where (taskType.FullyQualifiedName = "actionable.actionitem")
                select taskType
                exactlyOne }
            let groupedFields = typeDef.Fields |> Seq.groupBy (fun f -> f.FieldType)
                
            match entity with                   
            | null ->
                typeDef 
                |> buildTaskInstance userId streamId item.Fields
                |> context.TaskInstances.Add 
                |> ignore
                    
            | entity ->
                updateTaskInstance item.Fields entity
                |> ignore
                    
            context.SaveChanges |> ignore
    with
    // TODO: Need better exeception handling. Logging, perhaps?
    | ex -> System.Diagnostics.Debugger.Break ()
    
let fetchActionItems userId = 
    let first f queryable =     
        System.Linq.Queryable.First (queryable, f)
    use context = new ActionableDbContext ()
    let t = 
        query {
            for typ in context.TaskTypeDefinitions do
            where (typ.FullyQualifiedName = "actionable.actionitem")
            select typ
            exactlyOne
        }

    query {
        for actionItem in context.TaskInstances do
        where (actionItem.TaskTypeDefinitionId = 1 && actionItem.UserIdentity = userId)
        select actionItem }
    |> Seq.map mapToActionItemReadModel
    |> Seq.toList


