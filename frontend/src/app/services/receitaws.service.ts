import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment'; // Importe o environment

@Injectable({
  providedIn: 'root'
})
export class ReceitaWSService {
  private apiUrl = environment.receitaWsUrl; // Use a URL do environment

  constructor(private http: HttpClient) { }

  consultarCNPJ(cnpj: string): Observable<any> {
    cnpj = cnpj.replace(/\D/g, '');
    return this.http.get(`${this.apiUrl}/cnpj/${cnpj}`);
  }
}