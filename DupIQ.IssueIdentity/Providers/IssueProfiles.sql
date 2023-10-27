USE [FailureDb]
GO

/****** Object:  Table [dbo].[IssueProfiles]    Script Date: 7/21/2023 7:21:28 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[IssueProfiles](
	[Id] [nchar](50) NOT NULL,
	[ExampleMessage] [nvarchar](2048) NULL,
	[FirstRecordedDate] [datetime] NULL,
 CONSTRAINT [PK_IssueProfiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


