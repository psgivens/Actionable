module Actionable.Agents.Composition
open Actionable.Agents

//open System.Web.Http.Dispatcher
//open System.Web.Http.Controllers;

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
    static let sessionNotificationAggregate = new SessionNotificationAggregateAgent ()
    static let userNotificationAggregate = new UserNotificationAggregateAgent ()
    static let actionItemAgent = new ActionItemAggregateAgent ()
    static let actionItemPersister = new ActionItemPersistingAgent ()

    static do actionItemAgent.Subscribe
                ((fun env ->
                    actionItemPersister.Post env
                ), (fun (ex:Exception) -> ()
                    // TODO: Handle exceptions
                ))
            |> ignore

    static do actionItemPersister.Subscribe 
                ((fun env ->
                    ((fun item -> UserNotificationsCommand.AppendMessage ("Created")
                    |> repackage) >> userNotificationAggregate.Post) |> ignore
                ), (fun (ex:Exception) -> ()
                    // TODO: Handle exceptions
                ))
            |> ignore

    static do userNotificationAggregate.Subscribe 
                ((fun env -> ()
                    // TODO: Handle user notifications
                ), (fun (ex:Exception) -> ()
                    // TODO: Handle exceptions
                ))
            |> ignore

    static do sessionNotificationAggregate.Subscribe 
                ((fun env -> ()
                    // TODO: Handle session notifications
                ), (fun (ex:Exception) -> ()
                    // TODO: Handle exceptions
                ))
            |> ignore
        
    static do actionItemPersister.Subscribe 
                ((fun item -> UserNotificationsCommand.AppendMessage ("Created")
                |> repackage) >> userNotificationAggregate.Post) |> ignore

    static member ActionItemAgent 
        with get() = actionItemAgent
