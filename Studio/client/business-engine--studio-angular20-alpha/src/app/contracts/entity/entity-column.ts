import { Guid } from "../../shared/utils/types";

export interface EntityColumn {
    Id: Guid;
    EntityId: Guid;
    ColumnName: string;
    ColumnType: string;
    AllowNulls: boolean;
    DefaultValue: string;
    IsPrimary: boolean;
    IsUnique: boolean;
    IsComputedColumn: boolean;
    IsIdentity: boolean;
    Formula: string;
    Settings: string;
    Description: string;
    ViewOrder: number;
    LastModifiedOnDate: Date;
    LastModifiedByUserId: number;
    CreatedOnDate: Date;
    CreatedByUserId: number;
}
