import { CDevice } from "../../types";

export interface DevicesState {
    data: CDevice[] | null | undefined;
    loading: boolean;
    error: string | null;
}

export const initialDevicesState: DevicesState = {
    data: null,
    loading: false,
    error: null,
}