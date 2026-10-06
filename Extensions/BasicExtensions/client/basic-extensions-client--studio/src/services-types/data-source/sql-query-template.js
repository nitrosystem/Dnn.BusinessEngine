export const baseQuery = `
CREATE PROCEDURE {Schema}.{ProcedureName}
{SpParams}
AS BEGIN
    SELECT
        {SelectedColumns}
    FROM 
        {Entities}
    {Filters}
    {SortingQuery}
    {PagingQuery}

    {EnablePaging}
    SELECT COUNT_BIG(1) as [TotalCount]
    FROM 
        {Entities}
    {Filters}
    {/EnablePaging}
END
`;
