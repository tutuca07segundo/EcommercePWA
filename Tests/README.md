# Testes Automatizados - EcommercePWA

## 📋 Visão Geral

Este projeto de testes foi criado utilizando **xUnit** como framework de testes, **Moq** para mocking de dependências e **Entity Framework Core In-Memory** para testes de banco de dados.

## 🧪 Estrutura dos Testes

```
Tests/
├── Models/              # Testes de modelos de domínio
│   ├── ProductTests.cs
│   └── UserTests.cs
├── Services/            # Testes de serviços de negócio
│   ├── ProductServiceTests.cs
│   ├── AuthServiceTests.cs
│   ├── CartServiceTests.cs
│   └── OrderServiceTests.cs
└── EcommercePWA.Tests.csproj
```

## 🚀 Como Executar os Testes

### Executar todos os testes
```bash
cd Tests
dotnet test
```

### Executar testes com detalhes
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Executar testes específicos
```bash
dotnet test --filter "FullyQualifiedName~ProductServiceTests"
```

### Executar testes com cobertura de código
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Testes Implementados

### ModelTests (8 testes)

#### ProductTests
- `Product_CreateValidProduct_Succeeds` - Verifica criação de produto válido
- `Product_SetDiscount_SetsProperties` - Verifica configuração de desconto
- `Product_VariationsList_Initialized` - Verifica inicialização de variações
- `Product_TagsList_Initialized` - Verifica inicialização de tags

#### UserTests
- `User_CreateValidUser_Succeeds` - Verifica criação de usuário válido
- `User_SetAdminRole_SetsRole` - Verifica configuração de role admin
- `User_DeactivateUser_SetsInactive` - Verifica desativação de usuário
- `User_Collections_Initialized` - Verifica inicialização de coleções

### ServiceTests (31 testes)

#### ProductServiceTests (8 testes)
- `GetAllProductsAsync_ReturnsAllProducts` - Listagem de produtos
- `GetProductByIdAsync_ExistingProduct_ReturnsProduct` - Busca por ID
- `GetProductByIdAsync_NonExistingProduct_ReturnsNull` - Produto inexistente
- `CreateProductAsync_ValidProduct_ReturnsCreatedProduct` - Criação de produto
- `UpdateProductAsync_ExistingProduct_UpdatesProduct` - Atualização de produto
- `DeleteProductAsync_ExistingProduct_DeletesProduct` - Deleção de produto
- `SearchProductsAsync_SearchTerm_ReturnsMatchingProducts` - Busca de produtos
- `GetProductsByCategoryAsync_Category_ReturnsProductsInCategory` - Filtro por categoria

#### AuthServiceTests (11 testes)
- `RegisterAsync_ValidUser_ReturnsUser` - Registro de usuário
- `RegisterAsync_DuplicateEmail_ReturnsNull` - Email duplicado
- `AuthenticateAsync_ValidCredentials_ReturnsUser` - Autenticação válida
- `AuthenticateAsync_InvalidCredentials_ReturnsNull` - Credenciais inválidas
- `GetUserByIdAsync_ExistingUser_ReturnsUser` - Busca por ID
- `GetUserByEmailAsync_ExistingUser_ReturnsUser` - Busca por email
- `UpdateUserAsync_ValidUser_UpdatesUser` - Atualização de usuário
- `SoftDeleteUserAsync_ExistingUser_SoftDeletesUser` - Soft delete
- `HashPassword_ValidPassword_ReturnsHashedPassword` - Hash de senha
- `VerifyPassword_ValidPassword_ReturnsTrue` - Verificação de senha válida
- `VerifyPassword_InvalidPassword_ReturnsFalse` - Verificação de senha inválida

#### CartServiceTests (7 testes)
- `GetCartAsync_ExistingUser_ReturnsCart` - Busca carrinho existente
- `GetCartAsync_NonExistingUser_CreatesNewCart` - Cria novo carrinho
- `AddToCartAsync_ValidProduct_AddsItemToCart` - Adiciona item ao carrinho
- `UpdateCartItemAsync_ExistingItem_UpdatesQuantity` - Atualiza quantidade
- `RemoveFromCartAsync_ExistingItem_RemovesItem` - Remove item
- `ClearCartAsync_ExistingCart_ClearsAllItems` - Limpa carrinho
- `GetCartTotalAsync_ValidCart_ReturnsTotal` - Calcula total

#### OrderServiceTests (6 testes)
- `CreateOrderAsync_ValidCart_CreatesOrder` - Criação de pedido
- `GetOrderByIdAsync_ExistingOrder_ReturnsOrder` - Busca pedido por ID
- `GetOrdersByUserIdAsync_ExistingUser_ReturnsOrders` - Pedidos do usuário
- `UpdateOrderStatusAsync_ExistingOrder_UpdatesStatus` - Atualiza status
- `GetAllOrdersAsync_ReturnsAllOrders` - Lista todos os pedidos
- `CancelOrderAsync_ExistingOrder_CancelsOrder` - Cancela pedido

## 🔧 Tecnologias de Teste

- **xUnit 2.9.3** - Framework de testes
- **Moq 4.20.72** - Framework de mocking
- **Entity Framework Core In-Memory 9.0.0** - Banco de dados em memória
- **Microsoft.NET.Test.Sdk 17.14.1** - SDK de testes
- **coverlet.collector 6.0.4** - Coleta de cobertura de código

## 📝 Boas Práticas

### Arranjo-Atenção-Confirmação (AAA)
Todos os testes seguem o padrão AAA:
- **Arrange**: Configuração do cenário de teste
- **Act**: Execução da ação a ser testada
- **Assert**: Verificação do resultado esperado

### Nomenclatura
Nome dos testes segue o padrão: `Metodo_Cenario_ResultadoEsperado`

### Isolamento
Cada teste usa um banco de dados em memória isolado com GUID único para evitar interferência entre testes.

## 🎯 Cobertura de Código

A cobertura de código atual cobre:
- Modelos de domínio (Product, User)
- Serviços principais (ProductService, AuthService, CartService, OrderService)

Cobertura estimada: ~40% do código principal

## 🚧 Próximos Passos

### Testes a Implementar
- [ ] Tests de Controllers (MVC)
- [ ] Tests de Controllers de API
- [ ] Tests de integração com APIs externas (Mercado Pago, Melhor Envio)
- [ ] Tests de ViewModel validation
- [ ] Tests de CouponService
- [ ] Tests de DiscountService
- [ ] Tests de WishlistService
- [ ] Tests de EmailService

### Melhorias
- [ ] Aumentar cobertura de código para >80%
- [ ] Adicionar testes de performance
- [ ] Implementar testes de carga
- [ ] Adicionar testes de aceitação (E2E)
- [ ] Configurar CI/CD para execução automática

## 📚 Recursos

- [Documentação xUnit](https://xunit.net/)
- [Documentação Moq](https://github.com/moq/moq4)
- [Documentação EF Core In-Memory](https://docs.microsoft.com/en-us/ef/core/testing/)
