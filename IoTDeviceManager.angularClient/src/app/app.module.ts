import { NgModule, ApplicationConfig } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { StoreModule, MetaReducer } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { localStorageSync } from 'ngrx-store-localstorage';

import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';

import { userReducer } from './state/user.reducer';
import { devicesReducer } from './state/devices.reducer'
import { DataEffects } from './state/data.effects';

import { ImportsModule } from './imports';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DevicesComponent } from './pages/devices/devices.component';
import { provideHttpClient } from '@angular/common/http';
import { SingleDeviceViewComponent } from './pages/single-device-view/single-device-view.component';

export function localStorageSyncReducer(reducer: any): any {
  return localStorageSync({
    keys: ['user', 'devices'],
    rehydrate: true,
  })(reducer);
}

export const metaReducers: MetaReducer<any>[] = [localStorageSyncReducer];

@NgModule({
  declarations: [
    AppComponent,
    DevicesComponent,
    SingleDeviceViewComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    ImportsModule,
    StoreModule.forRoot({
      user: userReducer,
      devices: devicesReducer
    }, { metaReducers }),
    EffectsModule.forRoot([DataEffects])
  ],
  providers: [
    provideHttpClient(),
    provideAnimationsAsync(),
    providePrimeNG({
        theme: {
            preset: Aura,
            options: {
                darkModeSelector: '.idm-dark',
                name: 'primeng',
                order: 'theme, base, primeng'
            }
        }
    })
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
