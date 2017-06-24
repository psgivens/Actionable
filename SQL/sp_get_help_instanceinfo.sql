USE [Actionable.Data.ActionableDbContext]
GO

/****** Object:  StoredProcedure [dbo].[get_help_instanceinfo]    Script Date: 4/23/2017 11:59:04 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[get_help_instanceinfo]
	-- Add the parameters for the stored procedure here
	@InstanceId [uniqueidentifier] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

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

	SELECT TOP 1000 [StreamId]
		  ,[TransactionId]
		  ,[UserId]
		  ,[DeviceId]
		  ,[Version]
		  ,[Id]
		  ,[TimeStamp]
		  ,[Event]
	  FROM [Actionable.Data.ActionableDbContext].[dbo].[ActionItemEvents]
	  WHERE [StreamId] = @InstanceId
END

GO


