// src/app/core/global-state.service.ts
import { Injectable, signal } from '@angular/core';
import { Scenario } from '../../contracts/Scenario';
import { Group } from '../../contracts/Group';

@Injectable({ providedIn: 'root' })
export class GlobalStateService {
  readonly scenario = signal<Scenario | null>(null);
  readonly scenarios = signal<Scenario[]>([]);
  readonly groups = signal<Group[]>([]);

  setScenario(items: Scenario) {
    this.scenario.set(items);
  }

  setScenarios(items: Scenario[]) {
    this.scenarios.set(items);
  }

  get currentScenario(): Scenario | null {
    return this.scenario();
  }

  setgroups(items: Group[]) {
    this.groups.set(items);
  }
}
