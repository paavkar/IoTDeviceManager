import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DevicesComponent } from './pages/devices/devices.component';
import { SingleDeviceViewComponent } from './pages/single-device-view/single-device-view.component';

const routes: Routes = [
  { path: '', redirectTo: '/devices', pathMatch: 'full'  },
  { path: 'devices', component: DevicesComponent, data: { title: 'IDM | Devices'} },
  { path: 'devices/:serialNumber', component: SingleDeviceViewComponent, data: { title: 'IDM | Device'} },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
