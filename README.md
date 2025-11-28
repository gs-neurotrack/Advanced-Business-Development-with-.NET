![Logo NeuroTrack](images/logo.jpg)

# NeuroTrack: Sistema de Monitoramento de Estresse e Bem-Estar

## Definição do Projeto

### O que é o NeuroTrack?

O NeuroTrack é uma solução tecnológica desenvolvida para monitorar, analisar e prever o nível de estresse de colaboradores em ambientes corporativos.Utilizando dados comportamentais capturados pelo aplicativo mobile como velocidade de uso, quantidade de cliques, double-clicks, tempo ativo, horas trabalhadas e número de reuniões,aliado a uma camada de análise em Python com algoritmos de IA.

O objetivo é prevenir burnout digital, melhorar o bem-estar e apoiar gestores na tomada de decisões relacionadas à saúde mental e produtividade.
---
## 🏗️ Arquitetura e Tecnologia

A NeuroTrack adota uma arquitetura moderna e escalável, utilizando o melhor de cada tecnologia:

* **Backend & Microserviços:** **Java (Spring Boot)** e **C# (.NET 9 Web API)**
* **Mobile:** **React Native**
* **Banco de Dados:** **Oracle DB**
* **Cloud:** **Oracle Cloud Infrastructure**

A API em **.NET 9** segue uma arquitetura em **camadas**, com separação clara entre:
- **Domain Model (Entities)** → classes de domínio do banco Oracle.  
- **Repositories** → acesso a dados via Entity Framework Core.  
- **Services** → lógica de negócio, validações e tratamento de exceções.  
- **Controllers** → endpoints RESTful com suporte a **HATEOAS** (nível 3).  
- **DTOs (Data Transfer Objects)** → isolamento das entidades para transporte seguro de dados.  
---

## 🤝 Integrantes do Projeto

| Nome                                  | Função no Projeto          | LinkedIn | GitHub |
|---------------------------------------|----------------------------|----------|--------|
| Cleyton Enrike de Oliveira            | Desenvolvedor .NET & IOT   | [LinkedIn](https://www.linkedin.com/in/cleyton-enrike-de-oliveira99) | [@Cleytonrik99](https://github.com/Cleytonrik99) |
| Matheus Henrique Nascimento de Freitas| Desenvolvedor Mobile & DBA | [LinkedIn](https://www.linkedin.com/in/matheus-henrique-freitas)     | [@MatheusHenriqueNF](https://github.com/MatheusHenriqueNF) |
| Pedro Henrique Sena                   | Desenvolvedor Java & DevOps| [LinkedIn](https://www.linkedin.com/in/pedro-henrique-sena)          | [@devpedrosena1](https://github.com/devpedrosena1) |

---

## Escopo 

O NeuroTrack será uma solução Full-stack, utilizando Oracle Database para o armazenamento dos dados, React Native para a interface móvel, Java e DotNet para backend e microserviços. O sistema terá as seguintes funcionalidades principais:

### Funcionalidades Principais

1. **Alertas Inteligentes**:
   - Notificações em tempo real sobre sua saúde mental.

2. **Relatórios Gerenciais**:
   - Dashboards para acompanhamento da sua sáude mental.

---

## Requisitos Funcionais e Não Funcionais

### Requisitos Funcionais

1. **Cadastro de Usuários**
2. **Login de Usuários**
3. **Contabilizar Clicks**
4. **Contabilizar Double-clicks**
5. **Medir e Registrar Comportamento do Usuário**

### Requisitos Não Funcionais

- **Desempenho e Escalabilidade**
- **Segurança e Manutenibilidade**
- **Compatibilidade entre Plataformas**
- **Usabilidade e Responsividade**

---

# 📡 API MedSave — Endpoints e Exemplos  
> Por padrão, a API roda em **http://localhost:5162**

---
# 🧠 **Daily Logs — `/api/GsDailyLogs`**

| Método | Endpoint | Descrição | Corpo da Requisição (JSON) | Resposta Esperada |
|--------|-----------|------------|-----------------------------|-------------------|
| **GET** | `/api/GsDailyLogs` | Retorna todos os logs diários (com HATEOAS). | — | 200 OK com coleção + links. |
| **GET** | `/api/GsDailyLogs/{id}` | Retorna um log específico. | — | 200 OK ou 404 Not Found. |
| **POST** | `/api/GsDailyLogs` | Cria um novo registro de atividade diária. | `{ "workHours": 9, "meetings": 3, "idUser": 12 }` | 201 Created (objeto + links). |
| **DELETE** | `/api/GsDailyLogs/{id}` | Remove um log existente. | — | 200 OK (mensagem + links). |
| **GET** | `/api/GsDailyLogs/search` | Busca logs diários com filtros e paginação. | — | 200 OK com `PagedResult` + links. |

---

# 🔐 **Limits — `/api/GsLimits`**

| Método | Endpoint | Descrição | Body | Resposta |
|--------|----------|-----------|-------|----------|
| **GET** | `/api/GsLimits` | Retorna todos os limites configurados (horas e reuniões). | — | 200 OK |
| **GET** | `/api/GsLimits/{id}` | Retorna limite específico. | — | 200 OK ou 404 |
| **POST** | `/api/GsLimits` | Cria novos limites. | `{ "limitHours": 8, "limitMeetings": 5 }` | 201 Created |
| **PUT** | `/api/GsLimits/{id}` | Atualiza limites existentes. | `{ "limitHours": 10, "limitMeetings": 6 }` | 204 No Content |
| **GET** | `/api/GsLimits/search` | Busca limites com filtros e paginação. | — | 200 OK |

---

# 📊 **Scores — `/api/GsScores`**

| Método | Endpoint | Descrição | Body | Resposta |
|--------|----------|-----------|-------|----------|
| **GET** | `/api/GsScores` | Lista todos os scores registrados. | — | 200 OK |
| **GET** | `/api/GsScores/{id}` | Retorna um score específico. | — | 200 OK ou 404 |
| **POST** | `/api/GsScores` | Registra um novo score. | `{ "scoreValue": 72.5, "riskStatusId": 2, "idUser": 12 }` | 201 Created |
| **DELETE** | `/api/GsScores/{id}` | Remove um score pelo ID. | — | 200 OK |
| **GET** | `/api/GsScores/search` | Busca scores com filtros e ordenação. | — | 200 OK com paginação. |

---

# 🔍 **Exemplos de Busca com Filtros (Search)**

## 🧠 Daily Logs — `/api/GsDailyLogs/search`

**Parâmetros suportados:**

- `IdLog` *(long, opcional)*  
- `WorkHours` *(int, opcional)*  
- `IdUser` *(long, opcional)*  
- `page` *(int)*  
- `pageSize` *(int)*  
- `sortBy` *(idLog, workHours, idUser)*  
- `sortDir` *(asc/desc)*  

**Exemplo**

    GET /api/GsDailyLogs/search?IdUser=12&page=1&pageSize=5&sortBy=idLog&sortDir=asc

---

## 📊 Scores — `/api/GsScores/search`

**Parâmetros suportados:**

- `IdUser`
- `RiskStatusId`
- `page`, `pageSize`
- `sortBy`
- `sortDir`

**Exemplo**

    GET /api/GsScores/search?IdUser=12&page=1&pageSize=10

---

## 🔐 Limits — `/api/GsLimits/search`

**Parâmetros suportados:**
- `limitHours`
- `limitMeetings`
- Paginação (`page`, `pageSize`)

**Exemplo**

    GET /api/GsLimits/search?limitHours=8&page=1&pageSize=5

---

# 🧩 **HATEOAS — Exemplo de Resposta Completa**

```json
{
  "data": {
    "idLog": 25,
    "workHours": 9,
    "meetings": 3,
    "idUser": 12
  },
  "_links": [
    { "rel": "self", "href": "/api/GsDailyLogs/25", "method": "GET" },
    { "rel": "delete", "href": "/api/GsDailyLogs/25", "method": "DELETE" },
    { "rel": "list", "href": "/api/GsDailyLogs", "method": "GET" },
    { "rel": "search", "href": "/api/GsDailyLogs/search", "method": "GET" }
  ]
}
```
---
## ⚙️ Como Rodar o Projeto

### Pré-requisitos

1. **.NET 9.0 SDK**
2. **Oracle Database + ODP.NET**
3. **Entity Framework Core com Oracle Provider**
4. **Visual Studio ou Rider (opcional, mas recomendado)**

---

### 🚀 Executando o Projeto

1. **Clone o repositório**
   ```bash
   git clone https://github.com/gs-neurotrack/Advanced-Business-Development-with-.NET.git
   cd Advanced-Business-Development-with-.Net
   ```

2. **Restaure as dependências**
   ```bash
   dotnet restore
   ```

3. **Compile o projeto**
   ```bash
   dotnet build
   ```

4. **Configure a conexão com o banco**
   - No `appsettings.json`, defina:
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "User Id=USUARIO;Password=SENHA;Data Source=HOST:PORTA/SERVICO"
       }
     }
     ```

5. **Atualize o banco de dados (opcional)**
   ```bash
   dotnet ef database update
   ```

6. **Execute o servidor**
   ```bash
   dotnet run
   ```
   O servidor iniciará em:
   ```
   http://localhost:5162
   ```

7. **Acesse o Swagger**
   Abra o navegador e vá até:
   ```
   http://localhost:5162/swagger
   ```
   Lá você poderá **testar todos os endpoints da API**, incluindo `GET`, `POST`, `PUT`, `DELETE` e `SEARCH`.

---
