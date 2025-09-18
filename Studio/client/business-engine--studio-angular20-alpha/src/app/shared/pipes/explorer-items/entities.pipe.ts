import { Pipe, PipeTransform } from "@angular/core";
import { ExplorerItem } from "../../../contracts/ExplorerItem";

@Pipe({ name: 'entities' })
export class EntitiesPipe implements PipeTransform {
    transform(items: ExplorerItem[]): ExplorerItem[] {
        return items.filter(e => e.Type === 'Entity');
    }
}
