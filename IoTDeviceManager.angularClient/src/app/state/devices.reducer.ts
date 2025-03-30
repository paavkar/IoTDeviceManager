import { createReducer, on } from '@ngrx/store';
import { initialDevicesState } from './devices.state';
import * as DevicesActions from './devices.actions';

export const devicesReducer = createReducer(
    initialDevicesState,
  on(DevicesActions.loadDevices, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(DevicesActions.unloadDevices, (state) => ({
    data: null,
    loading: false,
    error: null
  })),
  on(DevicesActions.loadDevicesSuccess, (state, { devices }) => ({
    ...state,
    data: devices,
    loading: false
  })),
  on(DevicesActions.loadDevicesFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),
  on(DevicesActions.addDevice, (state, { device }) => ({
    ...state,
    data: state.data?.concat(device)
  }))
);