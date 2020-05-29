CREATE TABLE [dbo].[Employee]
(
	[empId] BIGINT NOT NULL PRIMARY KEY, 
    [firstName] VARCHAR(50) NOT NULL, 
    [lastName] VARCHAR(50) NOT NULL, 
    [dateTerminated] DATE NOT NULL
)
