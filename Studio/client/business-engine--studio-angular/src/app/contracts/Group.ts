import { Guid } from "../shared/utils/types";

export interface Group {
    Id: Guid;
    ScenarioId: Guid;
    GroupType: string
    ObjectType: string;
    GroupName: string;
    IsSystemGroup: boolean;
}
