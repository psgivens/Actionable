module Actionable.Actors.Infrastructure

open Akka.Actor
open Akka.FSharp

type SubjectAction<'TMessage> =
    | Msg of 'TMessage
    | Subscribe of IActorRef
    | Unsubscribe of IActorRef

let subject<'TMessage> system name = spawn system name (fun mailbox -> 
    let rec loop subscribers = actor {
        let! (message:SubjectAction<'TMessage>) = mailbox.Receive ()
        match message with 
        | Subscribe actor -> return! loop (actor::subscribers)
        | Unsubscribe actor -> return! loop (subscribers |> List.filter (fun item -> item <> actor))        
        | Msg msg -> 
            subscribers |> List.iter (fun actor -> actor.Tell msg)
            return! loop subscribers
    }        
    loop []) 

