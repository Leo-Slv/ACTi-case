export interface Parceiro {
  personalityType: string;     // 'F' ou 'J'
  companyName: string;         // Razão Social
  document: string;            // CNPJ/CPF
  zipCode: string;             // CEP
  state: string;               // UF
  city: string;                // Município
  street: string;              // Logradouro
  number: string;              // Número
  neighborhood: string;        // Bairro
  email: string;               // Email
  phone: string;               // Telefone
  complement?: string;         // Complemento (opcional)
  observation?: string;        // Observação (opcional)
}

export interface ApiResponse {
  success: boolean;
  message: string;
  code?: string;
  details?: any;
}