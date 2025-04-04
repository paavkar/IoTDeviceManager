import { Device } from "../../types";

export interface DevicesState {
    data: Device[] | null | undefined;
    loading: boolean;
    error: string | null;
}

export const initialDevicesState: DevicesState = {
    data: null,
    loading: false,
    error: null,
}