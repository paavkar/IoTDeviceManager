import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
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
  devices$: Observable<Device[] | null>;

  constructor(private store: Store) {
    this.devices$ = store.select(DevicesSelector.getDevices)
  }
  
  ngOnInit(): void {
    // if (this.devices$ == null) {
    //   this.store.dispatch(DevicesActions.loadDevices())
    // }
  }
}
