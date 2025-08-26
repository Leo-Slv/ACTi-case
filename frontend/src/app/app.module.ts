import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { ParceiroFormComponent } from './components/parceiro-form/parceiro-form.component';
import { MaskDirective } from './directives/mask.directive';

@NgModule({
  declarations: [
    AppComponent,
    ParceiroFormComponent,
    MaskDirective
  ],
  imports: [
    BrowserModule,
    ReactiveFormsModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }