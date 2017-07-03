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
open Actionable.Domain.Infrastructure

open Actionable.Domain.Persistance.EventSourcing.ActionItemEF
open System.Web.SessionState

type ActionItemsQueryResponse () =
    [<DefaultValue>] val mutable Results : ActionItemReadModel list
    [<DefaultValue>] val mutable Time : string
type ActionItemQueryResponse () =
    [<DefaultValue>] val mutable Result : ActionItemReadModel 
    [<DefaultValue>] val mutable Time : string
                
type ActionsController (post:Envelope<ActionItemCommand>->unit, fetchActionItem:System.Guid->ActionItemReadModel option, fetchActionItems:string->ActionItemReadModel list) =
    inherit ApiController ()
    
    let (|GuidPattern|_|) guid = 
        match Guid.TryParse guid with 
        | (true, id) -> Some(id)
        | _ -> Option.None
    interface IRequiresSessionState
    member this.Post (item: AddActionItemRendition) =
        // TODO Parse Int16.Parse(item.Status)
        let envelope = 
            match item.Id with 
            | GuidPattern id ->
                envelopWithDefaults 
                    (UserId.box <| this.User.Identity.GetUserId()) 
                    (TransId.create ()) 
                    (StreamId.box id) 
                    (Version.box 0s)
                    (Update(item.Fields))
            | _ ->
                let streamId = StreamId.create ()
                let userId = this.User.Identity.GetUserId()
                envelopWithDefaults 
                    (UserId.box <| userId) 
                    (TransId.create ()) 
                    (StreamId.create ())
                    (Version.box 0s)
                    (Create(userId, streamId |> StreamId.unbox, item.Fields))

        post envelope
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            ResponseCode(
                Message = "TODO: Respond with transaction Id",
                Time = DateTimeOffset.Now.ToString("o")))

    member this.Delete (item: ActionItemIdRendition) =
        let envelope =
            envelopWithDefaults
                (UserId.box <| this.User.Identity.GetUserId())        
                (TransId.create ()) 
                (StreamId.box <| Guid.Parse(item.ActionItemId))
                (Version.box 0s)
                (Delete)

        post envelope
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            ResponseCode(
                Message = "TODO: Respond with transaction Id",
                Time = DateTimeOffset.Now.ToString("o")))


    member this.Get () =
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            ActionItemsQueryResponse (
                Results = fetchActionItems (this.User.Identity.GetUserId()),
                Time = DateTimeOffset.Now.ToString("o")))
       
    member this.Get (item: ActionItemIdRendition) =
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            ActionItemQueryResponse (
                Result = ((Guid.Parse(item.ActionItemId) |> fetchActionItem) |> Option.get),
                Time = DateTimeOffset.Now.ToString("o")))
        

