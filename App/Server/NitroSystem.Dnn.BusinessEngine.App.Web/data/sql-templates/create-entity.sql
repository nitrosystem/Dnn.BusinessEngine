CREATE TABLE dbo.{TableName}
(
    {PrimaryColumnName} {PrimaryColumnType} {PrimaryIsIdentity} NOT NULL,
    CONSTRAINT PK_{TableName} PRIMARY KEY CLUSTERED ({PrimaryColumnName})
);