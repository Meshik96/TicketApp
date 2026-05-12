//import { LoginController } from './controllers/Login.js';
import { LoginController } from './controllers/LoginController.js';
import { EventsController } from './controllers/EventsController.js';
import { SeatsController } from './controllers/SeatsController.js';
import { ReservationsController } from './controllers/ReservationsController.js';
import { closeModals } from './utils/helpers.js';
import { apiService } from './api.js';
import { STORAGE_KEYS } from './utils/constants.js';

class AppRouter {
    constructor() {
        this.loginController = null;
        this.initGlobalListeners();
        this.checkAuthAndRoute();
    }

    initGlobalListeners() {
        document.addEventListener('click', (e) => {
            if (e.target.id === 'logoutBtn' || e.target.id === 'logoutBtn-seats') {
                apiService.logout();
                // Clear sessionStorage on logout
                sessionStorage.removeItem(STORAGE_KEYS.USER);
            }
            if (e.target.classList.contains('modal-close')) {
                closeModals();
            }
        });
        
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') closeModals();
        });
    }

    checkAuthAndRoute() {
        // Instanciar el controlador de login para que esté disponible
        if (!this.loginController) {
            this.loginController = new LoginController(this);
        }

        const user = sessionStorage.getItem(STORAGE_KEYS.USER);
        const storedEventId = sessionStorage.getItem(STORAGE_KEYS.CURRENT_EVENT);
        const path = window.location.pathname;

        if (path.includes('seats.html')) {
            if (!user) {
                // Si intenta entrar a seats.html sin sesión, enviar a events.html
                window.location.href = 'events.html';
                return;
            }
            if (storedEventId) {
                this.navigate('seats', { eventId: storedEventId }); 
            } else {
                window.location.href = 'events.html';
            }
        } else if (path.includes('reservations.html')) {
            if (!user) {
                // Si intenta entrar a reservations.html sin sesión, enviar a events.html
                window.location.href = 'events.html';
                return;
            }
            this.navigate('reservations');
        } else if (path.includes('events.html') || path.includes('index.html') || path.endsWith('/')) {
            this.navigate('events');
        }
    }

    navigate(route, params = {}) {
        const user = sessionStorage.getItem(STORAGE_KEYS.USER);
        
        // Quitar 'events' de la validación. Solo 'seats' y 'reservations' requieren autenticación.
        if ((route === 'seats' || route === 'reservations') && !user) {
            if (!this.loginController) {
                this.loginController = new LoginController(this);
            }
            this.loginController.showModal();
            return;
        }

        switch(route) {
            case 'events':
                if (!window.location.pathname.includes('events.html')) {
                    window.location.href = 'events.html';
                } else {
                    new EventsController(this);
                }
                break;
            case 'seats':
                if (!window.location.pathname.includes('seats.html')) {
                    window.location.href = 'seats.html';
                } else {
                    new SeatsController(this, params.eventId);
                }
                break;
            case 'reservations':
                if (!window.location.pathname.includes('reservations.html')) {
                    window.location.href = 'reservations.html';
                } else {
                    new ReservationsController(this);
                }
                break;
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    window.app = new AppRouter();
});