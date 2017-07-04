module Actionable.Domain.ActionItemsQueryResponse

open Actionable.Data

let mapFieldValuesToDefinitions<'a when 'a :> FieldInstanceBase> 
        (fieldDefs:((int * (FieldDefinition seq)) seq))
        (fields:Map<string,string>)     
        (fieldType:FieldType)
        (constructField:(FieldDefinition*string)->'a)
        (setField:string->'a->unit)
        (list:System.Collections.Generic.IList<'a>) = 
    
    fieldDefs
    |> Seq.find (fun (k,v) -> k = int fieldType) 
    |> snd
    |> Seq.iter (fun fd ->
        match list |> Seq.tryFind (fun fv -> fv.FieldDefinition = fd) with
        | Some (li) -> 
            match fields.TryFind fd.FullyQualifiedName with
            | Some (field)  -> li |> setField field
            | Option.None   -> li |> setField fd.DefaultValue
        | Option.None ->
            match fields.TryFind fd.FullyQualifiedName with
            | Some (field)  -> list.Add (constructField (fd, field))
            | Option.None   -> list.Add (constructField (fd, fd.DefaultValue))                     
        )
    list

let mapToFields<'a when 'a :> FieldInstanceBase> 
        (fieldDefs:((int * (FieldDefinition seq)) seq))
        (fields:Map<string,string>)     
        (fieldType:FieldType)
        (constructField:(FieldDefinition*string)->'a) = 
    System.Collections.Generic.List<'a>() 
    |> mapFieldValuesToDefinitions fieldDefs fields fieldType constructField (fun x y -> ())


type ActionItemReadModel = {
    Fields: Map<string,string>
    Id: System.Guid
    UserId: string
    }

let mapToActionItemReadModel (task:TaskTypeInstance) =
    {   ActionItemReadModel.Fields = 
            (task.StringFields |> Seq.map (fun f -> 
                f.FieldDefinition.FullyQualifiedName, f.Value.ToString())
            |> Seq.toList) @ (task.DateFields |> Seq.map (fun f ->  
                f.FieldDefinition.FullyQualifiedName, f.Value.ToString())
            |> Seq.toList) @ (task.IntFields |> Seq.map (fun f -> 
                f.FieldDefinition.FullyQualifiedName, f.Value.ToString())
            |> Seq.toList) |> Map.ofList
        Id = task.Id
        UserId = task.UserIdentity }

let createIntField (d,v) =
    IntFieldInstance(FieldDefinition = d, Value = System.Int32.Parse v)
let setIntField value (i:IntFieldInstance) =
    i.Value <- System.Int32.Parse value
let createStringField (d,v) =
    StringFieldInstance(FieldDefinition = d, Value = v)
let setStringField value (i:StringFieldInstance) =
    i.Value <- value
let createDateTimeField (d,v) =
    DateTimeFieldInstance(FieldDefinition = d, Value = System.DateTimeOffset.Parse v)
let setDateTimeField value (i:DateTimeFieldInstance) =
    i.Value <- System.DateTimeOffset.Parse value

let buildTaskInstance userId streamId fields (typeDef:TaskTypeDefinition) =
    let groupedFields = typeDef.Fields |> Seq.groupBy (fun f -> f.FieldType)
    TaskTypeInstance
        (Id = streamId,
         TaskTypeDefinition = typeDef,
         UserIdentity = userId,
         IntFields = mapToFields groupedFields fields FieldType.Int createIntField,
         StringFields = mapToFields groupedFields fields FieldType.String createStringField,
         DateFields = mapToFields groupedFields fields FieldType.DateTime createDateTimeField)

let updateTaskInstance fields (entity:TaskTypeInstance) =
    let groupedFields = entity.TaskTypeDefinition.Fields |> Seq.groupBy (fun f -> f.FieldType)
    entity.IntFields 
    |> mapFieldValuesToDefinitions groupedFields fields FieldType.Int createIntField setIntField 
    |> ignore
    entity.StringFields 
    |> mapFieldValuesToDefinitions groupedFields fields FieldType.String createStringField setStringField
    |> ignore
    entity.DateFields 
    |> mapFieldValuesToDefinitions groupedFields fields FieldType.DateTime createDateTimeField setDateTimeField
    |> ignore
    entity


