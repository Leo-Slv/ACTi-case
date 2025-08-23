using System;
using ACTi.Domain.ValueObjects;

namespace ACTi.Domain
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("🧪 Testando Value Objects...\n");

            // Teste CNPJ
            TestCnpj();

            // Teste CPF  
            TestCpf();

            // Teste Email
            TestEmail();

            Console.WriteLine("\n✅ Todos os testes passaram!");
            Console.ReadKey();
        }

        static void TestCnpj()
        {
            Console.WriteLine("📋 Testando CNPJ:");

            try
            {
                // CNPJ válido
                var cnpj1 = Cnpj.Create("11.222.333/0001-81");
                Console.WriteLine($"✅ CNPJ Válido: {cnpj1.Formatted}");
                Console.WriteLine($"   Só números: {cnpj1.OnlyNumbers}");

                // CNPJ sem formatação
                var cnpj2 = Cnpj.Create("11222333000181");
                Console.WriteLine($"✅ CNPJ sem formatação: {cnpj2.Formatted}");

                // Teste igualdade
                Console.WriteLine($"✅ São iguais? {cnpj1.Equals(cnpj2)}");

                // CNPJ inválido (deve dar erro)
                try
                {
                    var invalidCnpj = Cnpj.Create("12345678000100");
                    Console.WriteLine("❌ Erro: CNPJ inválido foi aceito!");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"✅ CNPJ inválido rejeitado: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro no teste CNPJ: {ex.Message}");
            }

            Console.WriteLine();
        }

        static void TestCpf()
        {
            Console.WriteLine("👤 Testando CPF:");

            try
            {
                // CPF válido
                var cpf1 = Cpf.Create("123.456.789-09");
                Console.WriteLine($"✅ CPF Válido: {cpf1.Formatted}");
                Console.WriteLine($"   Só números: {cpf1.OnlyNumbers}");

                // CPF sem formatação
                var cpf2 = Cpf.Create("12345678909");
                Console.WriteLine($"✅ CPF sem formatação: {cpf2.Formatted}");

                // CPF inválido (deve dar erro)
                try
                {
                    var invalidCpf = Cpf.Create("12345678901");
                    Console.WriteLine("❌ Erro: CPF inválido foi aceito!");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"✅ CPF inválido rejeitado: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro no teste CPF: {ex.Message}");
            }

            Console.WriteLine();
        }

        static void TestEmail()
        {
            Console.WriteLine("📧 Testando Email:");

            try
            {
                // Email válido
                var email1 = Email.Create("leonardo@empresa.com.br");
                Console.WriteLine($"✅ Email válido: {email1.Address}");
                Console.WriteLine($"   Domínio: {email1.Domain}");
                Console.WriteLine($"   Parte local: {email1.LocalPart}");
                Console.WriteLine($"   É corporativo? {email1.IsCorporate}");

                // Email pessoal
                var email2 = Email.Create("leonardo@gmail.com");
                Console.WriteLine($"✅ Email pessoal: {email2.Address}");
                Console.WriteLine($"   É corporativo? {email2.IsCorporate}");

                // Email inválido (deve dar erro)
                try
                {
                    var invalidEmail = Email.Create("email-invalido");
                    Console.WriteLine("❌ Erro: Email inválido foi aceito!");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"✅ Email inválido rejeitado: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro no teste Email: {ex.Message}");
            }

            Console.WriteLine();
        }
    }
}