/****** Object:  UserDefinedFunction [dbo].[CALCWEEKTASKENTRY]    Script Date: 5/4/2021 11:42:18 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  UserDefinedFunction [dbo].[CALCWEEKTASKENTRY]    Script Date: 05/11/2015 12:03:06 ******/

CREATE FUNCTION [dbo].[CALCWEEKTASKENTRY]
(	
 @EID INT,
 @DATE DATETIME
)
RETURNS INT
AS
BEGIN
	-- Declare the return variable here
	DECLARE @RES as int
	Declare @Lastweekstartdate as datetime
	Declare @PresentweekstartDate  as datetime
	Declare @Lastweekenddate as datetime
	Declare @PresentweekendDate  as datetime
	set @RES=1
	
Select @Lastweekstartdate=dateadd(wk, datediff(wk, 0, @Date) - 1, 0)  
Select @PresentweekstartDate=dateadd(wk, datediff(wk, 0, @Date), 0) 

set @Lastweekenddate=@Lastweekstartdate+4
set @PresentweekendDate=@PresentweekstartDate+4

	
	if(datename(dw,CONVERT(VARCHAR(15),@DATE,101))='Saturday' )
	Begin
		while (@PresentweekstartDate<=@PresentweekendDate)
		begin
		select @RES=3 from tasks where (select COUNT(*) from tasks where employeeid=@EID)=0
		SELECT @RES=2 FROM tasks WHERE 
			((case 
			when (select count(*) from tasks where employeeid=@EID and CONVERT(VARCHAR(15),executiondate,101)=CONVERT(VARCHAR(15),@PresentweekstartDate,101))=0 then 1
			when (CONVERT(VARCHAR(15),executiondate,101)=CONVERT(VARCHAR(15),@PresentweekstartDate,101)) and (select sum(hours) from tasks where EmployeeId=@EID and (CONVERT(VARCHAR(15),executiondate,101)=CONVERT(VARCHAR(15),@PresentweekstartDate,101)))<8  then 1
			else 0 end) =1)
			and employeeid=@EID
			set @PresentweekstartDate=@PresentweekstartDate+1
		end
	end
	else if(datename(dw,CONVERT(VARCHAR(15),@DATE,101))='Sunday' )
	begin
	while (@Lastweekstartdate<=@Lastweekenddate)
		begin
		select @RES=3 from tasks where (select COUNT(*) from tasks where employeeid=@EID)=0
		SELECT @RES=2 FROM tasks WHERE 
			((case 
			when (select count(*) from tasks where employeeid=@EID and CONVERT(VARCHAR(15),executiondate,101)=CONVERT(VARCHAR(15),@Lastweekstartdate,101))=0 then 1
			when (CONVERT(VARCHAR(15),executiondate,101)=CONVERT(VARCHAR(15),@Lastweekstartdate,101)) and (select sum(hours) from tasks where EmployeeId=@EID and (CONVERT(VARCHAR(15),executiondate,101)=CONVERT(VARCHAR(15),@Lastweekstartdate,101)))<8  then 1
			else 0 end) =1)
			and employeeid=@EID
			set @Lastweekstartdate=@Lastweekstartdate+1
		end
	end
	else
	begin
		while (@Lastweekstartdate<=@Lastweekenddate)
		begin
		select @RES=3 from tasks where (select COUNT(*) from tasks where employeeid=@EID)=0
			SELECT @RES=2 FROM tasks WHERE 
			((case 
			when (select count(*) from tasks where employeeid=@EID and CONVERT(VARCHAR(15),executiondate,101)=CONVERT(VARCHAR(15),@Lastweekstartdate,101))=0 then 1
			when (CONVERT(VARCHAR(15),executiondate,101)=CONVERT(VARCHAR(15),@Lastweekstartdate,101)) and (select sum(hours) from tasks where EmployeeId=@EID and (CONVERT(VARCHAR(15),executiondate,101)=CONVERT(VARCHAR(15),@Lastweekstartdate,101)))<8  then 1
			else 0 end) =1)
			and employeeid=@EID				
			set @Lastweekstartdate=@Lastweekstartdate+1
		end
	
	end
		

	

	RETURN @RES
END

GO
/****** Object:  UserDefinedFunction [dbo].[GETINCOMPLETEDATE]    Script Date: 5/4/2021 11:42:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GETINCOMPLETEDATE] 
(@EID INT)
RETURNS VARCHAR(MAX)
AS
BEGIN



DECLARE @dates VARCHAR(MAX)=''

SELECT @dates=@dates+CONVERT(VARCHAR(15),DATE,101)+', ' FROM (select * from Employees,workingdays) e 
CROSS APPLY (SELECT dbo.[VALIDATETASKENTRY](e.id,e.DATE) result) r
WHERE r.result IS NULL AND id=@EID and e.doj<=e.date



RETURN @dates

END


GO
/****** Object:  UserDefinedFunction [dbo].[GetWorkingDays]    Script Date: 5/4/2021 11:42:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GetWorkingDays](
)
RETURNS @output TABLE(
    Date datetime,DayName VARCHAR(15)
)
BEGIN

		DECLARE @Year AS INT,
		@FirstDateOfYear DATETIME,
		@LastDateOfYear DATETIME
		-- You can change @year to any year you desire
		SELECT @year = Year(getdate())
		SELECT @FirstDateOfYear = DATEADD(yyyy, @Year - 1900, 0)
		SELECT @LastDateOfYear = getdate()-7
		-- Creating Query to Prepare Year Data
		;WITH cte AS (
		SELECT 1 AS DayID,
		@FirstDateOfYear AS FromDate,
		DATENAME(dw, @FirstDateOfYear) AS Dayname
		UNION ALL
		SELECT cte.DayID + 1 AS DayID,
		DATEADD(d, 1 ,cte.FromDate),
		DATENAME(dw, DATEADD(d, 1 ,cte.FromDate)) AS Dayname
		FROM cte
		WHERE DATEADD(d,1,cte.FromDate) < @LastDateOfYear
		)
		
		INSERT INTO @output (Date,DayName) 
		SELECT FromDate AS Date, Dayname
		FROM CTE
		WHERE DayName NOT IN ('Saturday','Sunday')
		   option (maxrecursion 400);
    

    RETURN

END

GO
/****** Object:  UserDefinedFunction [dbo].[VALIDATETASKENTRY]    Script Date: 5/4/2021 11:42:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create FUNCTION [dbo].[VALIDATETASKENTRY] 
(@EID INT,
 @DATE DATETIME)
RETURNS INT
AS
BEGIN

DECLARE @RES AS INT

SELECT @RES=1 FROM tasks WHERE EMPLOYEEID=@EID AND executiondate=@DATE

RETURN @RES

END



GO
/****** Object:  Table [dbo].[Configurations]    Script Date: 5/4/2021 11:42:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Configurations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NULL,
	[Percentage] [varchar](50) NULL,
	[Rate] [numeric](15, 2) NULL,
 CONSTRAINT [PK_Configurations1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customer]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer](
	[CustomerId] [int] IDENTITY(1,1) NOT NULL,
	[CustomerName] [nvarchar](200) NULL,
	[CustomerType] [nvarchar](200) NULL,
	[CustomerAddress1] [nvarchar](200) NULL,
	[CustomerAddress2] [nvarchar](200) NULL,
	[Description] [nvarchar](200) NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeProjects]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeProjects](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[ProjectId] [int] NOT NULL,
 CONSTRAINT [PK_EmployeeProjects] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeRoles]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeRoles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
 CONSTRAINT [PK_EmployeeRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Employees]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employees](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LocationId] [int] NOT NULL,
	[Prefix] [nvarchar](25) NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[MiddleName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Suffix] [nvarchar](25) NULL,
	[CompanyName] [varchar](20) NULL,
	[Cost] [decimal](10, 2) NULL,
	[BillRate] [decimal](10, 2) NULL,
	[EmployeeCode] [nvarchar](50) NULL,
	[DOB] [nvarchar](50) NULL,
	[DOJ] [nvarchar](50) NULL,
	[Email] [nvarchar](50) NULL,
	[Mobile] [nvarchar](50) NULL,
	[DOR] [varchar](50) NULL,
	[EmailReminder] [bit] NULL,
 CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Locations]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Locations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Logins]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Logins](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](25) NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
	[Employee_Id] [int] NULL,
	[Status] [int] NULL,
 CONSTRAINT [PK_Logins] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MTS_EMAILMASTER]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MTS_EMAILMASTER](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TEMPLATEID] [bigint] NOT NULL,
	[EMAILSP] [varchar](200) NOT NULL,
	[REQUESTTIME] [datetime] NOT NULL,
	[STATUS] [tinyint] NOT NULL,
 CONSTRAINT [PK_MTS_EMAILMASTER] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MTS_EMAILSCHEDULE]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MTS_EMAILSCHEDULE](
	[SCHEDULEID] [bigint] IDENTITY(1,1) NOT NULL,
	[TEMPLATEID] [bigint] NOT NULL,
	[SCHEDULEDESCRIPTION] [varchar](30) NOT NULL,
	[SENDBY] [varchar](1) NOT NULL,
	[DAY] [nvarchar](100) NULL,
	[TIME] [time](7) NULL,
 CONSTRAINT [PK_MTS_EMAILSCHEDULE] PRIMARY KEY CLUSTERED 
(
	[SCHEDULEID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MTS_EMAILTEMPLATE]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MTS_EMAILTEMPLATE](
	[TEMPLATEID] [bigint] IDENTITY(1,1) NOT NULL,
	[TEMPLATENAME] [varchar](30) NOT NULL,
	[XMLTEMPLATE] [xml] NOT NULL,
	[SMTPID] [bigint] NOT NULL,
	[Active] [bit] NULL,
	[HtmlPage] [varchar](30) NULL,
 CONSTRAINT [PK_MTS_EMAILTEMPLATE] PRIMARY KEY CLUSTERED 
(
	[TEMPLATEID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MTS_GROUPS]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MTS_GROUPS](
	[Group_ID] [int] NOT NULL,
	[Group_Name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_MTS_GROUPS] PRIMARY KEY CLUSTERED 
(
	[Group_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MTS_PROJECTTYPE]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MTS_PROJECTTYPE](
	[Project_TypeID] [int] NOT NULL,
	[Project_Type] [varchar](50) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MTS_SERVICECONFIG]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MTS_SERVICECONFIG](
	[SERVICEID] [bigint] IDENTITY(1,1) NOT NULL,
	[SERVICENAME] [varchar](50) NOT NULL,
	[SERVICEDISPLAYNAME] [varchar](50) NOT NULL,
	[SERVICEDESCRIPTION] [varchar](255) NOT NULL,
	[SERVICEINVOKETYPE] [varchar](1) NOT NULL,
	[DLLNAME] [varchar](100) NOT NULL,
	[SERVICEPARAMS] [xml] NULL,
	[TIME] [varchar](5) NULL,
	[RETRYCOUNT] [smallint] NULL,
	[MAXERRORS] [smallint] NULL,
	[STATUS] [tinyint] NULL,
UNIQUE NONCLUSTERED 
(
	[SERVICENAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MTS_SMTPDETAILS]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MTS_SMTPDETAILS](
	[SmtpID] [bigint] IDENTITY(1,1) NOT NULL,
	[SmtpName] [varchar](30) NOT NULL,
	[SmtpClientHost] [varchar](50) NOT NULL,
	[SmtpClientPort] [int] NOT NULL,
	[UserName] [varchar](100) NOT NULL,
	[Password] [varbinary](128) NULL,
	[Domain] [varchar](100) NULL,
	[EnableSsl] [bit] NOT NULL,
	[TimeOut] [int] NOT NULL,
	[SmtpDeliveryMethod] [tinyint] NOT NULL,
	[UseDefaultCredentials] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[SmtpID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[SmtpName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MTS_WORK_GROUP_MAPPING]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MTS_WORK_GROUP_MAPPING](
	[ID] [int] NOT NULL,
	[WORK_ID] [int] NOT NULL,
	[GROUP_ID] [int] NOT NULL,
 CONSTRAINT [PK_MTS_WORK_GROUP_MAPPING] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notes]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Text] [nvarchar](max) NOT NULL,
	[UserId] [nvarchar](25) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[WeeklyReportId] [int] NOT NULL,
 CONSTRAINT [PK_Notes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Projects]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Projects](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[Number] [nvarchar](10) NULL,
	[EstStartDate] [datetime] NULL,
	[EstEndDate] [datetime] NULL,
	[IsCommon] [bit] NULL,
	[TotalAmount] [numeric](15, 2) NULL,
	[Risk] [nvarchar](20) NULL,
	[Discount] [nvarchar](20) NULL,
	[MarginValue] [nvarchar](20) NULL,
	[DiscountAmount] [numeric](15, 2) NULL,
	[Completed] [nvarchar](20) NULL,
	[Customer] [nvarchar](50) NULL,
	[Te] [bit] NOT NULL,
	[Tm] [bit] NOT NULL,
	[TypeOfProject] [int] NULL,
 CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](15) NOT NULL,
 CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tasks]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tasks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WorkCodeId] [int] NOT NULL,
	[ProjectId] [int] NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[ExecutionDate] [datetime] NOT NULL,
	[Hours] [decimal](10, 2) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[Charge] [decimal](10, 2) NULL,
 CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tasks_backup]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tasks_backup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WorkCodeId] [int] NOT NULL,
	[ProjectId] [int] NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[ExecutionDate] [datetime] NOT NULL,
	[Hours] [decimal](18, 0) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[Charge] [decimal](10, 2) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WeeklyReports]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WeeklyReports](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[ProjectId] [int] NOT NULL,
	[Scope] [int] NOT NULL,
	[Schedule] [int] NOT NULL,
	[Quality] [int] NOT NULL,
	[ClientSatisfaction] [int] NOT NULL,
	[ProjectStatus] [nvarchar](max) NOT NULL,
	[Risk] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[From] [datetime] NOT NULL,
	[To] [datetime] NOT NULL,
 CONSTRAINT [PK_WeeklyReports] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WorkCodes]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkCodes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](70) NOT NULL,
	[Number] [nvarchar](10) NULL,
	[Billable] [bit] NOT NULL,
 CONSTRAINT [PK_WorkCodes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[WORKINGDAYS]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[WORKINGDAYS]
AS
SELECT Date, Dayname from GetWorkingDays()
GO
ALTER TABLE [dbo].[Logins] ADD  CONSTRAINT [DF_Logins_Employee_Id]  DEFAULT ((1)) FOR [Employee_Id]
GO
ALTER TABLE [dbo].[Tasks] ADD  CONSTRAINT [DF_Tasks_Charge]  DEFAULT ((0.0)) FOR [Charge]
GO
ALTER TABLE [dbo].[EmployeeProjects]  WITH CHECK ADD  CONSTRAINT [FK_EmployeeEmployeeProject] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employees] ([Id])
GO
ALTER TABLE [dbo].[EmployeeProjects] CHECK CONSTRAINT [FK_EmployeeEmployeeProject]
GO
ALTER TABLE [dbo].[EmployeeProjects]  WITH CHECK ADD  CONSTRAINT [FK_ProjectEmployeeProject] FOREIGN KEY([ProjectId])
REFERENCES [dbo].[Projects] ([Id])
GO
ALTER TABLE [dbo].[EmployeeProjects] CHECK CONSTRAINT [FK_ProjectEmployeeProject]
GO
ALTER TABLE [dbo].[EmployeeRoles]  WITH CHECK ADD  CONSTRAINT [FK_EmployeeEmployeeRole] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employees] ([Id])
GO
ALTER TABLE [dbo].[EmployeeRoles] CHECK CONSTRAINT [FK_EmployeeEmployeeRole]
GO
ALTER TABLE [dbo].[EmployeeRoles]  WITH CHECK ADD  CONSTRAINT [FK_RoleEmployeeRole] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([Id])
GO
ALTER TABLE [dbo].[EmployeeRoles] CHECK CONSTRAINT [FK_RoleEmployeeRole]
GO
ALTER TABLE [dbo].[Employees]  WITH CHECK ADD  CONSTRAINT [FK_LocationEmployee] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Locations] ([Id])
GO
ALTER TABLE [dbo].[Employees] CHECK CONSTRAINT [FK_LocationEmployee]
GO
ALTER TABLE [dbo].[Logins]  WITH CHECK ADD  CONSTRAINT [FK_LoginEmployee] FOREIGN KEY([Employee_Id])
REFERENCES [dbo].[Employees] ([Id])
GO
ALTER TABLE [dbo].[Logins] CHECK CONSTRAINT [FK_LoginEmployee]
GO
ALTER TABLE [dbo].[MTS_WORK_GROUP_MAPPING]  WITH CHECK ADD  CONSTRAINT [FK_MTS_WORK_GROUP_MAPPING_MTS_GROUPS] FOREIGN KEY([GROUP_ID])
REFERENCES [dbo].[MTS_GROUPS] ([Group_ID])
GO
ALTER TABLE [dbo].[MTS_WORK_GROUP_MAPPING] CHECK CONSTRAINT [FK_MTS_WORK_GROUP_MAPPING_MTS_GROUPS]
GO
ALTER TABLE [dbo].[MTS_WORK_GROUP_MAPPING]  WITH CHECK ADD  CONSTRAINT [FK_MTS_WORK_GROUP_MAPPING_WorkCodes] FOREIGN KEY([WORK_ID])
REFERENCES [dbo].[WorkCodes] ([Id])
GO
ALTER TABLE [dbo].[MTS_WORK_GROUP_MAPPING] CHECK CONSTRAINT [FK_MTS_WORK_GROUP_MAPPING_WorkCodes]
GO
ALTER TABLE [dbo].[Notes]  WITH CHECK ADD  CONSTRAINT [FK_WeeklyReportNote] FOREIGN KEY([WeeklyReportId])
REFERENCES [dbo].[WeeklyReports] ([Id])
GO
ALTER TABLE [dbo].[Notes] CHECK CONSTRAINT [FK_WeeklyReportNote]
GO
ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_EmployeeTask] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employees] ([Id])
GO
ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_EmployeeTask]
GO
ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_ProjectTask] FOREIGN KEY([ProjectId])
REFERENCES [dbo].[Projects] ([Id])
GO
ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_ProjectTask]
GO
ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK_WorkCodeTask] FOREIGN KEY([WorkCodeId])
REFERENCES [dbo].[WorkCodes] ([Id])
GO
ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK_WorkCodeTask]
GO
ALTER TABLE [dbo].[WeeklyReports]  WITH CHECK ADD  CONSTRAINT [FK_EmployeeWeeklyReport] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employees] ([Id])
GO
ALTER TABLE [dbo].[WeeklyReports] CHECK CONSTRAINT [FK_EmployeeWeeklyReport]
GO
ALTER TABLE [dbo].[WeeklyReports]  WITH CHECK ADD  CONSTRAINT [FK_ProjectWeeklyReport] FOREIGN KEY([ProjectId])
REFERENCES [dbo].[Projects] ([Id])
GO
ALTER TABLE [dbo].[WeeklyReports] CHECK CONSTRAINT [FK_ProjectWeeklyReport]
GO
/****** Object:  StoredProcedure [dbo].[EMPLOYEE_ACTIVITY_WISE_SUMMARY_REPORT]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[EMPLOYEE_ACTIVITY_WISE_SUMMARY_REPORT] (
	@FROMDATE DATETIME
	,@TODATE DATETIME
	,@PROJECT_ID INT
	,@LOCATION_ID INT
	)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @PROJECT_ID = 0
		SET @PROJECT_ID = NULL

	IF @LOCATION_ID = 0
		SET @LOCATION_ID = NULL

	CREATE TABLE #ACTIVITY_TABLE (
		Resource VARCHAR(MAX)
		,Activity_Name VARCHAR(MAX)
		,Hours DECIMAL
		)

	INSERT INTO #ACTIVITY_TABLE (
		Resource
		,Activity_Name
		,Hours
		)
	SELECT E.LASTNAME + ' ' + E.FIRSTNAME AS Resource
		,REPLACE(WC.NAME, '&', ' AND ') AS ACTIVITY_NAME
		,SUM(T.Hours) AS TOTALHOURS
	FROM EMPLOYEES E WITH (NOLOCK)
	INNER JOIN TASKS T WITH (NOLOCK) ON T.EMPLOYEEID = E.ID
	INNER JOIN WORKCODES WC WITH (NOLOCK) ON WC.ID = T.WORKCODEID
	INNER JOIN PROJECTS PJ WITH (NOLOCK) ON T.PROJECTID = PJ.ID
	INNER JOIN LOCATIONS L WITH (NOLOCK) ON E.LOCATIONID = L.ID
	WHERE T.EXECUTIONDATE >= @FROMDATE
		AND T.EXECUTIONDATE <= @TODATE
		AND T.PROJECTID = isnull(@PROJECT_ID, T.PROJECTID)
		AND L.ID = isnull(@LOCATION_ID, E.LocationId)
	GROUP BY WC.NAME
		,E.FIRSTNAME
		,E.LASTNAME

	--SELECT * FROM #ACTIVITY_TABLE
	DECLARE @COLS NVARCHAR(MAX)

	SELECT @COLS = COALESCE(@COLS + ',[' + Activity_Name + ']', '[' + Activity_Name + ']')
	FROM (
		SELECT DISTINCT Activity_Name
		FROM #ACTIVITY_TABLE
		) AT
	ORDER BY Activity_Name

	SELECT @COLS += ',[Total]'

	--select @COLS
	DECLARE @NULLTOZEROCOLS NVARCHAR(MAX)

	SELECT @NULLTOZEROCOLS = SUBSTRING((
				SELECT ',ISNULL([' + Activity_Name + '],0) AS [' + Activity_Name + ']'
				FROM (
					SELECT DISTINCT Activity_Name
					FROM #ACTIVITY_TABLE
					) NULLTOZEROS
				ORDER BY Activity_Name
				FOR XML PATH('')
				), 2, 8000)

	SELECT @NULLTOZEROCOLS += ',ISNULL([Total],0) AS [Total]'

	--select @NULLTOZEROCOLS
	DECLARE @QUERY NVARCHAR(MAX)

	SET @QUERY = 'SELECT P.Resource,' + @NULLTOZEROCOLS + ' FROM 
				 (                
					 SELECT 
					 ISNULL(CAST(RESOURCE AS VARCHAR(MAX)),''Total'')Resource, 
					 SUM(Hours)Hours , 
					 ISNULL(Activity_Name,''Total'')Activity_Name              
					 FROM #ACTIVITY_TABLE
					 GROUP BY Activity_Name,Resource
					 WITH CUBE                   
				 ) X
				 PIVOT 
				 (
					 SUM(Hours)
					 FOR Activity_Name IN (' + @COLS + ')
				) P
				LEFT JOIN
				(  
					SELECT DISTINCT Resource
					FROM #ACTIVITY_TABLE 
					group by Resource  
				)T
				ON P.Resource=T.Resource
				ORDER BY CASE WHEN (P.Resource=''Total'') THEN 1 ELSE 0 END,P.Resource'

	EXEC SP_EXECUTESQL @QUERY

	DROP TABLE #ACTIVITY_TABLE
END
GO
/****** Object:  StoredProcedure [dbo].[EMPLOYEE_DATEWISE_SUMMARY_REPORT]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================  
-- Author:  <Author,,Name>  
-- Create date: <Create Date,,>  
-- Description: <Description,,>  
-- =============================================  
CREATE PROCEDURE [dbo].[EMPLOYEE_DATEWISE_SUMMARY_REPORT] (
	@FROMDATE DATETIME
	,@TODATE DATETIME
	,@PROJECT_ID INT
	,@RESOURCE_ID INT
	)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from  
	-- interfering with SELECT statements.  
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	IF @PROJECT_ID = 0
	BEGIN
		SET @PROJECT_ID = NULL
	END

	IF @RESOURCE_ID = 0
	BEGIN
		SET @RESOURCE_ID = NULL
	END

	CREATE TABLE #EMP_TABLE (
		DATE DATETIME
		,Activity_Name VARCHAR(MAX)
		,Hours DECIMAL
		)

	INSERT INTO #EMP_TABLE (
		DATE
		,Activity_Name
		,Hours
		)
	SELECT T.EXECUTIONDATE AS DATE
		,REPLACE(WC.NAME, '&', 'AND') AS ACTIVITY_NAME
		,SUM(T.Hours) AS TOTALHOURS
	FROM EMPLOYEES E WITH (NOLOCK)
	INNER JOIN EMPLOYEEROLES ER WITH (NOLOCK) ON ER.EMPLOYEEID = E.ID
	INNER JOIN TASKS T WITH (NOLOCK) ON T.EMPLOYEEID = E.ID
	INNER JOIN WORKCODES WC WITH (NOLOCK) ON WC.ID = T.WORKCODEID
	INNER JOIN PROJECTS PJ WITH (NOLOCK) ON T.PROJECTID = PJ.ID
	WHERE T.EXECUTIONDATE >= @FROMDATE
		AND T.EXECUTIONDATE < @TODATE + 1
		AND PJ.ID = ISNULL(@PROJECT_ID, PJ.ID)
		AND E.ID = ISNULL(@RESOURCE_ID, E.ID)
	GROUP BY WC.NAME
		,T.EXECUTIONDATE

	-- select * from  #EMP_TABLE
	DECLARE @COLS NVARCHAR(MAX)

	SELECT @COLS = COALESCE(@COLS + ',[' + Activity_Name + ']', '[' + Activity_Name + ']')
	FROM (
		SELECT DISTINCT Activity_Name
		FROM #EMP_TABLE
		) ET
	ORDER BY Activity_Name

	SELECT @COLS += ',[Total]'

	--select @COLS  
	DECLARE @NULLTOZEROCOLS NVARCHAR(MAX)

	SELECT @NULLTOZEROCOLS = SUBSTRING((
				SELECT ',ISNULL([' + Activity_Name + '],0) AS [' + Activity_Name + ']'
				FROM (
					SELECT DISTINCT Activity_Name
					FROM #EMP_TABLE
					) NULLTOZEROS
				ORDER BY Activity_Name
				FOR XML PATH('')
				), 2, 8000)

	SELECT @NULLTOZEROCOLS += ',ISNULL([Total],0) AS [Total]'

	--select @NULLTOZEROCOLS  
	DECLARE @QUERY NVARCHAR(MAX)

	SET @QUERY = 'SELECT P.Date,' + @NULLTOZEROCOLS + ' FROM   
     (                  
      SELECT   
      ISNULL(CONVERT(VARCHAR(10), Date, 101),''Total'')Date,   
      SUM(Hours)Hours ,   
      ISNULL(Activity_Name,''Total'')Activity_Name                
      FROM #EMP_TABLE  
      GROUP BY Activity_Name,Date  
      WITH CUBE                     
     ) X  
     PIVOT   
     (  
      SUM(Hours)  
      FOR Activity_Name IN (' + @COLS + ')  
    ) P  
    LEFT JOIN  
    (    
     SELECT DISTINCT Date  
     FROM #EMP_TABLE   
     group by Date    
    )T  
    ON P.Date=T.Date  
    ORDER BY CASE WHEN (P.Date=''Total'') THEN 1 ELSE 0 END,P.Date'

	EXEC SP_EXECUTESQL @QUERY

	DROP TABLE #EMP_TABLE
END
GO
/****** Object:  StoredProcedure [dbo].[EmpTimeSheetEntry]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[EmpTimeSheetEntry]@EMPLOYEEID int,@Date datetime
AS

declare @temp table (executiondate date, Dayofweekname varchar(30),Sumofhours int)
declare @Lastweekstartdate datetime
declare @Lastweekenddate datetime
declare @Presentweekstartdate datetime
declare @Presentweekenddate datetime

BEGIN

set @Lastweekstartdate=dateadd(wk,datediff(wk,0,@Date)-1,0)
set @Lastweekenddate=@Lastweekstartdate+4
set @Presentweekstartdate=dateadd(wk,datediff(wk,0,@Date),0)
set @Presentweekenddate=@Presentweekstartdate+4

-- Weekly hours calculation
if(datename(dw,CONVERT(VARCHAR(15),@DATE,101))='Saturday' )
	Begin
		while (@PresentweekstartDate<=@PresentweekendDate)
		begin
		insert into @temp (executiondate, Dayofweekname ,Sumofhours )
		select @PresentweekstartDate,datename(dw,@PresentweekstartDate),(select sum(hours) from tasks 		
		where employeeid=@EMPLOYEEID and convert(varchar(30),executiondate,101)=convert(varchar(30),@PresentweekstartDate,101) )
		set @PresentweekstartDate=@PresentweekstartDate+1
		end
	end
	else if(datename(dw,CONVERT(VARCHAR(15),@DATE,101))='Sunday' )
	begin
	while (@Lastweekstartdate<=@Lastweekenddate)
		begin
		insert into @temp (executiondate, Dayofweekname ,Sumofhours )
		select @Lastweekstartdate,datename(dw,@Lastweekstartdate),(select sum(hours) from tasks 		
		where employeeid=@EMPLOYEEID and convert(varchar(30),executiondate,101)=convert(varchar(30),@Lastweekstartdate,101) )
		set @Lastweekstartdate=@Lastweekstartdate+1
		end
	end
	else
	begin
		while (@Lastweekstartdate<=@Lastweekenddate)
		begin
		insert into @temp (executiondate, Dayofweekname ,Sumofhours )
		select @Lastweekstartdate		
		,datename(dw,@Lastweekstartdate),(select sum(hours) from tasks 		
		where employeeid=@EMPLOYEEID and convert(varchar(30),executiondate,101)=convert(varchar(30),@Lastweekstartdate,101) )
		set @Lastweekstartdate=@Lastweekstartdate+1
		end
	
	end
	
	
	select * from @temp	

	SELECT Email [To],FIRSTNAME,LASTNAME from Employees
	WHERE id=@EMPLOYEEID
	
	

END
GO
/****** Object:  StoredProcedure [dbo].[GET_EMPLOYEE_BASED_ON_PROJECTID]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GET_EMPLOYEE_BASED_ON_PROJECTID]
	-- Add the parameters for the stored procedure here
	(@PROJECT_ID INT)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT E.ID AS EMPLOYEEID
		,E.LASTNAME + ',' + E.FIRSTNAME AS EMPLOYEENAME
	FROM EMPLOYEES E
	INNER JOIN TASKS T ON T.EMPLOYEEID = E.ID
	WHERE T.PROJECTID = ISNULL(@PROJECT_ID, T.PROJECTID)
	GROUP BY E.LASTNAME
		,E.FIRSTNAME
		,E.ID
	ORDER BY E.LASTNAME
		,E.FIRSTNAME
END
GO
/****** Object:  StoredProcedure [dbo].[IncompleteTimeSheetEntry]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[IncompleteTimeSheetEntry]@EMPLOYEEID int
AS
BEGIN

SELECT Email [To],FIRSTNAME,LASTNAME,dbo.GETINCOMPLETEDATE(id) 'DATE' from Employees
WHERE id=@EMPLOYEEID


END
GO
/****** Object:  StoredProcedure [dbo].[IncompleteTimeSheetEntryStaging]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[IncompleteTimeSheetEntryStaging]

AS
BEGIN

INSERT INTO MTS_EMAILMASTER ([TEMPLATEID],
	[EMAILSP],
	[REQUESTTIME] ,
	[STATUS])
SELECT 1,'IncompleteTimeSheetEntry|'+CAST(id AS VARCHAR(100)),getdate(),0 FROM (select * from Employees,workingdays) e 
CROSS APPLY (SELECT dbo.[VALIDATETASKENTRY](e.id,e.DATE) result) r
WHERE r.result IS NULL
GROUP BY id
  
END

GO
/****** Object:  StoredProcedure [dbo].[MTS_GETEMAILSCHEDULE]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[MTS_GETEMAILSCHEDULE]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
           SCHEDULEID
         , TEMPLATEID
         , SENDBY
         , DAY
         , [TIME]
      FROM MTS_EMAILSCHEDULE WITH ( NOLOCK )
      WHERE SENDBY = 1;
END;

GO
/****** Object:  StoredProcedure [dbo].[MTS_GETEMAILSCHEDULEFORTIMESCHEDULER]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_GETEMAILSCHEDULEFORTIMESCHEDULER]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
           SCHEDULEID
         , TEMPLATEID
         , SENDBY
         , DAY
         , [TIME]
      FROM MTS_EMAILSCHEDULE WITH ( NOLOCK )
      WHERE SENDBY <> 1;
END;

GO
/****** Object:  StoredProcedure [dbo].[MTS_GETEMAILSWAITINGTOBESEND]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_GETEMAILSWAITINGTOBESEND]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
           ID
         , TEMPLATEID
         , EMAILSP
      FROM MTS_EMAILMASTER WITH ( NOLOCK )
      WHERE STATUS = 0;
END;

GO
/****** Object:  StoredProcedure [dbo].[MTS_GETEMAILTEMPLATE]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MTS_GETEMAILTEMPLATE]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
           TEMPLATEID
         , TEMPLATENAME
         , XMLTEMPLATE
         , SMTPID
		 ,ACTIVE
		 ,HTMLPAGE
      FROM MTS_EMAILTEMPLATE WITH ( NOLOCK );
END;
GO
/****** Object:  StoredProcedure [dbo].[MTS_GETEMAILTEMPLATEDATA]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_GETEMAILTEMPLATEDATA]             
 @TEMPLATEID BIGINT          
AS            
BEGIN             
 SET NOCOUNT ON;       
	SET ARITHABORT ON 
         
   SELECT
   TemplateName,              
   ToMail = EmailTemp.value('(To)[1]', 'varchar(50)'),           
   FromMail = EmailTemp.value('(From)[1]', 'varchar(50)'),           
   CcMail = EmailTemp.value('(Cc)[1]', 'varchar(50)'),           
   BCcMail = EmailTemp.value('(BCc)[1]', 'varchar(50)'),          
   Subjects = EmailTemp.value('(Subject)[1]', 'varchar(50)'),           
   Active
   --Body = EmailTemp.value('(Body)[1]','varchar(500)')  
   --Body = CAST(EmailTemp.value('(Body)[1]', 'varchar(500)') AS XML)   
   FROM          
   MTS_EMAILTEMPLATE WITH ( NOLOCK )          
   CROSS APPLY           
   XMLTemplate.nodes('/email') AS T(EmailTemp)          
   WHERE TEMPLATEID = @TEMPLATEID  
   
   SELECT  REPLACE(REPLACE(CAST(XMLTemplate.query('/email/Body') AS VARCHAR(MAX)),'<Body>',''),'</Body>','')  AS Body from MTS_EMAILTEMPLATE   
           WHERE TEMPLATEID = @TEMPLATEID
           
           
         
END

GO
/****** Object:  StoredProcedure [dbo].[MTS_GETSERVICECONFIG]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_GETSERVICECONFIG]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
           SERVICENAME
         , SERVICEDISPLAYNAME
         , SERVICEDESCRIPTION
         , SERVICEINVOKETYPE
         , SERVICEPARAMS
         , [TIME]
      FROM MTS_SERVICECONFIG WITH ( NOLOCK );
END;

GO
/****** Object:  StoredProcedure [dbo].[MTS_GETSERVICECONFIGFORSERVICE]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_GETSERVICECONFIGFORSERVICE] @Servicename VARCHAR( 50 )
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
           SERVICENAME
         , SERVICEDISPLAYNAME
         , SERVICEDESCRIPTION
         , SERVICEINVOKETYPE
         , DLLNAME
         , SERVICEPARAMS
         , [TIME]
         , RETRYCOUNT
         , MAXERRORS
      FROM MTS_SERVICECONFIG WITH ( NOLOCK )
      WHERE
           SERVICENAME = @Servicename;
END;

GO
/****** Object:  StoredProcedure [dbo].[MTS_GETSTMPDETAILS]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_GETSTMPDETAILS]
AS
BEGIN
    OPEN SYMMETRIC KEY PASSWORDKEY DECRYPTION BY CERTIFICATE SMTP;
    SELECT
           SMTPID
         , SMTPCLIENTHOST
         , SMTPCLIENTPORT
         , USERNAME
         , CONVERT( VARCHAR , DECRYPTBYKEY( PASSWORD ))AS PASSWORD
         , DOMAIN
         , ENABLESSL
         , TIMEOUT
         , SMTPDELIVERYMETHOD
         , USEDEFAULTCREDENTIALS
      FROM MTS_SMTPDETAILS WITH ( NOLOCK );
END;

GO
/****** Object:  StoredProcedure [dbo].[MTS_GETTEMPLATEIDFROMSCHEDULEID]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_GETTEMPLATEIDFROMSCHEDULEID] @Scheduleid INT
AS
BEGIN
    SELECT
           TEMPLATEID
      FROM MTS_EMAILSCHEDULE WITH ( NOLOCK )
      WHERE
           SCHEDULEID = @Scheduleid;
END;

GO
/****** Object:  StoredProcedure [dbo].[MTS_RECISSIONEMAIL]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_RECISSIONEMAIL] @To AS VARCHAR(50)
	,@Firstname AS VARCHAR(30)
	,@Lastname AS VARCHAR(30)
	,@IDENTITY AS VARCHAR(150)
AS
BEGIN
	SELECT @To AS 'To'
		,@Firstname + ' ' + @Lastname AS USERNAME		
		,@IDENTITY AS [IDENTITY]
END;

GO
/****** Object:  StoredProcedure [dbo].[MTS_SETUSEREMAILMAPPING]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_SETUSEREMAILMAPPING]
@TEMPLATEID BIGINT,
@USERID VARCHAR(MAX)
AS
BEGIN

SET NOCOUNT ON;
	
	BEGIN TRAN;
	
	DELETE MTS_USEREMAILMAPPING WHERE TEMPLATEID=@TEMPLATEID
	
	INSERT INTO MTS_USEREMAILMAPPING(TEMPLATEID,USERID)
	SELECT @TEMPLATEID,DATA FROM DBO.FUNCTION_STRING_TO_TABLE(@USERID,',')
	
	IF (@@ERROR <> 0)
	BEGIN
		ROLLBACK TRANSACTION
		
		RETURN 0
	END

	COMMIT TRANSACTION
	RETURN 1

END

GO
/****** Object:  StoredProcedure [dbo].[MTS_SMTPDETAIL]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[MTS_SMTPDETAIL]( @Sptype BIGINT ,
                                  @Smtpid BIGINT = NULL ,
                                  @Smtpname VARCHAR( 30 ) = NULL ,
                                  @Smtpclienthost VARCHAR( 50 ) = NULL ,
                                  @Smtpclientport INT = NULL ,
                                  @Username VARCHAR( 100 ) = NULL ,
                                  @Domain VARCHAR( 100 ) = NULL ,
                                  @Password VARCHAR( 100 ) = NULL ,
                                  @Enablessl BIT = NULL ,
                                  @Timeout INT = NULL ,
                                  @Smtpdeliverymethod TINYINT = NULL ,
                                  @Defaultcredentials BIT = NULL )
AS
BEGIN
    BEGIN TRAN;
    OPEN SYMMETRIC KEY PASSWORDKEY DECRYPTION BY CERTIFICATE SMTP;

    IF @Sptype = 0

        BEGIN
            SELECT
                   SMTPID
                 , SMTPNAME
                 , SMTPCLIENTHOST
                 , SMTPCLIENTPORT
                 , USERNAME
                 , DOMAIN
                 , ENABLESSL
                 , TIMEOUT
                 , SMTPDELIVERYMETHOD
                 , USEDEFAULTCREDENTIALS
              FROM MTS_SMTPDETAILS;

        END;
    ELSE
        BEGIN
            IF @Sptype = 1
                BEGIN
                    SELECT
                           SMTPID
                         , SMTPNAME
                         , SMTPCLIENTHOST
                         , SMTPCLIENTPORT
                         , USERNAME
                         , CONVERT( VARCHAR , DECRYPTBYKEY( PASSWORD ))AS PASSWORD
                         , DOMAIN
                         , ENABLESSL
                         , TIMEOUT
                         , SMTPDELIVERYMETHOD
                         , USEDEFAULTCREDENTIALS
                      FROM MTS_SMTPDETAILS
                      WHERE SMTPID = @Smtpid;

                END;
            ELSE
                BEGIN
                    IF @Sptype = 2
                        BEGIN

                            IF NOT EXISTS( SELECT
                                                  1
                                             FROM MTS_SMTPDETAILS WITH ( NOLOCK )
                                             WHERE
                                                  SMTPNAME = @Smtpname )
                                BEGIN
                                    INSERT INTO MTS_SMTPDETAILS(
                                                SMTPNAME
                                              , SMTPCLIENTHOST
                                              , SMTPCLIENTPORT
                                              , USERNAME
                                              , PASSWORD
                                              , DOMAIN
                                              , ENABLESSL
                                              , TIMEOUT
                                              , SMTPDELIVERYMETHOD
                                              , USEDEFAULTCREDENTIALS )
                                    VALUES( @Smtpname
                                          , @Smtpclienthost
                                          , @Smtpclientport
                                          , @Username
                                          , ENCRYPTBYKEY( KEY_GUID( 'PasswordKey' ) , @Password )
                                          , @Domain
                                          , @Enablessl
                                          , @Timeout
                                          , @Smtpdeliverymethod
                                          , @Defaultcredentials );

                                    IF @@Error <> 0
                                        BEGIN
                                            ROLLBACK TRAN;
                                            RETURN -1;
                                        END;

                                END;
                            ELSE
                                BEGIN
                                    COMMIT;
                                    RETURN 1;
                                END;

                        END;
                    ELSE
                        BEGIN
                            IF @Sptype = 3
                                BEGIN

                                    IF NOT EXISTS( SELECT
                                                          1
                                                     FROM MTS_SMTPDETAILS WITH ( NOLOCK )
                                                     WHERE
                                                          SMTPNAME = @Smtpname
                                                      AND
                                                          SMTPID <> @Smtpid )
                                        BEGIN
                                            UPDATE MTS_SMTPDETAILS
                                            SET
                                                SMTPNAME = @Smtpname
                                              ,
                                                SMTPCLIENTHOST = @Smtpclienthost
                                              ,
                                                SMTPCLIENTPORT = @Smtpclientport
                                              ,
                                                USERNAME = @Username
                                              ,[Password]=EncryptByKey(Key_GUID('PasswordKey'),@PASSWORD)
                                              ,
                                                DOMAIN = @Domain
                                              ,
                                                ENABLESSL = @Enablessl
                                              ,
                                                TIMEOUT = @Timeout
                                              ,
                                                SMTPDELIVERYMETHOD = @Smtpdeliverymethod
                                              ,
                                                USEDEFAULTCREDENTIALS = @Defaultcredentials
                                            WHERE
                                                  SMTPID = @Smtpid;

                                            IF @@Error <> 0
                                                BEGIN
                                                    ROLLBACK TRAN;
                                                    RETURN -1;
                                                END;

                                        END;
                                    ELSE
                                        BEGIN
                                            COMMIT;
                                            RETURN 1;
                                        END;
                                END;
                            ELSE
                                BEGIN
                                    IF @Sptype = 4
                                        BEGIN
                                            UPDATE MTS_SMTPDETAILS
                                            SET
                                                PASSWORD = ENCRYPTBYKEY( KEY_GUID( 'PasswordKey' ) , @Password )
                                            WHERE
                                                  SMTPID = @Smtpid;

                                            IF @@Error <> 0
                                                BEGIN
                                                    ROLLBACK TRAN;
                                                    RETURN -1;
                                                END;

                                        END;
                                    ELSE
                                        BEGIN
                                            COMMIT;
                                            RETURN 1;
                                        END;;
                                END;
                        END;
                END;
        END;
    COMMIT;
    RETURN 0;

END;

GO
/****** Object:  StoredProcedure [dbo].[MTS_UPDATEEMAILSTATUS]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_UPDATEEMAILSTATUS]( @Id BIGINT ,
                                            @Status TINYINT )
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MTS_EMAILMASTER WITH( ROWLOCK )
    SET
        STATUS = @Status
    WHERE
          ID = @Id;
END;

GO
/****** Object:  StoredProcedure [dbo].[MTS_UPDATESERVICECONFIG]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_UPDATESERVICECONFIG] @Servicename VARCHAR( 50 ) ,
                                             @Servicedisplayname VARCHAR( 50 ) ,
                                             @Servicedescription VARCHAR( 255 ) ,
                                             @Serviceinvoketype VARCHAR( 1 ) ,
                                             @Serviceparams VARCHAR( MAX ) ,
                                             @Time VARCHAR( 5 )
AS
BEGIN

    SET NOCOUNT ON;
    UPDATE MTS_SERVICECONFIG
    SET
        SERVICEDISPLAYNAME = @Servicedisplayname
      ,
        SERVICEDESCRIPTION = @Servicedescription
      ,
        SERVICEINVOKETYPE = @Serviceinvoketype
      ,
        SERVICEPARAMS = @Serviceparams
      ,
        [TIME] = @Time
    WHERE
           SERVICENAME = @Servicename;
END;

GO
/****** Object:  StoredProcedure [dbo].[MTS_UPDATESERVICESTATUS]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MTS_UPDATESERVICESTATUS] @Servicename VARCHAR( 50 ) ,
                                             @Status TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MTS_SERVICECONFIG
    SET
        STATUS = @Status
    WHERE
           SERVICENAME = @Servicename;
END;

GO
/****** Object:  StoredProcedure [dbo].[TimeSheetEntryweek]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[TimeSheetEntryweek]
(@DayofWeek varchar(20))
as
begin
if(@DayofWeek=datename(dw,getdate())) begin
		  INSERT INTO MTS_EMAILMASTER ([TEMPLATEID],
			[EMAILSP],
			[REQUESTTIME] ,
			[STATUS])
		SELECT 1,'EmpTimeSheetEntry|'+CAST(id AS VARCHAR(100))+'|'+replace(convert(varchar(20),getdate(),101),'/','-'),getdate(),0 FROM (select * from Employees) e 
		CROSS APPLY (SELECT dbo.CALCWEEKTASKENTRY(e.id,getDate()) result) r
		WHERE r.result <> 1 and e.EmailReminder=1  and 
		(case when e.DOR is NULL then 1 
		when e.DOR>=getdate() then 1
		else 0 end)=1
		GROUP BY id
		end

End 
  

GO
/****** Object:  StoredProcedure [dbo].[TREX_CATEGORY_REPORT]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


--drop table #FirstTable 

CREATE PROCEDURE [dbo].[TREX_CATEGORY_REPORT](@FROMDATE DATETIME,@TODATE DATETIME,@LOCATIONID INT)
AS
BEGIN
SET FMTONLY OFF;
CREATE  TABLE  #FirstTable 
(
     Resource    VARCHAR(max)	 
	  ,groupname   VARCHAR(max)
      ,hours      decimal
)

if @LOCATIONID=0
set @LOCATIONID=null

insert into #FirstTable(resource,groupname,hours) select
 
 e.lastname+ ' '+e.firstname  as Employeename
 ,mg.group_name,
t.hours
from employees e with(nolock)
--inner join EmployeeRoles er with(nolock) on er.employeeid=e.id
--inner join roles r with(nolock) on r.id=er.roleid
inner join Tasks t with(nolock) on t.employeeid=e.id
inner join MTS_WORK_GROUP_MAPPING mpgm with(nolock) on mpgm.work_id=t.workcodeid
inner join MTS_GROUPS mg with(nolock) on mg.group_id=mpgm.group_id
inner join locations l with(nolock) on e.LocationId=l.id 
where t.executiondate >= @FROMDATE and t.executiondate <= @TODATE
AND l.id=isnull(@LOCATIONID,e.locationid)
--select * from #FirstTable

DECLARE @cols NVARCHAR (MAX)

SELECT @cols = COALESCE (@cols + ',[' + groupname + ']', '[' + groupname + ']')
               FROM (SELECT DISTINCT groupname FROM #FirstTable) PV 
               ORDER BY groupname 
SELECT @cols += ',[Total]'

DECLARE @NulltoZeroCols NVARCHAR (MAX)
SELECT @NullToZeroCols = SUBSTRING((SELECT ',ISNULL(['+groupname+'],0) AS ['+groupname+']' 
FROM (SELECT DISTINCT groupname FROM #FirstTable)TAB  
ORDER BY groupname FOR XML PATH('')),2,8000) 

SELECT @NullToZeroCols += ',ISNULL([Total],0) AS [Total]'

DECLARE @query NVARCHAR(MAX)
SET @query = 'SELECT P.Resource,' + @NulltoZeroCols + ' FROM 
             (                
                 SELECT 
                 ISNULL(CAST(resource AS VARCHAR(MAX)),''Total'')resource, 
                 SUM(hours)hours , 
                 ISNULL(groupname,''Total'')groupname              
                 FROM #FirstTable
                 GROUP BY groupname,resource
                 WITH CUBE                   
             ) x
             PIVOT 
             (
                 SUM(hours)
                 FOR groupname IN (' + @cols + ')
            ) p
            LEFT JOIN
            (  
                SELECT DISTINCT Resource
                FROM #FirstTable  
            )T
            ON P.Resource=T.Resource
            ORDER BY CASE WHEN (P.Resource=''Total'') THEN 1 ELSE 0 END,P.Resource' 

EXEC SP_EXECUTESQL @query

drop table #FirstTable 

END


GO
/****** Object:  StoredProcedure [dbo].[TREX_PROJECT_REPORT]    Script Date: 5/4/2021 11:42:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[TREX_PROJECT_REPORT]
				 @FROMDATE DATETIME
				,@TODATE DATETIME
				,@LOCATIONID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

if @LOCATIONID=0
set @LOCATIONID=null

CREATE TABLE #FIRSTTABLE (
		Resource VARCHAR(MAX)		
		,PROJECTNAME VARCHAR(MAX)
		,HOURS DECIMAL
		)

	INSERT INTO #FIRSTTABLE (
		Resource
		,PROJECTNAME
		,HOURS
		)
	SELECT E.LASTNAME + ' ' + E.FIRSTNAME AS EMPLOYEENAME
		,REPLACE(PJ.NAME,'&',' AND ') + '|' + REPLACE(PT.PROJECT_TYPE,'&',' AND ')
		,T.HOURS
	FROM EMPLOYEES E WITH (NOLOCK)
	--INNER JOIN EMPLOYEEROLES ER WITH (NOLOCK) ON ER.EMPLOYEEID = E.ID
	--INNER JOIN ROLES R WITH (NOLOCK) ON R.ID = ER.ROLEID
	INNER JOIN TASKS T WITH (NOLOCK) ON T.EMPLOYEEID = E.ID
	INNER JOIN PROJECTS PJ WITH (NOLOCK) ON T.PROJECTID = PJ.ID
	INNER JOIN LOCATIONS L WITH (NOLOCK) ON E.LOCATIONID = L.ID
	INNER JOIN MTS_PROJECTTYPE PT WITH (NOLOCK) ON PJ.TYPEOFPROJECT = PT.PROJECT_TYPEID
	WHERE T.EXECUTIONDATE >=  @FROMDATE
		AND T.EXECUTIONDATE <=  @TODATE
	AND L.ID=isnull(@LOCATIONID,e.locationid)

	--SELECT * FROM #FIRSTTABLE
	DECLARE @COLS NVARCHAR(MAX)

	SELECT @COLS = COALESCE(@COLS + ',[' + PROJECTNAME + ']', '[' + PROJECTNAME + ']')
	FROM (
		SELECT DISTINCT PROJECTNAME
		FROM #FIRSTTABLE
		) PV
	ORDER BY PROJECTNAME

	SELECT @COLS += ',[TOTAL]'

	DECLARE @NULLTOZEROCOLS NVARCHAR(MAX)

	SELECT @NULLTOZEROCOLS = SUBSTRING((
				SELECT ',ISNULL([' + PROJECTNAME + '],0) AS [' + PROJECTNAME + ']'
				FROM (
					SELECT DISTINCT PROJECTNAME
					FROM #FIRSTTABLE
					) TAB
				ORDER BY PROJECTNAME
				FOR XML PATH('')
				), 2, 8000)

	SELECT @NULLTOZEROCOLS += ',ISNULL([TOTAL],0) AS [TOTAL]'

	DECLARE @QUERY NVARCHAR(MAX)

	SET @QUERY = 'SELECT P.Resource,' + @NULLTOZEROCOLS + ' FROM 
				 (                
					 SELECT 
					 ISNULL(CAST(RESOURCE AS VARCHAR(MAX)),''TOTAL'')Resource, 
					 SUM(HOURS)HOURS , 
					 ISNULL(PROJECTNAME,''TOTAL'')PROJECTNAME              
					 FROM #FIRSTTABLE
					 GROUP BY PROJECTNAME,RESOURCE
					 WITH CUBE                   
				 ) X
				 PIVOT 
				 (
					 SUM(HOURS)
					 FOR PROJECTNAME IN (' + @COLS + ')
				) P
				LEFT JOIN
				(  
					SELECT DISTINCT RESOURCE
					FROM #FIRSTTABLE  
				)T
				ON P.RESOURCE=T.RESOURCE
				ORDER BY CASE WHEN (P.RESOURCE=''TOTAL'') THEN 1 ELSE 0 END,P.RESOURCE'

	EXEC SP_EXECUTESQL @QUERY
	DROP TABLE #FIRSTTABLE
END

GO
