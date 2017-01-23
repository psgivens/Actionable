module Actionable.Domain.Persistance.EventSourcing.EF

open Actionable.Domain.Infrastructure
open Actionable.Domain.ActionItemModule
open Newtonsoft.Json
open Actionable.Data


type Actionable.Data.ActionableDbContext with 
    member this.GetActionItemEvents<'a> streamId :seq<Envelope<'a>>= 
        query {
            for event in this.ActionItemEvents do
            where (event.StreamId = streamId)
            select event
        } |> Seq.map (fun event ->
                {
                    Id = event.Id
                    UserId = event.UserId
                    DeviceId = event.DeviceId
                    StreamId = streamId
                    TransactionId = event.TransactionId
                    Version = event.Version
                    Created = event.TimeStamp
                    Item = (JsonConvert.DeserializeObject<'a> event.Event)
                }
            )


type GenericEventStore<'a> () =
    interface IEventStore<Envelope<'a>> with
        member this.GetEvents (streamId:StreamId) =
            use context = new ActionableDbContext ()
            context.GetActionItemEvents streamId 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEventAsync (streamId:StreamId) (envelope:Envelope<'a>) =
            async { 
                try
                    use context = new ActionableDbContext ()
                    context.ActionItemEvents.Add (
                        ActionItemEnvelopeEntity (  Id = envelope.Id,
                                                    StreamId = envelope.StreamId,
                                                    UserId = envelope.UserId,
                                                    TransactionId = envelope.TransactionId,
                                                    Version = envelope.Version,
                                                    TimeStamp = envelope.Created,
                                                    Event = JsonConvert.SerializeObject(envelope.Item)
                                                    )) |> ignore         
                    do! Async.AwaitTask (context.SaveChangesAsync()) |> Async.Ignore 
                    do! async.Zero ()
                with
                    | ex -> System.Diagnostics.Debugger.Break () 
                }


type ActionItemEventStore () =
    interface IEventStore<Envelope<ActionItemEvent>> with
        member this.GetEvents (streamId:StreamId) =
            use context = new ActionableDbContext ()
            context.GetActionItemEvents<ActionItemEvent> streamId 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEventAsync (streamId:StreamId) (envelope:Envelope<ActionItemEvent>) =
            async { 
                try
                    use context = new ActionableDbContext ()
                    context.ActionItemEvents.Add (
                        ActionItemEnvelopeEntity (  Id = envelope.Id,
                                                    StreamId = envelope.StreamId,
                                                    UserId = envelope.UserId,
                                                    TransactionId = envelope.TransactionId,
                                                    Version = envelope.Version,
                                                    TimeStamp = envelope.Created,
                                                    Event = JsonConvert.SerializeObject(envelope.Item)
                                                    )) |> ignore         
                    do! Async.AwaitTask (context.SaveChangesAsync()) |> Async.Ignore 
                    do! async.Zero ()
                with
                    | ex -> System.Diagnostics.Debugger.Break () 
                }

let mapFieldValuesToDefinitions<'a when 'a :> FieldInstanceBase> 
        (fieldDefs:((int * (FieldDefinition seq)) seq))
        (fields:Map<string,string>)     
        (fieldType:FieldType)
        (constructField:(FieldDefinition*string)->'a)
        (setField:'a->string->unit)
        (list':System.Collections.Generic.IList<'a>) = 
    
    fieldDefs
    |> Seq.find (fun (k,v) -> k = int fieldType) 
    |> snd
    |> Seq.iter (fun fd ->
        match list' |> Seq.tryFind (fun fv -> fv.FieldDefinition = fd) with
        | Some (li) -> 
            match fields.TryFind fd.FullyQualifiedName with
            | Some (field)  -> setField li field
            | Option.None   -> setField li fd.DefaultValue
        | Option.None ->
            match fields.TryFind fd.FullyQualifiedName with
            | Some (field)  -> list'.Add (constructField (fd, field))
            | Option.None   -> list'.Add (constructField (fd, fd.DefaultValue))                     
        )
    list'

let mapToFields'<'a when 'a :> FieldInstanceBase> 
    (fieldDefs:((int * (FieldDefinition seq)) seq))
    (fields:Map<string,string>)     
    (fieldType:FieldType)
    (constructField:(FieldDefinition*string)->'a) = 
    System.Collections.Generic.List<'a>() |> mapFieldValuesToDefinitions fieldDefs fields fieldType constructField (fun x y -> ())

let persistActionItem userId (streamId:StreamId) state = 
    async {
        try
            use context = new ActionableDbContext ()
            let! entity = context.TaskInstances.FindAsync(streamId) |> Async.AwaitTask

            match state with
            | DoesNotExist -> 
                match entity with
                | null -> ()
                | entity ->
                    context.TaskInstances.Remove (entity) |> ignore
                    do! Async.AwaitTask (context.SaveChangesAsync()) |> Async.Ignore  

            | State (item) -> 
                
                let typeDef = query {
                    for taskType in context.TaskTypeDefinitions do
                    where (taskType.FullyQualifiedName = "actionable.actionitem")
                    select taskType
                    exactlyOne }
                let groupedFields = typeDef.Fields |> Seq.groupBy (fun f -> f.FieldType)
                
                match entity with                   
                | null ->
                    context.TaskInstances.Add (
                        TaskTypeInstance(   
                            Id = streamId,
                            TaskTypeDefinition = typeDef,
                            UserIdentity = userId,
                            IntFields = mapToFields' groupedFields item.Fields FieldType.Int (fun (d,v) ->
                                IntFieldInstance(FieldDefinition = d, Value = System.Int32.Parse v)),
                            StringFields = mapToFields' groupedFields item.Fields FieldType.String (fun (d,v) ->
                                StringFieldInstance(FieldDefinition = d, Value = v)),
                            DateFields = mapToFields' groupedFields item.Fields FieldType.DateTime (fun (d,v) ->
                                DateTimeFieldInstance(FieldDefinition = d, Value = System.DateTimeOffset.Parse v))
                        )) |> ignore
                    do! Async.AwaitTask (context.SaveChangesAsync()) |> Async.Ignore  
                | entity ->
                    entity.IntFields |> mapFieldValuesToDefinitions groupedFields item.Fields FieldType.Int (fun (d,v) ->
                        IntFieldInstance(FieldDefinition = d, Value = System.Int32.Parse v)) (fun i v ->
                        i.Value <- System.Int32.Parse v) |> ignore
                    entity.StringFields |> mapFieldValuesToDefinitions groupedFields item.Fields FieldType.String (fun (d,v) ->
                        StringFieldInstance(FieldDefinition = d, Value = v)) (fun i v ->
                        i.Value <- v) |> ignore
                    entity.DateFields |> mapFieldValuesToDefinitions groupedFields item.Fields FieldType.DateTime (fun (d,v) ->
                        DateTimeFieldInstance(FieldDefinition = d, Value = System.DateTimeOffset.Parse v)) (fun i v ->
                        i.Value <- System.DateTimeOffset.Parse v) |> ignore
                    do! Async.AwaitTask (context.SaveChangesAsync()) |> Async.Ignore  
        with
        // TODO: Need better exeception handling. Logging, perhaps?
        | ex -> System.Diagnostics.Debugger.Break ()
    }


type ActionItemReadModel = {
    Fields: Map<string,string>
    Id: string
    UserId: string
    }

let mapToActionItemReadModel (task:TaskTypeInstance) =
    {   ActionItemReadModel.Fields = 
            (task.StringFields |> Seq.map (fun f -> 
                f.FieldDefinition.FullyQualifiedName, f.Value.ToString())
            |> Seq.toList) @ (task.DateFields |> Seq.map (fun f ->  
                f.FieldDefinition.FullyQualifiedName, f.Value.ToString())
            |> Seq.toList) @ (task.IntFields |> Seq.map (fun f -> 
                f.FieldDefinition.FullyQualifiedName, f.Value.ToString())
            |> Seq.toList) |> Map.ofList
        Id = task.Id.ToString() 
        UserId = task.UserIdentity }
