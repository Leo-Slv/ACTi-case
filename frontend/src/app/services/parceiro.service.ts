import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Parceiro, ApiResponse } from '../models/parceiro.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ParceiroService {
  private apiUrl = `${environment.apiUrl}/Parceiros`;

  constructor(private http: HttpClient) { }

  cadastrarParceiro(parceiro: Parceiro): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(this.apiUrl, parceiro);
  }
}