import { createFeatureSelector, createSelector } from '@ngrx/store';
import { DevicesState } from './devices.state';

export const selectDevicesState = createFeatureSelector<DevicesState>('devices');

export const getDevices = createSelector(
  selectDevicesState,
  (state) => state.data
);

export const selectDevicesLoading = createSelector(
  selectDevicesState,
  (state) => state.loading
);

export const selectDevicesError = createSelector(
  selectDevicesState,
  (state) => state.error
);