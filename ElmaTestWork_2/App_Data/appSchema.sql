
    PRAGMA foreign_keys = OFF

    drop table if exists "Document"

    PRAGMA foreign_keys = ON

    create table "Document" (
        Id TEXT not null,
       OriginalName TEXT,
       Description TEXT,
       Date DATETIME,
       Author TEXT,
       FileName TEXT,
       primary key (Id)
    )
