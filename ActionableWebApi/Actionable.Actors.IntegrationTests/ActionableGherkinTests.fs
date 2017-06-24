module ActionableGherkinTests

open Xunit
open Actionable.Actors.IntegrationTests.ActionItemGherkin
open EventSourceGherkin
open Actionable.Domain.ActionItemModule

[<Fact>]
let ``Create an action item from nothing`` () =
    Given(DoesNotExist |> Preconditions.State)
    |> When (
        ["actionable.title","Doing things";
            "actionable.description","Doing things is an important activity"]
        |> Map.ofList
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
                    |> fun map -> { Fields=map }
                    |> ActionItemEvent.Created   ]
                |> Some
            State = None
        }
