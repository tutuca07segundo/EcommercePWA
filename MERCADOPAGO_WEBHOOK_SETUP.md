# Configuração do Webhook do Mercado Pago

Este guia explica como configurar e testar o webhook do Mercado Pago no ambiente de desenvolvimento.

## Pré-requisitos

1. Conta no Mercado Pago (https://www.mercadopago.com.br/)
2. Ngrok instalado (https://ngrok.com/download)
3. Acesso ao painel do desenvolvedor do Mercado Pago

## Configuração do Webhook

### 1. Iniciar o Ngrok

**Instalar Ngrok:**
1. Baixe em: https://ngrok.com/download
2. Extraia o arquivo ngrok.exe
3. Abra o terminal na pasta do ngrok

**Executar Ngrok:**
```bash
# Para HTTPS (recomendado)
ngrok http https://localhost:5001

# OU para HTTP
ngrok http http://localhost:5000
```

**Resultado esperado:**
```
Forwarding  https://abc123.ngrok.io -> https://localhost:5001
```

⚠️ **IMPORTANTE:** Use a URL HTTPS que o ngrok gerar (ex: `https://abc123.ngrok.io`)

### 2. Configurar o Webhook no Painel do Mercado Pago

1. Acesse: [https://www.mercadopago.com.br/developers/panel/notifications/webhooks](https://www.mercadopago.com.br/developers/panel/notifications/webhooks)
2. No campo "Modo Teste", insira a URL do webhook:
   ```
   https://SEU-CODIGO-NGROK.ngrok.io/api/MercadoPagoWebhook/ReceiveWebhook
   ```
3. Adicione os seguintes eventos para notificação:
   - `payment`
   - `merchant_order`
4. Clique em "Salvar"

**⚠️ IMPORTANTE:** A URL correta é `/api/MercadoPagoWebhook/ReceiveWebhook` (não apenas `/api/MercadoPagoWebhook`)

### 3. Configuração no appsettings.json

Verifique se as seguintes configurações estão corretas no seu `appsettings.json`:

```json
"MercadoPago": {
  "PublicKey": "SUA_PUBLIC_KEY",
  "AccessToken": "SEU_ACCESS_TOKEN",
  "WebhookSecret": "SEU_WEBHOOK_SECRET",
  "SandboxMode": true
}
```

### 4. Testando o Webhook

1. Inicie o aplicativo em modo de desenvolvimento:
   ```bash
   dotnet run --environment Development
   ```

2. Use o painel do Mercado Pago para simular um pagamento:
   - Acesse: [https://www.mercadopago.com.br/developers/panel/notifications/test](https://www.mercadopago.com.br/developers/panel/notifications/test)
   - Selecione o tipo de notificação `payment`
   - Clique em "Enviar notificação de teste"

3. Verifique os logs do seu aplicativo para confirmar que o webhook foi recebido e processado corretamente.

## Solução de Problemas

### Webhook não recebido
- Verifique se o Ngrok está rodando e acessível
- Confirme se a URL do webhook está correta no painel do Mercado Pago
- Verifique se não há regras de firewall bloqueando as requisições

### Erro 401 Unauthorized
- Verifique se o `WebhookSecret` no `appsettings.json` corresponde ao configurado no painel do Mercado Pago
- Confirme se o cabeçalho `x-signature` está sendo enviado corretamente

### Erro 500 Internal Server Error
- Verifique os logs do servidor para mais detalhes
- Confirme se o banco de dados está acessível
- Verifique se as dependências estão corretamente configuradas

## Próximos Passos

1. Implementar tratamento de erros mais robusto
2. Adicionar logs detalhados para facilitar a depuração
3. Implementar fila de processamento para lidar com picos de notificações
4. Adicionar monitoramento e alertas para falhas no webhook
