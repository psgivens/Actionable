module Actionable.Actors.IntegrationTests.Perisistance 

open Actionable.Domain.Infrastructure
open Actionable.Domain
open Actionable.Domain.ActionItemModule
open Actionable.Domain.ActionItemsQueryResponse

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
open Actionable.Domain.UserNotificationsModule
open Actionable.Domain.Persistance.EventSourcing.UserNotificationEF
open Actionable.Data

type InMemoryPersistence () =   
    let mutable actionItems = Map.empty<System.Guid,ActionItem>
    let mutable userNotifications = Map.empty<string,UserNotificationEntity list>

    member this.PersistActionItem userId (StreamId.Id(streamId)) state = 
        match state with
        | ActionItemState.DoesNotExist -> 
            actionItems <- actionItems.Remove streamId
        | ActionItemState.State(item) ->
            actionItems <- actionItems.Add(streamId, item)

    member this.GetActionItem itemId :ActionItemReadModel option= 
    
        match actionItems |> Map.tryFind itemId with
        | Some(value) ->
            Some({ ActionItemReadModel.UserId = value.UserId
                   Fields = value.Fields
                   Id = value.Id })
        | _ -> None

    member this.GetActionItems userId :ActionItemReadModel list= 
        actionItems 
        |> Map.map (fun key value ->
            {
                ActionItemReadModel.UserId = userId
                ActionItemReadModel.Fields = value.Fields
                ActionItemReadModel.Id = key
            })
        |> Map.toList
        |> List.map snd

    member this.PersistUserNotification (UserId.Val(userId)) (StreamId.Id(streamId)) state = 
        match state with
        | UserNotificationsState.DoesNotExist -> 
            userNotifications <- userNotifications.Remove userId
        | UserNotificationsState.State (notifications) ->
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

    member this.GetUserNotifications userId :UserNotificationReadModel list option= 
        match userNotifications |> Map.tryFind userId with
        | None -> None
        | Some(items) ->
            items 
            |> List.map (fun item ->
                {
                    UserNotificationReadModel.Id=item.Id
                    Type="Notification"
                    Message=item.Message
                    Status=item.Status
                })
            |> Some
