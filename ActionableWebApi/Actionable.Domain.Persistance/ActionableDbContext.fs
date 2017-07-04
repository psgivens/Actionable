module Actionable.Domain.Persistance.Core

let saveChanges (context:Actionable.Data.ActionableDbContext) = 
    Async.AwaitTask (context.SaveChangesAsync()) |> Async.Ignore 
