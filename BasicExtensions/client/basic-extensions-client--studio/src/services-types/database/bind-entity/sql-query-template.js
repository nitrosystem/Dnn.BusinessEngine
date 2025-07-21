export const bindEntity_baseQuery = `
CREATE PROCEDURE {Schema}.{ProcedureName}
{SpParams}
AS BEGIN
    SELECT TOP 1 *
    FROM {Schema}.{Entity}
    {Filters}
END
`;