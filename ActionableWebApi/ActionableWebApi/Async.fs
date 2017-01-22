namespace ActionableWebApi

open System.Threading.Tasks

[<AutoOpen>]
module Async =
    let inline awaitPlainTask (task: Task) = 
        // rethrow exception from preceding task if it fauled
        let continuation (t : Task) : unit =
            match t.IsFaulted with
            | true -> raise t.Exception
            | arg -> ()
        task.ContinueWith continuation |> Async.AwaitTask

    let inline startAsPlainTask (work : Async<unit>) = 
        work |> Async.StartAsTask :> Task
        //Task.Factory.StartNew (
        //    fun () -> work |> Async.StartAsTask :> Task)
