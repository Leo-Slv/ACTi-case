import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';

import { MaskDirective } from '../../directives/mask.directive';
import { ParceiroService } from '../../services/parceiro.service';
import { ViaCepService } from '../../services/viacep.service';
import { ReceitaWSService } from '../../services/receitaws.service';
import { ValidatorsService } from '../../services/validators.service';
import { Parceiro } from '../../models/parceiro.model';

@Component({
  selector: 'app-parceiro-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule, MaskDirective],
  templateUrl: './parceiro-form.component.html',
  styleUrls: ['./parceiro-form.component.css']
})
export class ParceiroFormComponent implements OnInit {
  parceiroForm: FormGroup;
  loading = false;
  showSuccess = false;
  showError = false;
  errorMessage = '';
  documentoInvalido = false;
  cepInvalido = false;

  constructor(
    private fb: FormBuilder,
    private parceiroService: ParceiroService,
    private viaCepService: ViaCepService,
    private receitaWSService: ReceitaWSService,
    private validatorsService: ValidatorsService
  ) {
    this.parceiroForm = this.fb.group({
      personalidade: ['', Validators.required],
      razaoSocial: ['', Validators.required],
      cnpjCpf: ['', Validators.required],
      cep: ['', Validators.required],
      uf: ['', Validators.required],
      municipio: ['', Validators.required],
      logradouro: ['', Validators.required],
      numero: ['', Validators.required],
      bairro: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      telefone: ['', Validators.required],
      complemento: [''],
      observacao: ['']
    });
  }

  ngOnInit(): void {
    this.parceiroForm.get('personalidade')?.valueChanges.subscribe(value => {
      this.onPersonalidadeChange(value);
    });
  }

  onPersonalidadeChange(tipo: string): void {
    this.documentoInvalido = false;
    this.parceiroForm.get('cnpjCpf')?.setValue('');
  }

  consultarDocumento(): void {
    const documentoControl = this.parceiroForm.get('cnpjCpf');
    const personalidadeControl = this.parceiroForm.get('personalidade');
    
    if (!documentoControl?.value || !personalidadeControl?.value) return;

    const documentoLimpo = documentoControl.value.replace(/\D/g, '');

    if (personalidadeControl.value === 'F' && documentoLimpo.length === 11) {
      this.documentoInvalido = !this.validatorsService.validarCPF(documentoLimpo);
    } else if (personalidadeControl.value === 'J' && documentoLimpo.length === 14) {
      this.documentoInvalido = !this.validatorsService.validarCNPJ(documentoLimpo);
      
      if (!this.documentoInvalido) {
        this.loading = true;
        this.receitaWSService.consultarCNPJ(documentoLimpo).subscribe({
          next: (response) => {
            if (response.nome) {
              this.parceiroForm.get('razaoSocial')?.setValue(response.nome);
            }
            this.loading = false;
          },
          error: (error) => {
            console.error('Erro ao consultar CNPJ:', error);
            this.loading = false;
          }
        });
      }
    } else {
      this.documentoInvalido = true;
    }
  }

  consultarCep(): void {
    const cepControl = this.parceiroForm.get('cep');
    
    if (!cepControl?.value) return;

    const cepLimpo = cepControl.value.replace(/\D/g, '');

    if (cepLimpo.length !== 8) {
      this.cepInvalido = true;
      return;
    }

    this.cepInvalido = false;
    this.loading = true;

    this.viaCepService.consultarCep(cepLimpo).subscribe({
      next: (response) => {
        if (response.erro) {
          this.cepInvalido = true;
        } else {
          this.parceiroForm.get('logradouro')?.setValue(response.logradouro);
          this.parceiroForm.get('bairro')?.setValue(response.bairro);
          this.parceiroForm.get('municipio')?.setValue(response.localidade);
          this.parceiroForm.get('uf')?.setValue(response.uf);
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao consultar CEP:', error);
        this.cepInvalido = true;
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.parceiroForm.invalid || this.documentoInvalido || this.cepInvalido) {
      this.showError = true;
      this.errorMessage = 'Por favor, preencha todos os campos obrigatórios corretamente.';
      return;
    }

    this.loading = true;

    const formData: Parceiro = this.parceiroForm.value;
    
    // Limpar dados para envio (remover máscaras)
    formData.cnpjCpf = formData.cnpjCpf.replace(/\D/g, '');
    formData.cep = formData.cep.replace(/\D/g, '');
    formData.telefone = formData.telefone.replace(/\D/g, '');

    this.parceiroService.cadastrarParceiro(formData).subscribe({
      next: (response) => {
        this.loading = false;
        this.showSuccess = true;
        this.showError = false;
        this.parceiroForm.reset();
      },
      error: (error) => {
        this.loading = false;
        this.showSuccess = false;
        this.showError = true;
        
        if (error.error && error.error.mensagem) {
          this.errorMessage = error.error.mensagem;
        } else {
          this.errorMessage = 'Erro ao cadastrar parceiro. Tente novamente.';
        }
      }
    });
  }
}