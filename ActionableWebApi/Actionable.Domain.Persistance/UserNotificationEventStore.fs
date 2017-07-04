﻿module Actionable.Domain.Persistance.EventSourcing.UserNotificationEF

open Actionable.Domain.Persistance.Core
open Actionable.Domain.Infrastructure
open Actionable.Domain.UserNotificationsModule
open Newtonsoft.Json
open Actionable.Data


type UserNotificationReadModel = {
    Id: int
    Type: string
    Message: string
    Status: int
    }

type Actionable.Data.ActionableDbContext with 
    member this.GetUserNotificationStream userId =
        async {
            use context = new ActionableDbContext()
            let streamIds = 
                query {
                    for mapping in context.UserNotificationMappings do
                    where (mapping.UserId = userId)
                    select mapping.UserNotificationStreamId } 
                |> Seq.toList
            match streamIds with
            | [streamId] -> return streamId 
            | [] ->
                let streamId = System.Guid.NewGuid () 
                context.UserNotificationMappings.Add(
                    UserToNotificationMapping(
                        UserId=userId,
                        UserNotificationStreamId=streamId))
                |> ignore
                do! context |> saveChanges
                return streamId
            | _ -> return failwith "A user cannot have more than one user notification stream"                
        }

//
//
//type Actionable.Data.ActionableDbContext with 
//    member this.GetUserNotificationEvents<'a> (StreamId.Id (aggregateId):StreamId) :seq<Envelope<'a>>= 
//        query {
//            for event in this.ActionItemEvents do
//            where (event.StreamId = aggregateId)
//            select event
//        } |> Seq.map (fun event ->
//                {
//                    Id = event.Id
//                    UserId = UserId.box event.UserId
//                    StreamId = StreamId.box aggregateId
//                    TransactionId = TransId.box event.TransactionId
//                    Version = Version.box (event.Version)
//                    Created = event.TimeStamp
//                    Item = (JsonConvert.DeserializeObject<'a> event.Event)
//                }
//            )
//
//type ActionItemEventStore () =
//    interface IEventStore<Envelope<UserNotificationsEvent>> with
//        member this.GetEvents (streamId:StreamId) =
//            use context = new ActionableDbContext ()
//            context.GetUserNotificationEvents<ActionItemEvent> streamId 
//            |> Seq.toList 
//            |> List.sortBy(fun x -> x.Version)
//        member this.AppendEventAsync (streamId:StreamId) (envelope:Envelope<UserNotificationsEvent>) =
//            async { 
//                try
//                    use context = new ActionableDbContext ()
//                    context.ActionItemEvents.Add (
//                        ActionItemEnvelopeEntity (  Id = envelope.Id,
//                                                    StreamId = StreamId.unbox envelope.StreamId,
//                                                    UserId = UserId.unbox envelope.UserId,
//                                                    TransactionId = TransId.unbox envelope.TransactionId,
//                                                    Version = Version.unbox envelope.Version,
//                                                    TimeStamp = envelope.Created,
//                                                    Event = JsonConvert.SerializeObject(envelope.Item)
//                                                    )) |> ignore         
//                    do! Async.AwaitTask (context.SaveChangesAsync()) |> Async.Ignore 
//                    do! async.Zero ()
//                with
//                    | ex -> System.Diagnostics.Debugger.Break () 
//                }
//
//let mapFieldValuesToDefinitions<'a when 'a :> FieldInstanceBase> 
//        (fieldDefs:((int * (FieldDefinition seq)) seq))
//        (fields:Map<string,string>)     
//        (fieldType:FieldType)
//        (constructField:(FieldDefinition*string)->'a)
//        (setField:'a->string->unit)
//        (list':System.Collections.Generic.IList<'a>) = 
//    
//    fieldDefs
//    |> Seq.find (fun (k,v) -> k = int fieldType) 
//    |> snd
//    |> Seq.iter (fun fd ->
//        match list' |> Seq.tryFind (fun fv -> fv.FieldDefinition = fd) with
//        | Some (li) -> 
//            match fields.TryFind fd.FullyQualifiedName with
//            | Some (field)  -> setField li field
//            | Option.None   -> setField li fd.DefaultValue
//        | Option.None ->
//            match fields.TryFind fd.FullyQualifiedName with
//            | Some (field)  -> list'.Add (constructField (fd, field))
//            | Option.None   -> list'.Add (constructField (fd, fd.DefaultValue))                     
//        )
//    list'
//
//let mapToFields'<'a when 'a :> FieldInstanceBase> 
//    (fieldDefs:((int * (FieldDefinition seq)) seq))
//    (fields:Map<string,string>)     
//    (fieldType:FieldType)
//    (constructField:(FieldDefinition*string)->'a) = 
//    System.Collections.Generic.List<'a>() |> mapFieldValuesToDefinitions fieldDefs fields fieldType constructField (fun x y -> ())


//let persistUserNotification (UserId.Val(userId):UserId) (StreamId.Id (streamId):StreamId) state = 
//    async {
//        try
//            use context = new ActionableDbContext ()
//            let! entity = context.TaskInstances.FindAsync(streamId) |> Async.AwaitTask
//
//            match state with
//            | DoesNotExist -> 
//                match entity with
//                | null -> ()
//                | entity ->
//                    context.TaskInstances.Remove (entity) |> ignore
//                    do! Async.AwaitTask (context.SaveChangesAsync()) |> Async.Ignore  
//
//            | State (item) -> 
//                
//                let typeDef = query {
//                    for taskType in context.TaskTypeDefinitions do
//                    where (taskType.FullyQualifiedName = "actionable.actionitem")
//                    select taskType
//                    exactlyOne }
//                let groupedFields = typeDef.Fields |> Seq.groupBy (fun f -> f.FieldType)
//                
//                match entity with                   
//                | null ->
//                    context.TaskInstances.Add (
//                        TaskTypeInstance(   
//                            Id = streamId,
//                            TaskTypeDefinition = typeDef,
//                            UserIdentity = userId,
//                            IntFields = mapToFields' groupedFields item.Fields FieldType.Int (fun (d,v) ->
//                                IntFieldInstance(FieldDefinition = d, Value = System.Int32.Parse v)),
//                            StringFields = mapToFields' groupedFields item.Fields FieldType.String (fun (d,v) ->
//                                StringFieldInstance(FieldDefinition = d, Value = v)),
//                            DateFields = mapToFields' groupedFields item.Fields FieldType.DateTime (fun (d,v) ->
//                                DateTimeFieldInstance(FieldDefinition = d, Value = System.DateTimeOffset.Parse v))
//                        )) |> ignore
//                    do! Async.AwaitTask (context.SaveChangesAsync()) |> Async.Ignore  
//                | entity ->
//                    entity.IntFields |> mapFieldValuesToDefinitions groupedFields item.Fields FieldType.Int (fun (d,v) ->
//                        IntFieldInstance(FieldDefinition = d, Value = System.Int32.Parse v)) (fun i v ->
//                        i.Value <- System.Int32.Parse v) |> ignore
//                    entity.StringFields |> mapFieldValuesToDefinitions groupedFields item.Fields FieldType.String (fun (d,v) ->
//                        StringFieldInstance(FieldDefinition = d, Value = v)) (fun i v ->
//                        i.Value <- v) |> ignore
//                    entity.DateFields |> mapFieldValuesToDefinitions groupedFields item.Fields FieldType.DateTime (fun (d,v) ->
//                        DateTimeFieldInstance(FieldDefinition = d, Value = System.DateTimeOffset.Parse v)) (fun i v ->
//                        i.Value <- System.DateTimeOffset.Parse v) |> ignore
//                    do! Async.AwaitTask (context.SaveChangesAsync()) |> Async.Ignore  
//        with
//        // TODO: Need better exeception handling. Logging, perhaps?
//        | ex -> System.Diagnostics.Debugger.Break ()
//    }
//
//

//
////let mapToUserNotificationReadModel (task:TaskTypeInstance) =
////    {   ActionItemReadModel.Fields = 
////            (task.StringFields |> Seq.map (fun f -> 
////                f.FieldDefinition.FullyQualifiedName, f.Value.ToString())
////            |> Seq.toList) @ (task.DateFields |> Seq.map (fun f ->  
////                f.FieldDefinition.FullyQualifiedName, f.Value.ToString())
////            |> Seq.toList) @ (task.IntFields |> Seq.map (fun f -> 
////                f.FieldDefinition.FullyQualifiedName, f.Value.ToString())
////            |> Seq.toList) |> Map.ofList
////        Id = task.Id
////        UserId = task.UserIdentity }
//
//let fetchActionItems userId = 
//    let first f queryable =     
//        System.Linq.Queryable.First (queryable, f)
//    use context = new ActionableDbContext ()
//    let t = 
//        query {
//            for typ in context.TaskTypeDefinitions do
//            where (typ.FullyQualifiedName = "actionable.actionitem")
//            select typ
//            exactlyOne
//        }
//
//    query {
//        for actionItem in context.TaskInstances do
//        where (actionItem.TaskTypeDefinitionId = 1 && actionItem.UserIdentity = userId)
//        select actionItem }
//    |> Seq.map mapToActionItemReadModel
//    |> Seq.toList

