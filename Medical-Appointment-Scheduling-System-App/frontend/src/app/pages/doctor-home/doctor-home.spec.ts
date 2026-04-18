import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DoctorHome } from './doctor-home';

describe('DoctorHome', () => {
  let component: DoctorHome;
  let fixture: ComponentFixture<DoctorHome>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorHome]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DoctorHome);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
