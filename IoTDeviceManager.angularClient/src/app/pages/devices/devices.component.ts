import { Component, OnInit } from '@angular/core';
import { Store, select } from '@ngrx/store';
import { Subscription } from 'rxjs';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

import { Device, DeviceApiResponse, User } from '../../../types';
import * as DevicesSelector from '../../state/devices.selector';
import * as DevicesActions from '../../state/devices.actions';
import * as UserActions from '../../state/user.actions';
import { DataService } from '../../services/data.service';
import * as UserSelector from '../../state/user.selector';


@Component({
  selector: 'app-devices',
  standalone: false,
  templateUrl: './devices.component.html',
  styleUrl: './devices.component.css'
})
export class DevicesComponent implements OnInit {
  devices: Device[] = [];
  subscription: Subscription;
  createDeviceVisible: boolean = false;
  newDeviceName: string = "";
  user: User | undefined | null;
  
  private dataService = inject(DataService)
  private devicesEndpoint = '/api/Device';

  constructor(private store: Store, private http: HttpClient, private router: Router) {
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
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.store.select(UserSelector.selectCurrentUser).subscribe(
          (user) => {
            this.user = user;
          }
        )
      }
    })
  }

  saveDevice(): void {
    let device: Device = {
      name: this.newDeviceName,
      userId: this.user?.id
    }

    this.http.post<DeviceApiResponse>(`${this.devicesEndpoint}/create`, device, { withCredentials: true, observe: 'response' })
      .subscribe((response: HttpResponse<DeviceApiResponse>) => {
        if (response.ok && response.body) {
          this.devices.concat(response.body.device);
          this.store.dispatch(DevicesActions.addDevice({ device: response.body.device }));
        }
      });
    this.newDeviceName = "";

    this.createDeviceVisible = false;
  }
}
