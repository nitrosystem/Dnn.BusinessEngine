export const baseQuery = `
CREATE PROCEDURE {Schema}.{ProcedureName}
{SpParams}
AS BEGIN
    SELECT
        {SelectedColumns}
    FROM 
        {Entities}
    {Filters}
END
`;