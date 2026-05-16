import { apiService } from '../api.js';
import { STORAGE_KEYS, MESSAGES, API_CONFIG } from '../utils/constants.js';
import { formatDate, escapeHtml, showAlert } from '../utils/helpers.js';

export class CheckoutController {
    constructor(router) {
        this.router = router;
        this.selectedSeats = [];
        this.eventDetails = null;
        this.checkoutTimer = null;
        this.expiresAt = null;
        this.initListeners();
        this.loadCheckoutPage();
    }

    initListeners() {
        // User menu
        const menuBtn = document.getElementById('userMenuBtn');
        const dropdown = document.getElementById('userDropdown');
        const reservationsLink = document.getElementById('reservationsLink');

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

        if (reservationsLink) {
            reservationsLink.addEventListener('click', (e) => {
                e.preventDefault();
                if (dropdown) dropdown.classList.remove('show');
                this.router.navigate('reservations');
            });
        }

        // Back to seats button
        document.getElementById('backToSeatsBtn')?.addEventListener('click', () => {
            this.router.navigate('seats');
        });

        // Form submission
        document.getElementById('checkoutForm')?.addEventListener('submit', (e) => {
            e.preventDefault();
            this.handlePayment();
        });

        // Card number formatting
        const cardNumberInput = document.getElementById('cardNumber');
        if (cardNumberInput) {
            cardNumberInput.addEventListener('input', (e) => {
                let value = e.target.value.replace(/\s/g, '');
                let formattedValue = value.replace(/(\d{4})(?=\d)/g, '$1 ');
                e.target.value = formattedValue;
            });
        }

        // Card expiry formatting
        const cardExpiryInput = document.getElementById('cardExpiry');
        if (cardExpiryInput) {
            cardExpiryInput.addEventListener('input', (e) => {
                let value = e.target.value.replace(/\D/g, '');
                if (value.length >= 2) {
                    value = value.slice(0, 2) + '/' + value.slice(2, 4);
                }
                e.target.value = value;
            });
        }

        // Modal buttons
        document.getElementById('backToEventsBtn')?.addEventListener('click', () => {
            this.closeAllModals();
            sessionStorage.removeItem(STORAGE_KEYS.CURRENT_EVENT);
            this.router.navigate('events');
        });

        document.getElementById('continueShoppingBtn')?.addEventListener('click', () => {
            this.closeAllModals();
            this.router.navigate('seats');
        });

        document.getElementById('viewReservationsBtn')?.addEventListener('click', () => {
            this.closeAllModals();
            this.router.navigate('reservations');
        });

        document.getElementById('closeErrorBtn')?.addEventListener('click', () => {
            this.closeAllModals();
        });

        // Logout
        document.getElementById('logoutBtn')?.addEventListener('click', () => {
            apiService.logout();
            sessionStorage.removeItem(STORAGE_KEYS.USER);
        });
    }

    async loadCheckoutPage() {
        const user = JSON.parse(sessionStorage.getItem(STORAGE_KEYS.USER) || 'null');
        if (user) {
            const userNameDisplay = document.getElementById('userNameDisplay');
            if (userNameDisplay) userNameDisplay.textContent = user.name || 'Usuario';
        }

        // Get event ID and selected seats from sessionStorage
        const eventId = sessionStorage.getItem(STORAGE_KEYS.CURRENT_EVENT);
        const seatsData = JSON.parse(sessionStorage.getItem('CHECKOUT_SEATS') || '[]');

        if (!eventId || seatsData.length === 0) {
            // No data, redirect back to seats
            this.router.navigate('seats');
            return;
        }

        this.selectedSeats = seatsData;

        try {
            // Fetch event details
            const eventsResult = await apiService.getEvents(1, 100);
            this.eventDetails = eventsResult.events.find(e => e.id.toString() === eventId.toString());

            if (this.eventDetails) {
                this.displayEventDetails();
            }

            // Display seats and calculate totals
            this.displaySeatsAndPricing();

            // Start countdown
            const expirationData = JSON.parse(sessionStorage.getItem('CHECKOUT_EXPIRES') || 'null');
            if (expirationData) {
                this.expiresAt = expirationData;
                this.startCountdown(expirationData);
            }

            // Pre-fill buyer info if available
            this.preFillBuyerInfo(user);

        } catch (error) {
            console.error('Error loading checkout:', error);
            showAlert('Error al cargar el checkout. Intenta de nuevo.', 'error');
            this.router.navigate('seats');
        }
    }

    displayEventDetails() {
        const eventNameEl = document.getElementById('summaryEventName');
        const eventDateEl = document.getElementById('summaryEventDate');
        const eventVenueEl = document.getElementById('summaryEventVenue');

        if (eventNameEl) eventNameEl.textContent = this.eventDetails.name;
        if (eventDateEl) eventDateEl.textContent = formatDate(this.eventDetails.eventDate);
        if (eventVenueEl) eventVenueEl.textContent = this.eventDetails.venue;
    }

    displaySeatsAndPricing() {
        const seatsList = document.getElementById('seatsSummaryList');
        if (!seatsList) return;

        seatsList.innerHTML = this.selectedSeats.map(seat => `
            <div class="seat-summary-item">
                <span class="seat-info">${escapeHtml(seat.sectorName)} - Fila ${escapeHtml(seat.rowIdentifier)} - Asiento ${seat.seatNumber}</span>
                <span class="seat-price">$${(seat.price || 0).toLocaleString('es-AR')}</span>
            </div>
        `).join('');

        // Calculate total
        const total = this.selectedSeats.reduce((sum, s) => sum + (s.price || 0), 0);

        const totalEl = document.getElementById('summaryTotal');
        if (totalEl) totalEl.textContent = `$${total.toLocaleString('es-AR')}`;
    }

    preFillBuyerInfo(user) {
        if (user) {
            document.getElementById('buyerEmail').value = user.email || '';
            
            // Try to split name into first and last
            const nameParts = (user.name || '').split(' ');
            if (nameParts.length > 0) {
                document.getElementById('buyerFirstName').value = nameParts[0];
                if (nameParts.length > 1) {
                    document.getElementById('buyerLastName').value = nameParts.slice(1).join(' ');
                }
            }
        }
    }

    startCountdown(expirationTimestamp) {
        if (this.checkoutTimer) clearInterval(this.checkoutTimer);

        let dateStr = expirationTimestamp;
        if (typeof dateStr === 'string' && !dateStr.includes('Z') && !dateStr.includes('+')) {
            dateStr += 'Z';
        }

        let expirationDate = new Date(dateStr).getTime();
        const now = new Date().getTime();

        if (isNaN(expirationDate) || Math.abs(expirationDate - now) > 86400000) {
            expirationDate = now + (5 * 60 * 1000);
        }

        const update = () => {
            const currentTime = new Date().getTime();
            const distance = expirationDate - currentTime;

            if (distance <= 0) {
                clearInterval(this.checkoutTimer);
                this.checkoutTimer = null;
                this.handleExpiration();
                return;
            }
            this.updateTimerUI(distance);
        };

        update();
        this.checkoutTimer = setInterval(update, 1000);
    }

    updateTimerUI(distance) {
        const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((distance % (1000 * 60)) / 1000);
        
        const timerEl = document.getElementById('checkoutTimer');
        if (timerEl) {
            timerEl.textContent = `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
            timerEl.classList.toggle('timer-danger', minutes < 1);
        }
    }

    handleExpiration() {
        this.showErrorModal('El tiempo de compra ha expirado. Los asientos han sido liberados. Intenta nuevamente desde la página de asientos.');
        
        // Clear stored checkout data
        sessionStorage.removeItem('CHECKOUT_SEATS');
        sessionStorage.removeItem('CHECKOUT_EXPIRES');
        setTimeout(() => {
            this.closeAllModals();
            this.router.navigate('events');
        }, 3000);
    }

    async handlePayment() {
        const payBtn = document.getElementById('payBtn');
        const errorContainer = document.getElementById('checkoutError');

        try {
            payBtn.disabled = true;
            payBtn.textContent = 'PROCESANDO PAGO...';

            if (errorContainer) {
                errorContainer.style.display = 'none';
                errorContainer.textContent = '';
            }

            const user = apiService.getCurrentUser();
            const seatIds = this.selectedSeats.map(seat => seat.id);

            // Simulate payment processing (in real app, this would call a payment processor)
            await new Promise(resolve => setTimeout(resolve, 1500));

            // Call backend to finalize purchase
            const response = await apiService.buySeats(user.id, seatIds);

            // Stop timer
            if (this.checkoutTimer) {
                clearInterval(this.checkoutTimer);
                document.getElementById('checkoutTimer').textContent = '';
            }

            // Clear checkout data
            sessionStorage.removeItem('CHECKOUT_SEATS');
            sessionStorage.removeItem('CHECKOUT_EXPIRES');

            // Show success modal
            this.showSuccessModal(response);
            setTimeout(() => {
            this.closeAllModals();
            this.router.navigate('events');
        }, 15000);

        } catch (error) {
            console.error('Error in payment:', error);
            
            if (errorContainer) {
                errorContainer.textContent = error.message || 'Ha ocurrido un error al procesar el pago. Intenta de nuevo.';
                errorContainer.style.display = 'block';
            }

            this.showErrorModal(error.message || 'Ha ocurrido un error al procesar tu compra.');
            
            payBtn.disabled = false;
            payBtn.textContent = 'PAGAR AHORA';

            if (error.message.toLowerCase().includes("expirad")) {
                this.handleExpiration();
            }
        }
    }

    showSuccessModal(response) {
        const modal = document.getElementById('purchaseSuccessModal');
        if (!modal) return;

        const reservationDetails = document.getElementById('reservationDetails');
        
        let detailsHtml = '<div class="details-box">';
        
        if (response.bookingIds && response.bookingIds.length > 0) {
            detailsHtml += `<p><strong>Número(s) de Reserva:</strong> ${response.bookingIds.join(', ')}</p>`;
        }
        
        if (this.selectedSeats && this.selectedSeats.length > 0) {
            detailsHtml += '<p><strong>Asientos:</strong></p><ul>';
            this.selectedSeats.forEach(seat => {
                detailsHtml += `<li>${seat.sectorName} - Fila ${seat.rowIdentifier} - Asiento ${seat.seatNumber}</li>`;
            });
            detailsHtml += '</ul>';
        }
        
        const total = this.selectedSeats.reduce((sum, s) => sum + (s.price || 0), 0);
        detailsHtml += `<p><strong>Monto Total:</strong> $${total.toLocaleString('es-AR')}</p>`;
        
        detailsHtml += '</div>';
        
        if (reservationDetails) {
            reservationDetails.innerHTML = detailsHtml;
        }

        modal.style.display = 'flex';
    }

    showErrorModal(errorMessage) {
        const modal = document.getElementById('purchaseErrorModal');
        if (!modal) return;

        const errorMessageEl = document.getElementById('errorMessage');
        if (errorMessageEl) {
            errorMessageEl.textContent = errorMessage;
        }

        modal.style.display = 'flex';
    }

    closeAllModals() {
        const modals = [
            'purchaseSuccessModal',
            'purchaseErrorModal',
            'loginModal'
        ];

        modals.forEach(modalId => {
            const modal = document.getElementById(modalId);
            if (modal) {
                modal.style.display = 'none';
            }
        });
    }
}
