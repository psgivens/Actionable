module Actionable.Domain.Persistance.EventSourcing.UserNotificationEF

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



type Actionable.Data.ActionableDbContext with 
    member this.GetUserNotificationEvents<'a> (StreamId.Id (aggregateId):StreamId) :seq<Envelope<'a>>= 
        query {
            for event in this.ActionItemEvents do
            where (event.StreamId = aggregateId)
            select event
        } |> Seq.map (fun event ->
                {
                    Envelope.Id = event.Id
                    UserId = UserId.box event.UserId
                    StreamId = StreamId.box aggregateId
                    TransactionId = TransId.box event.TransactionId
                    Version = Version.box (event.Version)
                    Created = event.TimeStamp
                    Item = (JsonConvert.DeserializeObject<'a> event.Event)
                }
            )

type UserNotificationEventStore () =
    interface IEventStore<Envelope<UserNotificationsEvent>> with
        member this.GetEvents (streamId:StreamId) =
            use context = new ActionableDbContext ()
            context.GetUserNotificationEvents<UserNotificationsEvent> streamId 
            |> Seq.toList 
            |> List.sortBy(fun x -> x.Version)
        member this.AppendEvent (streamId:StreamId) (envelope:Envelope<UserNotificationsEvent>) =
            try
                use context = new ActionableDbContext ()
                context.ActionItemEvents.Add (
                    ActionItemEnvelopeEntity (  
                        Id = envelope.Id,
                        StreamId = StreamId.unbox envelope.StreamId,
                        UserId = UserId.unbox envelope.UserId,
                        TransactionId = TransId.unbox envelope.TransactionId,
                        Version = Version.unbox envelope.Version,
                        TimeStamp = envelope.Created,
                        Event = JsonConvert.SerializeObject(envelope.Item)
                        )) |> ignore         
                context.SaveChanges() |> ignore
                
            with
                | ex -> System.Diagnostics.Debugger.Break () 

open Actionable.Data
open Actionable.Domain.Infrastructure
let getUserNotificationStreamId (UserId.Val(userId)) = 
    use context = new ActionableDbContext() 
    query {
        for mapping in context.UserNotificationMappings do
        where (mapping.UserId = userId)
        select mapping
        exactlyOneOrDefault
    } 
    |> fun result ->
        if result = null then 
            let streamId = System.Guid.NewGuid()
            new UserToNotificationMapping (
                UserId = userId,
                UserNotificationStreamId = streamId)
            |> context.UserNotificationMappings.Add
            |> ignore
            streamId |> StreamId.box
        else 
            result.UserNotificationStreamId |> StreamId.box

let persistUserNotification (UserId.Val(userId)) (StreamId.Id (streamId)) (state:UserNotificationsState) =
    use context = new ActionableDbContext ()
    
    let entities = 
        query {
            for noti in context.UserNotifications do
            where (noti.UserIdentity = userId)
            select noti }
        |> List.ofSeq

    match state with
    | DoesNotExist ->
        match entities with 
        | [] -> ()
        | _ ->
            entities 
            |> List.iter (fun entity ->
                context.UserNotifications.Remove entity |> ignore)
            context.SaveChanges () |> ignore
    | State (package) ->
        let entities' = 
            entities 
            |> List.map (fun entity -> entity.Id, entity)
            |> Map.ofList

        package.items
        |> Map.fold (fun (acc:Map<int,UserNotificationEntity>) key item ->
            match acc |> Map.tryFind key with
            | None -> 
                context.UserNotifications.Add (                    
                    new UserNotificationEntity (
                        UserIdentity = userId,
                        Id = key,
                        Code = item.code,
                        Status = item.status,
                        Message = item.message    
                    )) |> ignore                
            | Some (entity) ->
                entity.Code <- item.code
                entity.Status <- item.status
                entity.Message <- item.message
            acc
          ) entities'
        |> ignore

        context.SaveChanges () |> ignore


let fetchUserNotifications userId =
    use context = new ActionableDbContext ()    
    query {
        for noti in context.UserNotifications do
        where (noti.UserIdentity = userId)
        select {
            UserNotificationReadModel.Id = noti.Id
            Type = noti.Code.ToString ()
            Message = noti.Message
            Status = noti.Status
        } }
    |> List.ofSeq
    |> Some    
    
