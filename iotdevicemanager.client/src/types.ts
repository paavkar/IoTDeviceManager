export interface RegisteredUser {
    username: string;
    email: string;
}

export interface IdentityError {
    code: string;
    description: string;
}

export interface HttpErrors {
    errors: IdentityError[];
}

export interface TokenInfo {
    accessTokenExpiresAt: Date;
    accessTokenExpiresIn: number;
    refreshTokenExpiresAt: Date;
    refreshTokenExpiresIn: number;
}

export interface User {
    id: string;
    userName: string;
    email: string;
    roles: string[];
    tokenInfo: TokenInfo;
}