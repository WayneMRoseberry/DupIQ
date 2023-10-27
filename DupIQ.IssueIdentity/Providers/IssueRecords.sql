USE [FailureDb]
GO

/****** Object:  Table [dbo].[IssueRecords]    Script Date: 7/21/2023 7:19:22 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[IssueRecords](
	[InstanceId] [nchar](50) NOT NULL,
	[IssueId] [nchar](50) NULL,
	[IssueMessage] [nvarchar](2048) NULL,
	[IssueDate] [datetime] NULL,
 CONSTRAINT [PK_IssueRecords] PRIMARY KEY CLUSTERED 
(
	[InstanceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


