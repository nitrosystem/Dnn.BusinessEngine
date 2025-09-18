import { Component, computed, inject, Input, OnInit, signal, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { GlobalStateService } from '../../core/services/global-state.service';
import { ApiService } from '../../core/services/api.service';
import { ActivatedRoute, RouterOutlet } from '@angular/router';
import { RouterService } from '../../routing/router-service';
import { MenuItem } from 'primeng/api';
import { PanelModule } from 'primeng/panel';
import { MenuModule } from 'primeng/menu';
import { EntityUI } from '../../ui-models/entity-uI';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';

@Component({
    selector: 'entities',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterOutlet,
        PanelModule,
        MenuModule,
        CardModule,
        ButtonModule,
        SelectModule,
        FormsModule
    ],
    templateUrl: './entities.html'
})
export class Entities implements OnInit {
    name = '';
    readonly entities = signal<EntityUI[]>([]);
    //readonly entities= Signal<EntityUI[]>;

    constructor(
        private api: ApiService,
        private router: RouterService,
        private route: ActivatedRoute
    ) { }

    async ngOnInit() {
        var items = await this.getEntities();
        this.entities.set(items);
        //this.entities = computed(() => items ?? []);
    }

    async getEntities(): Promise<EntityUI[]> {
        const result = await this.api.getAsync<{ Entities: EntityUI[] }>('GetEntities', {
            pageIndex: 1,
            pageSize: 10
        });

        return result.Entities;

        // return this.api.get<{ Entities: EntityUI[] }>('GetEntities', {
        //     pageIndex: 1,
        //     pageSize: 10
        // });
    }

    // openAddModal() {
    //     this.router.navigate([{ outlets: { modal: ['add'] } }], { relativeTo: this.route });
    // }

    openAddModal() {
        this.router.navigate(['edit'], { relativeTo: this.route });
    }

    openEditModal(id: number) {
        this.router.navigate(['edit', id], { relativeTo: this.route });
    }


    // async getEntitiesAsync(): Promise<EntityUI[]> {
    //     const response = await firstValueFrom(
    //         this.api.get<{ Entities: EntityUI[] }>('GetEntities', { ... })
    //     );
    //     return response.Entities;
    // }

    menuItems: MenuItem[] = [
        { label: 'Edit', icon: 'pi pi-pencil', command: () => this.onEdit() },
        { label: 'Delete', icon: 'pi pi-trash', command: () => this.onDelete() },
        { label: 'View', icon: 'pi pi-eye', command: () => this.onView() },
    ];

    onEdit() { console.log('Edit clicked'); }
    onDelete() { console.log('Delete clicked'); }
    onView() { console.log('View clicked'); }

    toggleExpand(entity: EntityUI) {
        entity.expanded = !entity.expanded;
    }

    onEntityUIDrop(event: any) {
        const dragIndex = event.dragIndex;
        const dropIndex = event.dropIndex;
        const draggedEntityUI = this.entities()[dragIndex];
        this.entities().splice(dragIndex, 1);
        this.entities().splice(dropIndex, 0, draggedEntityUI);
    }
}

