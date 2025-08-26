import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
  selector: '[appMask]',
  standalone: true
})
export class MaskDirective {
  @Input('appMask') maskType: string = '';

  constructor(private el: ElementRef) {}

  @HostListener('input', ['$event']) onInput(event: any) {
    const value = event.target.value.replace(/\D/g, '');
    
    switch(this.maskType) {
      case 'cpf':
        event.target.value = this.formatCPF(value);
        break;
      case 'cnpj':
        event.target.value = this.formatCNPJ(value);
        break;
      case 'cep':
        event.target.value = this.formatCEP(value);
        break;
      case 'telefone':
        event.target.value = this.formatTelefone(value);
        break;
    }
  }

  private formatCPF(value: string): string {
    if (value.length <= 11) {
      return value
        .replace(/\D/g, '')
        .replace(/(\d{3})(\d)/, '$1.$2')
        .replace(/(\d{3})(\d)/, '$1.$2')
        .replace(/(\d{3})(\d{1,2})/, '$1-$2')
        .replace(/(-\d{2})\d+?$/, '$1');
    }
    return value;
  }

  private formatCNPJ(value: string): string {
    if (value.length <= 14) {
      return value
        .replace(/\D/g, '')
        .replace(/(\d{2})(\d)/, '$1.$2')
        .replace(/(\d{3})(\d)/, '$1.$2')
        .replace(/(\d{3})(\d)/, '$1/$2')
        .replace(/(\d{4})(\d)/, '$1-$2')
        .replace(/(-\d{2})\d+?$/, '$1');
    }
    return value;
  }

  private formatCEP(value: string): string {
    if (value.length <= 8) {
      return value
        .replace(/\D/g, '')
        .replace(/(\d{5})(\d)/, '$1-$2')
        .replace(/(-\d{3})\d+?$/, '$1');
    }
    return value;
  }

  private formatTelefone(value: string): string {
    if (value.length <= 11) {
      if (value.length <= 10) {
        return value
          .replace(/\D/g, '')
          .replace(/(\d{2})(\d)/, '($1) $2')
          .replace(/(\d{4})(\d)/, '$1-$2')
          .replace(/(-\d{4})\d+?$/, '$1');
      } else {
        return value
          .replace(/\D/g, '')
          .replace(/(\d{2})(\d)/, '($1) $2')
          .replace(/(\d{5})(\d)/, '$1-$2')
          .replace('/(-\d{4})\d+?$', '$1');
      }
    }
    return value;
  }
}