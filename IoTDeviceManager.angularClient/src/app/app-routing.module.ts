import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DevicesComponent } from './pages/devices/devices.component';

const routes: Routes = [
  { path: '', redirectTo: '/devices', pathMatch: 'full'  },
  { path: 'devices', component: DevicesComponent, data: { title: 'IDM | Devices'} },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
