import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ParceiroService } from '../../services/parceiro.service';
import { ViaCepService } from '../../services/viacep.service';
import { ReceitaWSService } from '../../services/receitaws.service';
import { ValidatorsService } from '../../services/validators.service';

@Component({
  selector: 'app-parceiro-form',
  templateUrl: './parceiro-form.component.html',
  styleUrls: ['./parceiro-form.component.css'],
  standalone: false
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
      tipoPessoa: ['', Validators.required],
      documento: ['', Validators.required],
      nome: ['', Validators.required],
      cep: ['', Validators.required],
      telefone: ['', Validators.required],
      logradouro: ['', Validators.required],
      numero: ['', Validators.required],
      bairro: ['', Validators.required],
      cidade: ['', Validators.required],
      estado: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit(): void {
    this.parceiroForm.get('tipoPessoa')?.valueChanges.subscribe(value => {
      this.onTipoPessoaChange(value);
    });
  }

  onTipoPessoaChange(tipo: string): void {
    this.documentoInvalido = false;
    this.parceiroForm.get('documento')?.setValue('');
  }

  consultarDocumento(): void {
  const documentoControl = this.parceiroForm.get('documento');
  const tipoPessoaControl = this.parceiroForm.get('tipoPessoa');
  
  if (!documentoControl?.value || !tipoPessoaControl?.value) return;

  const documentoLimpo = documentoControl.value.replace(/\D/g, '');

  if (tipoPessoaControl.value === 'F' && documentoLimpo.length === 11) {
    this.documentoInvalido = !this.validatorsService.validarCPF(documentoLimpo);
  } else if (tipoPessoaControl.value === 'J' && documentoLimpo.length === 14) {
    this.documentoInvalido = !this.validatorsService.validarCNPJ(documentoLimpo);
    
    if (!this.documentoInvalido) {
      this.loading = true;
      this.receitaWSService.consultarCNPJ(documentoLimpo).subscribe({
        next: (response) => {
          if (response.nome) {
            this.parceiroForm.get('nome')?.setValue(response.nome);
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
    const cep = this.parceiroForm.get('cep')?.value;
    
    if (!cep) return;

    const cepLimpo = cep.replace(/\D/g, '');

    if (cepLimpo.length !== 8) {
      this.cepInvalido = true;
      return;
    }

    this.cepInvalido = false;
    this.loading = true;

    this.viaCepService.consultarCep(cep).subscribe({
      next: (response) => {
        if (response.erro) {
          this.cepInvalido = true;
        } else {
          this.parceiroForm.get('logradouro')?.setValue(response.logradouro);
          this.parceiroForm.get('bairro')?.setValue(response.bairro);
          this.parceiroForm.get('cidade')?.setValue(response.localidade);
          this.parceiroForm.get('estado')?.setValue(response.uf);
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

  const formData = this.parceiroForm.value;
  
  // Limpar dados para envio (remover máscaras)
  formData.documento = formData.documento.replace(/\D/g, '');
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