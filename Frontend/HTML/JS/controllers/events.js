import { formatDate, escapeHtml } from '../utils/helpers.js';
import { apiService } from '../api.js';
import { PAGINATION, API_CONFIG, STORAGE_KEYS } from '../utils/constants.js';

export class EventsController {
    constructor(router) {
        this.router = router;
        this.currentEventPage = 1;
        this.pageSize = PAGINATION.DEFAULT_PAGE_SIZE; // Definir constante local o importar
        this.initListeners();
        this.loadEventsPage();
    }

    initListeners() {
        document.getElementById('eventsGrid')?.addEventListener('click', (e) => {
            // Buscar el botón COMPRAR clickeado
            const button = e.target.closest('.btn-select-event');
            if (button) {
                // Desde el botón, buscar la tarjeta de evento más cercana
                const eventCard = button.closest('.event-card');
                if (eventCard) {
                    const eventId = eventCard.dataset.eventId;
                    console.log('Event ID:', eventId); // Debug
                    if (!eventId) {
                        console.error('No event ID found in data-event-id');
                        return;
                    }
                    localStorage.setItem(STORAGE_KEYS.CURRENT_EVENT, eventId);
                    console.log('Navigating to seats with eventId:', eventId); // Debug
                    window.location.href = `seats.html?eventId=${eventId}`;
                }
            }
        });

        document.getElementById('prevBtn')?.addEventListener('click', () => this.previousPage());
        document.getElementById('nextBtn')?.addEventListener('click', () => this.nextPage());
    }

    async loadEventsPage() {
        const user = apiService.getCurrentUser();
        if (user) {
            const userNameDisplay = document.getElementById('userNameDisplay');
            if (userNameDisplay) userNameDisplay.textContent = user.name || user.email;
        }
        await this.fetchAndDisplayEvents();
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

            const result = await apiService.getEvents(this.currentEventPage, this.pageSize);
            
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
                <div class="event-card-header">
                    ${escapeHtml(event.name)}
                </div>
                <div class="event-card-body">
                    <img src="${event.imageUrl ? `${API_CONFIG.BASE_URL}${event.imageUrl}` : 'assets/images/placeholder.jpg'}" 
                             alt="${escapeHtml(event.name)}" 
                             class="event-image"
                             onerror="this.src='assets/images/placeholder.jpg'">
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