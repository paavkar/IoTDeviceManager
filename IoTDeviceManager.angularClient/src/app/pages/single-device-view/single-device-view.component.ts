import { Component, OnInit , inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { HttpResponse } from '@angular/common/http';
import { switchMap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

import { DataService } from '../../services/data.service';
import { DeviceApiResponse, CDevice, CommandRequest, CommandResponse, CommandType } from '../../../types';

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
  statusCodeAndText: string | null = null;
  deviceLoading: boolean = true;
  deviceConfig: CommandRequest = {
    commandType: CommandType.ResetWiFi,
    value: undefined
  }
  CommandType = CommandType;
  commandOptions = [
    { label: 'Reset WiFi', value: CommandType.ResetWiFi },
    { label: 'Set Sensor Measurement Frequency', value: CommandType.SetFrequency }
  ];

  private dataService = inject(DataService)

  constructor(private route: ActivatedRoute, private titleService: Title) {}
  
  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        switchMap(params => {
          const serialNumber = params.get('serialNumber');
          return serialNumber ? this.dataService.fetchDevice(serialNumber) : of(null);
        }),
        catchError(error => {
          this.errorMessage = error.error?.message;
          if (error.status == 404) {
            this.statusCodeAndText = `${error.status} Not Found`;
          }

          return of(null);
        })
      )
      .subscribe(
        (response: HttpResponse<DeviceApiResponse> | null) => {
          if (response && response.ok && response.body) {
            this.device = response.body.device;
            if (this.device.name) {
              this.titleService.setTitle(`IDM | ${this.device.name}`)
            }
          }
        }
      )
    this.deviceLoading = false;
  }

  sendCommandRequest(): void {
    if (this.deviceConfig.commandType === CommandType.ResetWiFi) {
      this.dataService.postConfigCommand(this.device?.serialNumber!, this.deviceConfig).subscribe(
        (response: HttpResponse<CommandResponse>) => {
          if (response.ok && response.body) {
            console.log(response.body.message);
          }
        }
      );
    } else if (this.deviceConfig.value! > 0) {
      this.dataService.postConfigCommand(this.device?.serialNumber!, this.deviceConfig).subscribe(
        (response: HttpResponse<CommandResponse>) => {
          if (response.ok && response.body) {
            console.log(response.body.message);
          }
        }
      );
    }
    this.deviceConfig.value = undefined;
  }
}
