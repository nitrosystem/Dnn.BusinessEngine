export const baseQuery_insert = `
CREATE PROCEDURE {Schema}.{ProcedureName}
{SpParams}
AS BEGIN
    IF {InsertConditions}
    BEGIN
        INSERT INTO {Schema}.[{TableName}]
                ({InsertColumns})	
            VALUES
                ({InsertParams})

        SELECT Scope_Identity();
    END
END
`;

export const baseQuery_insertupdate = `
CREATE PROCEDURE {Schema}.{ProcedureName}
{SpParams}
AS BEGIN
    IF {InsertConditions}
    BEGIN
        INSERT INTO {Schema}.[{TableName}]
                ({InsertColumns})	
            VALUES
                ({InsertParams})

        SET {PrimaryKeyParam} = (SELECT Scope_Identity());
    END
    ELSE
    BEGIN
        UPDATE {Schema}.{TableName} 
            Set	{UpdateParams}
        WHERE {UpdateConditions}
    END

    SELECT {PrimaryKeyParam}
END
`;

export const baseQuery_update = `
CREATE PROCEDURE {Schema}.{ProcedureName}
{SpParams}
AS BEGIN
    UPDATE {Schema}.{TableName} 
            Set	{UpdateParams}
    WHERE {UpdateConditions}
END
`;