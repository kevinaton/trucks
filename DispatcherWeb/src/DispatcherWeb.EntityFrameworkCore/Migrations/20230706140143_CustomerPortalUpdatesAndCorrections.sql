INSERT INTO dbo.AbpRoles
SELECT 'Customer'[DisplayName]
		, 1[IsStatic]
		, 0[IsDefault]
		, t1.Id[TenantId]
		, 'Customer'[Name]
		, 0[IsDeleted]
		, NULL[DeleterUserId]
		, NULL[DeletionTime]
		, NULL[LastModificationTime]
		, NULL[LastModifierUserId]
		, GETDATE()[CreationTime]
		, NULL[CreatorUserId]
		, NEWID()[ConcurrencyStamp]
		, 'CUSTOMER'[NormalizedName]
FROM AbpTenants t1
WHERE NOT EXISTS(SELECT Id FROM dbo.AbpRoles WHERE [Name]='Customer' AND TenantId=t1.Id);



WITH CustomerContactNamesSource AS
(
	SELECT [Id][CustomerContactId]
		  ,[Name]
		  , (
			SELECT COUNT(value)[NamePartsCount]
			FROM STRING_SPLIT(
						(	SELECT ISNULL([Name], '') 
							FROM [dbo].[CustomerContact] b
							WHERE b.Id=a.Id

						), ' ')
		  )[NamePartsCount]
	FROM [dbo].[CustomerContact] a
),
CustomerContactsSplitNames AS
(
	SELECT [CustomerContactId]
			, [Name]
			, [NamePartsCount]

			, (	CASE 
					WHEN [NamePartsCount] = 1 THEN [Name]
					WHEN [NamePartsCount] = 2 THEN LEFT([Name], CHARINDEX(' ', [Name] + ' ') -1)
					WHEN [NamePartsCount] > 2 THEN (
						SELECT FirstPart FROM dbo.fn_GetCustomerContactNameParts([Name])
					)
					ELSE ''
				END)[FirstName]

			, (	CASE 
					WHEN [NamePartsCount] = 1 THEN [Name]
					WHEN [NamePartsCount] = 2 THEN STUFF(name, 1, Len(Name) +1- CHARINDEX(' ',Reverse(name)), '')
					WHEN [NamePartsCount] > 2 THEN (
						IIF(LEN([Name])>64,
						(SELECT SecondPart FROM dbo.fn_GetCustomerContactNameParts([Name])),
						[Name])
					)
					ELSE ''
				END)[LastName]

	FROM CustomerContactNamesSource
),
Final AS
(
	SELECT [CustomerContactId]
			  , [Name]
			  , [NamePartsCount]
			  , [FirstName]
			  , [LastName]
	FROM CustomerContactsSplitNames
)

--SELECT a.Name, LEN(a.Name)[NameLength], f.[NamePartsCount], f.FirstName, f.LastName
--FROM [dbo].[CustomerContact] a
--	INNER JOIN [Final] f ON 
--		a.Id = f.CustomerContactId

UPDATE a
SET a.FirstName = f.FirstName,
	a.LastName = f.LastName
FROM [dbo].[CustomerContact] a
	INNER JOIN [Final] f ON 
		a.Id = f.CustomerContactId;