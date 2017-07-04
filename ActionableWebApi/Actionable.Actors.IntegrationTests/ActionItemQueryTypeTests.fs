module QueryResponse_ActionItems

open Actionable.Data
open Xunit

open Actionable.Domain.ActionItemsQueryResponse

let titleFieldDef = 
    FieldDefinition
        (Id=1,
         FullyQualifiedName="actionable.title",
         DisplayName="Title",
         FieldType=1,
         DefaultValue=null)

let descriptionFieldDef = 
    FieldDefinition
        (Id=1,
         FullyQualifiedName="actionable.description",
         DisplayName="Description",
         FieldType=1,
         DefaultValue=null)

let statusFieldDef = 
    FieldDefinition
        (Id=1,
         FullyQualifiedName="actionable.status",
         DisplayName="Status",
         FieldType=2,
         DefaultValue="0")

let createdFieldDef = 
    FieldDefinition
        (Id=1,
         FullyQualifiedName="actionable.createddate",
         DisplayName="Created Date",
         FieldType=4,
         DefaultValue="6/29/2017 11:09:18 PM -07:00")

let typeDef = 
    let def = 
        TaskTypeDefinition 
            (Id = 1,
             FullyQualifiedName = "actionable.actionitem",
             DisplayName = "Action Item",
             UI = null)
    def.Fields <- 
        [titleFieldDef; descriptionFieldDef; statusFieldDef; createdFieldDef] 
        |> List.toArray :> System.Collections.Generic.IList<_>
    def

let sampleUserId = "sampleuser"
let sampleTitle = "have the most fun"
let sampleDescription = "Hibidy jibity 3432fce9-215c-4249-94c4-9a78292ecc32"
let sampleCreatedDate = "6/29/2017 11:09:18 PM -07:00"
let sampleFields = 
    [("actionable.title", sampleTitle)
     ("actionable.description", sampleDescription)
     ("actionable.status","0")
     ("actionable.createddate", sampleCreatedDate)]
    |> Map.ofList

[<Fact>]
let ``Create type through meta`` () =
    let streamId = System.Guid.NewGuid ()
    let inst = typeDef |> buildTaskInstance sampleUserId streamId sampleFields
    let readModel = mapToActionItemReadModel inst
    let title = readModel.Fields |> Map.find "actionable.title" 
    let description = readModel.Fields |> Map.find "actionable.description"
    let status = readModel.Fields |> Map.find "actionable.status"
    let createdDate = readModel.Fields |> Map.find "actionable.createddate"
    
    Assert.Equal (sampleUserId, readModel.UserId)
    Assert.Equal (streamId, readModel.Id)
    Assert.Equal (sampleTitle, title)
    Assert.Equal (sampleDescription, description)
    Assert.Equal ("0", status)
    Assert.Equal (sampleCreatedDate, createdDate)

[<Fact>]
let ``Update type through meta`` () =
    let streamId = System.Guid.NewGuid ()
    let inst = typeDef |> buildTaskInstance sampleUserId streamId sampleFields
    let readModel = mapToActionItemReadModel inst
    let title = readModel.Fields |> Map.find "actionable.title" 
    let description = readModel.Fields |> Map.find "actionable.description"
    let status = readModel.Fields |> Map.find "actionable.status"
    let createdDate = readModel.Fields |> Map.find "actionable.createddate"
    
    Assert.Equal (sampleUserId, readModel.UserId)
    Assert.Equal (streamId, readModel.Id)
    Assert.Equal (sampleTitle, title)
    Assert.Equal (sampleDescription, description)
    Assert.Equal ("0", status)
    Assert.Equal (sampleCreatedDate, createdDate)

    let fields = readModel.Fields |> Map.add "actionable.title" "other title"
    let inst' = inst |> updateTaskInstance fields
    let readModel' = mapToActionItemReadModel inst'
    let title' = readModel'.Fields |> Map.find "actionable.title" 
    Assert.Equal ("other title", title')
    