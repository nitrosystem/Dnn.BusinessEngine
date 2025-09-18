import { RouteReuseStrategy, DetachedRouteHandle, ActivatedRouteSnapshot } from '@angular/router';

export class CustomReuseStrategy implements RouteReuseStrategy {

  storedHandles = new Map<string, DetachedRouteHandle>();

  // آیا route قابل ذخیره است؟
  shouldDetach(route: ActivatedRouteSnapshot): boolean {
    return !!route.routeConfig; // فقط routeهای تعریف شده
  }

  // ذخیره handle
  store(route: ActivatedRouteSnapshot, handle: DetachedRouteHandle): void {
    if (route.routeConfig) {
      this.storedHandles.set(this.getRouteKey(route), handle);
    }
  }

  // آیا route قابل بازیابی است؟
  shouldAttach(route: ActivatedRouteSnapshot): boolean {
    return !!route.routeConfig && this.storedHandles.has(this.getRouteKey(route));
  }

  // بازیابی handle
  retrieve(route: ActivatedRouteSnapshot): DetachedRouteHandle | null {
    if (!route.routeConfig) return null;
    return this.storedHandles.get(this.getRouteKey(route)) || null;
  }

  // آیا باید دوباره route ساخته بشه؟
  shouldReuseRoute(future: ActivatedRouteSnapshot, curr: ActivatedRouteSnapshot): boolean {
    return future.routeConfig === curr.routeConfig;
  }

  private getRouteKey(route: ActivatedRouteSnapshot): string {
    return route.routeConfig?.path || '';
  }
}
