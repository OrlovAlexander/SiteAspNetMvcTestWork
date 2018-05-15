
    if exists (select * from dbo.sysobjects where id = object_id(N'[Document]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [Document]

    create table [Document] (
        Id NVARCHAR(255) not null,
       OriginalName NVARCHAR(255) null,
       Description NVARCHAR(255) null,
       Date DATETIME2 null,
       Author NVARCHAR(255) null,
       FileName NVARCHAR(255) null,
       primary key (Id)
    )
