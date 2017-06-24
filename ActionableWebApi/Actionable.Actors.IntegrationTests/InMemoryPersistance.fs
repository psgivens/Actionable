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
            | None -> List.empty<Envelope<'TEventType>> 
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

open Actionable.Domain.Persistance.EventSourcing.ActionItemEF
let mutable actionItems = Map.empty<System.Guid,ActionItem>
let persistActionItem userId (StreamId.Id(streamId)) state = 
    async {
        match state with
        | ActionItemState.DoesNotExist -> 
            actionItems <- actionItems.Remove streamId
        | State(item) ->
            actionItems <- actionItems.Add(streamId, item)
        do! async.Zero ()
    }

let fetchActionItem userId :ActionItemReadModel list= 
    actionItems 
    |> Map.map (fun key value ->
        {
            ActionItemReadModel.UserId = userId
            ActionItemReadModel.Fields = value.Fields
            ActionItemReadModel.Id = key
        })
    |> Map.toList
    |> List.map snd

open Actionable.Domain.UserNotificationsModule
open Actionable.Domain.Persistance.EventSourcing.UserNotificationEF
open Actionable.Data

let mutable userNotifications = Map.empty<string,UserNotificationEntity list>
let persistUserNotification (UserId.Val(userId)) (StreamId.Id(streamId)) state = 
    async {
        match state with
        | UserNotificationsState.DoesNotExist -> 
            userNotifications <- userNotifications.Remove userId
        | State(notifications) ->
            userNotifications <- userNotifications.Add(userId, 
                notifications.items
                |> Map.map (fun key item ->
                    UserNotificationEntity(
                        Id = key,
                        UserIdentity = userId,
                        Code = item.code,
                        Message = item.message,
                        Status = item.status
                    ))
                |> Map.toList |> List.map snd)

            
        do! async.Zero ()
    }

let fetchUserNotifications userId :UserNotificationEntity list option= 
    userNotifications |> Map.tryFind userId