module EventSourceGherkin

type Preconditions<'TEvent,'TState> = 
    | State of 'TState
    | Events of 'TEvent list 

type SystemUnderTest<'TCommand,'TEvent> =
    | Events of 'TEvent list
    | Command of 'TCommand

type TestResults<'TEvent,'TState> = 
    | OK of 'TEvent * 'TState
    | Error of System.Exception

type PostConditions<'TEvent,'TState> = {
    Events: 'TEvent list option
    State: 'TState option
    Error: System.Exception option }

type TestFailure (error) = 
    inherit System.Exception (error)

//exception TestFailure of string

   
type TestConditions<'TCommand,'TEvent,'TState when 'TEvent : equality and 'TState : equality>  
        (buildInitialState, buildState:('TState -> 'TEvent list -> 'TState), handle) = 

    let testFailure (name:string, expected:'a, actual:'a) = 
        // TODO: Create better error message
        sprintf "%s\n\texpected: %A\n\tactual: %A" name expected actual
        |> TestFailure 
        |> raise
    
    
    let testException name (expected:System.Exception option) (actual:System.Exception) = 
        match expected with
        | Some(value) -> 
            if actual.GetType() <> value.GetType()  && actual.Message <> value.Message then 
                testFailure(name, value, actual) 
        | None -> ()
    
    let test name (expected:'a option) (actual:'a) = 
        match expected with
        | Some(value) -> 
            if actual <> value then 
                testFailure(name, value, actual) 
        | None -> ()
    
    member this.Given (preconditions: Preconditions<'TEvent,'TState>) =
        preconditions

    member this.When 
            (sut: SystemUnderTest<'TCommand,'TEvent>) 
            (preconditions:Preconditions<'TEvent,'TState>) = 
        let execute () = 
            let preState = 
                match preconditions with
                | Preconditions.Events(events) -> 
                    buildInitialState events
                | State(state) -> state

            try 
                match sut with
                | Events(events) -> 
                    let state = events |> buildState preState 
                    OK(events,state)
                    
                | Command(command) -> 
                    let event = handle preState command
                    let state = [event] |> buildState preState 
                    OK([event],state)
            with
            | ex -> Error(ex)
        
        execute


    member this.Then (expected:PostConditions<'TEvent,'TState>) execute  = 
    
        // validate preconditions
        if (Option.isSome expected.Error
            && (Option.isSome expected.State || Option.isSome expected.Events))
            || (Option.isNone expected.Error && Option.isNone expected.State && Option.isNone expected.Events) then
                failwith "Invalid postconditions"
    
        match execute () with 
        | OK(events,state) ->
            events |> test "events" expected.Events
            state  |> test "state" expected.State 
        | Error(ex) ->
            if Option.isSome expected.State then failwith <| sprintf "Expected a state.\n\terror:%A" ex
            if Option.isSome expected.Events then failwith <| sprintf "Expected events.\n\terror:%A" ex
            ex |> testException "error" expected.Error
    
    

module TestData =
    
    let nullPostConditions = {
        State = None
        Events = None
        Error = None
    }
