import { Routes } from '@angular/router';
import { ParceiroFormComponent } from './components/parceiro-form/parceiro-form.component';

export const routes: Routes = [
  { path: '', component: ParceiroFormComponent },
  { path: '**', redirectTo: '' }
];