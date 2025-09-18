import { Component, signal, OnInit, inject } from '@angular/core';
import { NavigationEnd, RouteReuseStrategy, RouterOutlet } from '@angular/router';
import { ApiService } from './core/services/api.service';
import { ExplorerItem } from './contracts/ExplorerItem';
import { SidebarExplorerComponent } from './shared/components/sidebar-explorer/sidebar-explorer';
import { GlobalStateService } from './core/services/global-state.service';
import { Scenario } from './contracts/Scenario';
import { Group } from './contracts/Group';
import { activityBars } from './shared/config/activity-bars.config';
import { CommonModule } from '@angular/common';
import { Dialog } from 'primeng/dialog';
import { Tooltip } from 'primeng/tooltip';
import { TabsModule } from 'primeng/tabs';
import { WorkspaceTab } from './ui-models/workspace-tab';
import { RouterService } from './routing/router-service';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    CommonModule,
    Dialog,
    Tooltip,
    TabsModule,
    SidebarExplorerComponent
  ],
  templateUrl: './app.html',
  styleUrls: []
})
export class App implements OnInit {
  activityBars = activityBars;
  currentActivityBar = 'explorer';
  scenarioWindowVisible = false;
  tabs = signal<WorkspaceTab[]>([]);
  currentTab = '';

  constructor(
    public global: GlobalStateService,
    private api: ApiService,
    private router: RouterService

  ) { }

  explorerItems!: ExplorerItem[];

  ngOnInit(): void {
    this.api.get<{
      Scenario: Scenario,
      Groups: Group[],
      ExplorerItems: ExplorerItem[]
    }>('GetStudioOptions').subscribe(data => {
      this.global.setScenario(data.Scenario);
      this.global.setgroups(data.Groups);

      this.explorerItems = data.ExplorerItems;

      this.tabs.set([
        {
          key: 'entities',
          page: 'entities',
          title: 'Entities',
          icon: 'plus'
        },
        {
          key: 'entities/edit',
          page: 'entities',
          params: ['edit'],
          title: 'Add Entity',
          icon: 'plus'
        },
        {
          key: 'entities/edit/course',
          page: 'entities',
          params: ['edit', '123'],
          title: 'Course',
          icon: 'plus'
        }
      ]);

      this.setActiveTabByRoute(this.router.router.url.substring(1));

      this.router.router.events.pipe(
        filter(e => e instanceof NavigationEnd)
      ).subscribe((e: NavigationEnd) => {
        this.setActiveTabByRoute(e.urlAfterRedirects.substring(1));
      });
    });
  }

  private setActiveTabByRoute(key: string) {
    this.currentTab = key;
  }

  // effect(() => {
  //     const value = items();
  //     if (value) {
  //       this.global.setScenario(value.Scenario);
  //       this.global.setgroups(value.Groups);

  //       this.explorerItems = value.ExplorerItems;
  //     }
  //   });

  onActivityBarItemClick(name: string): void {
    this.currentActivityBar = name;
  }

  onClearChaceClick(): void {

  }

  onSelectScenarioClick(): void {

  }

  onAddScenarioClick(): void {

  }

  onTabClick(tab: WorkspaceTab): void {
    this.currentTab = tab.key;

    const routeParts: any[] = [tab.page];
    if (tab.params?.length) routeParts.push(...tab.params);

    this.router.navigate(routeParts);
  }

  onCloseTabClick(key: string): void {

  }

  onCloseAllTabsClick(): void {

  }
}
