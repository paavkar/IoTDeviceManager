import { User } from "../../types";

export interface UserState {
    data: User | null;
    loading: boolean;
    error: string | null;
}

export const initialUserState: UserState = {
    data: null,
    loading: false,
    error: null,
}