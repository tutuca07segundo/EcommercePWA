namespace EcommercePWA.Constants
{
    /// <summary>
    /// Constantes de negócio centralizadas para evitar magic numbers
    /// </summary>
    public static class BusinessConstants
    {
        // Impostos e Taxas
        public const decimal TAX_RATE = 0.10m; // 10% de impostos
        
        // Sessão e Cache
        public const int SESSION_TIMEOUT_MINUTES = 30;
        public const int CACHE_SIZE_LIMIT = 1024;
        public const int CACHE_EXPIRATION_DAYS = 30;
        
        // Produtos e Estoque
        public const int LOW_STOCK_THRESHOLD = 5;
        public const int DEFAULT_PAGE_SIZE = 12;
        public const int MAX_SEARCH_RESULTS = 50;
        public const int SEARCH_SUGGESTIONS_COUNT = 5;
        public const int MIN_SEARCH_LENGTH = 2;
        
        // Carrinho e Pedidos
        public const int MAX_CART_ITEMS = 100;
        public const int MIN_QUANTITY = 1;
        public const decimal MIN_ORDER_VALUE = 0.01m;
        
        // Frete e Dimensões
        public const decimal DEFAULT_WEIGHT_GRAMS = 500m;
        public const int DEFAULT_WIDTH_CM = 20;
        public const int DEFAULT_HEIGHT_CM = 15;
        public const int DEFAULT_LENGTH_CM = 25;
        public const decimal MAX_WEIGHT_KG = 50m;
        public const int MAX_DIMENSION_CM = 200;
        
        // Notificações
        public const int ABANDONED_CART_HOURS = 2;
        public const int NOTIFICATION_CLEANUP_DAYS = 30;
        public const int NOTIFICATION_CHECK_INTERVAL_MINUTES = 30;
        
        // Performance
        public const int DEBOUNCE_DELAY_MS = 300;
        public const int MAX_CONCURRENT_NOTIFICATIONS = 1000;
        public const int NOTIFICATION_SEND_TIMEOUT_MS = 100;
        
        // Validação
        public const int MIN_PASSWORD_LENGTH = 6;
        public const int MAX_NAME_LENGTH = 100;
        public const int MAX_DESCRIPTION_LENGTH = 500;
        public const int MAX_COUPON_CODE_LENGTH = 50;
        
        // Ratings
        public const decimal MIN_RATING = 1.0m;
        public const decimal MAX_RATING = 5.0m;
        public const decimal DEFAULT_RATING = 0.0m;
    }
}
