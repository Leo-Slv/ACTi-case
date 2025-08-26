import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ParceiroForm } from './parceiro-form.component';

describe('ParceiroForm', () => {
  let component: ParceiroForm;
  let fixture: ComponentFixture<ParceiroForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ParceiroForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ParceiroForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
