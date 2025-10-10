// Advanced Filters JavaScript for Kaboom E-commerce
class AdvancedFilters {
    constructor() {
        this.currentFilters = {
            searchTerm: '',
            categories: [],
            brands: [],
            minPrice: null,
            maxPrice: null,
            minRating: null,
            inStockOnly: false,
            onSaleOnly: false,
            sortBy: 'name',
            sortDirection: 'asc',
            page: 1,
            pageSize: 12
        };
        
        this.searchTimeout = null;
        this.filterOptions = null;
        this.isLoading = false;
        
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.loadFilterOptions();
        this.setupAutocomplete();
    }

    setupEventListeners() {
        // Toggle filters panel
        const toggleBtn = document.getElementById('toggleFilters');
        const filtersPanel = document.getElementById('filtersPanel');
        
        if (toggleBtn && filtersPanel) {
            toggleBtn.addEventListener('click', () => {
                const isVisible = filtersPanel.style.display !== 'none';
                filtersPanel.style.display = isVisible ? 'none' : 'block';
                
                const icon = toggleBtn.querySelector('i');
                const text = toggleBtn.querySelector('span') || toggleBtn.childNodes[1];
                
                if (isVisible) {
                    icon.className = 'fas fa-chevron-down me-1';
                    toggleBtn.innerHTML = '<i class="fas fa-chevron-down me-1"></i>Expandir Filtros';
                } else {
                    icon.className = 'fas fa-chevron-up me-1';
                    toggleBtn.innerHTML = '<i class="fas fa-chevron-up me-1"></i>Recolher Filtros';
                }
            });
        }

        // Apply filters button
        const applyBtn = document.getElementById('applyFilters');
        if (applyBtn) {
            applyBtn.addEventListener('click', () => this.applyFilters());
        }

        // Clear filters button
        const clearBtn = document.getElementById('clearFilters');
        if (clearBtn) {
            clearBtn.addEventListener('click', () => this.clearFilters());
        }

        // Search input with debounce
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                clearTimeout(this.searchTimeout);
                this.searchTimeout = setTimeout(() => {
                    this.currentFilters.searchTerm = e.target.value;
                    if (e.target.value.length >= 2 || e.target.value.length === 0) {
                        this.applyFilters();
                    }
                }, 300);
            });
        }

        // Price inputs
        const minPriceInput = document.getElementById('minPrice');
        const maxPriceInput = document.getElementById('maxPrice');
        
        if (minPriceInput) {
            minPriceInput.addEventListener('change', (e) => {
                this.currentFilters.minPrice = e.target.value ? parseFloat(e.target.value) : null;
            });
        }
        
        if (maxPriceInput) {
            maxPriceInput.addEventListener('change', (e) => {
                this.currentFilters.maxPrice = e.target.value ? parseFloat(e.target.value) : null;
            });
        }


        // Checkboxes
        const inStockCheck = document.getElementById('inStockOnly');
        const onSaleCheck = document.getElementById('onSaleOnly');
        
        if (inStockCheck) {
            inStockCheck.addEventListener('change', (e) => {
                this.currentFilters.inStockOnly = e.target.checked;
            });
        }
        
        if (onSaleCheck) {
            onSaleCheck.addEventListener('change', (e) => {
                this.currentFilters.onSaleOnly = e.target.checked;
            });
        }

        // Sort select
        const sortSelect = document.getElementById('sortBy');
        if (sortSelect) {
            sortSelect.addEventListener('change', (e) => {
                const value = e.target.value;
                if (value === 'price-desc') {
                    this.currentFilters.sortBy = 'price';
                    this.currentFilters.sortDirection = 'desc';
                } else {
                    this.currentFilters.sortBy = value;
                    this.currentFilters.sortDirection = 'asc';
                }
            });
        }
    }

    async loadFilterOptions() {
        try {
            const response = await fetch('/Home/GetFilterOptions');
            const data = await response.json();
            
            if (data.success) {
                this.filterOptions = data;
                this.populateFilterOptions();
            }
        } catch (error) {
            console.error('Erro ao carregar opções de filtro:', error);
        }
    }

    populateFilterOptions() {
        if (!this.filterOptions) return;

        // Populate categories
        const categoryContainer = document.getElementById('categoryFilters');
        if (categoryContainer && this.filterOptions.categories) {
            categoryContainer.innerHTML = '';
            this.filterOptions.categories.forEach(category => {
                const div = document.createElement('div');
                div.className = 'form-check';
                div.innerHTML = `
                    <input class="form-check-input category-filter" type="checkbox" 
                           value="${category}" id="cat_${category.replace(/\s+/g, '_')}">
                    <label class="form-check-label" for="cat_${category.replace(/\s+/g, '_')}">
                        ${category}
                    </label>
                `;
                categoryContainer.appendChild(div);
            });

            // Add event listeners to category checkboxes
            categoryContainer.querySelectorAll('.category-filter').forEach(checkbox => {
                checkbox.addEventListener('change', (e) => {
                    if (e.target.checked) {
                        if (!this.currentFilters.categories.includes(e.target.value)) {
                            this.currentFilters.categories.push(e.target.value);
                        }
                    } else {
                        this.currentFilters.categories = this.currentFilters.categories.filter(
                            cat => cat !== e.target.value
                        );
                    }
                });
            });
        }

        // Populate brands
        const brandContainer = document.getElementById('brandFilters');
        if (brandContainer && this.filterOptions.brands) {
            brandContainer.innerHTML = '';
            this.filterOptions.brands.forEach(brand => {
                const div = document.createElement('div');
                div.className = 'form-check';
                div.innerHTML = `
                    <input class="form-check-input brand-filter" type="checkbox" 
                           value="${brand}" id="brand_${brand.replace(/\s+/g, '_')}">
                    <label class="form-check-label" for="brand_${brand.replace(/\s+/g, '_')}">
                        ${brand}
                    </label>
                `;
                brandContainer.appendChild(div);
            });

            // Add event listeners to brand checkboxes
            brandContainer.querySelectorAll('.brand-filter').forEach(checkbox => {
                checkbox.addEventListener('change', (e) => {
                    if (e.target.checked) {
                        if (!this.currentFilters.brands.includes(e.target.value)) {
                            this.currentFilters.brands.push(e.target.value);
                        }
                    } else {
                        this.currentFilters.brands = this.currentFilters.brands.filter(
                            brand => brand !== e.target.value
                        );
                    }
                });
            });
        }

        // Set price range placeholders
        const minPriceInput = document.getElementById('minPrice');
        const maxPriceInput = document.getElementById('maxPrice');
        
        if (minPriceInput && this.filterOptions.minPrice !== undefined) {
            minPriceInput.placeholder = `Min: R$ ${this.filterOptions.minPrice.toFixed(2)}`;
        }
        
        if (maxPriceInput && this.filterOptions.maxPrice !== undefined) {
            maxPriceInput.placeholder = `Max: R$ ${this.filterOptions.maxPrice.toFixed(2)}`;
        }
    }

    setupAutocomplete() {
        const searchInput = document.getElementById('searchInput');
        const suggestionsContainer = document.getElementById('searchSuggestions');
        
        if (!searchInput || !suggestionsContainer) return;

        let suggestionTimeout;

        searchInput.addEventListener('input', (e) => {
            clearTimeout(suggestionTimeout);
            
            if (e.target.value.length < 2) {
                suggestionsContainer.style.display = 'none';
                return;
            }

            suggestionTimeout = setTimeout(async () => {
                try {
                    const response = await fetch(`/Home/SearchSuggestions?q=${encodeURIComponent(e.target.value)}`);
                    const data = await response.json();
                    
                    if (data.success && data.suggestions.length > 0) {
                        this.displaySuggestions(data.suggestions, suggestionsContainer);
                    } else {
                        suggestionsContainer.style.display = 'none';
                    }
                } catch (error) {
                    console.error('Erro ao buscar sugestões:', error);
                }
            }, 300);
        });

        // Hide suggestions when clicking outside
        document.addEventListener('click', (e) => {
            if (!searchInput.contains(e.target) && !suggestionsContainer.contains(e.target)) {
                suggestionsContainer.style.display = 'none';
            }
        });
    }

    displaySuggestions(suggestions, container) {
        container.innerHTML = '';
        
        suggestions.forEach(suggestion => {
            const div = document.createElement('div');
            div.className = 'p-2 border-bottom suggestion-item';
            div.style.cursor = 'pointer';
            div.innerHTML = `
                <div class="d-flex align-items-center">
                    <img src="${suggestion.imageUrl}" alt="${suggestion.name}" 
                         style="width: 40px; height: 40px; object-fit: cover;" class="me-2 rounded">
                    <div class="flex-grow-1">
                        <div class="fw-bold">${suggestion.name}</div>
                        <small class="text-muted">${suggestion.category} - ${suggestion.brand}</small>
                        <div class="text-primary">R$ ${suggestion.price.toFixed(2)}</div>
                    </div>
                </div>
            `;
            
            div.addEventListener('click', () => {
                document.getElementById('searchInput').value = suggestion.name;
                this.currentFilters.searchTerm = suggestion.name;
                container.style.display = 'none';
                this.applyFilters();
            });

            div.addEventListener('mouseenter', () => {
                div.style.backgroundColor = '#f8f9fa';
            });

            div.addEventListener('mouseleave', () => {
                div.style.backgroundColor = '';
            });
            
            container.appendChild(div);
        });
        
        container.style.display = 'block';
    }

    async applyFilters() {
        if (this.isLoading) return;
        
        this.isLoading = true;
        this.showLoading(true);
        
        try {
            const response = await fetch('/Home/FilterProducts', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(this.currentFilters)
            });
            
            const data = await response.json();
            
            if (data.success) {
                this.updateProductsDisplay(data);
                this.updateActiveFilters();
                this.updatePagination(data);
            } else {
                console.error('Erro ao aplicar filtros:', data.message);
            }
        } catch (error) {
            console.error('Erro ao aplicar filtros:', error);
        } finally {
            this.isLoading = false;
            this.showLoading(false);
        }
    }

    updateProductsDisplay(data) {
        const container = document.getElementById('productsContainer');
        const resultsCount = document.getElementById('resultsCount');
        const resultsTitle = document.getElementById('resultsTitle');
        
        if (resultsCount) {
            resultsCount.textContent = `${data.totalCount} produto(s) encontrado(s)`;
        }
        
        if (resultsTitle) {
            if (this.currentFilters.searchTerm) {
                resultsTitle.textContent = `Resultados para "${this.currentFilters.searchTerm}"`;
            } else if (this.currentFilters.categories.length > 0) {
                resultsTitle.textContent = this.currentFilters.categories.join(', ');
            } else {
                resultsTitle.textContent = 'Todos os Produtos';
            }
        }
        
        if (container) {
            if (data.products.length > 0) {
                container.innerHTML = this.generateProductsHTML(data.products);
            } else {
                container.innerHTML = `
                    <div class="text-center py-5">
                        <i class="fas fa-search fa-3x text-muted mb-3"></i>
                        <h4 class="text-muted">Nenhum produto encontrado</h4>
                        <p class="text-muted">Tente ajustar os filtros ou fazer uma nova busca.</p>
                        <button class="btn btn-warning" onclick="advancedFilters.clearFilters()">
                            <i class="fas fa-times me-1"></i>Limpar Filtros
                        </button>
                    </div>
                `;
            }
        }
    }

    generateProductsHTML(products) {
        const isLoggedIn = document.body.getAttribute('data-user-authenticated') === 'True';
        
        return `
            <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xl-4 g-4">
                ${products.map(product => `
                    <div class="col">
                        <div class="card h-100 product-card position-relative" style="cursor: pointer;" onclick="navigateToProduct(${product.id}, event)">
                            <div class="position-relative">
                                <img src="${product.imageUrl}" class="card-img-top" alt="${product.name}" 
                                     style="height: 200px; object-fit: cover;">
                                <div class="position-absolute top-0 start-0 m-2">
                                    ${product.stockQuantity <= 5 && product.stockQuantity > 0 ? 
                                        '<span class="badge bg-warning">Últimas unidades</span>' :
                                        product.stockQuantity === 0 ? 
                                        '<span class="badge bg-danger">Indisponível</span>' :
                                        '<span class="badge bg-success">Em estoque</span>'
                                    }
                                </div>
                                <div class="position-absolute top-0 end-0 m-2">
                                    <span class="badge bg-primary">${product.type}</span>
                                </div>
                                ${isLoggedIn ? `
                                    <button class="btn btn-light position-absolute wishlist-btn" 
                                            style="bottom: 10px; right: 10px; border-radius: 50%; width: 45px; height: 45px; box-shadow: 0 2px 8px rgba(0,0,0,0.15); display: flex; align-items: center; justify-content: center; border: 1px solid #e0e0e0;"
                                            data-product-id="${product.id}"
                                            onclick="toggleWishlist(${product.id}, null, this); event.stopPropagation();"
                                            title="Adicionar aos favoritos">
                                        <i class="fas fa-heart text-muted" style="font-size: 16px;"></i>
                                    </button>
                                ` : ''}
                            </div>
                            <div class="card-body d-flex flex-column">
                                <div class="mb-2">
                                    <small class="text-muted">${product.brand}</small>
                                </div>
                                <h6 class="card-title text-dark">${product.name}</h6>
                                <p class="card-text text-muted small flex-grow-1">${product.description}</p>
                                
                                <div class="mb-2">
                                    <div class="d-flex align-items-center mb-1">
                                    </div>
                                </div>

                                <div class="mt-auto">
                                    <div class="d-flex justify-content-between align-items-center mb-2">
                                        <div>
                                            ${product.hasDiscount && product.discountedPrice ? `
                                                <div>
                                                    <span class="h6 text-muted text-decoration-line-through mb-0">R$ ${product.price.toFixed(2)}</span>
                                                    <span class="h5 text-danger mb-0 ms-2">R$ ${product.discountedPrice.toFixed(2)}</span>
                                                    <span class="badge bg-danger ms-1">
                                                        -${Math.round((1 - (product.discountedPrice / product.price)) * 100)}%
                                                    </span>
                                                </div>
                                            ` : `
                                                <span class="h5 text-primary mb-0">R$ ${product.price.toFixed(2)}</span>
                                            `}
                                        </div>
                                        <small class="text-muted">${product.stockQuantity} em estoque</small>
                                    </div>
                                    
                                    <div class="d-grid gap-2">
                                        ${isLoggedIn ? `
                                            <button class="btn btn-warning btn-sm" data-product-id="${product.id}" onclick="addToCart(this); event.stopPropagation();">
                                                <i class="fas fa-shopping-cart me-1"></i>Adicionar ao Carrinho
                                            </button>
                                        ` : `
                                            <button type="button" class="btn btn-warning btn-sm" onclick="redirectToLogin(); event.stopPropagation();">
                                                <i class="fas fa-sign-in-alt me-1"></i>Fazer Login para Comprar
                                            </button>
                                        `}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                `).join('')}
            </div>
        `;
    }


    updateActiveFilters() {
        const activeFiltersRow = document.getElementById('activeFiltersRow');
        const badgesContainer = document.getElementById('activeFiltersBadges');
        
        if (!activeFiltersRow || !badgesContainer) return;
        
        const badges = [];
        
        if (this.currentFilters.searchTerm) {
            badges.push(`<span class="badge bg-primary me-1">
                Busca: ${this.currentFilters.searchTerm}
                <button type="button" class="btn-close btn-close-white ms-1" onclick="advancedFilters.removeFilter('searchTerm')"></button>
            </span>`);
        }
        
        this.currentFilters.categories.forEach(category => {
            badges.push(`<span class="badge bg-success me-1">
                ${category}
                <button type="button" class="btn-close btn-close-white ms-1" onclick="advancedFilters.removeFilter('category', '${category}')"></button>
            </span>`);
        });
        
        this.currentFilters.brands.forEach(brand => {
            badges.push(`<span class="badge bg-info me-1">
                ${brand}
                <button type="button" class="btn-close btn-close-white ms-1" onclick="advancedFilters.removeFilter('brand', '${brand}')"></button>
            </span>`);
        });
        
        if (this.currentFilters.minPrice || this.currentFilters.maxPrice) {
            const priceText = `R$ ${this.currentFilters.minPrice || '0'} - R$ ${this.currentFilters.maxPrice || '∞'}`;
            badges.push(`<span class="badge bg-warning me-1">
                ${priceText}
                <button type="button" class="btn-close btn-close-white ms-1" onclick="advancedFilters.removeFilter('price')"></button>
            </span>`);
        }
        
        
        if (this.currentFilters.inStockOnly) {
            badges.push(`<span class="badge bg-success me-1">
                Em estoque
                <button type="button" class="btn-close btn-close-white ms-1" onclick="advancedFilters.removeFilter('inStock')"></button>
            </span>`);
        }
        
        if (this.currentFilters.onSaleOnly) {
            badges.push(`<span class="badge bg-danger me-1">
                Em promoção
                <button type="button" class="btn-close btn-close-white ms-1" onclick="advancedFilters.removeFilter('onSale')"></button>
            </span>`);
        }
        
        if (badges.length > 0) {
            badgesContainer.innerHTML = badges.join('');
            activeFiltersRow.style.display = 'block';
        } else {
            activeFiltersRow.style.display = 'none';
        }
    }

    updatePagination(data) {
        const paginationContainer = document.getElementById('paginationContainer');
        const paginationList = document.getElementById('paginationList');
        
        if (!paginationContainer || !paginationList) return;
        
        if (data.totalPages <= 1) {
            paginationContainer.style.display = 'none';
            return;
        }
        
        paginationContainer.style.display = 'block';
        
        let paginationHTML = '';
        
        // Previous button
        if (data.currentPage > 1) {
            paginationHTML += `
                <li class="page-item">
                    <a class="page-link" href="#" onclick="advancedFilters.goToPage(${data.currentPage - 1}); return false;">
                        <i class="fas fa-chevron-left"></i>
                    </a>
                </li>
            `;
        }
        
        // Page numbers
        const startPage = Math.max(1, data.currentPage - 2);
        const endPage = Math.min(data.totalPages, data.currentPage + 2);
        
        for (let i = startPage; i <= endPage; i++) {
            paginationHTML += `
                <li class="page-item ${i === data.currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="advancedFilters.goToPage(${i}); return false;">${i}</a>
                </li>
            `;
        }
        
        // Next button
        if (data.currentPage < data.totalPages) {
            paginationHTML += `
                <li class="page-item">
                    <a class="page-link" href="#" onclick="advancedFilters.goToPage(${data.currentPage + 1}); return false;">
                        <i class="fas fa-chevron-right"></i>
                    </a>
                </li>
            `;
        }
        
        paginationList.innerHTML = paginationHTML;
    }

    goToPage(page) {
        this.currentFilters.page = page;
        this.applyFilters();
    }

    removeFilter(type, value = null) {
        switch (type) {
            case 'searchTerm':
                this.currentFilters.searchTerm = '';
                document.getElementById('searchInput').value = '';
                break;
            case 'category':
                this.currentFilters.categories = this.currentFilters.categories.filter(cat => cat !== value);
                const categoryCheckbox = document.querySelector(`input[value="${value}"].category-filter`);
                if (categoryCheckbox) categoryCheckbox.checked = false;
                break;
            case 'brand':
                this.currentFilters.brands = this.currentFilters.brands.filter(brand => brand !== value);
                const brandCheckbox = document.querySelector(`input[value="${value}"].brand-filter`);
                if (brandCheckbox) brandCheckbox.checked = false;
                break;
            case 'price':
                this.currentFilters.minPrice = null;
                this.currentFilters.maxPrice = null;
                document.getElementById('minPrice').value = '';
                document.getElementById('maxPrice').value = '';
                break;
            case 'inStock':
                this.currentFilters.inStockOnly = false;
                document.getElementById('inStockOnly').checked = false;
                break;
            case 'onSale':
                this.currentFilters.onSaleOnly = false;
                document.getElementById('onSaleOnly').checked = false;
                break;
        }
        
        this.applyFilters();
    }

    clearFilters() {
        // Reset all filters
        this.currentFilters = {
            searchTerm: '',
            categories: [],
            brands: [],
            minPrice: null,
            maxPrice: null,
            minRating: null,
            inStockOnly: false,
            onSaleOnly: false,
            sortBy: 'name',
            sortDirection: 'asc',
            page: 1,
            pageSize: 12
        };
        
        // Reset form elements
        document.getElementById('searchInput').value = '';
        document.getElementById('minPrice').value = '';
        document.getElementById('maxPrice').value = '';
        document.getElementById('minRating').value = '';
        document.getElementById('inStockOnly').checked = false;
        document.getElementById('onSaleOnly').checked = false;
        document.getElementById('sortBy').value = 'name';
        
        // Reset checkboxes
        document.querySelectorAll('.category-filter, .brand-filter').forEach(checkbox => {
            checkbox.checked = false;
        });
        
        // Hide suggestions
        const suggestionsContainer = document.getElementById('searchSuggestions');
        if (suggestionsContainer) {
            suggestionsContainer.style.display = 'none';
        }
        
        this.applyFilters();
    }

    showLoading(show) {
        const spinner = document.getElementById('loadingSpinner');
        if (spinner) {
            spinner.style.display = show ? 'inline-block' : 'none';
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    window.advancedFilters = new AdvancedFilters();
});
