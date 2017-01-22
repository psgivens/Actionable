namespace ActionableWebApi

open System.Web.Http.Dispatcher
open System.Web.Http.Controllers;

open Actionable.Domain.ActionItemModule


type CompositRoot () = 
    let actionItemAgent = new ActionItemAggregateAgent ()
    let actionItemPersister = new ActionItemPersistingAgent ()

    do actionItemAgent.Subscribe actionItemPersister.Post |> ignore

    interface IHttpControllerActivator with
        member this.Create (request, controllerDescriptor, controllerType) =
            match controllerType with
            | t when t = typeof<ActionsController> -> 
                new ActionsController (actionItemAgent) :> IHttpController
            | _ -> System.Activator.CreateInstance(controllerType) :?> IHttpController

   
