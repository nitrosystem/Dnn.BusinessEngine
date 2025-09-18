import { Pipe, PipeTransform } from '@angular/core';
import { ExplorerItem } from '../../../contracts/ExplorerItem';

@Pipe({
    name: 'filteredItems',
    standalone: true
})
export class FilteredItemsPipe implements PipeTransform {
    transform(
        items: ExplorerItem[],
        searchTerm: string | null
    ): ExplorerItem[] {
        if (!items) return [];

        return items.filter(item => {
            const matchSearch = !searchTerm || searchTerm === 'all' ? true :
                item.Title.toLowerCase().includes(searchTerm.toLowerCase());

            return matchSearch;
        });
    }
}
