module Actionable.Actors.IntegrationTests.ActionsActorsTests

open Xunit

open Akka
open Akka.Actor
open Akka.FSharp

open Actionable.Domain.Infrastructure
open Actionable.Domain
open Actionable.Domain.ActionItemModule

open Actionable.Actors 
open Actionable.Actors.Initialization
open Actionable.Actors.Composition

open InMemoryPersistance

let system = Configuration.defaultConfig () |> System.create "ActionableSystem"
let actionable = composeSystem (system, MemoryStore (), persist) // Actionable.Domain.Persistance.EventSourcing.EF.persistActionItem)

[<Fact>]
let ``Simple first test`` () =
//    DoX ()
    actionable.actionItemAggregateProcessor <!
        envelopWithDefaults 
            (UserId.box "")
            (TransId.create ()) 
            (StreamId.create ()) 
            (Version.box 0s) 
            ActionItemCommand.Delete

    System.Threading.Thread.Sleep 10000
    printfn "done"
    Assert.True true
//    true |> should equal true


open Actionable.Actors.Infrastructure

[<Fact>]
let ``Create an item, retrieve it, update it, and delete it`` () =

//    composeSystem ()

    // TODO: make the signal waiter more generic
    use signal = new System.Threading.AutoResetEvent false
    let waiter = spawn system "testsignalwaiter" <| actorOf (fun msg ->
        signal.Set () |> ignore)
    actionable.actionItemPersisterEventBroadcaster <! Subscribe waiter

    let title = "Hoobada Da Jubada Jistaliee"
    let description = "hiplity fublin"
    let description' = "hiplity dw mitibly fublin"
    let streamId = StreamId.create ()
    actionable.actionItemAggregateProcessor 
    <! envelopWithDefaults 
        (UserId.box "sampleuserid")
        (TransId.create ())
        (streamId) 
        (Version.box 0s) 
        (ActionItemCommand.Create 
            <| (["actionable.title",title;
                 "actionable.description", description] |> Map.ofList))

    System.TimeSpan.FromSeconds 60.0 
    |> signal.WaitOne 
    |> Assert.True

    let results = fetch "sampleuserid"
    match results |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
        with
            | None -> failwith <| sprintf "item '%s' was not found" title
            | Some item -> 
                let ident = item.Id
                Assert.True (item.Fields.["actionable.description"] = description)

                actionable.actionItemAggregateProcessor 
                <! envelopWithDefaults 
                    (UserId.box "sampleuserid")
                    (TransId.create ())
                    (streamId) 
                    (Version.box 1s) 
                    (ActionItemCommand.Update 
                        <| (["actionable.title",title;
                             "actionable.description", description'] |> Map.ofList))
                                         
                System.TimeSpan.FromSeconds 60.0 
                |> signal.WaitOne 
                |> Assert.True

                let results' = fetch "sampleuserid"

                match results' |> List.tryFind (fun r -> r.Id = StreamId.unbox streamId)
                    with
                    | None -> failwith "Could not find item"
                    | Some (item') -> 
                        Assert.Equal (ident, item'.Id)
                        Assert.Equal (description', item'.Fields.["actionable.description"])

    Assert.True true
    
type Class1() = 
    member this.X = "F#"
