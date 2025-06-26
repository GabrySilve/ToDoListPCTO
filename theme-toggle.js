/**
 * Theme Toggle System
 * Sistema per gestire il cambio tra tema chiaro e scuro
 */

class ThemeManager {
    constructor() {
        this.currentTheme = this.getStoredTheme();
        this.init();
    }

    init() {
        // Applica il tema salvato al caricamento
        this.applyTheme(this.currentTheme);
        
        // Crea il toggle button se non esiste
        this.createToggleButton();
        
        // Assicura che l'icona sia impostata correttamente anche se il bottone esisteva già
        setTimeout(() => {
            this.updateToggleIcon();
        }, 100);
        
        // Aggiungi event listener
        this.addEventListeners();
    }

    getStoredTheme() {
        return localStorage.getItem('theme') || 'light';
    }

    setStoredTheme(theme) {
        localStorage.setItem('theme', theme);
    }

    applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        this.currentTheme = theme;
        this.setStoredTheme(theme);
        this.updateToggleIcon();
    }

    toggleTheme() {
        const newTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.applyTheme(newTheme);
        
        // Animazione di transizione fluida
        document.body.style.transition = 'all 0.3s ease';
        setTimeout(() => {
            document.body.style.transition = '';
        }, 300);
    }

    createToggleButton() {
        // Verifica se il bottone esiste già
        if (document.getElementById('theme-toggle')) return;

        const navbar = document.querySelector('.navbar .d-flex.gap-3');
        if (!navbar) return;

        const toggleButton = document.createElement('button');
        toggleButton.id = 'theme-toggle';
        toggleButton.className = 'theme-toggle';
        toggleButton.setAttribute('aria-label', 'Toggle tema');
        toggleButton.setAttribute('title', 'Cambia tema');
        
        const icon = document.createElement('i');
        icon.id = 'theme-icon';
        
        // Aggiungi l'icona al bottone prima di impostare la classe
        toggleButton.appendChild(icon);
        navbar.appendChild(toggleButton);
        
        // Ora aggiorna l'icona
        this.updateToggleIcon();
    }

    updateToggleIcon() {
        const icon = document.getElementById('theme-icon');
        if (!icon) return;

        if (this.currentTheme === 'light') {
            icon.className = 'bi bi-moon-stars-fill';
        } else {
            icon.className = 'bi bi-sun-fill';
        }
    }

    addEventListeners() {
        // Event listener per il toggle button
        document.addEventListener('click', (e) => {
            if (e.target.closest('#theme-toggle')) {
                e.preventDefault();
                this.toggleTheme();
            }
        });

        // Event listener per shortcut da tastiera (Ctrl/Cmd + Shift + T)
        document.addEventListener('keydown', (e) => {
            if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'T') {
                e.preventDefault();
                this.toggleTheme();
            }
        });
    }
}

// Inizializza il sistema di temi quando il DOM è pronto
document.addEventListener('DOMContentLoaded', () => {
    window.themeManager = new ThemeManager();
});

// Export per compatibilità con moduli
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ThemeManager;
}
