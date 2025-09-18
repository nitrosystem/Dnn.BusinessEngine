// src/app/components/sidebar-explorer/sidebar-explorer.component.ts
import { Component, inject, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExplorerItem } from '../../../contracts/ExplorerItem';
import { GlobalStateService } from '../../../core/services/global-state.service';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { GroupFilterPipe } from '../../pipes/explorer-items/group-filter.pipe';
import { EntitiesPipe } from '../../pipes/explorer-items/entities.pipe';
import { FilteredItemsPipe } from '../../pipes/explorer-items/filtered-items.pipe';
import { GroupTypeItemsPipe } from '../../pipes/explorer-items/group-type-items.pipe';
import { ApiService } from '../../../core/services/api.service';
import { RouterService } from '../../../routing/router-service';

@Component({
  selector: 'sidebar-explorer',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    EntitiesPipe,
    GroupFilterPipe,
    GroupTypeItemsPipe,
    FilteredItemsPipe],
  templateUrl: './sidebar-explorer.html'
})
export class SidebarExplorerComponent {
  private global = inject(GlobalStateService);
  readonly scenario = this.global.currentScenario;
  readonly groups = this.global.groups();

  constructor(
    private api: ApiService,
    private router: RouterService
  ) { }

  @Input() items: ExplorerItem[] = [];

  searchFilter = new FormControl();

  onItemClick(page: string, subPage?: string | null, param?: any | null): void {
    const routeParts: any[] = [page];

    if (subPage) {
      routeParts.push(subPage);
    }

    if (param !== null && param !== undefined) {
      if (Array.isArray(param)) {
        routeParts.push(...param); // برای چند پارامتر مثل ['23', 'extra']
      } else {
        routeParts.push(param); // برای یک پارامتر مثل '23'
      }
    }

    this.router.navigate(routeParts);
  }
}

