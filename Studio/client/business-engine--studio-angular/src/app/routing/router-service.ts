import { Injectable } from '@angular/core';
import { Router, NavigationExtras } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class RouterService {
    constructor(public router: Router) { }

    navigate(commands: any[], extras: NavigationExtras = {}) {
        extras.queryParams = { ...extras.queryParams, s: this.getSParam(), sr: this.getSRParam() };
        extras.queryParamsHandling = 'merge';
        return this.router.navigate(commands, extras);
    }

    getCurrentTab(): string {
        return this.router.url.substring(1);
    }

    private getSParam(): string | null {
        const params = new URLSearchParams(window.location.search);
        return params.get('s');
    }

    private getSRParam(): string | null {
        const params = new URLSearchParams(window.location.search);
        return params.get('sr');
    }
}
