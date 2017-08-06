module HashPoolExample
//-----------------------------------------------------------------------
// <copyright file="Supervisioning.fs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

//module Supervisioning

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp

type CustomException() =
    inherit Exception()

type Message =
    | Echo of string
    | Crash

//https://github.com/akkadotnet/akka.net/issues/999
let userHash = Akka.Routing.ConsistentHashingPool 50
let userHash' = userHash.WithHashMapping(fun msg ->
    match msg with
    | :? Message as msg -> 
        match msg with
        | Echo(value) -> value.Length :> obj
        | Crash -> "Let it crash!" :> obj
    | :? string as value -> value :> obj
    | _ -> "not a string" :> obj
)

let main() =
    use system = System.create "system" (Configuration.defaultConfig())
    // create parent actor to watch over jobs delegated to it's child
    let parent = 
        spawnOpt system "parent" 
            <| fun parentMailbox ->
                // define child actor
                let child = 
                    spawn parentMailbox "child" <| fun childMailbox ->
                        childMailbox.Defer (fun () -> printfn "Child stopping")
                        printfn "Child started"
                        let rec childLoop lastMsg = 
                            actor {
                                let! msg = childMailbox.Receive()
                                //childMailbox.Context.
                                match msg with
                                | Echo info -> 
                                    // respond to original sender
                                    let response = "Child " + (childMailbox.Self.Path.ToStringWithAddress()) + " received: "  + info
                                    childMailbox.Sender() <! response
                                    printfn "%s; Curr: %s" lastMsg info
                                    return! childLoop ("Last: " + info)
                                | Crash -> 
                                    // log crash request and crash
                                    printfn "Child %A received crash order" (childMailbox.Self.Path)
                                    raise (CustomException())
                                    return! childLoop "crash!"
                            }
                        childLoop "Last: first"
                // define parent behavior
                let rec parentLoop() =
                    actor {
                        let! (msg: Message) = parentMailbox.Receive()
                        child.Forward(msg)  // forward all messages through
                        return! parentLoop()
                    }
                parentLoop()
            // define supervision strategy
            <| [ SpawnOption.SupervisorStrategy (
                    // restart on Custom Exception, default behavior on all other exception types
                    Strategy.OneForOne(fun e ->
                    match e with
                    | :? CustomException -> Directive.Restart 
                    | _ -> SupervisorStrategy.DefaultDecider.Decide(e))) ;
                 SpawnOption.Router (userHash')]

    async {
        let! response = parent <? Echo "hello world"
        printfn "%s" response

        let! response = parent <? Echo "a"
        let! response = parent <? Echo "ab"
        let! response = parent <? Echo "abc"
        let! response = parent <? Echo "1"
        let! response = parent <? Echo "12"
        let! response = parent <? Echo "123"
        let! response = parent <? Echo "xyz"
        let! response = parent <? Echo "xy"
        let! response = parent <? Echo "x"
        let! response = parent <? Echo "jkl"
        let! response = parent <? Echo "jk"
        let! response = parent <? Echo "j"

        // after this one child should crash
        //parent <! Crash
        //System.Threading.Thread.Sleep 200
        
        // actor should be restarted
        let! response = parent <? Echo "hello worl2"
        printfn "%s" response
    } |> Async.RunSynchronously

