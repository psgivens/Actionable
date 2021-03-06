/****** Script for SelectTopNRows command from SSMS  ******/

--exec get_help_instanceinfo '120FE3C7-E23F-4F1E-BF1A-0B661390A271'

DECLARE @InstanceId [uniqueidentifier] = '120FE3C7-E23F-4F1E-BF1A-0B661390A271'

SELECT TOP 1000 [Id]
      ,[TaskTypeDefinitionId]
      ,[UserIdentity]
  FROM [Actionable.Data.ActionableDbContext].[dbo].[TaskTypeInstances]
WHERE Id = @InstanceId


SELECT TOP 1000 * FROM [Actionable.Data.ActionableDbContext].[dbo].StringFieldInstances
WHERE TaskTypeInstanceId = @InstanceId

SELECT TOP 1000 * FROM [Actionable.Data.ActionableDbContext].[dbo].DateTimeFieldInstances
WHERE TaskTypeInstanceId = @InstanceId

SELECT TOP 1000 * FROM [Actionable.Data.ActionableDbContext].[dbo].IntFieldInstances 
WHERE TaskTypeInstanceId = @InstanceId

SELECT TOP 1000 
      [Version]
      ,[TimeStamp]
      ,[Event]
  FROM [Actionable.Data.ActionableDbContext].[dbo].[ActionItemEvents]
  WHERE [StreamId] = @InstanceId
  ORDER BY [Version]

