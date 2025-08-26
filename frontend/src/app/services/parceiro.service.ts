import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Parceiro } from '../models/parceiro.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ParceiroService {
  private apiUrl = `${environment.apiUrl}/Parceiros`;

  constructor(private http: HttpClient) { }

  cadastrarParceiro(parceiro: Parceiro): Observable<any> {
    return this.http.post(this.apiUrl, parceiro);
  }
}