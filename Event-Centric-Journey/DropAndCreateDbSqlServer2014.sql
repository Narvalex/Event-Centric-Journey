
USE [master]
GO

/****** Object:  Database [journey]    Script Date: 15/05/2015 10:43:13 ******/
ALTER DATABASE journey SET SINGLE_USER WITH ROLLBACK IMMEDIATE
DROP DATABASE [journey]
GO

/****** Object:  Database [journey]    Script Date: 15/05/2015 10:43:13 ******/
CREATE DATABASE [journey]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'journey', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.SQLEXPRESS\MSSQL\DATA\journey.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'journey_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.SQLEXPRESS\MSSQL\DATA\journey_log.ldf' , SIZE = 1280KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

ALTER DATABASE [journey] SET COMPATIBILITY_LEVEL = 120
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

ALTER DATABASE [journey] SET DELAYED_DURABILITY = DISABLED 
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

CREATE TABLE [EventStore].[Events](
	[SourceId] [uniqueidentifier] NOT NULL,
	[SourceType] [nvarchar](128) NOT NULL,
	[Version] [int] NOT NULL,
	[EventType] [nvarchar](max) NULL,
	[Payload] [nvarchar](max) NULL,
	[CreationDate] [datetime] NOT NULL,
	[LastUpdateTime] [datetime] NOT NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[SourceId] ASC,
	[SourceType] ASC,
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


USE [journey]
GO

/****** Object:  Table [EventStore].[Snapshots]    Script Date: 15/05/2015 10:44:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [EventStore].[Snapshots](
	[PartitionKey] [nvarchar](128) NOT NULL,
	[Memento] [nvarchar](max) NULL,
	[LastUpdateTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[PartitionKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


USE [journey]
GO

/****** Object:  Table [MessageLog].[Messages]    Script Date: 15/05/2015 10:44:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [MessageLog].[Messages](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Kind] [nvarchar](max) NULL,
	[SourceId] [nvarchar](max) NULL,
	[Version] [nvarchar](max) NULL,
	[AssemblyName] [nvarchar](max) NULL,
	[Namespace] [nvarchar](max) NULL,
	[FullName] [nvarchar](max) NULL,
	[TypeName] [nvarchar](max) NULL,
	[SourceType] [nvarchar](max) NULL,
	[CreationDate] [nvarchar](max) NULL,
	[LastUpdateTime] [nvarchar](max) NULL,
	[Payload] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

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


