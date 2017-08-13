namespace Actionable.Actors.UnitTests 

open EventSourceGherkin
module UserNotificationsGherkin =
    open Actionable.Domain.UserNotificationsModule
    let testing = 
        TestConditions<UserNotificationsCommand, UserNotificationsEvent, UserNotificationsState> 
            (buildState DoesNotExist, buildState, handle)
  
    let Given = testing.Given
    let When  = testing.When
    let Then  = testing.Then
