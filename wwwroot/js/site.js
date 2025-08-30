// Funcionalidades do E-commerce Kaboom

// Função para inicializar todas as funcionalidades
document.addEventListener('DOMContentLoaded', function() {
    initializeSidebar();
    initializeSearch();
    initializeProductCards();
    initializeCategoryCarousel();
    initializeLazyLoading();
    initializeTooltips();
    // Inicializar bind para detalhes do produto (se existir no DOM)
    const priceEl = document.getElementById('unitPrice');
    if (priceEl) {
        const basePrice = parseFloat(priceEl.textContent.replace('.', '').replace(',', '.')) || 0;
        bindVariationAndQuantityHandlers(basePrice);
    }
});

// Inicialização da Sidebar
function initializeSidebar() {
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarClose = document.getElementById('sidebarClose');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    const mainContent = document.querySelector('.main-content');

    if (!sidebar || !sidebarToggle) return;

    function openSidebar() {
        sidebar.classList.add('active');
        sidebarOverlay.classList.add('active');
        document.body.style.overflow = 'hidden';
        
        // Adicionar classe para animação
        setTimeout(() => {
            sidebar.style.transform = 'translateX(0)';
        }, 10);
    }

    function closeSidebar() {
        sidebar.style.transform = 'translateX(-100%)';
        setTimeout(() => {
            sidebar.classList.remove('active');
            sidebarOverlay.classList.remove('active');
            document.body.style.overflow = '';
        }, 300);
    }

    sidebarToggle.addEventListener('click', openSidebar);
    sidebarClose.addEventListener('click', closeSidebar);
    sidebarOverlay.addEventListener('click', closeSidebar);

    // Fechar sidebar ao redimensionar
    window.addEventListener('resize', function() {
        if (window.innerWidth > 768) {
            closeSidebar();
        }
    });

    // Fechar sidebar ao pressionar ESC
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && sidebar.classList.contains('active')) {
            closeSidebar();
        }
    });
}

// Inicialização da Busca
function initializeSearch() {
    const searchForm = document.querySelector('form[asp-action="Search"]');
    const searchInput = document.querySelector('input[name="q"]');

    if (!searchForm || !searchInput) return;

    // Busca em tempo real (debounced)
    let searchTimeout;
    searchInput.addEventListener('input', function() {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            if (this.value.length >= 3) {
                performLiveSearch(this.value);
            }
        }, 500);
    });

    // Busca ao pressionar Enter
    searchInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            searchForm.submit();
        }
    });

    // Foco automático no campo de busca
    searchInput.focus();
}

// Busca em tempo real
function performLiveSearch(query) {
    // Aqui você pode implementar busca AJAX se desejar
    console.log('Buscando por:', query);
}

// Inicialização dos Cards de Produtos
function initializeProductCards() {
    const productCards = document.querySelectorAll('.product-card');

    productCards.forEach(card => {
        // Adicionar efeito de loading ao clicar em "Adicionar ao Carrinho"
        const addToCartBtn = card.querySelector('.btn-warning');
        if (addToCartBtn) {
            addToCartBtn.addEventListener('click', function(e) {
                e.preventDefault();
                addToCart(this);
            });
        }

        // Adicionar efeito de loading ao clicar em "Favoritar"
        const favoriteBtn = card.querySelector('.btn-outline-secondary');
        if (favoriteBtn) {
            favoriteBtn.addEventListener('click', function(e) {
                e.preventDefault();
                toggleFavorite(this);
            });
        }

        // Adicionar tooltip para informações do produto
        const productName = card.querySelector('.card-title');
        if (productName && productName.textContent.length > 50) {
            productName.title = productName.textContent;
        }
    });
}

// Adicionar ao carrinho
function addToCart(button) {
    const productCard = button.closest('.product-card, .product-detail');
    const productId = button.dataset.productId;
    const productName = productCard.querySelector('.product-title, h1')?.textContent || 'Produto';
    
    // Obter preço base do produto
    const priceElement = document.getElementById('unitPrice') || document.querySelector('.product-price, .price');
    const productPrice = priceElement ? 
        parseFloat(priceElement.textContent.replace(/[^0-9,-]+/g, '').replace('.', '').replace(',', '.')) : 0;
    
    // Obter quantidade (usar o valor do input de quantidade ou 1 como padrão)
    const quantityInput = document.getElementById('quantity');
    const quantity = quantityInput ? parseInt(quantityInput.value) || 1 : 1;
    
    // Obter variação selecionada, se houver
    const variationSelect = document.querySelector('input[name="variationId"]:checked');
    const variationId = variationSelect ? parseInt(variationSelect.value) : null;
    const variationName = variationSelect ? 
        variationSelect.parentElement.textContent.trim() : null;
    
    // Verificar se o usuário está logado
    const isLoggedIn = document.body.getAttribute('data-user-authenticated') === 'True';
    
    // Mostrar loading no botão
    const originalText = button.innerHTML;
    button.disabled = true;
    button.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Adicionando...';
    button.classList.remove('btn-warning');
    button.classList.add('btn-success', 'loading');
    
    // Preparar dados do produto para adicionar ao carrinho
    const productData = {
        productId: parseInt(productId),
        variationId: variationId,
        quantity: quantity,
        productName: productName,
        productPrice: productPrice,
        variationName: variationName,
        returnUrl: window.location.pathname + window.location.search
    };
    
    if (!isLoggedIn) {
        // Armazenar os dados do produto para adicionar após o login
        sessionStorage.setItem('pendingAddToCart', JSON.stringify(productData));
        
        // Redirecionar para a página de login
        window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(productData.returnUrl);
        return;
    }
    
    // Adicionar ao carrinho via AJAX
    fetch('/Cart/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        },
        body: JSON.stringify({
            productId: parseInt(productId),
            variationId: variationId,
            quantity: quantity
        })
    })
    .then(response => {
        if (!response.ok) {
            return response.json().then(err => {
                throw new Error(err.message || 'Erro ao adicionar ao carrinho');
            });
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            // Atualizar contador do carrinho
            updateCartCount(data.cartItemCount);
            
            // Mostrar notificação com detalhes da variação, se houver
            let message = 'Produto adicionado ao carrinho!';
            if (variationName) {
                message += ` (${variationName})`;
            }
            showNotification(message, 'success');
            
            // Atualizar o carrinho na interface, se necessário
            if (typeof updateCartTotals === 'function') {
                updateCartTotals(data);
            }
        } else {
            throw new Error(data.message || 'Erro ao adicionar ao carrinho');
        }
    })
    .catch(error => {
        console.error('Erro:', error);
        showNotification(error.message || 'Erro ao adicionar ao carrinho', 'danger');
    })
    .finally(() => {
        // Resetar botão após 2 segundos
        setTimeout(() => {
            button.innerHTML = originalText;
            button.disabled = false;
            button.classList.remove('btn-success', 'loading');
            button.classList.add('btn-warning');
        }, 2000);
    });
}

// Toggle favorito
function toggleFavorite(button) {
    const isFavorite = button.classList.contains('btn-danger');
    
    if (isFavorite) {
        button.classList.remove('btn-danger');
        button.classList.add('btn-outline-secondary');
        button.innerHTML = '<i class="fas fa-heart me-1"></i>Favoritar';
        showNotification('Removido dos favoritos', 'info');
    } else {
        button.classList.remove('btn-outline-secondary');
        button.classList.add('btn-danger');
        button.innerHTML = '<i class="fas fa-heart me-1"></i>Favoritado';
        showNotification('Adicionado aos favoritos!', 'success');
    }
}

// Inicialização do Carrossel de Categorias
function initializeCategoryCarousel() {
    const categoryCarousel = document.querySelector('.category-carousel');
    if (!categoryCarousel) return;

    let isDown = false;
    let startX;
    let scrollLeft;

    categoryCarousel.addEventListener('mousedown', (e) => {
        isDown = true;
        categoryCarousel.classList.add('active');
        startX = e.pageX - categoryCarousel.offsetLeft;
        scrollLeft = categoryCarousel.scrollLeft;
    });

    categoryCarousel.addEventListener('mouseleave', () => {
        isDown = false;
        categoryCarousel.classList.remove('active');
    });

    categoryCarousel.addEventListener('mouseup', () => {
        isDown = false;
        categoryCarousel.classList.remove('active');
    });

    categoryCarousel.addEventListener('mousemove', (e) => {
        if (!isDown) return;
        e.preventDefault();
        const x = e.pageX - categoryCarousel.offsetLeft;
        const walk = (x - startX) * 2;
        categoryCarousel.scrollLeft = scrollLeft - walk;
    });

    // Adicionar navegação por teclado
    categoryCarousel.addEventListener('keydown', (e) => {
        if (e.key === 'ArrowLeft') {
            categoryCarousel.scrollLeft -= 200;
        } else if (e.key === 'ArrowRight') {
            categoryCarousel.scrollLeft += 200;
        }
    });

    // Adicionar indicadores de scroll
    addScrollIndicators(categoryCarousel);
}

// Adicionar indicadores de scroll
function addScrollIndicators(carousel) {
    const scrollLeft = document.createElement('button');
    const scrollRight = document.createElement('button');
    
    scrollLeft.innerHTML = '<i class="fas fa-chevron-left"></i>';
    scrollRight.innerHTML = '<i class="fas fa-chevron-right"></i>';
    
    scrollLeft.className = 'btn btn-light position-absolute start-0 top-50 translate-middle-y ms-2';
    scrollRight.className = 'btn btn-light position-absolute end-0 top-50 translate-middle-y me-2';
    
    scrollLeft.addEventListener('click', () => {
        carousel.scrollLeft -= 400;
    });
    
    scrollRight.addEventListener('click', () => {
        carousel.scrollLeft += 400;
    });
    
    carousel.parentElement.style.position = 'relative';
    carousel.parentElement.appendChild(scrollLeft);
    carousel.parentElement.appendChild(scrollRight);
}

// Inicialização de Lazy Loading
function initializeLazyLoading() {
    const images = document.querySelectorAll('img[data-src]');
    
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });

        images.forEach(img => imageObserver.observe(img));
    } else {
        // Fallback para navegadores mais antigos
        images.forEach(img => {
            img.src = img.dataset.src;
            img.classList.remove('lazy');
        });
    }
}

// Inicialização de Tooltips
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Sistema de Notificações
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto-remover após 5 segundos
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

// Função para scroll suave
function smoothScrollTo(element) {
    element.scrollIntoView({
        behavior: 'smooth',
        block: 'start'
    });
}

// Função para debounce
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Função para throttle
function throttle(func, limit) {
    let inThrottle;
    return function() {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

// Adicionar funcionalidade de "Voltar ao topo"
function addBackToTop() {
    const backToTop = document.createElement('button');
    backToTop.innerHTML = '<i class="fas fa-arrow-up"></i>';
    backToTop.className = 'btn btn-warning position-fixed';
    backToTop.style.cssText = 'bottom: 20px; right: 20px; z-index: 999; border-radius: 50%; width: 50px; height: 50px; display: none;';
    
    backToTop.addEventListener('click', () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
    
    document.body.appendChild(backToTop);
    
    // Mostrar/ocultar baseado no scroll
    window.addEventListener('scroll', throttle(() => {
        if (window.pageYOffset > 300) {
            backToTop.style.display = 'block';
        } else {
            backToTop.style.display = 'none';
        }
    }, 100));
}

// Inicializar botão "Voltar ao topo"
document.addEventListener('DOMContentLoaded', addBackToTop);

// Função para salvar preferências do usuário
function saveUserPreferences(preferences) {
    localStorage.setItem('kaboom_preferences', JSON.stringify(preferences));
}

// Função para carregar preferências do usuário
function loadUserPreferences() {
    const preferences = localStorage.getItem('kaboom_preferences');
    return preferences ? JSON.parse(preferences) : {};
}

// Função para aplicar tema escuro/claro
function toggleTheme() {
    const body = document.body;
    const isDark = body.classList.contains('dark-theme');
    
    if (isDark) {
        body.classList.remove('dark-theme');
        saveUserPreferences({ theme: 'light' });
    } else {
        body.classList.add('dark-theme');
        saveUserPreferences({ theme: 'dark' });
    }
}

// Carregar tema salvo
document.addEventListener('DOMContentLoaded', () => {
    const preferences = loadUserPreferences();
    if (preferences.theme === 'dark') {
        document.body.classList.add('dark-theme');
    }
});

// Função para compartilhar produto
function shareProduct(productName, productUrl) {
    if (navigator.share) {
        navigator.share({
            title: productName,
            url: productUrl
        });
    } else {
        // Fallback para copiar URL
        navigator.clipboard.writeText(productUrl).then(() => {
            showNotification('Link copiado para a área de transferência!', 'success');
        });
    }
}

// ===== Produto: Preço dinâmico por variação/quantidade =====
function updatePrices(basePrice, variationPriceModifier) {
    try {
        console.log('updatePrices - Entrada - basePrice:', basePrice, 'type:', typeof basePrice, '| modifier:', variationPriceModifier, 'type:', typeof variationPriceModifier);
        
        const qtyInput = document.getElementById('quantity');
        const qty = Math.max(1, parseInt(qtyInput?.value || '1'));
        
        // Garantir que basePrice seja um número
        let base = 0;
        if (typeof basePrice === 'number') {
            base = basePrice;
        } else if (typeof basePrice === 'string') {
            // Se for string, converter para número
            base = parseFloat(basePrice.replace(/\./g, '').replace(',', '.')) || 0;
        }
        
        // Garantir que o modificador seja um número
        let modifier = 0;
        if (typeof variationPriceModifier === 'number') {
            modifier = variationPriceModifier;
        } else if (typeof variationPriceModifier === 'string') {
            modifier = parseFloat(variationPriceModifier.replace(/\./g, '').replace(',', '.')) || 0;
        }
        
        console.log('DEBUG - Valores processados - Base:', base, '| Modificador:', modifier);
        
        // Calcular preço unitário (preço base + modificador)
        const unitPrice = parseFloat((base + modifier).toFixed(2));
        console.log('DEBUG - Preço unitário calculado:', unitPrice);
        const totalPrice = unitPrice * qty;
        
        // Formatando para o padrão brasileiro (R$ 1.234,56)
        const formatCurrency = (value) => {
            return 'R$ ' + value.toLocaleString('pt-BR', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
        };
        
        // Atualizar os elementos na página
        const unitSpan = document.getElementById('unitPrice');
        const totalSpan = document.getElementById('totalPrice');
        const priceElement = document.querySelector('.product-price');
        
        if (unitSpan) unitSpan.textContent = formatCurrency(unitPrice);
        if (totalSpan) totalSpan.textContent = formatCurrency(totalPrice);
        if (priceElement) priceElement.textContent = formatCurrency(unitPrice);
        
    } catch (e) {
        console.error('Falha ao atualizar preços', e);
    }
}

// Armazenar o preço base como uma variável no escopo do módulo
let _basePrice = 0;
let _isBasePriceInitialized = false;

function bindVariationAndQuantityHandlers(basePrice) {
    console.log('bindVariationAndQuantityHandlers - Preço base recebido:', basePrice, 'Tipo:', typeof basePrice);
    
    const qtyInput = document.getElementById('quantity');
    const radios = document.querySelectorAll('input[name="variationId"]');
    
    // Atualizar o preço base apenas se for um número válido e não for zero
    if (basePrice !== undefined && basePrice !== null && basePrice !== 0) {
        const newBasePrice = parseFloat(basePrice);
        if (!isNaN(newBasePrice)) {
            _basePrice = newBasePrice;
            _isBasePriceInitialized = true;
            console.log('Preço base definido para:', _basePrice);
        }
    }
    
    // Se o preço base não foi inicializado, tentar obter do elemento HTML
    if (!_isBasePriceInitialized) {
        const priceElement = document.querySelector('.product-price');
        if (priceElement) {
            const priceText = priceElement.textContent.replace(/[^0-9,-]/g, '').replace(',', '.');
            const priceFromElement = parseFloat(priceText);
            if (!isNaN(priceFromElement) && priceFromElement > 0) {
                _basePrice = priceFromElement;
                _isBasePriceInitialized = true;
                console.log('Preço base obtido do elemento HTML:', _basePrice);
            }
        }
    }
    
    // Função para obter o modificador de preço da variação selecionada
    const getSelectedModifier = () => {
        const selected = document.querySelector('input[name="variationId"]:checked');
        if (!selected) return 0;
        
        const modifier = selected.getAttribute('data-modifier');
        if (!modifier || modifier.trim() === '') return 0;
        
        // Converte o modificador para número, tratando vírgula como separador decimal
        const modifierStr = modifier.toString().replace(/\./g, '').replace(',', '.');
        const modifierValue = parseFloat(modifierStr);
        
        console.log('Selected variation modifier:', modifier, 'Parsed:', modifierValue);
        return isNaN(modifierValue) ? 0 : modifierValue;
    };
    
    // Função para atualizar a exibição do preço
    const updatePriceDisplay = () => {
        // Garantir que temos um preço base válido
        const currentBasePrice = _isBasePriceInitialized ? _basePrice : 0;
        
        console.log('updatePriceDisplay - Preço base atual:', currentBasePrice, 'Tipo:', typeof currentBasePrice);
        
        const modifier = getSelectedModifier();
        console.log('updatePriceDisplay - Modificador:', modifier, 'Tipo:', typeof modifier);
        
        // Atualizar os preços com o preço base atual
        updatePrices(currentBasePrice, modifier);
        
        // Atualizar o botão de adicionar ao carrinho com o ID da variação
        const addToCartBtn = document.getElementById('addToCartBtn');
        if (addToCartBtn) {
            const selectedVariation = document.querySelector('input[name="variationId"]:checked');
            if (selectedVariation) {
                addToCartBtn.setAttribute('data-variation-id', selectedVariation.value);
            } else {
                addToCartBtn.removeAttribute('data-variation-id');
            }
        }
    };
    
    // Adicionar eventos de mudança
    if (qtyInput) {
        qtyInput.addEventListener('input', updatePriceDisplay);
    }
    
    // Adicionar eventos para as variações
    if (radios.length > 0) {
        radios.forEach(radio => {
            radio.addEventListener('change', updatePriceDisplay);
        });
        
        // Selecionar a primeira variação por padrão se nenhuma estiver selecionada
        if (!document.querySelector('input[name="variationId"]:checked') && radios.length > 0) {
            radios[0].checked = true;
        }
    }
    
    // Atualizar preço inicial
    updatePriceDisplay();
}

// Função para comparar produtos
function compareProducts(productIds) {
    if (productIds.length < 2) {
        showNotification('Selecione pelo menos 2 produtos para comparar', 'warning');
        return;
    }
    
    // Implementar lógica de comparação
    console.log('Comparando produtos:', productIds);
    showNotification('Funcionalidade de comparação em desenvolvimento', 'info');
}

// Função para filtrar produtos por preço
function filterByPrice(minPrice, maxPrice) {
    const productCards = document.querySelectorAll('.product-card');
    
    productCards.forEach(card => {
        const priceElement = card.querySelector('.h5.text-primary');
        if (priceElement) {
            const price = parseFloat(priceElement.textContent.replace('R$ ', '').replace(',', '.'));
            
            if (price >= minPrice && price <= maxPrice) {
                card.style.display = 'block';
            } else {
                card.style.display = 'none';
            }
        }
    });
}

// Função para ordenar produtos dinamicamente
function sortProducts(sortBy) {
    const productContainer = document.querySelector('.row.row-cols-1');
    if (!productContainer) return;
    
    const products = Array.from(productContainer.children);
    
    products.sort((a, b) => {
        const aPrice = parseFloat(a.querySelector('.h5.text-primary')?.textContent.replace('R$ ', '').replace(',', '.') || 0);
        const bPrice = parseFloat(b.querySelector('.h5.text-primary')?.textContent.replace('R$ ', '').replace(',', '.') || 0);
        const aName = a.querySelector('.card-title')?.textContent || '';
        const bName = b.querySelector('.card-title')?.textContent || '';
        const aRating = parseFloat(a.querySelector('.text-warning')?.getAttribute('data-rating') || 0);
        const bRating = parseFloat(b.querySelector('.text-warning')?.getAttribute('data-rating') || 0);
        
        switch (sortBy) {
            case 'price_asc':
                return aPrice - bPrice;
            case 'price_desc':
                return bPrice - aPrice;
            case 'name':
                return aName.localeCompare(bName);
            case 'rating':
                return bRating - aRating;
            default:
                return 0;
        }
    });
    
    // Reordenar no DOM
    products.forEach(product => productContainer.appendChild(product));
    
    // Adicionar animação de reordenação
    products.forEach((product, index) => {
        product.style.animationDelay = `${index * 0.1}s`;
        product.classList.add('reordering');
        setTimeout(() => product.classList.remove('reordering'), 500);
    });
}
