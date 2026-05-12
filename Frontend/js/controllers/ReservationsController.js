import { apiService } from '../api.js';
import { STORAGE_KEYS } from '../utils/constants.js';
import { formatDate, escapeHtml } from '../utils/helpers.js';

export class ReservationsController {
    constructor(router) {
        this.router = router;
        this.reservations = [];
        this.initListeners();
        this.loadReservations();
    }

    initListeners() {
        const menuBtn = document.getElementById('userMenuBtn');
        const dropdown = document.getElementById('userDropdown');
        if (menuBtn && dropdown) {
            menuBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                dropdown.classList.toggle('show');
            });

            window.addEventListener('click', (e) => {
                if (dropdown.classList.contains('show')) {
                    dropdown.classList.remove('show');
                }
            });
        }

        document.getElementById('logoutBtn')?.addEventListener('click', () => {
            apiService.logout();
        });

        document.getElementById('backBtn')?.addEventListener('click', () => {
            this.router.navigate('events');
        });

        document.getElementById('browseEventsBtn')?.addEventListener('click', () => {
            this.router.navigate('events');
        });

        document.addEventListener('click', (e) => {
            if (e.target.id === 'logoutBtn') {
                apiService.logout();
                sessionStorage.removeItem(STORAGE_KEYS.USER);
                this.router.navigate('events');
            }
        });
    }

    async loadReservations() {
        const user = JSON.parse(sessionStorage.getItem(STORAGE_KEYS.USER) || 'null');
        
        if (user) {
            const userNameDisplay = document.getElementById('userNameDisplay');
            if (userNameDisplay) userNameDisplay.textContent = user.name || 'Usuario';
        }

        if (!user) {
            this.router.navigate('events');
            return;
        }

        const loadingSpinner = document.getElementById('loadingSpinner');
        const errorContainer = document.getElementById('errorContainer');
        const reservationsContainer = document.getElementById('reservationsContainer');
        const emptyState = document.getElementById('emptyState');

        try {
            if (loadingSpinner) loadingSpinner.style.display = 'flex';
            if (errorContainer) errorContainer.style.display = 'none';
            if (reservationsContainer) reservationsContainer.style.display = 'none';
            if (emptyState) emptyState.style.display = 'none';

            // Obtener las reservas del usuario
            const result = await apiService.getUserReservations(user.id);
            this.reservations = result.reservations || [];

            if (this.reservations.length === 0) {
                if (emptyState) emptyState.style.display = 'flex';
            } else {
                this.displayReservations();
                if (reservationsContainer) reservationsContainer.style.display = 'grid';
            }

        } catch (error) {
            console.error('Error cargando reservas:', error);
            if (errorContainer) {
                errorContainer.textContent = error.message || 'Error al cargar las reservas';
                errorContainer.style.display = 'block';
            }
        } finally {
            if (loadingSpinner) loadingSpinner.style.display = 'none';
        }
    }

    displayReservations() {
        const reservationsContainer = document.getElementById('reservationsContainer');
        if (!reservationsContainer) return;

        // Agrupar reservas por evento
        const reservationsByEvent = {};
        
        this.reservations.forEach(reservation => {
            const eventKey = reservation.eventId || 'unknown';
            if (!reservationsByEvent[eventKey]) {
                reservationsByEvent[eventKey] = {
                    event: reservation,
                    seats: []
                };
            }
            reservationsByEvent[eventKey].seats.push(reservation);
        });

        // Crear HTML para cada evento con sus asientos
        const reservationCards = Object.values(reservationsByEvent).map(group => `
            <div class="reservation-card">
                <div class="reservation-header">
                    <h3>${escapeHtml(group.event.eventName || 'Evento')}</h3>
                    <span class="reservation-status ${this.getStatusClass(group.event.status)}">
                        ${this.getStatusLabel(group.event.status)}
                    </span>
                </div>
                
                <div class="reservation-details">
                    <div class="detail-row">
                        <span class="detail-label">📅 Fecha:</span>
                        <span class="detail-value">${formatDate(group.event.eventDate)}</span>
                    </div>
                    
                    <div class="detail-row">
                        <span class="detail-label">📍 Lugar:</span>
                        <span class="detail-value">${escapeHtml(group.event.venueName || 'N/A')}</span>
                    </div>
                    
                    <div class="detail-row">
                        <span class="detail-label">💺 Asientos:</span>
                        <span class="detail-value">${this.formatSeats(group.seats)}</span>
                    </div>
                    
                    ${group.event.bookingId ? `
                    <div class="detail-row">
                        <span class="detail-label">🎫 Número de Reserva:</span>
                        <span class="detail-value booking-id">${escapeHtml(group.event.bookingId)}</span>
                    </div>
                    ` : ''}
                    
                    ${group.event.totalPrice ? `
                    <div class="detail-row">
                        <span class="detail-label">💵 Total:</span>
                        <span class="detail-value">$${group.event.totalPrice.toLocaleString('es-AR')}</span>
                    </div>
                    ` : ''}
                </div>
                
                <div class="reservation-actions">
                    <button class="btn btn-secondary" onclick="alert('Descarga de entrada no implementada aún')">
                        Descargar Entrada
                    </button>
                </div>
            </div>
        `).join('');

        reservationsContainer.innerHTML = reservationCards;
    }

    formatSeats(seats) {
        return seats.map(seat => 
            `${escapeHtml(seat.sectorName)} - Fila ${escapeHtml(seat.rowIdentifier)} - Asiento ${seat.seatNumber}`
        ).join('<br>');
    }

    getStatusLabel(status) {
        const statusMap = {
            'confirmed': 'Confirmada',
            'purchased': 'Comprada',
            'pending': 'Pendiente',
            'expired': 'Expirada',
            'cancelled': 'Cancelada'
        };
        return statusMap[status?.toLowerCase()] || 'Desconocida';
    }

    getStatusClass(status) {
        const classMap = {
            'confirmed': 'status-confirmed',
            'purchased': 'status-confirmed',
            'pending': 'status-pending',
            'expired': 'status-expired',
            'cancelled': 'status-cancelled'
        };
        return classMap[status?.toLowerCase()] || 'status-unknown';
    }
}
