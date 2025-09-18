import { Guid } from "../shared/utils/types";

export interface ExplorerItem {
    Id: Guid;
    ScenarioId: Guid;
    ParentId?: Guid;
    GroupId?: Guid;
    DashboardPageParentId?: Guid;
    Type: string
    Title: string;
    ViewOrder: number;
}
