--SELECT *  FROM [insite2dev].[dbo].[ReportCategory]
--SELECT *  FROM [dbo].[ReportCategory]
SET IDENTITY_INSERT [dbo].[ReportCategory] ON;
INSERT INTO [dbo].[ReportCategory] (ID, [Name], CreationTime, IsDeleted)
VALUES (1, 'Default Category', GETDATE(), 0);
SET IDENTITY_INSERT [dbo].[ReportCategory] OFF;


--SELECT *  FROM [insite2dev].[dbo].[Report]
--SELECT *  FROM [dbo].[Report]
SET IDENTITY_INSERT [dbo].[Report] ON;
INSERT INTO [dbo].[Report] (ID, [Name], [Description], [Path], CategoryId, CreationTime, IsDeleted)
VALUES (1, 'TenantStatisticsReport', 'Tenant Statistics Report', 'TenantStatisticsReport', 1, GETDATE(), 0);
SET IDENTITY_INSERT [dbo].[Report] OFF;

