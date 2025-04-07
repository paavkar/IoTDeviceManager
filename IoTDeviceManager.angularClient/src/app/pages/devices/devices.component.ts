import { Component, OnInit } from '@angular/core';
import { Store, select } from '@ngrx/store';
import { Subscription } from 'rxjs';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter, map, mergeMap } from 'rxjs/operators';

import { CDevice, DeviceApiResponse, DevicesApiResponse, User } from '../../../types';
import * as DevicesSelector from '../../state/devices.selector';
import * as DevicesActions from '../../state/devices.actions';
import { DataService } from '../../services/data.service';
import * as UserSelector from '../../state/user.selector';


@Component({
  selector: 'app-devices',
  standalone: false,
  templateUrl: './devices.component.html',
  styleUrl: './devices.component.css'
})
export class DevicesComponent implements OnInit {
  devices: CDevice[] = [];
  subscription: Subscription;
  createDeviceVisible: boolean = false;
  newDeviceName: string = "";
  user: User | null = null;
  
  private dataService = inject(DataService)
  private devicesEndpoint = '/api/v2/Device';

  constructor(private store: Store, private http: HttpClient, private router: Router) {
    this.subscription = this.store
      .pipe(select(DevicesSelector.getDevices))
      .subscribe((devices) => {
        if (!devices) {
          this.store.dispatch(DevicesActions.loadDevices());
        } else {
          this.devices = devices;
        }
      });
    store.select(UserSelector.selectCurrentUser).subscribe(
      (user) => {
        this.user = user;
      }
    )
  }
  
  ngOnInit(): void {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        if (this.user == null) {
          this.store.select(UserSelector.selectCurrentUser).subscribe(
            (user) => {
              this.user = user;
            }
          )
        }
      }
    })
  }

  saveDevice(): void {
    let device: CDevice = {
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

  refreshDevices(): void {
    this.dataService.fetchUserDevices().subscribe(
      (response: HttpResponse<DevicesApiResponse>) => {
        if (response.ok && response.body) {
          this.store.dispatch(DevicesActions.loadDevicesSuccess({ devices: response.body.devices }))
          this.devices = response.body.devices
        }
      }
    )
  }
}
