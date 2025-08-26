import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.css'],
  standalone: false // Adicione esta linha
})
export class AppComponent {
  title = 'ACTi - Cadastro de Parceiros';
}