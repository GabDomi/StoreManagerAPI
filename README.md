# 📦 Loja API

API RESTful para gerenciamento de clientes, produtos, fornecedores e vendas. Permite operações como cadastro, listagem, atualização e remoção de entidades, além de controle de estoque e vendas.

## 🚀 Tecnologias Utilizadas

- .NET Core 6
- Entity Framework Core
- MySQL
- JWT (JSON Web Token) para autenticação
- Swagger para documentação de API

## 📌 Instalação

1. Clone o repositório:
   ```sh
   git clone https://github.com/seu-usuario/nome-do-repositorio.git
   cd nome-do-repositorio
   ```

2. Configure a conexão no `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;database=loja;user=root;password=root"
     }
   }
   ```

3. Instale as dependências:
   ```sh
   dotnet restore
   ```

4. Rode as migrações do banco:
   ```sh
   dotnet ef database update
   ```

5. Inicie a API:
   ```sh
   dotnet run
   ```

6. Acesse o **Swagger UI** para explorar a API:
   ```
   http://localhost:5000/swagger/index.html
   ```

## 🔑 Autenticação JWT

1. **Gerar Token**:
   - `POST /login`
   - Body:
   ```json
   {
     "username": "admin",
     "senha": "1029"
   }
   ```
   - Resposta: `JWT Token`

2. **Acessar Endpoints Protegidos**
   - Enviar token no **Header**:
     ```
     Authorization: Bearer <TOKEN>
     ```

## 🏗 Como a Aplicação Funciona

A API permite o gerenciamento de entidades relacionadas a uma loja virtual, incluindo:

### 🔹 Cadastro e Gerenciamento
- **Clientes**: CRUD de clientes, incluindo nome, CPF e e-mail.
- **Produtos**: Cadastro de produtos, incluindo nome, preço e fornecedor.
- **Fornecedores**: Registro de fornecedores com CNPJ e contato.
- **Depósitos**: Controle de estoque por depósitos.
- **Vendas**: Registro de vendas, vinculando clientes e produtos.

### 🔹 Relacionamentos Principais
- Um **cliente** pode fazer várias **compras**.
- Um **produto** pode estar em vários **depósitos**.
- Uma **venda** está associada a um **produto** e a um **cliente**.

## 📌 Endpoints Principais

| Método | Endpoint            | Descrição                  |
|--------|---------------------|----------------------------|
| `POST` | `/login`            | Gera um token JWT         |
| `GET`  | `/produtos`         | Lista todos os produtos   |
| `POST` | `/createcliente`    | Cria um novo cliente      |
| `GET`  | `/clientes`         | Lista todos os clientes   |
| `GET`  | `/clientes/{id}`    | Busca um cliente pelo ID  |
| `PUT`  | `/clientes/{id}`    | Atualiza um cliente       |
| `DELETE` | `/clientes/{id}`  | Remove um cliente        |
| `GET`  | `/fornecedores`     | Lista todos os fornecedores |
| `POST` | `/vendas`           | Registra uma nova venda   |
| `GET`  | `/vendas/produto/detalhada/{produtoId}` | Lista vendas de um produto |
| `GET`  | `/depositos/{depositoId}/produtos` | Lista produtos no depósito |
| `POST` | `/depositos/{depositoId}/produtos/{produtoId}/adicionar` | Adiciona estoque |

## 🎯 Como Testar no Postman

1️⃣ **Autenticação (Gerar Token)**
   - Método: `POST`
   - URL: `http://localhost:5000/login`
   - Body (JSON):
   ```json
   {
     "username": "admin",
     "email": "admin@email.com",
     "senha": "1029"
   }
   ```
   - Resposta:
   ```json
   "eyJhbGciOiJIUzI1..."
   ```

2️⃣ **Requisição Protegida (Exemplo: Buscar Produtos)**
   - Método: `GET`
   - URL: `http://localhost:5000/produtos`
   - Headers:
     - `Authorization: Bearer <TOKEN_AQUI>`

3️⃣ **Criar Cliente**
   - Método: `POST`
   - URL: `http://localhost:5000/createcliente`
   - Body:
   ```json
   {
     "nome": "Gabriele",
     "cpf": "12345678900",
     "email": "gabriele@email.com"
   }
   ```


