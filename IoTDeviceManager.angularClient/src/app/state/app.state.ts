import { UserState, initialUserState } from './user.state';
import { DevicesState, initialDevicesState } from './devices.state';

export interface AppState {
  user: UserState;
  devices: DevicesState;
}

export const initialAppState: AppState = {
  user: initialUserState,
  devices: initialDevicesState,
};
