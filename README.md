# BarelyBank: Simulação de Banco Digital

O BarelyBank é um sistema de simulação de banco digital desenvolvido para demonstrar conceitos-chave de Programação Orientada a Objetos, modelagem de dados e arquitetura de software moderna. O sistema gerencia clientes e suas contas bancárias, processa operações financeiras e mantém um registro imutável de todas as transações.

## Tabela de Conteúdos
- [Objetivo](#objetivo)
- [Funcionalidades Principais](#funcionalidades-principais)
- [Destaques da Arquitetura](#destaques-da-arquitetura)
- [Configuração do Banco de Dados](#configuração-do-banco-de-dados)
- [Instalação](#instalação)
- [Executando a Aplicação](#executando-a-aplicação)
- [Imagens e Figuras](#imagens-e-figuras)
- [Testes](#testes)

## Objetivo

Desenvolver um sistema de simulação de banco digital que gerencia clientes e suas contas, realiza operações financeiras e mantém um registro imutável de todas as transações, demonstrando um sólido domínio dos conceitos de POO e modelagem de dados.

## Funcionalidades Principais

- **Gerenciamento de Clientes**: Registra novos clientes com CPF único.
- **Gerenciamento de Contas**: Abre contas correntes ou poupança para clientes existentes.
- **Operações Financeiras**:
  - **Depósito**: Adiciona fundos a uma conta.
  - **Saque**: Retira fundos, com validação de saldo suficiente.
  - **Transferência**: Transfere fundos entre duas contas de forma atômica.
- **Consultas**: Recupera todas as contas de um cliente ou exibe um extrato detalhado de transações para uma conta específica.
- **Autenticação**: Protege endpoints usando JWT, exigindo que os clientes façam login com suas credenciais registradas.

## Destaques da Arquitetura

Este projeto incorpora vários padrões de projeto e boas práticas para garantir uma arquitetura robusta, escalável e de fácil manutenção.

- **Imutabilidade com Records**: Para garantir a integridade e a imutabilidade do extrato bancário, a entidade `Transaction` é implementada como um `record` do C#. Isso assegura que, uma vez criada, uma transação não pode ser alterada, prevenindo inconsistências no histórico financeiro.

- **Padrão de Estratégia (Strategy) para Criação de Contas**: O `AccountService` utiliza o padrão Strategy para selecionar a lógica de criação de conta apropriada. Cada tipo de conta (ex: `CheckingAccount`, `SavingsAccount`) tem sua própria fábrica que implementa `IAccountFactory`. Em tempo de execução, o serviço seleciona a fábrica correta (a "estratégia") com base no tipo de conta solicitado. Este design permite uma fácil extensão — novos tipos de conta podem ser adicionados simplesmente criando uma nova fábrica, sem modificar a camada de serviço.

- **Padrão Unidade de Trabalho (Unit of Work) para Atomicidade**: Para garantir a atomicidade de operações complexas como transferências bancárias, o projeto utiliza o padrão Unit of Work. Ele agrupa múltiplas ações de repositório em uma única unidade transacional. As alterações só são persistidas no banco de dados após a conclusão bem-sucedida de todas as etapas, evitando atualizações parciais e mantendo a consistência dos dados.

- **Tratamento Global de Exceções**: Um middleware centralizado `GlobalExceptionHandler` intercepta exceções lançadas de qualquer camada da aplicação. Ele traduz exceções de domínio (ex: `NotFoundException`, `InsufficientFundsException`) em códigos de status HTTP apropriados, mantendo as actions dos controllers limpas e livres de blocos `try-catch` repetitivos.

- **Autenticação com JWT**: Endpoints selecionados são protegidos para ilustrar conceitos de autenticação. Basicamente toda a ClientController exige autenticação para funcionar apropriadamente. Os clientes devem se registrar e depois fazer login para obter um JWT. Este token deve ser incluído no cabeçalho de autorização para acessar recursos protegidos. Evidentemente, as senhas são armazenadas de forma segura usando um algoritmo de hash. Diferentemente da criptografia, uma senha hasheada não pode ser revertida para seu valor original; além disso, utiliza-se normalmente um salt (um valor aleatório adicionado à senha antes do hash) para impedir ataques com tabelas pré-computadas e garantir que senhas iguais gerem hashes distintos.

## Configuração do Banco de Dados

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

## Instalação

1.  **Clonar o Repositório**:
    ```sh
    git clone <url-do-seu-repositorio>
    cd <diretorio-do-projeto>
    ```

2.  **Aplicar Migrações do Entity Framework**:
    Assim que o contêiner do banco de dados estiver em execução, execute os comandos a seguir a partir do diretório raiz para criar o esquema do banco de dados.

    ```sh
    # Crie uma nova migração se tiver alterações no modelo
    dotnet ef migrations add InitialCreate --project BarelyBank.Infra --startup-project BarelyBank

    # Aplique as migrações ao banco de dados
    dotnet ef database update --project BarelyBank.Infra --startup-project BarelyBank
    ```

## Executando a Aplicação

Para executar a API, navegue até a pasta do projeto `BarelyBank` e execute o comando `dotnet run`.
 ```sh
cd BarelyBank
dotnet run
 ```

Assim a API será iniciada e você poderá acessar a interface do Swagger em `https://localhost:<porta>/swagger/index.html` para explorar e interagir com os endpoints.

## Imagens

<!-- Exemplo de como adicionar uma imagem: -->
<!-- ![Descrição da Imagem](caminho/para/sua/imagem.png) -->

<!-- Exemplo de resultado dos testes: -->
<!-- ![Resultados dos Testes](caminho/para/imagem-testes.png) -->

<!-- Exemplo de cobertura de código: -->
<!-- ![Cobertura de Código](caminho/para/imagem-cobertura.png) -->

## Testes

O projeto inclui um conjunto de testes unitários, focados principalmente nas camadas de aplicação e domínio, para garantir a correção da lógica de negócio. Você pode executar esses testes usando o Gerenciador de Testes do Visual Studio ou a linha de comando: