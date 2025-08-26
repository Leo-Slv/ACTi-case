export interface Parceiro {
  personalidade: string; // 'F' para Física, 'J' para Jurídica
  razaoSocial: string;
  cnpjCpf: string;
  cep: string;
  uf: string;
  municipio: string;
  logradouro: string;
  numero: string;
  bairro: string;
  email: string;
  telefone: string;
  complemento?: string; // Opcional
  observacao?: string;  // Opcional
}