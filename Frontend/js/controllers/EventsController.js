import { formatDate, escapeHtml } from '../utils/helpers.js';
import { apiService } from '../api.js';
import { PAGINATION, API_CONFIG, STORAGE_KEYS } from '../utils/constants.js';

export class EventsController {
    constructor(router) {
        this.router = router;
        this.currentEventPage = 1;
        this.pageSize = PAGINATION.DEFAULT_PAGE_SIZE; // Definir constante local o importar
        this.initListeners();
        this.eventsPerPage = this.calculateEventsPerPage();
        this.setupResponsiveListener();
        this.loadEventsPage();
    }

    initListeners() {  
            document.getElementById('eventsGrid')?.addEventListener('click', (e) => {
            const eventCard = e.target.closest('.event-card');
            if (eventCard && e.target.classList.contains('btn-select-event')) {
                const eventId = eventCard.dataset.eventId;
                
                // Check if user is logged in
                const user = sessionStorage.getItem(STORAGE_KEYS.USER);
                if (!user) {
                    // Show login modal if not logged in
                    if (window.app && window.app.loginController) {
                        window.app.loginController.showModal();
                    }
                    return;
                }
                
                // Persistir ID para que sobreviva a la recarga de página
                sessionStorage.setItem(STORAGE_KEYS.CURRENT_EVENT, eventId);
                
                console.log('Evento seleccionado y guardado:', eventId);
                this.router.navigate('seats', { eventId });
            }
        });

        document.getElementById('prevBtn')?.addEventListener('click', () => this.previousPage());
        document.getElementById('nextBtn')?.addEventListener('click', () => this.nextPage());
        const menuBtn = document.getElementById('userMenuBtn');
        const dropdown = document.getElementById('userDropdown');

        if (menuBtn && dropdown) {
            // Maneja la apertura/cierre del menú
            menuBtn.addEventListener('click', (e) => {
                e.stopPropagation(); // Evita que el evento suba al window
                dropdown.classList.toggle('show');
            });

            // Cierra el menú si se hace clic fuera de él
            window.addEventListener('click', (e) => {
                if (dropdown.classList.contains('show')) {
                    dropdown.classList.remove('show');
                }
            });
        }
        const openLoginMenuBtn = document.getElementById('openLoginMenuBtn');
        const logoutBtn = document.getElementById('logoutBtn');
        const reservationsLink = document.getElementById('reservationsLink');

        if (openLoginMenuBtn) {
            openLoginMenuBtn.addEventListener('click', (e) => {
                e.preventDefault();
                dropdown.classList.remove('show'); // Cierra el menú
                if (window.app && window.app.loginController) {
                    window.app.loginController.showModal();
                }
            });
        }

        if (logoutBtn) {
            logoutBtn.addEventListener('click', (e) => {
                e.preventDefault();
                sessionStorage.removeItem(STORAGE_KEYS.USER);
                dropdown.classList.remove('show'); // Cierra el menú
                this.updateUserDisplay(); // Actualiza la UI a estado desloggeado
            });
        }

        if (reservationsLink) {
            reservationsLink.addEventListener('click', (e) => {
                e.preventDefault();
                if (dropdown) dropdown.classList.remove('show');
                this.router.navigate('reservations');
            });
        }
        this.updateUserDisplay();
    }

    async loadEventsPage() {
        this.updateUserDisplay();
        await this.fetchAndDisplayEvents();
    }

    /**
     * Update user display in header
     */

    updateUserDisplay() {
        const user = JSON.parse(sessionStorage.getItem(STORAGE_KEYS.USER) || 'null');
        const loggedOutMenu = document.getElementById('loggedOutMenu');
        const loggedInMenu = document.getElementById('loggedInMenu');
        const userNameDisplay = document.getElementById('userNameDisplay');

        if (user) {
            if (loggedOutMenu) loggedOutMenu.style.display = 'none';
            if (loggedInMenu) loggedInMenu.style.display = 'block';
            if (userNameDisplay) userNameDisplay.textContent = user.name || 'Usuario';
        } else {
            if (loggedOutMenu) loggedOutMenu.style.display = 'block';
            if (loggedInMenu) loggedInMenu.style.display = 'none';
        }
    }

    async fetchAndDisplayEvents() {
        const loadingSpinner = document.getElementById('loadingSpinner');
        const eventsGrid = document.getElementById('eventsGrid');
        const errorContainer = document.getElementById('errorContainer');
        const paginationContainer = document.getElementById('paginationContainer');

        try {
            if (loadingSpinner) loadingSpinner.style.display = 'flex';
            if (eventsGrid) eventsGrid.innerHTML = '';
            if (errorContainer) errorContainer.style.display = 'none';
            const result = await apiService.getEvents(this.currentEventPage, this.eventsPerPage);
            
            if (!result.events || result.events.length === 0) {
                if (eventsGrid) {
                    eventsGrid.innerHTML = '<div class="empty-state">No hay eventos disponibles</div>';
                }
                if (paginationContainer) paginationContainer.style.display = 'none';
                return;
            }

            this.displayEvents(result.events);
            this.updatePagination(result);

        } catch (error) {
            console.error('Error fetching events:', error);
            if (errorContainer) {
                errorContainer.textContent = error.message;
                errorContainer.style.display = 'block';
            }
        } finally {
            if (loadingSpinner) loadingSpinner.style.display = 'none';
        }
    }
    calculateEventsPerPage() {
        const width = window.innerWidth;
        if (width >= 1500) return 6;
        if (width >= 800) return 4;
        return 3;
    }
    setupResponsiveListener() {
        let resizeTimer;
        
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimer);
            
            resizeTimer = setTimeout(() => {
                const newEventsPerPage = this.calculateEventsPerPage();
                
                if (this.eventsPerPage !== newEventsPerPage) {
                    this.eventsPerPage = newEventsPerPage;
                    this.currentEventPage = 1; // Variable corregida
                    this.fetchAndDisplayEvents(); // Método de recarga corregido
                }
            }, 1); 
        });
    }
    async previousPage() {
        if (this.currentEventPage > 1) {
            this.currentEventPage--;
            await this.fetchAndDisplayEvents();
            window.scrollTo(0, 0);
        }
    }

    async nextPage() {
        this.currentEventPage++;
        await this.fetchAndDisplayEvents();
        window.scrollTo(0, 0);
    }
    
    displayEvents(events) {
        const eventsGrid = document.getElementById('eventsGrid');
        if (!eventsGrid) return;

        eventsGrid.innerHTML = events.map(event => `
            <div class="event-card" data-event-id="${event.id}">
                <div class="event-card-body">
                    <img src="${event.imageUrl ? `${API_CONFIG.BASE_URL}${event.imageUrl}` : 'assets/images/placeholder.jpg'}" 
                             alt="${escapeHtml(event.name)}" 
                             class="event-image"
                             onerror="this.src='assets/images/placeholder.jpg'">
                    <div class="event-card-header">
                        ${escapeHtml(event.name)}
                    </div>
                    <div class="event-info">
                        <span class="event-icon">📅</span>
                        <div class="event-info-text">
                            <div class="event-info-label">Fecha</div>
                            <div class="event-info-value">${formatDate(event.eventDate)}</div>
                        </div>
                    </div>
                    <div class="event-info">
                        <span class="event-icon">📍</span>
                        <div class="event-info-text">
                            <div class="event-info-label">Lugar</div>
                            <div class="event-info-value">${escapeHtml(event.venue)}</div>
                        </div>
                    </div>
                </div>
                <div class="event-card-footer">
                    <button class="btn btn-primary btn-select-event">COMPRAR</button>
                </div>
            </div>
        `).join('');
    }

    /**
     * Update pagination controls
     */
    updatePagination(result) {
        const paginationContainer = document.getElementById('paginationContainer');
        const prevBtn = document.getElementById('prevBtn');
        const nextBtn = document.getElementById('nextBtn');
        const pageInfo = document.getElementById('pageInfo');

        if (!paginationContainer) return;

        if (pageInfo) {
            pageInfo.textContent = `Página ${result.page} de ${result.totalPages}`;
        }

        if (prevBtn) {
            prevBtn.disabled = result.page === 1;
        }

        if (nextBtn) {
            nextBtn.disabled = result.page >= result.totalPages;
        }

        paginationContainer.style.display = 'flex';
    }
}