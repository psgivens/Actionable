namespace Actionable.Actors.IntegrationTests

open EventSourceGherkin
module ActionItemGherkin =
    open Actionable.Domain.ActionItemModule    
    let testing = 
        TestConditions<ActionItemCommand, ActionItemEvent, ActionItemState> 
            (buildState DoesNotExist, buildState, handle)
  
    let Given = testing.Given
    let When  = testing.When
    let Then  = testing.Then
