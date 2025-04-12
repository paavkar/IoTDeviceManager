import { Component, OnInit , inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { HttpResponse } from '@angular/common/http';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';

import { DataService } from '../../services/data.service';
import { DeviceApiResponse, CDevice } from '../../../types';

@Component({
  selector: 'app-single-device-view',
  standalone: false,
  templateUrl: './single-device-view.component.html',
  styleUrl: './single-device-view.component.css'
})
export class SingleDeviceViewComponent implements OnInit {
  device: CDevice | null = null;
  serialNumber: string | null = null;
  errorMessage: string | null | undefined = null;

  private dataService = inject(DataService)

  constructor(private route: ActivatedRoute, private titleService: Title) {}
  
  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        switchMap(params => {
          const serialNumber = params.get('serialNumber');
          return serialNumber ? this.dataService.fetchDevice(serialNumber) : of(null);
        })
      )
      .subscribe(
        (response: HttpResponse<DeviceApiResponse> | null) => {
          if (response) {
            if (response.ok && response.body) {
              this.device = response.body.device;
              if (this.device.name) {
                this.titleService.setTitle(`IDM | ${this.device.name}`)
              }
            } else {
              this.errorMessage = response.body?.message;
            }
          }
        }
      )
    // this.route.paramMap.subscribe(params => {
    //   const serialNumber = params.get('serialNumber');
    //   if (serialNumber) {
    //     this.dataService.fetchDevice(serialNumber).subscribe(
    //       (response: HttpResponse<DeviceApiResponse>) => {
    //         if (response.ok && response.body) {
    //           this.device = response.body.device;
    //         } else {
    //           this.errorMessage = response.body?.message;
    //         }
    //       }
    //     )
    //   }
    // })
  }
}
