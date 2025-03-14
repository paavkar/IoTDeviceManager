import { NgModule, ApplicationConfig } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

import { userReducer } from './state/user.reducer';
import { devicesReducer } from './state/devices.reducer'
import { DataEffects } from './state/data.effects';

import { Menubar, MenubarModule } from 'primeng/menubar';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DevicesComponent } from './pages/devices/devices.component';

@NgModule({
  declarations: [
    AppComponent,
    DevicesComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    PanelModule,
    ButtonModule,
    Menubar,
    StoreModule.forRoot({
      user: userReducer,
      devices: devicesReducer
    }),
    EffectsModule.forRoot([DataEffects])
  ],
  providers: [ ],
  bootstrap: [AppComponent]
})
export class AppModule { }
