import { Entity } from "../contracts/entity/entity";

export interface EntityUI extends Entity {
    expanded?: boolean;
}