namespace ActionableWebApi

open System
open System.Net
open System.Net.Http
open System.Web.Http

open System.Security.Principal
open Microsoft.AspNet.Identity

open Actionable.Data

//open Composition
open Actionable.Domain.ActionItemModule
open Actionable.Domain.ActionItemsQueryResponse
open Actionable.Domain.Infrastructure

open Actionable.Domain.Persistance.EventSourcing.ActionItemEF
open System.Web.SessionState

type ActionItemsQueryResponse () =
    [<DefaultValue>] val mutable Results : ActionItemReadModel list
    [<DefaultValue>] val mutable Time : string
type ActionItemQueryResponse () =
    [<DefaultValue>] val mutable Result : ActionItemReadModel 
    [<DefaultValue>] val mutable Time : string
                
type ActionsController 
        (post:Envelope<ActionItemCommand> -> unit, 
         getActionItem:System.Guid -> ActionItemReadModel option, 
         getActionItems:string -> ActionItemReadModel list) =
    inherit ApiController ()
    
    interface IRequiresSessionState
    member this.Post (item: AddActionItemRendition) =
        // TODO Parse Int16.Parse(item.Status)
        let userId = this.User.Identity.GetUserId()
        let streamId, cmd = 
            match item.Id |> Guid.TryParse with
            | true, id -> 
                StreamId.box id, Update (item.Fields)
            | _ -> 
                let streamId = StreamId.create ()
                streamId, (Create(userId, streamId |> StreamId.unbox, item.Fields))
            
        cmd
        |> envelopWithDefaults 
            (UserId.box <| userId) 
            (TransId.create ()) 
            (StreamId.create ())
            (Version.box 0s)
        |> post

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            ResponseCode(
                Message = "TODO: Respond with transaction Id",
                Time = DateTimeOffset.Now.ToString("o")))

    member this.Delete (item: ActionItemIdRendition) =
        Delete
        |> envelopWithDefaults
            (UserId.box <| this.User.Identity.GetUserId())        
            (TransId.create ()) 
            (StreamId.box <| item.GetActionItemId ())
            (Version.box 0s)
        |> post      

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            ResponseCode(
                Message = "TODO: Respond with transaction Id",
                Time = DateTimeOffset.Now.ToString("o")))

//
//    [<HttpGet; Route()>]
//    member this.Get () =
//        this.Request.CreateResponse(
//            HttpStatusCode.OK,
//            ActionItemsQueryResponse (
//                Results = getActionItems (this.User.Identity.GetUserId()),
//                Time = DateTimeOffset.Now.ToString("o")))
       
//    [<HttpGet; Route("{id}")>]
    member this.Get (item: ActionItemIdRendition) =
        if obj.ReferenceEquals (item, null) then 
            this.Request.CreateResponse(
                HttpStatusCode.OK,
                ActionItemsQueryResponse (
                    Results = getActionItems (this.User.Identity.GetUserId()),
                    Time = DateTimeOffset.Now.ToString("o")))
        else
            this.Request.CreateResponse(
                HttpStatusCode.OK,
                ActionItemQueryResponse (
                    Result = ((item.GetActionItemId () |> getActionItem) |> Option.get),
                    Time = DateTimeOffset.Now.ToString("o")))
        

