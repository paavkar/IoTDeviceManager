import { Component, OnInit } from '@angular/core';
import { Store, select } from '@ngrx/store';
import { Observable, Subscription } from 'rxjs';
import { Device } from '../../../types';
import * as DevicesSelector from '../../state/devices.selector';
import * as DevicesActions from '../../state/devices.actions';


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

  constructor(private store: Store) {
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
    // if (this.devices$ == null) {
    //   this.store.dispatch(DevicesActions.loadDevices())
    // }
  }

  addDevice(): void {
    this.visible = true;
  }

  saveDevice(): void {

    this.visible = false;
  }
}
