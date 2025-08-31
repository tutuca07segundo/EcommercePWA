# 🚀 Configuração do Mercado Pago - Kaboom E-commerce

## 📋 Pré-requisitos
- Conta no Mercado Pago Developers
- Public Key e Access Token (sandbox ou produção)

## ⚙️ Configuração das Credenciais

### 1. Editar appsettings.json
Substitua as credenciais em `appsettings.json`:

```json
"MercadoPago": {
  "PublicKey": "SUA_PUBLIC_KEY_AQUI",
  "AccessToken": "SEU_ACCESS_TOKEN_AQUI", 
  "WebhookSecret": "SEU_WEBHOOK_SECRET_AQUI",
  "SandboxMode": true
}
```

### 2. Configurar Webhook (Opcional)
Para receber notificações automáticas de pagamento:

**URL do Webhook:** `https://seudominio.com/Checkout/MercadoPagoWebhook`

**Eventos a configurar:**
- `payment` (pagamentos)
- `merchant_order` (pedidos)

## 🔧 Funcionalidades Implementadas

### ✅ Integração Completa
- **Criação de Preferências**: Gera link de pagamento automaticamente
- **Redirecionamento**: Usuário é direcionado para checkout do Mercado Pago
- **Retorno Automático**: Após pagamento, volta para confirmação
- **Webhook**: Atualiza status do pedido automaticamente
- **Fallback**: Se MP falhar, usa simulação local

### 🎯 Fluxo de Pagamento
1. **Usuário finaliza compra** → Sistema cria pedido
2. **Gera preferência MP** → Obtém link de pagamento
3. **Redireciona para MP** → Usuário paga no ambiente seguro
4. **Retorna para site** → Confirma pagamento e mostra pedido
5. **Webhook atualiza** → Status do pedido é sincronizado

### 🔄 URLs de Retorno Configuradas
- **Sucesso**: `/Checkout/PaymentSuccess`
- **Falha**: `/Checkout/PaymentFailure` 
- **Pendente**: `/Checkout/PaymentPending`

### 📊 Status de Pedidos Mapeados
- `approved` → "Confirmado"
- `pending` → "Aguardando Pagamento"
- `rejected` → "Cancelado"
- `cancelled` → "Cancelado"

## 🧪 Como Testar

### Modo Sandbox (Recomendado)
1. Use credenciais de teste do Mercado Pago
2. `"SandboxMode": true` no appsettings.json
3. Teste com cartões de teste fornecidos pelo MP

### Cartões de Teste Mercado Pago
```
VISA: 4509 9535 6623 3704
Mastercard: 5031 7557 3453 0604
CVV: 123
Validade: 11/25
```

## 🚨 Segurança
- **Nunca** commite credenciais reais no código
- Use variáveis de ambiente em produção
- Configure HTTPS obrigatório para webhooks
- Valide assinaturas dos webhooks

## 📝 Logs e Monitoramento
O sistema registra:
- Criação de preferências
- Retornos de pagamento
- Processamento de webhooks
- Erros de integração

## 🔧 Troubleshooting

### Problema: "Preferência não criada"
- Verifique Access Token
- Confirme se está no modo correto (sandbox/produção)
- Veja logs para detalhes do erro

### Problema: "Webhook não funciona"
- Confirme URL pública acessível
- Verifique se HTTPS está configurado
- Teste manualmente o endpoint

### Problema: "Pedido não encontrado"
- Verifique se external_reference está sendo passado
- Confirme se o pedido foi criado antes da preferência

## 🎉 Pronto para Usar!
Após configurar as credenciais, o sistema está totalmente funcional com pagamentos reais via Mercado Pago!
