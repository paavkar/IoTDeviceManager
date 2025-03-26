export interface TokenInfo {
    accessTokenExpiresAt: Date;
    accessTokenExpiresIn: number;
    refreshTokenExpiresAt: Date;
    refreshTokenExpiresIn: number;
}

export interface User {
    userName: string;
    email: string;
    roles: string[];
    tokenInfo: TokenInfo;
}

export interface Sensor {
    id: string;
    isOnline: boolean;
    lastConnectionTime: Date;
    measurementType: string;
    unit: string;
    name: string;
    latestReading: string;
    deviceSerialNumber: string;
}

export interface Device {
    id?: string;
    isOnline?: boolean;
    lastConnectionTime?: Date;
    name: string;
    serialNumber?: string;
    userId?: string;
    sensors?: Sensor[];
}

export interface DeviceApiResponse {
    message: string;
    device: Device;
}