USE [Actionable.Data.ActionableDbContext]
GO

/****** Object:  StoredProcedure [dbo].[SP_TaskInstance_Get]    Script Date: 9/17/2017 1:43:00 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Phillip Scott Givens
-- Create date: September 17th, 2017
-- Description:	Pulls all of the information about a task instance. Mainly for debugging
-- =============================================
CREATE PROCEDURE [dbo].[SP_TaskInstance_Get]
	@TaskTypeInstanceId UNIQUEIDENTIFIER
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

SELECT TOP 1000 ti.*, td.DisplayName, td.FullyQualifiedName, td.UI
  FROM [Actionable.Data.ActionableDbContext].[dbo].[TaskTypeInstances] ti
  LEFT JOIN [Actionable.Data.ActionableDbContext].[dbo].[TaskTypeDefinitions] td
  ON ti.TaskTypeDefinitionId = td.Id
  WHERE ti.[Id] = @TaskTypeInstanceId

	SELECT TOP 1000 fi.*, fd.DisplayName, fd.FullyQualifiedName, fd.FieldType, fd.DefaultValue
  FROM [Actionable.Data.ActionableDbContext].[dbo].[StringFieldInstances] fi
  LEFT JOIN [Actionable.Data.ActionableDbContext].dbo.FieldDefinitions fd
  ON fi.FieldDefinitionId = fd.Id
  WHERE [TaskTypeInstanceId] = @TaskTypeInstanceId

	SELECT TOP 1000 fi.*, fd.DisplayName, fd.FullyQualifiedName, fd.FieldType, fd.DefaultValue
  FROM [Actionable.Data.ActionableDbContext].[dbo].[DateTimeFieldInstances] fi
  LEFT JOIN [Actionable.Data.ActionableDbContext].dbo.FieldDefinitions fd
  ON fi.FieldDefinitionId = fd.Id
  WHERE [TaskTypeInstanceId] = @TaskTypeInstanceId

	SELECT TOP 1000 fi.*, fd.DisplayName, fd.FullyQualifiedName, fd.FieldType, fd.DefaultValue
  FROM [Actionable.Data.ActionableDbContext].[dbo].[IntFieldInstances] fi
  LEFT JOIN [Actionable.Data.ActionableDbContext].dbo.FieldDefinitions fd
  ON fi.FieldDefinitionId = fd.Id
  WHERE [TaskTypeInstanceId] = @TaskTypeInstanceId

  SELECT TOP 1000 [StreamId]
	  ,[Event]
      ,[TransactionId]
      ,[UserId]
      ,[DeviceId]
      ,[Version]
      ,[Id]
      ,[TimeStamp]
  FROM [Actionable.Data.ActionableDbContext].[dbo].[ActionItemEvents]
  WHERE [StreamId] = 'EAA1AC11-3C8D-4DCC-97B2-26544B7F74B8'
  ORDER BY [Version]

END

GO


