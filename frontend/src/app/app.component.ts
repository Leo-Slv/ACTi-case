import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ParceiroFormComponent } from './components/parceiro-form/parceiro-form.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ParceiroFormComponent],
  template: '<router-outlet></router-outlet>'
})
export class AppComponent {
  title = 'ACTi - Cadastro de Parceiros';
}