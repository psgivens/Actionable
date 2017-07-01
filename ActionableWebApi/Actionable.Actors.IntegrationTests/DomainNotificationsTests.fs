module ``1 - Domain - Notifications Tests``

open Xunit
open Actionable.Actors.IntegrationTests.UserNotificationsGherkin
open EventSourceGherkin
open Actionable.Domain.UserNotificationsModule

let sampleUserId = "sample user id"
let existingItemState:Preconditions<UserNotificationsEvent,UserNotificationsState> = 
    {   UserNotifications.userId=sampleUserId
        items = 
            [(0,{UserNotification.code=0; message="sample message"; status=0})]
            |> Map.ofList 
    }
    |> UserNotificationsState.State
    |> Preconditions.State

[<Fact>]
let ``1. Create a notification`` () =
    let message = "message to user"
    Given(UserNotificationsState.DoesNotExist |> Preconditions.State)
    |> When (
        (sampleUserId, 0, message)
        |> UserNotificationsCommand.AppendMessage 
        |> Command)
    |> Then {
        TestData.nullPostConditions             
        with 
            Events =         
                [   (sampleUserId, 1, 0, message)
                    |> UserNotificationsEvent.MessageCreated   
                ] |> Some
            State = None
        }

[<Fact>]
let ``2. Acknowledge a notification`` () =
    Given(existingItemState)
    |> When (0 |> UserNotificationsCommand.AcknowledgeMessage |> Command)
    |> Then {
        TestData.nullPostConditions
        with 
            Events = 
                [0 |> UserNotificationsEvent.MessageAcknowledged] |> Some }

//[<Fact>]
//let ``Delete a notification`` () =
//    failwith "Not implemented"
