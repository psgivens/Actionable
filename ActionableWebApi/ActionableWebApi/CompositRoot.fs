﻿namespace ActionableWebApi

//open Actionable.Agents
open System.Web.Http.Dispatcher
open System.Web.Http.Controllers;

open Actionable.Actors.Initialization

//open Actionable.Agents.Composition
open Actionable.Actors.Composition
open Actionable.Domain.Persistance.EventSourcing.ActionItemEF
open Actionable.Domain.Persistance.EventSourcing.UserNotificationEF

open Akka
open Akka.Actor
open Akka.FSharp

type CompositRoot 
        (actionable:ActionableActors,
         fetchActionItems:string -> ActionItemReadModel list,
         fetchUserNotifications: string -> UserNotificationReadModel list option) = 
    interface IHttpControllerActivator with
        member this.Create (request, controllerDescriptor, controllerType) =
            match controllerType with
            | t when t = typeof<ActionsController> -> 
                new ActionsController (actionable.ActionItemAggregateProcessor.Tell, fetchActionItems) :> IHttpController
            | t when t = typeof<UserNotificationsController> -> 
                new UserNotificationsController (actionable.UserNotificationsAggregateProcessor.Tell, fetchUserNotifications) :> IHttpController
            | _ -> System.Activator.CreateInstance(controllerType) :?> IHttpController

   
