namespace ActionableWebApi

open System.Web.Http.Dispatcher
open System.Web.Http.Controllers;

open Actionable.Domain.UserNotificationsModule
type UserNotificationAggregateAgent () = 
    inherit AggregateAgent<UserNotifications, UserNotificationsState, UserNotificationsCommand, UserNotificationsEvent> (
        DoesNotExist, Actionable.Domain.Persistance.EventSourcing.EF.GenericEventStore(), 
        buildState, 
        handle) 

open Actionable.Domain.SessionNotificationsModule
type SessionNotificationAggregateAgent () = 
    inherit AggregateAgent<SessionNotifications, SessionNotificationsState, SessionNotificationsCommand, SessionNotificationsEvent> (
        DoesNotExist, Actionable.Domain.Persistance.EventSourcing.EF.GenericEventStore(), 
        buildState, 
        handle) 

open Actionable.Domain.ActionItemModule
type ActionItemAggregateAgent () = 
    inherit AggregateAgent<ActionItem, ActionItemState, ActionItemCommand, ActionItemEvent> (
        DoesNotExist, Actionable.Domain.Persistance.EventSourcing.EF.ActionItemEventStore(), 
        buildState, 
        handle) 

type ActionItemPersistingAgent () = 
    inherit PersistingAgent<ActionItem, ActionItemState, ActionItemCommand, ActionItemEvent> (
        DoesNotExist, Actionable.Domain.Persistance.EventSourcing.EF.ActionItemEventStore(), 
        buildState, 
        Actionable.Domain.Persistance.EventSourcing.EF.persistActionItem)

open Actionable.Domain.Infrastructure
open System

type CompositRoot () = 
    let sessionNotificationAggregate = new SessionNotificationAggregateAgent ()
    let userNotificationAggregate = new UserNotificationAggregateAgent ()
    let actionItemAgent = new ActionItemAggregateAgent ()
    let actionItemPersister = new ActionItemPersistingAgent ()

    do actionItemAgent.Subscribe  
        ((fun env ->
            actionItemPersister.Post env
        ), (fun (ex:Exception) -> ()
            // TODO: Handle exceptions
        ))
    |> ignore

    do actionItemPersister.Subscribe 
        ((fun env ->
            ((fun item -> UserNotificationsCommand.AppendMessage ("Created")
            |> repackage) >> userNotificationAggregate.Post) |> ignore
        ), (fun (ex:Exception) -> ()
            // TODO: Handle exceptions
        ))
    |> ignore

    do userNotificationAggregate.Subscribe 
        ((fun env -> ()
            // TODO: Handle user notifications
        ), (fun (ex:Exception) -> ()
            // TODO: Handle exceptions
        ))
    |> ignore

    do sessionNotificationAggregate.Subscribe 
        ((fun env -> ()
            // TODO: Handle session notifications
        ), (fun (ex:Exception) -> ()
            // TODO: Handle exceptions
        ))
    |> ignore
        
    do actionItemPersister.Subscribe 
        ((fun item -> UserNotificationsCommand.AppendMessage ("Created")
        |> repackage) >> userNotificationAggregate.Post) |> ignore

    interface IHttpControllerActivator with
        member this.Create (request, controllerDescriptor, controllerType) =
            match controllerType with
            | t when t = typeof<ActionsController> -> 
                new ActionsController (actionItemAgent.Post) :> IHttpController
            | _ -> System.Activator.CreateInstance(controllerType) :?> IHttpController

   
