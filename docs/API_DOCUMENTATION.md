# 📚 Documentação da API - Kaboom E-commerce

## 🌐 Visão Geral

A API REST do Kaboom E-commerce oferece endpoints completos para todas as funcionalidades do sistema, incluindo autenticação, gerenciamento de produtos, carrinho de compras, pedidos, lista de desejos e muito mais.

**Base URL:** `http://localhost:5005/api`  
**Swagger UI:** `http://localhost:5005/swagger`  
**Versão:** v2.0

---

## 🔐 Autenticação

A API utiliza autenticação baseada em sessão. Faça login através do endpoint `/api/AuthApi/login` para obter acesso aos recursos protegidos.

### Endpoints de Autenticação

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/AuthApi/login` | Realizar login |
| `POST` | `/api/AuthApi/register` | Registrar novo usuário |

---

## 📦 Produtos

### Endpoints Principais

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/ProductApi` | Listar todos os produtos |
| `GET` | `/api/ProductApi/{id}` | Obter produto por ID |
| `GET` | `/api/ProductApi/categories` | Listar categorias |

### Busca e Filtros Avançados

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/HomeApi/filter-products` | Filtrar produtos com critérios avançados |
| `GET` | `/api/HomeApi/filter-options` | Obter opções de filtro disponíveis |
| `GET` | `/api/HomeApi/search-suggestions` | Sugestões de busca em tempo real |

#### Exemplo de Filtro de Produtos

```json
POST /api/HomeApi/filter-products
{
  "searchTerm": "mouse",
  "categories": ["Periféricos"],
  "brands": ["Logitech", "Razer"],
  "minPrice": 50,
  "maxPrice": 300,
  "minRating": 4.0,
  "inStockOnly": true,
  "onSaleOnly": false,
  "sortBy": "price_asc",
  "page": 1,
  "pageSize": 12
}
```

---

## 🛒 Carrinho de Compras

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/CartApi/add` | Adicionar produto ao carrinho |
| `GET` | `/api/CartApi/{sessionId}` | Obter carrinho por sessão |
| `DELETE` | `/api/CartApi/{sessionId}/items/{itemId}` | Remover item do carrinho |

#### Exemplo de Adicionar ao Carrinho

```json
POST /api/CartApi/add
{
  "sessionId": "abc123",
  "productId": 1,
  "quantity": 2,
  "variationId": 5
}
```

---

## 📋 Pedidos

### Endpoints de Pedidos

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/OrderApi/user/{userId}` | Pedidos do usuário |
| `GET` | `/api/OrderApi/{orderId}` | Detalhes do pedido |
| `PUT` | `/api/OrderApi/{orderId}/status` | Atualizar status |
| `PUT` | `/api/OrderApi/{orderId}/payment-status` | Atualizar pagamento |
| `PUT` | `/api/OrderApi/{orderId}/tracking` | Adicionar rastreamento |
| `POST` | `/api/OrderApi/{orderId}/cancel` | Cancelar pedido |
| `GET` | `/api/OrderApi/statistics` | Estatísticas (admin) |

#### Exemplo de Resposta de Pedido

```json
{
  "success": true,
  "order": {
    "id": 123,
    "orderNumber": "KAB-2025-001",
    "status": "Processando",
    "paymentStatus": "Pago",
    "paymentMethod": "Cartão de Crédito",
    "totalAmount": 299.90,
    "items": [
      {
        "productName": "Mouse Gamer RGB",
        "quantity": 1,
        "unitPrice": 149.90,
        "totalPrice": 149.90
      }
    ]
  }
}
```

---

## ❤️ Lista de Desejos

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/WishlistApi/user/{userId}` | Obter wishlist do usuário |
| `POST` | `/api/WishlistApi/add` | Adicionar à wishlist |
| `DELETE` | `/api/WishlistApi/remove` | Remover da wishlist |
| `GET` | `/api/WishlistApi/check` | Verificar se está na wishlist |
| `DELETE` | `/api/WishlistApi/clear/{userId}` | Limpar wishlist |
| `GET` | `/api/WishlistApi/count/{userId}` | Contagem de itens |

---

## 🤖 Chatbot

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/Chatbot/message` | Enviar mensagem |
| `GET` | `/api/Chatbot/welcome` | Mensagem de boas-vindas |
| `GET` | `/api/Chatbot/suggestions` | Sugestões de perguntas |
| `GET` | `/api/Chatbot/history/{sessionId}` | Histórico da conversa |

#### Exemplo de Mensagem para Chatbot

```json
POST /api/Chatbot/message
{
  "message": "Quais são as formas de pagamento?",
  "sessionId": "chat_abc123"
}
```

---

## 💳 Mercado Pago Webhook

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/MercadoPagoWebhook` | Health check |
| `POST` | `/api/MercadoPagoWebhook/ReceiveWebhook` | Receber notificações |

---

## 📊 Códigos de Status HTTP

| Código | Descrição |
|--------|-----------|
| `200` | Sucesso |
| `400` | Requisição inválida |
| `401` | Não autorizado |
| `404` | Não encontrado |
| `500` | Erro interno do servidor |

---

## 🔧 Exemplos de Uso

### 1. Fluxo Completo de Compra

```javascript
// 1. Fazer login
const loginResponse = await fetch('/api/AuthApi/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'password123'
  })
});

// 2. Buscar produtos
const products = await fetch('/api/ProductApi?search=mouse');

// 3. Adicionar ao carrinho
await fetch('/api/CartApi/add', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    sessionId: 'session123',
    productId: 1,
    quantity: 1
  })
});

// 4. Obter carrinho
const cart = await fetch('/api/CartApi/session123');
```

### 2. Gerenciar Lista de Desejos

```javascript
// Adicionar à wishlist
await fetch('/api/WishlistApi/add', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    userId: 123,
    productId: 456
  })
});

// Verificar se está na wishlist
const isInWishlist = await fetch('/api/WishlistApi/check?userId=123&productId=456');
```

### 3. Usar Chatbot

```javascript
// Enviar mensagem para o chatbot
const response = await fetch('/api/Chatbot/message', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    message: 'Como faço para rastrear meu pedido?',
    sessionId: 'chat_session_123'
  })
});
```

---

## 🛠️ Ferramentas de Desenvolvimento

### Swagger UI
Acesse `http://localhost:5005/swagger` para:
- Visualizar todos os endpoints
- Testar APIs interativamente
- Ver modelos de dados
- Baixar especificação OpenAPI

### Postman Collection
Importe a especificação OpenAPI no Postman para testes automatizados.

### cURL Examples

```bash
# Login
curl -X POST "http://localhost:5005/api/AuthApi/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@kaboom.com","password":"admin123"}'

# Buscar produtos
curl -X GET "http://localhost:5005/api/ProductApi?search=mouse"

# Adicionar ao carrinho
curl -X POST "http://localhost:5005/api/CartApi/add" \
  -H "Content-Type: application/json" \
  -d '{"sessionId":"test123","productId":1,"quantity":1}'
```

---

## 📈 Performance e Limites

- **Rate Limiting:** Não implementado (desenvolvimento)
- **Paginação:** Padrão de 12 itens por página
- **Cache:** Implementado para consultas de produtos
- **Timeout:** 30 segundos para requisições

---

## 🔒 Segurança

- **HTTPS:** Recomendado para produção
- **CORS:** Configurado para localhost
- **Validação:** Todos os inputs são validados
- **Sanitização:** Dados são sanitizados antes do processamento

---

## 📞 Suporte

Para dúvidas sobre a API:
- **Email:** admin@kaboom.com
- **Documentação:** `/swagger`
- **Logs:** Verifique os logs da aplicação para debug

---

*Documentação atualizada em: 30/09/2025*
