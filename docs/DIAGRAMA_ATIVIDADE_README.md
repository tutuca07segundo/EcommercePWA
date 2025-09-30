# 📊 Diagrama de Atividade - E-commerce Kaboom

## 🎯 Visão Geral

Este diagrama representa o fluxo completo de atividades do sistema e-commerce Kaboom, desde o acesso inicial do usuário até a finalização e acompanhamento de pedidos.

## 🔄 Fluxo Principal

### 1. **Início do Sistema**
- Usuário acessa o site
- Verificação de autenticação
- Carregamento da página inicial

### 2. **Navegação e Busca**
- Navegação por produtos
- Sistema de busca com autocomplete
- Aplicação de filtros avançados
- Visualização de resultados

### 3. **Seleção de Produtos**
- Visualização de detalhes
- Seleção de variações (cores, tamanhos)
- Definição de quantidade

### 4. **Autenticação**
- Verificação de login
- Processo de autenticação
- Registro de novos usuários

### 5. **Carrinho de Compras**
- Adição de produtos
- Aplicação de cupons
- Cálculo de totais
- Modificação de itens

### 6. **Processo de Checkout**
- Preenchimento de dados
- Cálculo de frete
- Seleção de método de pagamento
- Validação de dados

### 7. **Processamento de Pagamento**
- **PIX**: Geração de código
- **Cartão**: Validação com algoritmo de Luhn
- **Boleto**: Geração de boleto
- **Mercado Pago**: Redirecionamento e webhook

### 8. **Finalização**
- Criação do pedido
- Limpeza do carrinho
- Envio de confirmação
- Página de sucesso

### 9. **Acompanhamento**
- Visualização de pedidos
- Rastreamento de entrega
- Sistema de notificações
- Avaliação de produtos

## 🔀 Estruturas de Controle

### Decisões (Sim/Não):
- ✅ **Usuário está logado?**
- ✅ **Produto tem variações?**
- ✅ **Login bem-sucedido?**
- ✅ **Cupom válido?**
- ✅ **Modificar carrinho?**
- ✅ **Cartão válido?**
- ✅ **Pagamento aprovado?**
- ✅ **Pedido foi enviado?**
- ✅ **Verificar novamente?**

### Estruturas de Repetição:
- 🔄 **Continuar comprando**: Volta para navegação
- 🔄 **Verificar status**: Loop de acompanhamento
- 🔄 **Modificar carrinho**: Ciclo de ajustes
- 🔄 **Tentar pagamento**: Retry em caso de erro

## 🎨 Elementos Visuais

### Cores no Diagrama:
- 🟢 **Verde**: Início do fluxo
- 🔴 **Rosa**: Fim do fluxo
- 🟡 **Amarelo**: Pontos de decisão (Sim/Não)
- 🔵 **Azul**: Ações normais
- ⚪ **Branco**: Fluxo padrão

## 🛠️ Implementação Técnica

### Controllers Envolvidos:
- `HomeController` - Navegação e busca
- `AuthController` - Autenticação
- `CartController` - Carrinho de compras
- `CheckoutController` - Processo de pagamento
- `OrderController` - Gestão de pedidos
- `ProductController` - Detalhes de produtos

### Services Principais:
- `ProductService` - Lógica de produtos
- `AuthService` - Autenticação
- `CartService` - Carrinho
- `OrderService` - Pedidos
- `MercadoPagoService` - Pagamentos
- `EmailService` - Notificações

### APIs Integradas:
- **Mercado Pago SDK** - Processamento de pagamentos
- **Melhor Envio API** - Cálculo de frete
- **ViaCEP API** - Validação de endereços
- **Webhook System** - Notificações automáticas

## 📱 Funcionalidades Especiais

### Chatbot Inteligente:
- Suporte em tempo real
- Respostas automáticas
- Integração com WhatsApp
- Histórico de conversas

### Sistema de Notificações:
- Atualizações de pedidos
- Promoções personalizadas
- Lembretes de carrinho
- Push notifications

### Lista de Desejos:
- Produtos favoritos
- Notificações de volta ao estoque
- Transferência para carrinho
- Sincronização entre dispositivos

## 🔐 Segurança Implementada

- **Autenticação**: Sessões seguras
- **Validação**: CSRF tokens
- **Criptografia**: Hash de senhas com BCrypt
- **Sanitização**: Inputs validados
- **Webhook**: Assinatura verificada

## 📊 Métricas e Analytics

- Acompanhamento de conversão
- Análise de abandono de carrinho
- Estatísticas de produtos
- Relatórios de vendas
- Performance de cupons

## 🚀 Performance

- **Cache**: Produtos e categorias
- **Paginação**: Server-side
- **Otimização**: Consultas EF
- **Compressão**: Gzip/Brotli
- **CDN**: Arquivos estáticos

---

**Última atualização:** 30/09/2025  
**Versão do diagrama:** 2.0  
**Status:** ✅ Implementado e funcional
