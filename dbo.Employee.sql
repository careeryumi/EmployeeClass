CREATE TABLE [dbo].[Employee] (
    [empId]     BIGINT       NOT NULL,
    [firstName] VARCHAR (50) NOT NULL,
    [lastName]  VARCHAR (50) NOT NULL,
    [dateTerminate] DATE NOT NULL, 
    PRIMARY KEY CLUSTERED ([empId] ASC)
);

