import { Component, OnInit } from '@angular/core';
import { Store, select } from '@ngrx/store';
import { Observable, Subscription } from 'rxjs';
import { Device, DeviceApiResponse } from '../../../types';
import * as DevicesSelector from '../../state/devices.selector';
import * as DevicesActions from '../../state/devices.actions';
import { HttpClient, HttpResponse } from '@angular/common/http';


@Component({
  selector: 'app-devices',
  standalone: false,
  templateUrl: './devices.component.html',
  styleUrl: './devices.component.css'
})
export class DevicesComponent implements OnInit {
  devices: Device[] = [];
  subscription: Subscription;
  visible: boolean = false;
  newDeviceName: string = "";
  private devicesEndpoint = '/api/Device';

  constructor(private store: Store, private http: HttpClient) {
    this.subscription = this.store
      .pipe(select(DevicesSelector.getDevices))
      .subscribe((devices) => {
        if (!devices || devices.length === 0) {
          this.store.dispatch(DevicesActions.loadDevices());
        } else {
          this.devices = devices;
        }
      });
  }
  
  ngOnInit(): void {
    
  }

  addDevice(): void {
    this.visible = true;
  }

  saveDevice(): void {
    let device: Device = {
      name: this.newDeviceName
    }

    this.http.post<DeviceApiResponse>(`${this.devicesEndpoint}/create`, device, { withCredentials: true, observe: 'response' })
      .subscribe((response: HttpResponse<DeviceApiResponse>) => {
        if (response.ok && response.body) {
          this.devices.concat(response.body.device);
          this.store.dispatch(DevicesActions.addDevice({ device }));
        }
      });
    this.newDeviceName = "";

    this.visible = false;
  }
}
