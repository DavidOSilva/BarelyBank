# BarelyBank: Simulação de Banco Digital

O BarelyBank é um sistema de simulação de banco digital desenvolvido para demonstrar conceitos-chave de Programação Orientada a Objetos, modelagem de dados e arquitetura de software moderna. O sistema gerencia clientes e suas contas bancárias, processa operações financeiras e mantém um registro imutável de todas as transações.

## Tabela de Conteúdos
- [🎯 Objetivo](#objetivo)
- [✨ Funcionalidades Principais](#funcionalidades-principais)
- [🖼️ Imagens e Figuras](#imagens-e-figuras)
- [🧩 Particularidades](#particularidades)
- [🗄️ Configuração do Banco de Dados](#configuração-do-banco-de-dados)
- [🚀 Instalação e Execução](#instalação-e-execução)

## 🎯 Objetivo

Desenvolver um sistema de simulação de banco digital que gerencia clientes e suas contas, realiza operações financeiras e mantém um registro imutável de todas as transações, demonstrando um sólido domínio dos conceitos de POO e modelagem de dados.

## ✨ Funcionalidades Principais

- 👤 **Gerenciamento de Clientes**: Cadastro de clientes com CPF único e validação de dados.
- 🏦 **Gerenciamento de Contas**: Abertura de contas correntes e poupança vinculadas a clientes existentes.
- 💸 **Operações Financeiras**:
  - ➕ **Depósito**: Credita valores em uma conta.
  - ➖ **Saque**: Debita valores de uma conta, com validação de saldo disponível.
  - 🔁 **Transferência**: Move fundos entre contas de forma atômica e transacional.
- 🔍 **Consultas**: Recupera todas as contas de um cliente e exibe extratos detalhados com histórico de transações.
- 🔐 **Autenticação**: Protege endpoints usando JWT; clientes se registram e fazem login para obter tokens. Senhas são armazenadas de forma segura (hash + salt).

## 🖼️ Imagens e Figuras

<!-- Coloque imagens relevantes na pasta `docs/images/` e referencie aqui. Exemplos: -->

<!-- ![Resultados dos Testes](docs/images/test-results.png) -->
<!-- ![Cobertura de Código](docs/images/coverage.png) -->

## 🧩 Particularidades

Este projeto incorpora vários padrões de projeto e boas práticas para garantir uma arquitetura robusta, escalável e de fácil manutenção.

- **Imutabilidade com Records**: Para garantir a integridade e a imutabilidade do extrato bancário, a entidade `Transaction` é implementada como um `record` do C#. Isso assegura que, uma vez criada, uma transação não pode ser alterada, prevenindo inconsistências no histórico financeiro.

- **Padrão de Estratégia (Strategy) para Criação de Contas**: O `AccountService` utiliza o padrão Strategy para selecionar a lógica de criação de conta apropriada. Cada tipo de conta (ex: `CheckingAccount`, `SavingsAccount`) tem sua própria fábrica que implementa `IAccountFactory`. Em tempo de execução, o serviço seleciona a fábrica correta (a "estratégia") com base no tipo de conta solicitado. Este design permite uma fácil extensão — novos tipos de conta podem ser adicionados simplesmente criando uma nova fábrica, sem modificar a camada de serviço.

- **Padrão Unidade de Trabalho (Unit of Work) para Atomicidade**: Para garantir a atomicidade de operações complexas como transferências bancárias, o projeto utiliza o padrão Unit of Work. Ele agrupa múltiplas ações de repositório em uma única unidade transacional. As alterações só são persistidas no banco de dados após a conclusão bem-sucedida de todas as etapas, evitando atualizações parciais e mantendo a consistência dos dados.

- **Tratamento Global de Exceções**: Um middleware centralizado `GlobalExceptionHandler` intercepta exceções lançadas de qualquer camada da aplicação. Ele traduz exceções de domínio (ex: `NotFoundException`, `InsufficientFundsException`) em códigos de status HTTP apropriados, mantendo as actions dos controllers limpas e livres de blocos `try-catch` repetitivos.

- **Autenticação com JWT**: Endpoints selecionados são protegidos para ilustrar conceitos de autenticação. Basicamente toda a ClientController exige autenticação para funcionar apropriadamente. Os clientes devem se registrar e depois fazer login para obter um JWT. Este token deve ser incluído no cabeçalho de autorização para acessar recursos protegidos. Evidentemente, as senhas são armazenadas de forma segura usando um algoritmo de hash. Diferentemente da criptografia, uma senha hasheada não pode ser revertida para seu valor original; além disso, utiliza-se normalmente um salt (um valor aleatório adicionado à senha antes do hash) para impedir ataques com tabelas pré-computadas e garantir que senhas iguais gerem hashes distintos.

## 🗄️ Configuração do Banco de Dados

A aplicação utiliza o **SQL Server** como banco de dados. A maneira recomendada de executá-lo para desenvolvimento local é através de um contêiner Docker.

1.  **Execute o SQL Server no Docker**:
    Execute o comando a seguir para iniciar um contêiner do SQL Server. Substitua `SuaSenhaForte123!` por uma senha segura.

    ```sh
    docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SuaSenhaForte123!" \
    -p 1434:1433 --name bb-sql -d mcr.microsoft.com/mssql/server:2022-latest
    ```

2.  **Configure a String de Conexão**:
    Atualize a string de conexão no arquivo `appsettings.Development.json` no projeto `BarelyBank` com as credenciais do seu banco de dados.

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=localhost,1434;Database=BBDb;User=sa;Password=SuaSenhaForte123!;TrustServerCertificate=True;"
      }
    }
    ```

## 🚀 Instalação e Execução

1.  **Clonar o Repositório**:
    ```ps1
    git clone <url-do-seu-repositorio>
    cd <diretorio-do-projeto>
    ```

2.  **Iniciar o banco de dados (Docker)**

    Certifique-se de executar o contêiner do SQL Server (veja a seção "Configuração do Banco de Dados" para o comando de exemplo) antes de aplicar as migrações.

3.  **Aplicar Migrações do Entity Framework**:

    Execute os comandos a seguir a partir do diretório raiz para criar o esquema do banco de dados.

    ```ps1
    # Crie uma nova migração se tiver alterações no modelo
    dotnet ef migrations add InitialCreate --project BarelyBank.Infra --startup-project BarelyBank

    # Aplique as migrações ao banco de dados
    dotnet ef database update --project BarelyBank.Infra --startup-project BarelyBank
    ```

4.  **Executar a API**:

    Navegue até a pasta do projeto `BarelyBank` e execute:

    ```ps1
    cd BarelyBank; dotnet run
    ```

    A API será iniciada e você poderá acessar a interface do Swagger em `https://localhost:<porta>/swagger/index.html` para explorar e interagir com os endpoints.

<!-- Seções detalhadas de testes foram removidas do README; coloque instruções de testes em um arquivo `TESTS.md` se desejar. -->