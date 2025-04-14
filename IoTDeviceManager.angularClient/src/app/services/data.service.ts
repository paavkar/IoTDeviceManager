// services/data.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CDevice, DevicesApiResponse, DeviceApiResponse, User, CommandRequest, CommandResponse } from '../../types';

@Injectable({ providedIn: 'root' })
export class DataService {
  private devicesEndpoint = '/api/v2/Device';
  private authEndpoint = '/api/v1/Auth';

  constructor(private http: HttpClient) {}

  fetchDevices(): Observable<DevicesApiResponse> {
    return this.http.get<DevicesApiResponse>(`${this.devicesEndpoint}/`, { withCredentials: true });
  }

  fetchUserDevices(): Observable<HttpResponse<DevicesApiResponse>> {
    return this.http.get<DevicesApiResponse>(`${this.devicesEndpoint}/`, { withCredentials: true, observe: 'response' });
  }

  fetchDevice(serialNumber: string): Observable<HttpResponse<DeviceApiResponse>> {
    return this.http.get<DeviceApiResponse>(`${this.devicesEndpoint}/${serialNumber}`, { withCredentials: true, observe: 'response' });
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

  postDevice(device: CDevice): Observable<CDevice> {
    return this.http.post<CDevice>(`${this.devicesEndpoint}/create`, device, { withCredentials: true });
  }
  
  postConfigCommand(serialNumber: string, command: CommandRequest): Observable<HttpResponse<CommandResponse>> {
    return this.http.post<CommandResponse>(`${this.devicesEndpoint}/send-command/${serialNumber}`, command, { withCredentials: true, observe: 'response' });
  }
}