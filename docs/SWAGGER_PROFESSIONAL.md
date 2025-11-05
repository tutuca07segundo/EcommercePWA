# Documentação da API - Kaboom E-commerce

## Visão Geral

API REST completa para gerenciamento de e-commerce de periféricos e peças de computador, desenvolvida em ASP.NET Core 9.0.

---

## Acesso à Documentação Swagger

### URL de Acesso
```
http://localhost:5005/swagger
```

### Iniciar a Aplicação
```powershell
cd c:\Users\Pedro\Desktop\EcommercePWA
dotnet run
```

### Observação Importante sobre Autenticação

O campo "Value" na janela "Available authorizations" **NÃO PRECISA SER PREENCHIDO**.

**Por quê?**
- A autenticação é feita via **sessão HTTP** (cookies)
- O campo "Value" é apenas informativo
- Você não precisa clicar em "Authorize"
- A sessão é criada automaticamente ao fazer login via API

**Como autenticar corretamente:**
1. Use o endpoint `POST /api/AuthApi/login`
2. Envie as credenciais no body da requisição
3. A sessão será criada automaticamente
4. Os próximos requests usarão a sessão via cookies

---

## Autenticação

### Método
Autenticação baseada em sessão HTTP (cookies)

### Credenciais Padrão
```
Email: admin@kaboom.com
Senha: admin123
```

### Fluxo de Autenticação

1. **Login**
   - Endpoint: `POST /api/AuthApi/login`
   - Body:
   ```json
   {
     "email": "admin@kaboom.com",
     "password": "admin123"
   }
   ```
   - Response:
   ```json
   {
     "userId": 1,
     "name": "Administrador",
     "email": "admin@kaboom.com",
     "role": "admin",
     "success": true
   }
   ```

2. **Sessão Automática**
   - A sessão é armazenada em cookies HTTP
   - Timeout: 30 minutos de inatividade
   - Não é necessário enviar tokens ou headers adicionais

3. **Endpoints Protegidos**
   - Endpoints marcados com cadeado requerem login prévio
   - A autenticação é validada automaticamente via sessão

---

## Endpoints Disponíveis

### AuthApi - Autenticação

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| POST | `/api/AuthApi/login` | Realizar login | Não |
| POST | `/api/AuthApi/register` | Registrar novo usuário | Não |

### CartApi - Carrinho de Compras

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| POST | `/api/CartApi/add` | Adicionar produto ao carrinho | Não |
| GET | `/api/CartApi/{sessionId}` | Obter carrinho | Não |
| DELETE | `/api/CartApi/{sessionId}/items/{itemId}` | Remover item | Não |

### ProductApi - Produtos

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/api/ProductApi` | Listar produtos | Não |
| GET | `/api/ProductApi/{id}` | Detalhes do produto | Não |
| GET | `/api/ProductApi/categories` | Listar categorias | Não |

### OrderApi - Pedidos

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/api/OrderApi` | Listar pedidos do usuário | Sim |
| GET | `/api/OrderApi/{id}` | Detalhes do pedido | Sim |
| POST | `/api/OrderApi` | Criar novo pedido | Sim |

### WishlistApi - Lista de Desejos

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/api/WishlistApi` | Listar wishlist | Sim |
| POST | `/api/WishlistApi/add` | Adicionar à wishlist | Sim |
| DELETE | `/api/WishlistApi/{id}` | Remover da wishlist | Sim |

### HomeApi - Página Inicial

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/api/HomeApi/featured` | Produtos em destaque | Não |
| GET | `/api/HomeApi/categories` | Categorias principais | Não |

---

## Exemplos de Requisições

### 1. Login de Usuário

**Request:**
```http
POST /api/AuthApi/login
Content-Type: application/json

{
  "email": "admin@kaboom.com",
  "password": "admin123"
}
```

**Response (200 OK):**
```json
{
  "userId": 1,
  "name": "Administrador",
  "email": "admin@kaboom.com",
  "role": "admin",
  "success": true
}
```

### 2. Buscar Produtos

**Request:**
```http
GET /api/ProductApi?search=mouse&sortBy=price
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Mouse Gamer RGB",
    "price": 149.90,
    "description": "Mouse gamer com iluminação RGB",
    "category": "Periféricos",
    "brand": "Logitech",
    "stock": 50
  }
]
```

### 3. Adicionar ao Carrinho

**Request:**
```http
POST /api/CartApi/add
Content-Type: application/json

{
  "sessionId": "abc123",
  "productId": 1,
  "quantity": 2,
  "variationId": null
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Produto adicionado ao carrinho"
}
```

### 4. Obter Detalhes de Produto

**Request:**
```http
GET /api/ProductApi/1
```

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Mouse Gamer RGB",
  "price": 149.90,
  "description": "Mouse gamer com iluminação RGB e 7 botões programáveis",
  "category": "Periféricos",
  "brand": "Logitech",
  "stock": 50,
  "weight": 120,
  "width": 12,
  "height": 4,
  "length": 7,
  "images": ["/images/products/mouse-gamer.jpg"]
}
```

---

## Códigos de Resposta HTTP

| Código | Descrição |
|--------|-----------|
| 200 | Requisição bem-sucedida |
| 400 | Dados inválidos ou erro de validação |
| 401 | Não autenticado (requer login) |
| 404 | Recurso não encontrado |
| 500 | Erro interno do servidor |

---

## Tecnologias Utilizadas

- **Framework:** ASP.NET Core 9.0
- **Linguagem:** C# 12
- **ORM:** Entity Framework Core 9.0
- **Banco de Dados:** SQLite
- **Documentação:** Swagger/OpenAPI 3.0
- **Autenticação:** Session-based (HTTP Cookies)

---

## Integrações Externas

- **Mercado Pago:** Processamento de pagamentos
- **Melhor Envio:** Cálculo de frete
- **ViaCEP:** Validação de CEP
- **SendGrid:** Envio de emails

---

## Ambientes

### Desenvolvimento
```
Aplicação Web:  http://localhost:5005
Swagger UI:     http://localhost:5005/swagger
API Base:       http://localhost:5005/api
Painel Admin:   http://localhost:5005/Admin
```

### Produção
```
(Configurar conforme deployment)
```

---

## Dados de Teste

### Usuários
```
Admin:
  Email: admin@kaboom.com
  Senha: admin123

Usuário Regular:
  Email: user@kaboom.com
  Senha: user123
```

### Cupons de Desconto
```
WELCOME10  - 10% de desconto em qualquer compra
FREESHIP   - Frete grátis
GAMING20   - 20% de desconto em periféricos
```

### Cartões de Teste (Mercado Pago)
```
Aprovado:
  Número: 4509 9535 6623 3704
  CVV: 123
  Validade: 11/25
  Nome: APRO

Negado:
  Número: 4000 0000 0000 0002
  CVV: 123
  Validade: 12/26
  Nome: OTHE
```

---

## Solução de Problemas

### Swagger não carrega
1. Verifique se a aplicação está rodando
2. Acesse primeiro http://localhost:5005
3. Depois acesse http://localhost:5005/swagger

### Endpoint retorna 401 (Não autorizado)
1. Faça login via `POST /api/AuthApi/login`
2. Use as credenciais corretas
3. Certifique-se de que os cookies estão habilitados

### Erro 500 (Erro interno)
1. Verifique os logs da aplicação no console
2. Confirme se o banco de dados está inicializado
3. Valide os dados enviados na requisição

### Campo "Value" na autenticação
- **Não preencha este campo**
- É apenas informativo sobre o tipo de autenticação
- A autenticação real é feita via login na API

---

## Exportar Especificação OpenAPI

Para exportar a especificação completa da API em formato JSON:

```
http://localhost:5005/swagger/v1/swagger.json
```

Este arquivo pode ser importado em ferramentas como:
- Postman
- Insomnia
- API Testing Tools

---

## Suporte

### Contato
- Email: admin@kaboom.com
- GitHub: https://github.com/tutuca07segundo/EcommercePWA

### Documentação Adicional
- Swagger UI: http://localhost:5005/swagger
- README: /docs/README.md
- API Summary: /docs/API_SUMMARY.md

---

## Licença

MIT License - Projeto desenvolvido pela equipe Kaboom Development Team

---

**Última atualização:** Novembro 2025  
**Versão da API:** v2.0
