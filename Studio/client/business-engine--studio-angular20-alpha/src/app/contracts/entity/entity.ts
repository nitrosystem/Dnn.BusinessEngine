import { Guid } from "../../shared/utils/types";
import { EntityColumn } from "./entity-column";
import { EntityType } from "./entity-type.enum";

export interface Entity {
    Id: Guid;
    ScenarioId: Guid;
    DatabaseId?: Guid;
    GroupId?: Guid;
    EntityName: string;
    EntityType: EntityType;
    TableName: string;
    IsReadonly: boolean;
    Description: string;
    CreatedOnDate: Date;
    CreatedByUserId: number;
    LastModifiedOnDate: Date;
    LastModifiedByUserId: number;
    ViewOrder: number;
    Settings: Record<string, any> | null;
    Columns: EntityColumn[];
}

export class EntityFactory {
    static create(initial?: Partial<Entity>): Entity {
        return {
            Id: initial?.Id ?? null,
            ScenarioId: initial?.ScenarioId ?? null,
            DatabaseId: initial?.DatabaseId ?? null,
            GroupId: initial?.GroupId ?? null,
            EntityName: initial?.EntityName ?? '',
            EntityType: initial?.EntityType ?? EntityType.Table,
            TableName: initial?.TableName ?? '',
            IsReadonly: initial?.IsReadonly ?? false,
            Description: initial?.Description ?? '',
            CreatedOnDate: initial?.CreatedOnDate ?? new Date(),
            CreatedByUserId: initial?.CreatedByUserId ?? 0,
            LastModifiedOnDate: initial?.LastModifiedOnDate ?? new Date(),
            LastModifiedByUserId: initial?.LastModifiedByUserId ?? 0,
            ViewOrder: initial?.ViewOrder ?? 0,
            Settings: initial?.Settings ?? null,
            Columns: initial?.Columns ?? []
        };
    }
}
