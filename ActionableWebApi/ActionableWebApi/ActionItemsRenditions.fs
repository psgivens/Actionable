namespace ActionableWebApi

open System
open System.Net
open System.Net.Http
open System.Web.Http

open System.Security.Principal
open Microsoft.AspNet.Identity

open Actionable.Data
open Actionable.Domain.Persistance.EventSourcing.ActionItemEF

type ResponseCode () =
    [<DefaultValue>] val mutable Message : string
    [<DefaultValue>] val mutable Time : string
     
[<CLIMutable>]
type ActionItemIdRendition = {
    ActionItemId: string
}

[<CLIMutable>]
type AddActionItemRendition = {
    Id: string
    Fields: Map<string,string>
    Date : string 
    }

type FieldRendition = {
    Key: string
    Value: string
}

