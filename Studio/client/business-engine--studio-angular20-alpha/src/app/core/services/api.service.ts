import { inject, Injectable, runInInjectionContext } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { firstValueFrom, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = '/DesktopModules/BusinessEngineStudio/API/Studio';

  private defaultHeaders = new HttpHeaders({
    'Content-Type': 'application/json',
    'scenarioId': '05DF435E-BE67-46F0-B783-8E11216C44C3'

  });


  get<T>(endpoint: string, params?: Record<string, any>): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}/${endpoint}`, {
      headers: this.defaultHeaders,
      params: new HttpParams({ fromObject: params || {} })
    });
  }

  async getAsync<T>(endpoint: string, params?: Record<string, any>): Promise<T> {
    return await firstValueFrom(
      this.http.get<T>(`${this.baseUrl}/${endpoint}`, {
        headers: this.defaultHeaders,
        params: new HttpParams({ fromObject: params || {} })
      })
    );
  }

  post<T>(endpoint: string, body: any) {
    const request$ = this.http.post<T>(`${this.baseUrl}/${endpoint}`, body, {
      headers: this.defaultHeaders
    });
    return toSignal(request$, { initialValue: null as unknown as T });
  }

  async postAsync<T>(endpoint: string, body: any): Promise<T> {
    return await firstValueFrom(
      this.http.post<T>(`${this.baseUrl}/${endpoint}`, body, {
        headers: this.defaultHeaders
      })
    );
  }

  put<T>(endpoint: string, body: any) {
    const request$ = this.http.put<T>(`${this.baseUrl}/${endpoint}`, body, {
      headers: this.defaultHeaders
    });
    return toSignal(request$, { initialValue: null as unknown as T });
  }

  async putAsync<T>(endpoint: string, body: any): Promise<T> {
    return await firstValueFrom(
      this.http.put<T>(`${this.baseUrl}/${endpoint}`, body, {
        headers: this.defaultHeaders
      })
    );
  }

  delete<T>(endpoint: string) {
    const request$ = this.http.delete<T>(`${this.baseUrl}/${endpoint}`, {
      headers: this.defaultHeaders
    });
    return toSignal(request$, { initialValue: null as unknown as T });
  }

  async deleteAsync<T>(endpoint: string): Promise<T> {
    return await firstValueFrom(
      this.http.delete<T>(`${this.baseUrl}/${endpoint}`, {
        headers: this.defaultHeaders
      })
    );
  }
}

//  post<T>(endpoint: string, body: any): Observable<T> {
//     return this.http.post<T>(`${this.baseUrl}/${endpoint}`, body, {
//       headers: this.defaultHeaders
//     });
//   }

//   put<T>(endpoint: string, body: any): Observable<T> {
//     return this.http.put<T>(`${this.baseUrl}/${endpoint}`, body, {
//       headers: this.defaultHeaders
//     });
//   }

//   delete<T>(endpoint: string): Observable<T> {
//     return this.http.delete<T>(`${this.baseUrl}/${endpoint}`, {
//       headers: this.defaultHeaders
//     });
//   }