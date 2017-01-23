namespace ActionableWebApi

open System
open System.Web.Http.Dispatcher
open System.Web.Http.Controllers;

open Actionable.Domain.ActionItemModule

type ActionItemsObserver (onNext:ActionItemEvent->unit, onError:Exception->unit) =
    interface IObserver<ActionItemEvent> with
        member this.OnCompleted () = ()
        member this.OnError ex = onError ex
        member this.OnNext actionItem = onNext actionItem
        
