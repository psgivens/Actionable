namespace ActionableWebApi

open System
open System.Net
open System.Net.Http
open System.Web.Http

open Actionable.Data
open System.Security.Principal
open Microsoft.AspNet.Identity
//open Actionable.Domain.Infrastructure.Envelope

type HomeRendition () =
    [<DefaultValue>] val mutable Message : string
    [<DefaultValue>] val mutable Time : string

type ResponseCode () =
    [<DefaultValue>] val mutable Message : string
    [<DefaultValue>] val mutable Time : string
     
type HomeController() =
    inherit ApiController()
    member this.Get() =
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            HomeRendition(
                Message = "Hello from F#",
                Time = DateTimeOffset.Now.ToString("o")))

[<CLIMutable>]
type DeleteActionItemRendition = {
    ActionItemId: string
}

[<CLIMutable>]
type AddActionItemRendition = {
    Id: string
//    Title: string
//    Description: string
//    Status: string
    Fields: Map<string,string>
    Date : string }

//[<CLIMutable>]
//type ReadActionItemRendition = {
//    Title: string
//    Description: string
//    Status: string
//    Id: string
//}

type FieldRendition = {
    Key: string
    Value: string
}

//open Composition
open Actionable.Domain.ActionItemModule
open Actionable.Domain.Infrastructure
open Actionable.Domain.ActionItemModule
open Actionable.Domain.Infrastructure.Envelope

open Actionable.Domain.Persistance.EventSourcing.EF

type ResponseToQuery () =
    [<DefaultValue>] val mutable Results : ActionItemReadModel list
    [<DefaultValue>] val mutable Time : string

type ActionItemAggregateAgent () = 
    inherit AggregateAgent<ActionItem, ActionItemState, ActionItemCommand, ActionItemEvent> (
        None, Actionable.Domain.Persistance.EventSourcing.EF.ActionItemEventStore(), 
        buildState, 
        handle) 

type ActionItemPersistingAgent () = 
    inherit PersistingAgent<ActionItem, ActionItemState, ActionItemCommand, ActionItemEvent> (
        None, Actionable.Domain.Persistance.EventSourcing.EF.ActionItemEventStore(), 
        buildState, 
        Actionable.Domain.Persistance.EventSourcing.EF.persistActionItem)
            
type ActionsController (actionItemAgent:ActionItemAggregateAgent) = 
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
                    (Guid.NewGuid()) id 
                    0s (Update(item.Fields))
            | _ ->
                envelopWithDefaults 
                    (this.User.Identity.GetUserId()) 
                    (Guid.NewGuid()) (Guid.NewGuid())
                    0s (Create(item.Fields))

        actionItemAgent.Post envelope
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            ResponseCode(
                Message = "TODO: Respond with transaction Id",
                Time = DateTimeOffset.Now.ToString("o")))

    member this.Delete (item: DeleteActionItemRendition) =
        let envelope =
            envelopWithDefaults
                (this.User.Identity.GetUserId())        
                (Guid.NewGuid()) (Guid.Parse(item.ActionItemId))
                0s (Delete)

        actionItemAgent.Post envelope
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
       
