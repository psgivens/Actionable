module Program

open Actionable.Actors.IntegrationTests.ActionsActorsTests
open ActionableGherkinTests

//[<EntryPoint>]
let main argv = 
    printf "Hello World"
    ``Create, retrieve, update, and delete an item`` ()
    ``Create an action item from nothing`` () 
    0

