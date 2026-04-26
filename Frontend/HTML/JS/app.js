//import { LoginController } from './controllers/Login.js';
import { EventsController } from './controllers/EventsController.js';
import { SeatsController } from './controllers/SeatsController.js';
import { closeModals } from './utils/helpers.js';
import { apiService } from './api.js';
import { STORAGE_KEYS } from './utils/constants.js';


class AppRouter {
    constructor() {
        this.initGlobalListeners();
        this.checkAuthAndRoute();
    }

    initGlobalListeners() {
        document.addEventListener('click', (e) => {
            if (e.target.id === 'logoutBtn' || e.target.id === 'logoutBtn-seats') {
                apiService.logout();
                this.navigate('login');
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
        //if (!apiService.isLoggedIn()) {
        //    this.navigate('login');
        //    return;
        //}

        const storedEventId = localStorage.getItem(STORAGE_KEYS.CURRENT_EVENT);
        const path = window.location.pathname;

        if (path.includes('seats.html')) {
            if (storedEventId) {
                this.navigate('seats', { eventId: storedEventId }); 
            } else {
                window.location.href = 'events.html';
            }
        } else if (path.includes('events.html') || path.includes('index.html') || path.endsWith('/')) {
            this.navigate('events');
        }
    }

    navigate(route, params = {}) {
        // Manejo de SPA o redirección física
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
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    window.app = new AppRouter();
});