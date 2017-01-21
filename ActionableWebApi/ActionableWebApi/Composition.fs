namespace ActionableWebApi

open System.Web.Http.Dispatcher
open System.Web.Http.Controllers;

//open Actionable.Domain.Infrastructure
open Actionable.Domain.ActionItemModule
//open Actionable.Domain.Infrastructure.Envelope

type CompositRoot () = 
    let actionItemAgent = new ActionItemAggregateAgent ()
    let actionItemPersister = new ActionItemPersistingAgent ()

    do actionItemAgent.Subscribe actionItemPersister.Post |> ignore

    interface IHttpControllerActivator with
        member this.Create (request, controllerDescriptor, controllerType) =
            if controllerType = typeof<ActionsController> then
                new ActionsController (actionItemAgent) :> IHttpController
            else
                System.Activator.CreateInstance(controllerType) :?> IHttpController

   
