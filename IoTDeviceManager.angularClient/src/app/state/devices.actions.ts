import { createAction, props } from '@ngrx/store';
import { CDevice } from '../../types';

export const loadDevices = createAction('[Devices] Load Devices')
export const unloadDevices = createAction('[Devices] Unload Devices')
export const loadDevicesSuccess = createAction(
    '[Devices] Load Devices Success',
    props<{ devices: CDevice[] }>()
)
export const loadDevicesFailure = createAction(
    '[Devices] Load Devices Failure',
    props<{ error: string }>()
);

export const addDevice = createAction(
  '[Devices Page] Add Device',
  props<{ device: CDevice }>()
);
  