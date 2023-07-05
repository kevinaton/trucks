UPDATE [dbo].[CustomerContact]
SET 
    [FirstName] = ISNULL(CASE
                     WHEN [name] LIKE '% %' THEN LEFT([name], Charindex(' ', [name]) - 1)
                     ELSE [name]
                   END, ''),
    [LastName] = ISNULL(CASE
                     WHEN [name] LIKE '% %' THEN RIGHT([name], Charindex(' ', Reverse([name])) - 1)
                   END, ISNULL(CASE
                     WHEN [name] LIKE '% %' THEN LEFT([name], Charindex(' ', [name]) - 1)
                     ELSE [name]
                   END, ''))