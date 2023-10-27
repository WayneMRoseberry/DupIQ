SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[InsertOrUpdateIssueReport]
    @InstanceId nchar(50),
	@IssueId nchar(50),
    @IssueMessage nvarchar(2048),
    @IssueDate datetime
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.IssueReports WHERE InstanceId = @InstanceId)
    BEGIN
        -- The IssueProfile already exists, update the row
        UPDATE dbo.IssueReports
        SET IssueMessage = @IssueMessage,
            IssueDate = @IssueDate,
			IssueId = @IssueId
        WHERE InstanceId = @InstanceId;
    END
    ELSE
    BEGIN
        -- The IssueProfile does not exist, insert a new row
        INSERT INTO dbo.IssueReports(InstanceId, IssueId, IssueMessage, IssueDate)
        VALUES (@InstanceId, @IssueId, @IssueMessage, @IssueDate);
    END
END
GO
