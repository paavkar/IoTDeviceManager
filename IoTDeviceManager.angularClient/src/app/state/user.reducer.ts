import { createReducer, on } from '@ngrx/store';
import { initialUserState } from './user.state';
import * as UserActions from './user.actions';

export const userReducer = createReducer(
  initialUserState,
  on(UserActions.loadUser, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(UserActions.logout, (state) => ({
    data: null,
    loading: false,
    error: null
  })),
  on(UserActions.loadUserSuccess, (state, { user }) => ({
    ...state,
    data: user,
    loading: false,
  })),
  on(UserActions.loadUserFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  }))
);