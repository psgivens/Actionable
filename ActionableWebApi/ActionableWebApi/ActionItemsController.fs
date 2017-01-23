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

open Actionable.Domain.Persistance.EventSourcing.EF
            
type ActionsController (post:Envelope<ActionItemCommand>->unit) =
    inherit ApiController ()
    let (|GuidPattern|_|) guid = 
        match Guid.TryParse guid with 
        | (true, id) -> Some(id)
        | _ -> Option.None
    member this.Post (item: AddActionItemRendition) =
        // TODO Parse Int16.Parse(item.Status)
        let envelope = 
            match item.Id with 
            | GuidPattern id ->
                envelopWithDefaults 
                    (this.User.Identity.GetUserId()) 
                    (System.Web.HttpContext.Current.Session.SessionID)
                    (Guid.NewGuid()) id 
                    0s (Update(item.Fields))
            | _ ->
                envelopWithDefaults 
                    (this.User.Identity.GetUserId()) 
                    (System.Web.HttpContext.Current.Session.SessionID)
                    (Guid.NewGuid()) (Guid.NewGuid())
                    0s (Create(item.Fields))

        post envelope
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            ResponseCode(
                Message = "TODO: Respond with transaction Id",
                Time = DateTimeOffset.Now.ToString("o")))

    member this.Delete (item: DeleteActionItemRendition) =
        let envelope =
            envelopWithDefaults
                (this.User.Identity.GetUserId())        
                (System.Web.HttpContext.Current.Session.SessionID)
                (Guid.NewGuid()) (Guid.Parse(item.ActionItemId))
                0s (Delete)

        post envelope
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            ResponseCode(
                Message = "TODO: Respond with transaction Id",
                Time = DateTimeOffset.Now.ToString("o")))


    member this.Get () =
        let first f queryable =     
            System.Linq.Queryable.First (queryable, f)
        use context = new ActionableDbContext ()
        let t = 
            query {
                for typ in context.TaskTypeDefinitions do
                where (typ.FullyQualifiedName = "actionable.actionitem")
                select typ
                exactlyOne
            }
        let userId = this.User.Identity.GetUserId()
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            ResponseToQuery (
                Results = (
                    query {
                        for actionItem in context.TaskInstances do
                        where (actionItem.TaskTypeDefinitionId = 1 && actionItem.UserIdentity = userId)
                        select actionItem }
                    |> Seq.map mapToActionItemReadModel
                    |> Seq.toList),
                Time = DateTimeOffset.Now.ToString("o")))
       
