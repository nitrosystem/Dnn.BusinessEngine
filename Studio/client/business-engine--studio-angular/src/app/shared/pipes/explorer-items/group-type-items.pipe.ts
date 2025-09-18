import { Pipe, PipeTransform } from "@angular/core";
import { Group } from "../../../contracts/Group";

@Pipe({ name: 'groupTypeItems' })
export class GroupTypeItemsPipe implements PipeTransform {
    transform(items: Group[], objectType: string): Group[] {
        return items.filter(e => e.ObjectType === objectType);
    }
}
