
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[InsertOrUpdateIssueProfile]
    @Id nchar(50),
    @ExampleMessage nvarchar(2048),
    @FirstReportedDate datetime
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.IssueProfiles WHERE Id = @Id)
    BEGIN
        -- The IssueProfile already exists, update the row
        UPDATE dbo.IssueProfiles
        SET ExampleMessage = @ExampleMessage,
            FirstReportedDate = @FirstReportedDate
        WHERE Id = @Id;
    END
    ELSE
    BEGIN
        -- The IssueProfile does not exist, insert a new row
        INSERT INTO dbo.IssueProfiles (Id, ExampleMessage, FirstReportedDate)
        VALUES (@Id, @ExampleMessage, @FirstReportedDate);
    END
END
GO
