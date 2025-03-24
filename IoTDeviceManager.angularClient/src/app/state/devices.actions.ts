import { createAction, props } from '@ngrx/store';
import { Device } from '../../types';

export const loadDevices = createAction('[Devices] Load Devices')
export const unloadDevices = createAction('[Devices] Unload Devices')
export const loadDevicesSuccess = createAction(
    '[Devices] Load Devices Success',
    props<{ devices: Device[] }>()
)
export const loadDevicesFailure = createAction(
    '[Devices] Load Devices Failure',
    props<{ error: string }>()
);

export const addDevice = createAction(
  '[Devices Page] Add Device',
  props<{ device: Device }>()
);

export const addDeviceSuccess = createAction(
  '[Devices API] Add Device Success',
  props<{ device: Device }>()
);

export const addDeviceFailure = createAction(
  '[Devices API] Add Device Failure',
  props<{ error: any }>()
);
  