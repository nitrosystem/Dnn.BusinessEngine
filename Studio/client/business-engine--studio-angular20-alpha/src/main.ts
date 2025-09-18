import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { provideHttpClient } from '@angular/common/http';
import { RouteReuseStrategy } from '@angular/router';
import { CustomReuseStrategy } from './app/routing/custom-reuse.strategy';

bootstrapApplication(App, {
  providers: [
    { provide: RouteReuseStrategy, useClass: CustomReuseStrategy },
    ...appConfig.providers,
    provideHttpClient()
  ]
});
