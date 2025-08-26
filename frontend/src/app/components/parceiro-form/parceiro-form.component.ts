import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';

import { MaskDirective } from '../../directives/mask.directive';
import { ParceiroService } from '../../services/parceiro.service';
import { ViaCepService } from '../../services/viacep.service';
import { ReceitaWSService } from '../../services/receitaws.service';
import { ValidatorsService } from '../../services/validators.service';
import { Parceiro, ApiResponse } from '../../models/parceiro.model';

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
      personalityType: ['', Validators.required],
      companyName: ['', Validators.required],
      document: ['', Validators.required],
      zipCode: ['', Validators.required],
      state: ['', Validators.required],
      city: ['', Validators.required],
      street: ['', Validators.required],
      number: ['', Validators.required],
      neighborhood: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required],
      complement: [''],
      observation: ['']
    });
  }

  ngOnInit(): void {
    this.parceiroForm.get('personalityType')?.valueChanges.subscribe(value => {
      this.onPersonalidadeChange(value);
    });
  }

  onPersonalidadeChange(tipo: string): void {
    this.documentoInvalido = false;
    this.parceiroForm.get('document')?.setValue('');
  }

  consultarDocumento(): void {
    const documentoControl = this.parceiroForm.get('document');
    const personalidadeControl = this.parceiroForm.get('personalityType');
    
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
              this.parceiroForm.get('companyName')?.setValue(response.nome);
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
    const cepControl = this.parceiroForm.get('zipCode');
    
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
          this.parceiroForm.get('street')?.setValue(response.logradouro);
          this.parceiroForm.get('neighborhood')?.setValue(response.bairro);
          this.parceiroForm.get('city')?.setValue(response.localidade);
          this.parceiroForm.get('state')?.setValue(response.uf);
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
    this.showError = false;

    const formData: Parceiro = this.parceiroForm.value;
    
    // Limpar dados para envio (remover máscaras)
    formData.document = formData.document.replace(/\D/g, '');
    formData.zipCode = formData.zipCode.replace(/\D/g, '');
    formData.phone = formData.phone.replace(/\D/g, '');

    this.parceiroService.cadastrarParceiro(formData).subscribe({
      next: (response: ApiResponse) => {
        this.loading = false;
        
        if (response.success) {
          this.showSuccess = true;
          this.showError = false;
          this.parceiroForm.reset();
        } else {
          this.showSuccess = false;
          this.showError = true;
          this.errorMessage = response.message || 'Erro ao cadastrar parceiro.';
        }
      },
      error: (error) => {
        this.loading = false;
        this.showSuccess = false;
        this.showError = true;
        
        if (error.error && error.error.message) {
          this.errorMessage = error.error.message;
        } else {
          this.errorMessage = 'Erro ao cadastrar parceiro. Tente novamente.';
        }
      }
    });
  }
}