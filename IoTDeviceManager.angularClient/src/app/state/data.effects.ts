// state/data.effects.ts
import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { mergeMap, map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { Store } from '@ngrx/store';

import * as UserActions from './user.actions';
import * as DevicesActions from './devices.actions';
import { DataService } from '../services/data.service';
import { AppState } from './app.state';

@Injectable()
export class DataEffects {

  constructor(private actions$: Actions, private dataService: DataService) {}

  loadDevices$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DevicesActions.loadDevices),
      mergeMap(() =>
        this.dataService.fetchDevices().pipe(
          map((devices) => DevicesActions.loadDevicesSuccess({ devices })),
          catchError((error) =>
            of(DevicesActions.loadDevicesFailure({ error: error.message }))
          )
        )
      )
    )
  );

  loadUser$ = createEffect(() => 
    this.actions$.pipe(
      ofType(UserActions.loadUser),
      mergeMap(() =>
        this.dataService.fetchUser().pipe(
          map((user) => UserActions.loadUserSuccess({ user })),
          catchError((error) =>
            of(UserActions.loadUserFailure({ error: error.message }))
          )
        )
      )
    )
  );

  addDevice$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DevicesActions.addDevice),
      mergeMap(action =>
        this.dataService.postDevice(action.device).pipe(
          map((device) => DevicesActions.addDeviceSuccess({ device })),
          catchError(error => of(DevicesActions.addDeviceFailure({ error })))
        )
      )
    )
  );

}