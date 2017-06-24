namespace ActionableWebApi

open Actionable.Agents
open System.Web.Http.Dispatcher
open System.Web.Http.Controllers;

open Actionable.Agents.Composition
open Actionable.Domain.Persistance.EventSourcing.ActionItemEF


type CompositRoot () = 
    interface IHttpControllerActivator with
        member this.Create (request, controllerDescriptor, controllerType) =
            match controllerType with
            | t when t = typeof<ActionsController> -> 
                new ActionsController (Actionable.Agents.Composition.CompositRoot.ActionItemAgent.Post, fetchActionItems) :> IHttpController
            | _ -> System.Activator.CreateInstance(controllerType) :?> IHttpController

   
