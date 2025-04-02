// services/data.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Device, User } from '../../types';

@Injectable({ providedIn: 'root' })
export class DataService {
  private devicesEndpoint = '/api/Device';
  private authEndpoint = '/api/Auth';

  constructor(private http: HttpClient) {}

  fetchDevices(): Observable<Device[]> {
    return this.http.get<Device[]>(`${this.devicesEndpoint}/`, { withCredentials: true });
  }

  fetchUser(): Observable<HttpResponse<User>> {
    return this.http.get<User>(`${this.authEndpoint}/me`, { withCredentials: true, observe: 'response' });
  }

  refreshLogin(): Observable<HttpResponse<User>> {
    return this.http.post<User>(`${this.authEndpoint}/refresh`, null, { withCredentials: true, observe: 'response' });
  }

  logout(): Observable<HttpResponse<string>> {
    return this.http.post<string>(`${this.authEndpoint}/logout`, null, { withCredentials: true, observe: 'response' })
  }

  postDevice(device: Device): Observable<Device> {
    return this.http.post<Device>(`${this.devicesEndpoint}/create`, device, { withCredentials: true });
  }
}