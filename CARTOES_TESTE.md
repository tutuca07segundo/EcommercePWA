# 🧪 Cartões de Teste - Validação de Pagamento

## Cartões que SEMPRE são APROVADOS
```
Visa Teste MP: 4509 9535 6623 3704
Mastercard Teste MP: 5031 7557 3453 0604
Visa Genérico: 4000 0000 0000 0010
Mastercard Genérico: 5555 5555 5555 4448
```

## Cartões que SEMPRE são NEGADOS
```
Visa Negado: 4000 0000 0000 0002
Mastercard Negado: 5555 5555 5555 4444
Visa Expirado: 4000 0000 0000 0069
Visa CVV Incorreto: 4000 0000 0000 0127
Mastercard Negado: 5105 1051 0510 5100
```

## Dados Complementares para Teste
```
CVV: 123 ou 1234
Data de Validade: 11/25 ou 12/26
Nome no Cartão: Qualquer nome válido
```

## Como Testar
1. Use os cartões NEGADOS para verificar se a validação está funcionando
2. Use os cartões APROVADOS para completar compras de teste
3. Outros cartões têm 80% de chance de aprovação (simulação realística)

## Validações Implementadas
✅ Algoritmo de Luhn para validar número do cartão
✅ Validação de data de validade
✅ Validação de CVV (3-4 dígitos)
✅ Validação de nome no cartão
✅ Simulação realística de aprovação/negação
