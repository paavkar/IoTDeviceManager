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