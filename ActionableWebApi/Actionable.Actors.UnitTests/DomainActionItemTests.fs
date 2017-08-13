module Domain_ActionItems

open Xunit
open Actionable.Actors.UnitTests.ActionItemGherkin
open EventSourceGherkin
open Actionable.Domain.ActionItemModule

let existingItemState:Preconditions<ActionItemEvent,ActionItemState> = 
    {   ActionItem.UserId = "sampleuser"
        Id = System.Guid.NewGuid ()
        Fields = 
            ["actionable.title","Doing things";
                "actionable.description","Doing things is an important activity"]
            |> Map.ofList }
    |> ActionItemState.State
    |> Preconditions.State

[<Fact>]
let ``Create an action item`` () =
    let sampleUserId = "sampleuserid"
    let itemId = System.Guid.NewGuid ()
    Given(DoesNotExist |> Preconditions.State)
    |> When (
        (sampleUserId, itemId,
            ["actionable.title","Doing things";
                "actionable.description","Doing things is an important activity"]
            |> Map.ofList)
        |> ActionItemCommand.Create
        |> Command)
    |> Then {
        TestData.nullPostConditions             
        with 
            Events =         
                [   ["actionable.title","Doing things";
                        "actionable.description","Doing things is an important activity";
                        "actionable.status","0"]
                    |> Map.ofList
                    |> fun map -> { UserId=sampleUserId; Id=itemId; Fields=map }
                    |> ActionItemEvent.Created   ]
                |> Some
            State = None
        }

[<Fact>]
let ``Update an action item`` () =
    let newTitle = "Doing other things"
    Given(existingItemState)
    |> When (
        ["actionable.title",newTitle]
        |> Map.ofList
        |> ActionItemCommand.Update
        |> Command)
    |> Then {
        TestData.nullPostConditions             
        with 
            Events =         
                [   ["actionable.title",newTitle]
                    |> Map.ofList
                    |> ActionItemEvent.Updated ]
                |> Some
            State = None
        }

[<Fact>]
let ``Delete an action item`` () =
    Given(existingItemState)
    |> When (ActionItemCommand.Delete |> Command)
    |> Then {
        TestData.nullPostConditions             
        with 
            Events = [ActionItemEvent.Deleted] |> Some
            State = ActionItemState.DoesNotExist |> Some
        }