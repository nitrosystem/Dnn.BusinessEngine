import { Pipe, PipeTransform } from "@angular/core";
import { ExplorerItem } from "../../../contracts/ExplorerItem";
import { Guid } from "../../utils/types";

@Pipe({ name: 'groupFilter' })
export class GroupFilterPipe implements PipeTransform {
    transform(items: ExplorerItem[], groupId: Guid): ExplorerItem[] {
        if (groupId === 'all') return items;
        if (groupId === 'null') return items.filter(e => e.GroupId === null);
        return items.filter(e => e.GroupId === groupId);
    }
}
