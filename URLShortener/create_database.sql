IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'Enshorten')
	BEGIN
		CREATE DATABASE [Enshorten]
	END

GO
USE [Enshorten]
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Urls' and xtype='U')
BEGIN
    CREATE TABLE Urls (
        ShortHash INT PRIMARY KEY CLUSTERED NOT NULL,
		FullUrl NVARCHAR(2048) NOT NULL
    )
END
