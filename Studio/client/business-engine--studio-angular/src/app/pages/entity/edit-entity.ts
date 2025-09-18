import { Component, inject, Input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { GlobalStateService } from '../../core/services/global-state.service';
import { ApiService } from '../../core/services/api.service';
import { Entity, EntityFactory } from '../../contracts/entity/entity';
import { ActivatedRoute, Router } from '@angular/router';
import { Dialog } from 'primeng/dialog';

@Component({
    selector: 'edit-entity',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        Dialog
    ],
    templateUrl: './edit-entity.html'
})
export class EditEntity implements OnInit {
    entity = EntityFactory.create();
    visible = true;

    constructor(
        private global: GlobalStateService,
        private api: ApiService,
        private router: Router,
        private route: ActivatedRoute
    ) {

    }

    ngOnInit(): void {
    }

    submit(): void {
        const aa = this.entity;

    }

    close(): void {
        this.router.navigate(['/entities']);
    }

    // async getEntitiesAsync(): Promise<Entity[]> {
    //     const response = await firstValueFrom(
    //         this.api.get<{ Entities: Entity[] }>('GetEntities', { ... })
    //     );
    //     return response.Entities;
    // }
}

