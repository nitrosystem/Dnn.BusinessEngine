import { Routes } from '@angular/router';
import { Entities } from './pages/entity/entities';

export const routes: Routes = [
    {
        path: 'entities',
        component: Entities,
        children: [
            {
                path: 'edit',
                loadComponent: () => import('./pages/entity/edit-entity').then(m => m.EditEntity)
            },
            {
                path: 'edit/:id',
                loadComponent: () => import('./pages/entity/edit-entity').then(m => m.EditEntity)
            }
        ]
        // children: [
        //     {
        //         path: 'edit',
        //         loadComponent: () => import('./pages/entity/edit-entity').then(m => m.EditEntity),
        //         outlet: 'modal'
        //     }
        // ]
    }
];
