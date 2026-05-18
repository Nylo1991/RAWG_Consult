# RAWG_Consult

# 🎮 RAWG Game Explorer

## 📌 Sobre o Projeto

O **RAWG Game Explorer** é uma aplicação desktop desenvolvida em **C# com WPF**, utilizando o padrão **MVVM**, com integração à API pública da **RAWG** para busca de jogos, armazenamento local em **SQLite** e envio dos dados salvos para uma **API externa**.

O sistema permite que o usuário pesquise jogos, visualize informações retornadas pela API, salve jogos selecionados no banco local, consulte os registros salvos, atualize, exclua e envie os dados para uma API de integração.

O projeto foi desenvolvido com foco em aprendizado de:

- Consumo de API REST;
- Aplicação desktop com WPF;
- Organização em camadas;
- Padrão MVVM;
- Persistência local com SQLite;
- CRUD;
- Integração entre APIs;
- Tratamento de erros;
- Serialização JSON;
- Melhorias visuais em XAML.

---

## 🎯 Objetivo do Sistema

O objetivo principal do sistema é criar uma aplicação que realize o fluxo completo de integração entre dados externos, banco local e API externa.

Fluxo principal:

```text
Pesquisar jogo na RAWG
        ↓
Exibir resultado na tela principal
        ↓
Selecionar um jogo
        ↓
Salvar no banco local SQLite
        ↓
Visualizar na tela Meu Banco Local
        ↓
Atualizar, excluir ou enviar para API externa
```

---

## 🧰 Tecnologias Utilizadas

- **C#**
- **.NET 8**
- **WPF**
- **XAML**
- **Entity Framework Core**
- **SQLite**
- **HttpClient**
- **System.Text.Json**
- **RAWG API**
- **API externa**
- **MVVM**
- **Visual Studio**

---

## 🧱 Arquitetura do Projeto

O projeto foi organizado seguindo uma estrutura baseada em separação de responsabilidades.

```text
RawgApi
│
├── Data
│   └── LocalDbContex.cs
│
├── Models
│   ├── Games.cs
│   ├── RawgResponse.cs
│   └── RelayCommand.cs
│
├── Services
│   ├── RawgApiService.cs
│   └── Aluno2ApiService.cs
│
├── ViewModels
│   ├── MainViewModel.cs
│   └── BancoLocalViewModel.cs
│
├── Views
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── BancoLocalWindow.xaml
│   └── BancoLocalWindow.xaml.cs
│
├── App.xaml
└── localdb.db
```

---

## 🧩 Explicação das Camadas

### 📁 Models

A pasta `Models` contém as classes que representam os dados principais da aplicação.

#### `Games.cs`

Representa o jogo utilizado no sistema.

Principais propriedades:

| Propriedade | Descrição |
|---|---|
| `Id` | ID original do jogo vindo da API RAWG |
| `Nome` | Nome do jogo |
| `Descricao` | Descrição do jogo |
| `ImagemUrl` | URL da capa/imagem do jogo |
| `Avaliacao` | Avaliação do jogo |
| `Classificacao` | Nota Metacritic |
| `Upload` | Data de salvamento/envio |
| `IsSelected` | Usado para marcar itens na tabela |
| `DisplayId` | ID sequencial visual usado apenas na tela do banco local |

A propriedade `DisplayId` foi criada para mostrar um ID local simples na tela do banco:

```text
1, 2, 3, 4, 5...
```

Ela não substitui o ID original da RAWG, pois o ID original continua sendo usado internamente para salvar, atualizar, excluir e enviar registros.

---

#### `RawgResponse.cs`

Classe usada para representar a estrutura de resposta da API RAWG.

Ela contém uma lista de jogos retornados pela busca.

---

#### `RelayCommand.cs`

Classe utilizada para implementar comandos no padrão MVVM.

Ela permite ligar os botões da interface aos métodos do ViewModel.

Exemplo:

```csharp
FetchFromRawgCommand = new RelayCommand(async (o) => await ProcurarNaRawg());
```

---

### 📁 Data

#### `LocalDbContex.cs`

Classe responsável pela configuração do banco de dados local usando Entity Framework Core com SQLite.

O banco utilizado é:

```text
localdb.db
```

A tabela principal é:

```text
Games
```

A string de conexão é:

```csharp
Data Source=localdb.db
```

---

### 📁 Services

A pasta `Services` contém as classes responsáveis pela comunicação com APIs externas.

---

#### `RawgApiService.cs`

Responsável por consumir a API RAWG.

Funções principais:

- Montar a URL de busca;
- Enviar requisição HTTP;
- Ler a resposta JSON;
- Converter os dados em objetos `Games`;
- Retornar a lista de jogos para exibição.

A busca utiliza o endpoint da RAWG:

```text
https://api.rawg.io/api/games
```

Exemplo de dados retornados:

- ID;
- Nome;
- Imagem;
- Avaliação;
- Metacritic.

---

#### `Aluno2ApiService.cs`

Responsável por enviar os jogos salvos para a API externa.

URL utilizada no projeto:

```text
https://api-rawg.runasp.net/api/Jogos
```

Durante o desenvolvimento, foi necessário ajustar os dados enviados para o formato esperado pela API externa.

A API esperava:

- `Id` como `string`;
- `Classificacao` como `int`;
- `Upload` como `DateTime` em UTC.

Por isso, antes do envio, o sistema faz conversões como:

```csharp
Id = jogo.Id.ToString();
Classificacao = ConverterClassificacao(jogo.Classificacao);
Upload = ConverterParaUtc(jogo.Upload);
```

---

### 📁 ViewModels

Os ViewModels concentram a lógica das telas.

---

#### `MainViewModel.cs`

Controla a tela principal do sistema.

Responsabilidades:

- Buscar jogos na API RAWG;
- Exibir resultados;
- Selecionar um jogo;
- Salvar jogo selecionado no SQLite;
- Manter a tela principal limpa ao iniciar;
- Exibir mensagens de status.

A tela principal não carrega automaticamente os dados do banco local. Isso foi feito para separar claramente a função da tela principal da função da tela do banco.

---

#### `BancoLocalViewModel.cs`

Controla a tela **Meu Banco de Dados Local**.

Responsabilidades:

- Carregar jogos salvos no SQLite;
- Exibir ID local sequencial;
- Atualizar jogo selecionado;
- Excluir jogo selecionado;
- Excluir jogos marcados;
- Selecionar todos;
- Enviar jogos para API externa;
- Exibir mensagens de status.

A tela do banco local também inicia limpa. Os registros aparecem somente após clicar no botão:

```text
Carregar Banco
```

---

### 📁 Views

A pasta `Views` contém as telas da aplicação.

---

#### `MainWindow.xaml`

Tela principal do sistema.

Contém:

- Campo de pesquisa;
- Botão **Buscar RAWG**;
- Botão **Salvar Selecionado**;
- Botão **Meu Banco Local**;
- Tabela de resultados da busca;
- Área de status;
- Botão de fechar.

Essa tela é usada apenas para buscar jogos na RAWG e salvar o jogo selecionado.

---

#### `BancoLocalWindow.xaml`

Tela responsável pela visualização e gerenciamento dos dados salvos no banco local.

Contém:

- Botão **Carregar Banco**;
- Botão **Atualizar**;
- Botão **Excluir**;
- Botão **Selecionar Todos**;
- Botão **Excluir Marcados**;
- Botão **Enviar API**;
- Tabela com jogos salvos;
- Área de status;
- Botão de fechar.

---

## 🖥️ Telas do Sistema

### Tela Principal

A tela principal é usada para pesquisar jogos na API RAWG.

Funcionalidades:

- Pesquisar jogos;
- Exibir resultados;
- Selecionar um jogo;
- Salvar o jogo selecionado;
- Abrir a tela do banco local.

A tela principal inicia limpa, sem carregar registros do banco automaticamente.

---

### Tela Meu Banco de Dados Local

A tela do banco local é usada para gerenciar os jogos salvos no SQLite.

Funcionalidades:

- Carregar registros salvos;
- Visualizar jogos;
- Atualizar registros;
- Excluir registros;
- Selecionar vários registros;
- Enviar para API externa.

---

## ✅ Funcionalidades Implementadas

### 1. Buscar jogos na API RAWG

O usuário digita o nome de um jogo no campo de busca e clica em:

```text
Buscar RAWG
```

O sistema consulta a API RAWG e exibe os resultados na tabela.

---

### 2. Salvar jogo selecionado

Após a busca, o usuário seleciona um jogo e clica em:

```text
Salvar Selecionado
```

O sistema salva somente o jogo selecionado no banco SQLite.

Foi implementada verificação para evitar registros duplicados.

---

### 3. Tela principal iniciando limpa

Inicialmente, a tela principal carregava os registros salvos automaticamente.

Isso foi alterado.

Agora a tela principal inicia vazia, exibindo apenas a mensagem:

```text
Digite o nome de um jogo para pesquisar.
```

---

### 4. Tela de banco local separada

Foi criada uma segunda tela chamada:

```text
Meu Banco de Dados Local
```

Essa tela centraliza as operações sobre os dados salvos localmente.

---

### 5. Carregar banco local

Na tela do banco local, o usuário clica em:

```text
Carregar Banco
```

O sistema carrega os registros salvos no SQLite.

---

### 6. ID local sequencial

Na tela do banco local, o ID exibido é sequencial:

```text
1, 2, 3, 4, 5...
```

Esse ID é apenas visual.

O ID real da RAWG continua armazenado internamente.

---

### 7. Atualizar jogo

O usuário pode selecionar um jogo salvo, editar dados na tabela e clicar em:

```text
Atualizar
```

O sistema atualiza o registro no banco local.

---

### 8. Excluir jogo

O usuário pode selecionar um jogo e clicar em:

```text
Excluir
```

Antes da exclusão, o sistema exibe uma confirmação.

---

### 9. Excluir jogos marcados

O usuário pode marcar vários jogos e clicar em:

```text
Excluir Marcados
```

O sistema remove todos os jogos marcados após confirmação.

---

### 10. Selecionar todos

O botão:

```text
Selecionar Todos
```

marca ou desmarca todos os itens da tabela.

---

### 11. Enviar para API externa

Na tela do banco local, o usuário pode marcar jogos e clicar em:

```text
Enviar API
```

O sistema envia os jogos selecionados para a API externa.

O envio foi ajustado para seguir o formato esperado pela API.

---

## 🔄 Fluxo de Funcionamento

### Fluxo de busca e salvamento

```text
Usuário digita o nome do jogo
        ↓
Clica em Buscar RAWG
        ↓
Sistema consulta a API RAWG
        ↓
Resultados aparecem na tabela
        ↓
Usuário seleciona um jogo
        ↓
Clica em Salvar Selecionado
        ↓
Sistema salva no SQLite
```

---

### Fluxo do banco local

```text
Usuário clica em Meu Banco Local
        ↓
Abre a tela do banco local
        ↓
Usuário clica em Carregar Banco
        ↓
Sistema exibe jogos salvos
        ↓
Usuário pode atualizar, excluir ou enviar para API
```

---

### Fluxo de envio para API

```text
Usuário abre Meu Banco Local
        ↓
Clica em Carregar Banco
        ↓
Marca os jogos desejados
        ↓
Clica em Enviar API
        ↓
Sistema converte os dados
        ↓
Sistema envia para API externa
        ↓
Sistema mostra sucessos e falhas
```

---

## 🗃️ Banco de Dados

O banco local utilizado é SQLite.

Arquivo gerado:

```text
localdb.db
```

Tabela principal:

```text
Games
```

Campos principais:

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | ID original da RAWG |
| `Nome` | string | Nome do jogo |
| `Descricao` | string | Descrição |
| `ImagemUrl` | string | URL da imagem |
| `Avaliacao` | string | Avaliação |
| `Classificacao` | string | Metacritic |
| `Upload` | DateTime | Data de salvamento |

---

## 🔌 APIs Utilizadas

### RAWG API

API usada para buscar jogos.

Endpoint base:

```text
https://api.rawg.io/api/games
```

Exemplo de busca:

```text
https://api.rawg.io/api/games?key=SUA_CHAVE&search=GTA
```

---

### API externa

API usada para receber os jogos salvos localmente.

Endpoint usado:

```text
https://api-rawg.runasp.net/api/Jogos
```

---

## 📦 Exemplo de JSON enviado para API externa

O sistema envia os dados em formato semelhante a:

```json
{
  "Id": "264734",
  "Nome": "Dragon Ball Z: Budokai Tenkaichi",
  "Descricao": "",
  "ImagemUrl": "https://media.rawg.io/media/screenshots/imagem.jpg",
  "Avaliacao": "3.74",
  "Classificacao": 72,
  "Upload": "2026-05-17T22:57:27Z"
}
```

Observações:

- `Id` é enviado como texto;
- `Classificacao` é enviada como número inteiro;
- `Upload` é enviado em UTC.

---

## ⚙️ Como Executar o Projeto

### Pré-requisitos

Antes de executar, instale:

- Visual Studio 2022 ou superior;
- .NET 8 SDK;
- Workload **Desenvolvimento para Desktop com .NET**;
- Conexão com a internet;
- Chave da API RAWG.

---

### Instalar pacotes NuGet

O projeto utiliza Entity Framework Core com SQLite.

Pacotes necessários:

```text
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Tools
```

Caso seja necessário instalar manualmente, use o Package Manager Console:

```powershell
Install-Package Microsoft.EntityFrameworkCore
Install-Package Microsoft.EntityFrameworkCore.Sqlite
Install-Package Microsoft.EntityFrameworkCore.Tools
```

## 🧪 Como Usar o Sistema

### Pesquisar jogo

1. Abra o programa.
2. Digite o nome do jogo.
3. Clique em **Buscar RAWG**.
4. Aguarde os resultados.

---

### Salvar jogo

1. Pesquise um jogo.
2. Clique em uma linha da tabela.
3. Clique em **Salvar Selecionado**.
4. O jogo será salvo no SQLite.

---

### Visualizar banco local

1. Clique em **Meu Banco Local**.
2. Clique em **Carregar Banco**.
3. Os jogos salvos aparecerão na tabela.

---

### Atualizar jogo

1. Abra **Meu Banco Local**.
2. Clique em **Carregar Banco**.
3. Selecione um jogo.
4. Edite os campos desejados.
5. Clique em **Atualizar**.

---

### Excluir jogo

1. Abra **Meu Banco Local**.
2. Clique em **Carregar Banco**.
3. Selecione um jogo.
4. Clique em **Excluir**.
5. Confirme a exclusão.

---

### Excluir vários jogos

1. Abra **Meu Banco Local**.
2. Clique em **Carregar Banco**.
3. Marque os jogos desejados.
4. Clique em **Excluir Marcados**.
5. Confirme a exclusão.

---

### Enviar jogos para API

1. Abra **Meu Banco Local**.
2. Clique em **Carregar Banco**.
3. Marque os jogos desejados.
4. Clique em **Enviar API**.
5. O sistema mostrará a quantidade de sucessos e falhas.

---

## 🎨 Melhorias Visuais

Foram feitas melhorias na interface:

- Layout escuro;
- Tabelas com faixas escuras;
- Remoção de linhas brancas;
- Imagens menores e mais proporcionais;
- Datas formatadas;
- Botões coloridos por ação;
- Botão de fechar personalizado;
- Melhor organização das colunas;
- Redução da altura das linhas;
- Separação clara entre tela principal e banco local.

---

## 📁 Sugestão de `.gitignore`

Recomenda-se não versionar arquivos temporários, banco local e pastas de build.

Exemplo:

```gitignore
bin/
obj/
.vs/
*.db
*.db-shm
*.db-wal
```

---

## 🔐 Observações de Segurança

A chave da API RAWG não deve ser exposta publicamente em repositórios públicos.

Para projetos futuros, recomenda-se armazenar chaves em:

- Variáveis de ambiente;
- Arquivo de configuração local ignorado pelo Git;
- User Secrets;
- Serviço seguro de configuração.

---

## 🚀 Status do Projeto

O projeto está funcional.

Fluxo completo implementado:

```text
Buscar na RAWG
→ Exibir resultados
→ Salvar jogo selecionado
→ Visualizar no SQLite
→ Atualizar ou excluir
→ Enviar para API externa
```

## 📝 Histórico de Principais Alterações

Durante o desenvolvimento foram realizadas as seguintes alterações:

- Criação da tela principal;
- Consumo da API RAWG;
- Criação do banco SQLite;
- Implementação do salvamento local;
- Criação da tela de banco local;
- Implementação de CRUD;
- Implementação do envio para API externa;
- Correção de erros de validação da API;
- Ajuste de tipo do ID;
- Ajuste de classificação;
- Conversão da data para UTC;
- Ajustes visuais no DataGrid;
- Remoção das faixas brancas;
- Inclusão de ID local sequencial;
- Separação entre busca e banco local.


