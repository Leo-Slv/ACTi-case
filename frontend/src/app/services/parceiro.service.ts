import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Parceiro } from '../models/parceiro.model';
import { environment } from '../../environments/environment'; // Importe o environment

@Injectable({
  providedIn: 'root'
})
export class ParceiroService {
  private apiUrl = `${environment.apiUrl}/Parceiros`; // Use a URL do environment

  constructor(private http: HttpClient) { }

  cadastrarParceiro(parceiro: Parceiro): Observable<any> {
    return this.http.post(this.apiUrl, parceiro);
  }
}