import { Guid } from "../shared/utils/types";

export interface Scenario {
    Id: Guid;
    ScenarioName: string;
    ScenarioTitle: string;
    DatabaseObjectPrefix: string;
}
