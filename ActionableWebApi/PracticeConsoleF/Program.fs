// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open Akka
open Akka.Actor
open Akka.FSharp

open Actionable.Actors 
open Actionable.Actors.Composition

type Greet (who:string) = 
    member this.Who = who

type GreetingActor () as g =    
    inherit ReceiveActor ()
    do g.Receive<Greet> (
        fun (greet:Greet) -> 
            printfn "Hello %s" greet.Who)

type ProcessorMessage = ProcessJob of int * int * int

open System
type SStreamId = SStreamId of Guid with 
    static member create () = SStreamId (Guid.NewGuid ())
    static member unbox (SStreamId(aggregateId)) = aggregateId
type SUserId = SUserId of String with 
    static member unbox (SUserId(value)) = value

[<EntryPoint>]
let main argv = 
    let guid = (System.Guid.NewGuid())
    let v = SStreamId (guid)
    let y = SStreamId.unbox v
    let w = SUserId "foobar"
    let z = SUserId.unbox w


//    DoX ()
    let example = ActorExample ()
    example.run ()

    let system = Configuration.load () |> System.create "system" 
    //let actorRef = spawn system "myActor" (actorOf (fun msg -> (* Handle message here *) () ))

    let processor (mailbox: Actor<_>) = 
        let rec loop () = actor {
            let! ProcessJob (x,y,z) = mailbox.Receive ()
            printfn "Processor: received ProcessJob %i %i %i" x y z
            return! loop ()
        }
        loop ()
      
    let processorRef = spawn system "processor" processor
    processorRef <! ProcessJob (1, 3, 5)
    (ProcessJob (7, 9, 11)) |> processorRef.Tell
      
    let system = ActorSystem.Create "MySystem"

    // Create your actor and get a reference to it.
    // This will be an "ActorRef", which is not a
    // reference to the actual actor instance
    // but rather a client or proxy to it.
    let greeter = system.ActorOf<GreetingActor> "greeter"

    // Send a message to the actor    
    "World" |> Greet |> greeter.Tell
    
    // This prevents the app from exiting
    // before the async work is done
    System.Console.ReadLine() |> ignore

    0 // return an integer exit code
