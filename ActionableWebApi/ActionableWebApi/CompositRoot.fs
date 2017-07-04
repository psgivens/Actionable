namespace ActionableWebApi

//open Actionable.Agents
open System.Web.Http.Dispatcher
open System.Web.Http.Controllers;

open Actionable.Actors.Initialization

//open Actionable.Agents.Composition
open Actionable.Actors.Composition
open Actionable.Domain.ActionItemsQueryResponse
open Actionable.Domain.Persistance.EventSourcing.ActionItemEF
open Actionable.Domain.Persistance.EventSourcing.UserNotificationEF
open Actionable.Domain.Infrastructure

open Akka
open Akka.Actor
open Akka.FSharp

type CompositRoot 
        (actionable:ActionableActors,
         getUserNotificationStreamId:UserId -> StreamId,
         getActionItem:System.Guid -> ActionItemReadModel option,
         getActionItems:string -> ActionItemReadModel list,
         getUserNotifications: string -> UserNotificationReadModel list option) = 
    interface IHttpControllerActivator with
        member this.Create (request, controllerDescriptor, controllerType) =
            match controllerType with
            | t when t = typeof<ActionsController> -> 
                new ActionsController (actionable.ActionItemAggregateProcessor.Tell, getActionItem, getActionItems) :> IHttpController
            | t when t = typeof<UserNotificationsController> -> 
                new UserNotificationsController (actionable.UserNotificationsAggregateProcessor.Tell, getUserNotificationStreamId, getUserNotifications) :> IHttpController
            | _ -> System.Activator.CreateInstance(controllerType) :?> IHttpController

   
