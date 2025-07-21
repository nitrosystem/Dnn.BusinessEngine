export const deleteEntityRow_baseQuery = `
CREATE PROCEDURE {Schema}.{ProcedureName}
{SpParams}
AS BEGIN
    DELETE
    FROM {Schema}.{Entity}
    {Conditions}

    RETURN @@ROWCOUNT
END
`;