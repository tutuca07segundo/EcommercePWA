class Chatbot {
    constructor() {
        this.isOpen = false;
        this.sessionId = this.generateSessionId();
        this.whatsappNumber = '5515991562526'; // ⚠️ SUBSTITUA PELO SEU NÚMERO REAL
        this.init();
    }

    generateSessionId() {
        return 'chat_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    init() {
        this.createChatbotHTML();
        this.bindEvents();
        this.loadWelcomeMessage();
    }

    createChatbotHTML() {
        const chatbotHTML = `
            <div class="chatbot-container">
                <button class="chatbot-toggle" id="chatbot-toggle">
                    <i class="fas fa-comments"></i>
                </button>
                
                <div class="chatbot-window" id="chatbot-window">
                    <div class="chatbot-header">
                        <div>
                            <h4><i class="fas fa-robot"></i> Assistente Kaboom</h4>
                            <small>Online agora</small>
                        </div>
                        <button class="chatbot-close" id="chatbot-close">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    
                    <div class="chatbot-messages" id="chatbot-messages">
                        <!-- Mensagens aparecerão aqui -->
                    </div>
                    
                    <div class="chatbot-input">
                        <input type="text" id="chatbot-input" placeholder="Digite sua mensagem..." maxlength="500">
                        <button class="chatbot-send" id="chatbot-send">
                            <i class="fas fa-paper-plane"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', chatbotHTML);
    }

    bindEvents() {
        const toggle = document.getElementById('chatbot-toggle');
        const close = document.getElementById('chatbot-close');
        const input = document.getElementById('chatbot-input');
        const send = document.getElementById('chatbot-send');

        toggle.addEventListener('click', () => this.toggleChat());
        close.addEventListener('click', () => this.closeChat());
        send.addEventListener('click', () => this.sendMessage());
        
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                this.sendMessage();
            }
        });

        // Auto-resize input
        input.addEventListener('input', (e) => {
            if (e.target.value.length > 0) {
                send.disabled = false;
            } else {
                send.disabled = true;
            }
        });
    }

    toggleChat() {
        const window = document.getElementById('chatbot-window');
        const toggle = document.getElementById('chatbot-toggle');
        
        if (this.isOpen) {
            this.closeChat();
        } else {
            window.style.display = 'flex';
            toggle.classList.add('active');
            this.isOpen = true;
            this.scrollToBottom();
            document.getElementById('chatbot-input').focus();
        }
    }

    closeChat() {
        const window = document.getElementById('chatbot-window');
        const toggle = document.getElementById('chatbot-toggle');
        
        window.style.display = 'none';
        toggle.classList.remove('active');
        this.isOpen = false;
    }

    async loadWelcomeMessage() {
        try {
            const response = await fetch('/api/chatbot/welcome');
            const data = await response.json();
            
            if (response.ok) {
                this.addBotMessage(data.message, data.quickReplies);
            }
        } catch (error) {
            console.error('Erro ao carregar mensagem de boas-vindas:', error);
            this.addBotMessage('Olá! 👋 Sou o assistente virtual da Kaboom. Como posso ajudar você hoje?');
        }
    }

    async sendMessage() {
        const input = document.getElementById('chatbot-input');
        const send = document.getElementById('chatbot-send');
        const message = input.value.trim();

        if (!message) return;

        // Adicionar mensagem do usuário
        this.addUserMessage(message);
        
        // Limpar input e desabilitar botão
        input.value = '';
        send.disabled = true;
        
        // Mostrar indicador de digitação
        this.showTypingIndicator();

        try {
            const response = await fetch('/api/chatbot/message', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    message: message,
                    sessionId: this.sessionId
                })
            });

            const data = await response.json();
            
            // Remover indicador de digitação
            this.hideTypingIndicator();

            if (response.ok) {
                this.addBotMessage(data.message, data.quickReplies, data.showWhatsAppButton, data.whatsAppMessage);
            } else {
                this.addBotMessage('Desculpe, ocorreu um erro. Tente novamente em alguns instantes.');
            }
        } catch (error) {
            console.error('Erro ao enviar mensagem:', error);
            this.hideTypingIndicator();
            this.addBotMessage('Desculpe, não consegui processar sua mensagem. Verifique sua conexão e tente novamente.');
        }
    }

    addUserMessage(message) {
        const messagesContainer = document.getElementById('chatbot-messages');
        const messageHTML = `
            <div class="message user">
                <div class="message-content">${this.escapeHtml(message)}</div>
                <div class="message-avatar">
                    <i class="fas fa-user"></i>
                </div>
            </div>
        `;
        
        messagesContainer.insertAdjacentHTML('beforeend', messageHTML);
        this.scrollToBottom();
    }

    addBotMessage(message, quickReplies = [], showWhatsAppButton = false, whatsAppMessage = '') {
        const messagesContainer = document.getElementById('chatbot-messages');
        
        // Converter quebras de linha e formatação
        const formattedMessage = this.formatMessage(message);
        
        let quickRepliesHTML = '';
        if (quickReplies && quickReplies.length > 0) {
            quickRepliesHTML = `
                <div class="quick-replies">
                    ${quickReplies.map(reply => 
                        `<button class="quick-reply" onclick="chatbot.sendQuickReply('${this.escapeHtml(reply)}')">${reply}</button>`
                    ).join('')}
                </div>
            `;
        }

        let whatsAppButtonHTML = '';
        if (showWhatsAppButton) {
            const whatsAppMsg = whatsAppMessage || 'Olá! Vim do site Kaboom e gostaria de falar com um atendente.';
            whatsAppButtonHTML = `
                <button class="whatsapp-button" onclick="chatbot.openWhatsApp('${this.escapeHtml(whatsAppMsg)}')">
                    <i class="fab fa-whatsapp"></i>
                    Falar no WhatsApp
                </button>
            `;
        }

        const messageHTML = `
            <div class="message bot">
                <div class="message-avatar">
                    <i class="fas fa-robot"></i>
                </div>
                <div>
                    <div class="message-content">${formattedMessage}</div>
                    ${quickRepliesHTML}
                    ${whatsAppButtonHTML}
                </div>
            </div>
        `;
        
        messagesContainer.insertAdjacentHTML('beforeend', messageHTML);
        this.scrollToBottom();
    }

    sendQuickReply(reply) {
        const input = document.getElementById('chatbot-input');
        input.value = reply;
        this.sendMessage();
    }

    openWhatsApp(message) {
        const encodedMessage = encodeURIComponent(message);
        const whatsappUrl = `https://wa.me/${this.whatsappNumber}?text=${encodedMessage}`;
        window.open(whatsappUrl, '_blank');
    }

    showTypingIndicator() {
        const messagesContainer = document.getElementById('chatbot-messages');
        const typingHTML = `
            <div class="message bot typing-message">
                <div class="message-avatar">
                    <i class="fas fa-robot"></i>
                </div>
                <div class="typing-indicator">
                    <div class="typing-dot"></div>
                    <div class="typing-dot"></div>
                    <div class="typing-dot"></div>
                </div>
            </div>
        `;
        
        messagesContainer.insertAdjacentHTML('beforeend', typingHTML);
        this.scrollToBottom();
    }

    hideTypingIndicator() {
        const typingMessage = document.querySelector('.typing-message');
        if (typingMessage) {
            typingMessage.remove();
        }
    }

    formatMessage(message) {
        return message
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>') // **texto** -> <strong>texto</strong>
            .replace(/\*(.*?)\*/g, '<em>$1</em>') // *texto* -> <em>texto</em>
            .replace(/\n/g, '<br>') // quebras de linha
            .replace(/📦|🛍️|💳|🚚|🔄|🎉|👤|📞|⭐|💰|📱|🎫|🔒|✅|🆕|🚚|⚡|📍|📋|🔧|💰|🔔|📧|🕒|📍|💬|🖱️|⌨️|🎧|🖥️|🧠|🎮|💾|⚡/g, '<span style="font-size: 16px;">$&</span>'); // emojis maiores
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    scrollToBottom() {
        const messagesContainer = document.getElementById('chatbot-messages');
        setTimeout(() => {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }, 100);
    }
}

// Inicializar chatbot quando a página carregar
document.addEventListener('DOMContentLoaded', function() {
    window.chatbot = new Chatbot();
});

// Função global para compatibilidade
function openWhatsApp(message) {
    if (window.chatbot) {
        window.chatbot.openWhatsApp(message);
    }
}
