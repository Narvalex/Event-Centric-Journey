﻿
USE [master]
GO

/****** Object:  Database [journey]    Script Date: 15/05/2015 10:43:13 ******/
ALTER DATABASE journey SET SINGLE_USER WITH ROLLBACK IMMEDIATE
DROP DATABASE [journey]
GO

USE [master]
GO

/****** Object:  Database [journey]    Script Date: 18/05/2015 7:53:39 ******/
CREATE DATABASE [journey]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'journey', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\journey.mdf' , SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'journey_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\journey_log.ldf' , SIZE = 2048KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

ALTER DATABASE [journey] SET COMPATIBILITY_LEVEL = 110
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [journey].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [journey] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [journey] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [journey] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [journey] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [journey] SET ARITHABORT OFF 
GO

ALTER DATABASE [journey] SET AUTO_CLOSE OFF 
GO

--ALTER DATABASE [journey] SET AUTO_CREATE_STATISTICS ON 
GO

ALTER DATABASE [journey] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [journey] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [journey] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [journey] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [journey] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [journey] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [journey] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [journey] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [journey] SET  DISABLE_BROKER 
GO

ALTER DATABASE [journey] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [journey] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [journey] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [journey] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [journey] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [journey] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [journey] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [journey] SET RECOVERY SIMPLE 
GO

ALTER DATABASE [journey] SET  MULTI_USER 
GO

ALTER DATABASE [journey] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [journey] SET DB_CHAINING OFF 
GO

ALTER DATABASE [journey] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [journey] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO

ALTER DATABASE [journey] SET  READ_WRITE 
GO


Use journey
EXECUTE sp_executesql N'CREATE SCHEMA [Bus] AUTHORIZATION [dbo]';
EXECUTE sp_executesql N'CREATE SCHEMA [EventStore] AUTHORIZATION [dbo]';
EXECUTE sp_executesql N'CREATE SCHEMA [MessageLog] AUTHORIZATION [dbo]';
EXECUTE sp_executesql N'CREATE SCHEMA [ReadModel] AUTHORIZATION [dbo]';
EXECUTE sp_executesql N'CREATE SCHEMA [SubscriptionLog] AUTHORIZATION [dbo]';


CREATE TABLE [Bus].[Commands](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[DeliveryDate] [datetime] NULL,
	[CorrelationId] [nvarchar](max) NULL,
	[IsDeadLetter] [bit] NOT NULL,
	[TraceInfo] [nvarchar](max) NULL,
 CONSTRAINT [PK_Bus.Commands] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

USE [journey]
GO

/****** Object:  Table [Bus].[Events]    Script Date: 15/05/2015 10:44:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [Bus].[Events](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[DeliveryDate] [datetime] NULL,
	[CorrelationId] [nvarchar](max) NULL,
	[IsDeadLetter] [bit] NOT NULL,
	[TraceInfo] [nvarchar](max) NULL,
 CONSTRAINT [PK_Bus.Events] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


USE [journey]
GO

/****** Object:  Table [EventStore].[Events]    Script Date: 15/05/2015 10:44:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create table [EventStore].[Events] (
    [SourceId] [uniqueidentifier] not null,
    [SourceType] [nvarchar](255) not null,
    [Version] [int] not null,
    [EventType] [nvarchar](255) null,
    [Payload] [nvarchar](max) null,
    [CreationDate] [datetime] not null,
    [LastUpdateTime] [datetime] not null,
    [CorrelationId] [uniqueidentifier] not null,
	[IsProjectable] [bit] not null,
    primary key ([SourceId], [SourceType], [Version])
);
GO


USE [journey]
GO

/****** Object:  Table [EventStore].[Snapshots]    Script Date: 15/05/2015 10:44:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create table [EventStore].[Snapshots] (
    [PartitionKey] [nvarchar](300) not null,
    [Memento] [nvarchar](max) null,
    [LastUpdateTime] [datetime] null,
    primary key ([PartitionKey])
);

GO


USE [journey]
GO

/****** Object:  Table [MessageLog].[Messages]    Script Date: 15/05/2015 10:44:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create table [MessageLog].[Messages] (
    [Id] [bigint] not null identity,
    [Kind] [nvarchar](50) null,
    [SourceId] [nvarchar](50) null,
    [Version] [nvarchar](50) null,
    [AssemblyName] [nvarchar](255) null,
    [Namespace] [nvarchar](255) null,
    [FullName] [nvarchar](255) null,
    [TypeName] [nvarchar](255) null,
    [SourceType] [nvarchar](255) null,
    [CreationDate] [nvarchar](50) null,
    [LastUpdateTime] [nvarchar](50) null,
    [Payload] [nvarchar](max) null,
    [Origin] [nvarchar](50) null,
    primary key ([Id])
);

GO


USE [journey]
GO


/****** Object:  Table [ReadModel].[ResumenDeAnimalesDeTodosLosPeriodos]    Script Date: 15/05/2015 10:45:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ReadModel].[ResumenDeAnimalesDeTodosLosPeriodos](
	[Periodo] [nvarchar](128) NOT NULL,
	[Cantidad] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Periodo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


USE [journey]
GO

/****** Object:  Table [SubscriptionLog].[ReadModelingEvents]    Script Date: 15/05/2015 10:45:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [SubscriptionLog].[ReadModelingEvents](
	[SourceId] [uniqueidentifier] NOT NULL,
	[SourceType] [nvarchar](128) NOT NULL,
	[Version] [int] NOT NULL,
	[EventType] [nvarchar](max) NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[SourceId] ASC,
	[SourceType] ASC,
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


