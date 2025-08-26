import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment'; // Importe o environment

@Injectable({
  providedIn: 'root'
})
export class ViaCepService {
  private apiUrl = environment.viaCepUrl; // Use a URL do environment

  constructor(private http: HttpClient) { }

  consultarCep(cep: string): Observable<any> {
    cep = cep.replace(/\D/g, '');
    return this.http.get(`${this.apiUrl}/${cep}/json/`);
  }
}