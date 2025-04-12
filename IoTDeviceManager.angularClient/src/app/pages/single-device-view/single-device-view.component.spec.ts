import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SingleDeviceViewComponent } from './single-device-view.component';

describe('SingleDeviceViewComponent', () => {
  let component: SingleDeviceViewComponent;
  let fixture: ComponentFixture<SingleDeviceViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SingleDeviceViewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SingleDeviceViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
