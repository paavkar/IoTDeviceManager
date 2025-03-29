// services/data.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Device, User } from '../../types';

@Injectable({ providedIn: 'root' })
export class DataService {
  private devicesEndpoint = '/api/Device';
  private userEndpoint = '/api/Auth/me';

  constructor(private http: HttpClient) {}

  fetchDevices(): Observable<Device[]> {
    return this.http.get<Device[]>(`${this.devicesEndpoint}/`, { withCredentials: true });
  }

  fetchUser(): Observable<HttpResponse<User>> {
    return this.http.get<User>(this.userEndpoint, { withCredentials: true, observe: 'response' });
  }

  postDevice(device: Device): Observable<Device> {
    return this.http.post<Device>(`${this.devicesEndpoint}/create`, device, { withCredentials: true });
  }
}