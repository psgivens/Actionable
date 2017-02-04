module Actionable.Actors.IntegrationTests.ActionsActorsTests

open Xunit

open Akka
open Akka.Actor
open Akka.FSharp

open Actionable.Domain.Infrastructure
//open Actionable.Domain
open Actionable.Domain.ActionItemModule

open Actionable.Actors 
open Actionable.Actors.Initialization
open Actionable.Actors.Composition

[<Fact>]
let ``Simple first test`` () =
    DoX ()
    actionItemAggregateActor <!
        envelopWithDefaults 
            (UserId.box "")
//            (DeviceId.box "devid")
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
    
    composeSystem ()

    // TODO: make the signal waiter more generic
    use signal = new System.Threading.AutoResetEvent false
    let waiter = spawn system "testsignalwaiter" <| actorOf (fun msg ->
        signal.Set () |> ignore)
    actionItemEventListener <! Subscribe waiter

    let title = "Hoobada Da Jubada"
    let streamId = StreamId.create ()
    actionItemAggregateActor <!
    envelopWithDefaults 
        (UserId.box "sampleuserid")
//        (DeviceId.box "sampledeviceid")
        streamId 
        (StreamId.create ()) 
        (Version.box 0s) 
        (ActionItemCommand.Create 
            <| (["actionable.title",title;
                 "actionable.description","Another time around"] |> Map.ofList))

    System.TimeSpan.FromSeconds 10.0 
    |> signal.WaitOne 
    |> Assert.True
    
//    let cresult' = actionController.Post {
//        Id = ""
//        Fields = [("actionable.title", title);("actionable.description","have fun")] |> Map.ofList
//        Date = System.DateTimeOffset.Now.ToString ()
//    }
//    System.Threading.Thread.Sleep 10000
//    let result = actionController.Get ()
//    let response = HttpTest.unpack<ActionableWebApi.ResponseToQuery> result
//    let item = response.Results |> List.find (fun r -> r.Fields.["actionable.title"] = title)
//    Assert.NotNull item
//    let ident = item.Id
//    Assert.True (item.Fields.["actionable.description"]="have fun")
//        
//    let cresult'' = actionController.Post {
//        Id = item.Id
//        Fields = [("actionable.title", title);("actionable.description","have the most fun")] |> Map.ofList
//        Date = System.DateTimeOffset.Now.ToString ()
//    }
//    System.Threading.Thread.Sleep 10000
//    let result' = actionController.Get ()
//    let response' = HttpTest.unpack<ActionableWebApi.ResponseToQuery> result'
//    let item' = response'.Results |> List.find (fun r -> r.Fields.["actionable.title"] = title)
//    Assert.NotNull item'
//    Assert.Equal (ident, item'.Id)
//    Assert.Equal ("have the most fun", item'.Fields.["actionable.description"])
//        
    Assert.True (false, "This test has not been written to completion.")
    
type Class1() = 
    member this.X = "F#"
