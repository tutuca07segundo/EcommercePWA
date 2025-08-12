# 🚀 Kaboom - E-commerce de Periféricos e Peças de Computador

## 📋 Descrição do Projeto

O **Kaboom** é um e-commerce completo desenvolvido em **C# ASP.NET Core** com foco em periféricos e peças de computador. O projeto foi criado como trabalho semestral e demonstra as melhores práticas de desenvolvimento web moderno.

## ✨ Funcionalidades Implementadas

### 🔍 Sistema de Busca Avançado
- **Barra de pesquisa funcional** com busca em tempo real
- Busca por nome, descrição, marca, categoria e tags
- Filtros dinâmicos e resultados instantâneos
- Histórico de buscas e sugestões

### 🎯 Categorização Inteligente
- **8 categorias principais** organizadas por tipo de produto:
  - 🖱️ **Mouses** - Gaming e profissionais
  - ⌨️ **Teclados** - Mecânicos e membrana
  - 🎧 **Headsets** - Áudio e microfones
  - 🖥️ **Monitores** - Gaming e profissionais
  - 🧠 **Processadores** - Intel e AMD
  - 🎮 **Placas de Vídeo** - NVIDIA e AMD
  - 💾 **Memórias RAM** - DDR4 e DDR5
  - ⚡ **SSDs** - NVMe e SATA

### 📱 Interface Responsiva e Moderna
- **Sidebar hambúrguer** na lateral esquerda
- Design adaptativo para todos os dispositivos
- Animações suaves e transições elegantes
- Interface intuitiva e amigável ao usuário

### 💰 Sistema de Ordenação
- **Ordenação por preço**: Menor para maior / Maior para menor
- **Ordenação por nome**: Alfabética
- **Ordenação por avaliação**: Melhor avaliados primeiro
- **Ordenação por data**: Mais recentes primeiro

### 🎨 Design System Completo
- Paleta de cores consistente e profissional
- Ícones Font Awesome para melhor UX
- Cards de produto com hover effects
- Carrossel de categorias interativo

## 🛠️ Tecnologias Utilizadas

### Backend
- **C# 9.0** - Linguagem principal
- **ASP.NET Core 9.0** - Framework web
- **MVC Pattern** - Arquitetura de aplicação
- **Dependency Injection** - Injeção de dependências

### Frontend
- **HTML5** - Estrutura semântica
- **CSS3** - Estilos e animações
- **JavaScript ES6+** - Interatividade
- **Bootstrap 5** - Framework CSS responsivo
- **Font Awesome 6** - Ícones

### Arquitetura
- **Repository Pattern** - Padrão de acesso a dados
- **Service Layer** - Lógica de negócio
- **ViewModel Pattern** - Dados para views
- **Responsive Design** - Design adaptativo

## 🚀 Como Executar o Projeto

### Pré-requisitos
- .NET 9.0 SDK ou superior
- Visual Studio 2022 ou VS Code
- Navegador web moderno

### Passos para Execução

1. **Clone o repositório**
   ```bash
   git clone [URL_DO_REPOSITORIO]
   cd EcommercePWA
   ```

2. **Restaura as dependências**
   ```bash
   dotnet restore
   ```

3. **Executa o projeto**
   ```bash
   dotnet run
   ```

4. **Acessa no navegador**
   ```
   https://localhost:5001
   ```

## 📁 Estrutura do Projeto

```
EcommercePWA/
├── Controllers/          # Controladores MVC
│   └── HomeController.cs # Controlador principal
├── Models/               # Modelos de dados
│   ├── Product.cs        # Modelo de produto
│   ├── Category.cs       # Modelo de categoria
│   └── ProductViewModel.cs # ViewModel para produtos
├── Services/             # Camada de serviços
│   └── ProductService.cs # Serviço de produtos
├── Views/                # Views Razor
│   ├── Home/            # Views da página inicial
│   └── Shared/          # Layouts compartilhados
├── wwwroot/             # Arquivos estáticos
│   ├── css/             # Estilos CSS
│   ├── js/              # JavaScript
│   └── images/          # Imagens (placeholder)
├── Program.cs            # Ponto de entrada da aplicação
└── appsettings.json     # Configurações
```

## 🎯 Funcionalidades Detalhadas

### 1. Barra de Pesquisa
- **Localização**: Header principal da aplicação
- **Funcionalidade**: Busca em tempo real com debounce de 500ms
- **Campos de busca**: Nome, descrição, marca, categoria, tags
- **Resultados**: Filtrados e ordenados dinamicamente

### 2. Sidebar de Categorias
- **Posição**: Lateral esquerda (hambúrguer)
- **Funcionalidade**: Navegação por categorias
- **Design**: Gradiente colorido e ícones temáticos
- **Responsivo**: Adapta-se a dispositivos móveis

### 3. Sistema de Filtros
- **Filtros ativos**: Exibidos como badges removíveis
- **Categorias**: Filtro por tipo de produto
- **Busca**: Filtro por termo de pesquisa
- **Persistência**: Mantém filtros durante navegação

### 4. Ordenação de Produtos
- **Por preço**: Crescente e decrescente
- **Por nome**: Ordem alfabética
- **Por avaliação**: Melhor avaliados primeiro
- **Por data**: Mais recentes primeiro

### 5. Cards de Produto
- **Informações**: Nome, descrição, preço, estoque
- **Avaliações**: Sistema de estrelas (1-5)
- **Status**: Indicadores de disponibilidade
- **Ações**: Adicionar ao carrinho, favoritar

### 6. Carrossel de Categorias
- **Navegação**: Scroll horizontal com mouse
- **Indicadores**: Botões de navegação
- **Responsivo**: Adapta-se ao tamanho da tela
- **Interativo**: Hover effects e animações

## 🎨 Sistema de Design

### Cores Principais
- **Primária**: `#ffc107` (Amarelo)
- **Secundária**: `#667eea` (Azul)
- **Sucesso**: `#28a745` (Verde)
- **Perigo**: `#dc3545` (Vermelho)
- **Info**: `#17a2b8` (Azul claro)

### Tipografia
- **Família**: Segoe UI, Tahoma, Geneva, Verdana, sans-serif
- **Tamanhos**: Hierarquia clara de títulos e textos
- **Pesos**: Regular (400) e Medium (500)

### Componentes
- **Botões**: Bordas arredondadas e efeitos hover
- **Cards**: Sombras e transições suaves
- **Formulários**: Foco destacado e validação visual
- **Navegação**: Breadcrumbs e indicadores ativos

## 📱 Responsividade

### Breakpoints
- **Mobile**: < 576px
- **Tablet**: 576px - 768px
- **Desktop**: 768px - 992px
- **Large**: > 992px

### Adaptações
- **Sidebar**: Full-screen em dispositivos móveis
- **Grid**: Colunas adaptativas (1-4 colunas)
- **Navegação**: Menu hambúrguer em telas pequenas
- **Carrossel**: Scroll horizontal em todas as telas

## 🔧 Configurações

### appsettings.json
```json
{
  "EcommerceSettings": {
    "StoreName": "Kaboom",
    "DefaultPageSize": 12,
    "EnableLiveSearch": true,
    "EnableProductComparison": true
  }
}
```

### Personalizações
- **Nome da loja**: Configurável via appsettings
- **Tamanho da página**: Padrão de 12 produtos
- **Funcionalidades**: Ativar/desativar recursos
- **Banco de dados**: Configuração de conexão

## 🚀 Melhorias Futuras

### Funcionalidades Planejadas
- [ ] Sistema de carrinho de compras
- [ ] Autenticação de usuários
- [ ] Sistema de avaliações e reviews
- [ ] Comparação de produtos
- [ ] Lista de desejos
- [ ] Histórico de compras
- [ ] Sistema de cupons e descontos
- [ ] Integração com APIs de pagamento

### Otimizações Técnicas
- [ ] Cache Redis para performance
- [ ] Banco de dados SQL Server/PostgreSQL
- [ ] API REST para frontend separado
- [ ] Testes unitários e de integração
- [ ] CI/CD pipeline
- [ ] Monitoramento e logs

## 📚 Aprendizados do Projeto

### Desenvolvimento Web
- **MVC Pattern**: Separação clara de responsabilidades
- **Responsive Design**: Design adaptativo para todos os dispositivos
- **User Experience**: Foco na usabilidade e acessibilidade
- **Performance**: Otimizações de carregamento e renderização

### C# e .NET
- **Dependency Injection**: Injeção de dependências
- **Async/Await**: Programação assíncrona
- **LINQ**: Consultas eficientes em coleções
- **Configuration**: Gerenciamento de configurações

### Frontend
- **CSS Grid/Flexbox**: Layouts modernos e responsivos
- **JavaScript ES6+**: Funcionalidades interativas
- **Bootstrap 5**: Framework CSS utilitário
- **Font Awesome**: Sistema de ícones consistente

## 🤝 Contribuição

### Como Contribuir
1. Faça um fork do projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

### Padrões de Código
- **C#**: Seguir convenções Microsoft
- **CSS**: BEM methodology
- **JavaScript**: ES6+ com ESLint
- **HTML**: Semântico e acessível

## 📄 Licença

Este projeto é parte de um trabalho semestral acadêmico. Todos os direitos reservados.

## 👨‍💻 Autor

**Desenvolvido para o curso de [NOME_DO_CURSO]**
**Semestre**: [SEMESTRE]
**Ano**: 2025

---

## 🎉 Conclusão

O projeto **Kaboom** demonstra um e-commerce completo e funcional, implementando todas as funcionalidades solicitadas:

✅ **Barra de pesquisa funcional** com busca em tempo real  
✅ **Sidebar hambúrguer** na lateral esquerda  
✅ **Categorização** por tipo de produto  
✅ **Ordenação por preço** (maior/menor)  
✅ **Interface responsiva** para todos os dispositivos  
✅ **Design moderno** com animações e transições  
✅ **Código limpo** seguindo boas práticas  

O projeto está pronto para ser executado e pode servir como base sólida para futuras expansões e funcionalidades adicionais.
