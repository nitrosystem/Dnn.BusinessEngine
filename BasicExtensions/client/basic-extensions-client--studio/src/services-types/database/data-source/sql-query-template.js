export const baseQuery = `
CREATE PROCEDURE {Schema}.{ProcedureName}
{SpParams}
AS BEGIN
    SELECT
        {SelectedColumns}[STARTPAGING],{TotalCountColumnName}[ENDPAGING]
    FROM 
        {Entities}
    [STARTPAGING]CROSS APPLY (SELECT COUNT(*) as {TotalCountColumnName} FROM {Entities} {Filters}) as {TotalCountAliasName}[ENDPAGING]
    {Filters}
    {SortingQuery}
    {PagingQuery}
END
`;