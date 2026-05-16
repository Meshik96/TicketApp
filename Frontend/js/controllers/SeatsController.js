import { apiService } from '../api.js';
import { STORAGE_KEYS, MESSAGES, API_CONFIG } from '../utils/constants.js';
import { formatDate, escapeHtml, showAlert } from '../utils/helpers.js';

export class SeatsController {
    constructor(router, eventId) {
        this.router = router;
        this.eventId = eventId;
        this.selectedSeats = [];
        this.maxSeats = 6;
        this.allSeats = [];
        this.pz = null;
        this.reservationTimer = null;
        this.initListeners();
        this.loadSeatsPage();
    }

    initListeners() {
        const mapContainer = document.getElementById('seatMapContainer');
        if (mapContainer) {
            mapContainer.addEventListener('click', (e) => {
                const seatElement = e.target.closest('.seat');
                
                const errorContainer = document.getElementById('errorContainer');
                if (errorContainer) {
                    errorContainer.style.display = 'none';
                    errorContainer.textContent = '';
                }
                
                if (seatElement && (seatElement.classList.contains('available') || seatElement.classList.contains('selected'))) {
                    e.preventDefault();
                    e.stopPropagation();
                    this.selectSeat(seatElement, e);
                }
            });
        }

        const menuBtn = document.getElementById('userMenuBtn');
        const dropdown = document.getElementById('userDropdown');
        const reservationsLink = document.getElementById('reservationsLink');

        if (menuBtn && dropdown) {
            menuBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                dropdown.classList.toggle('show');
            });

            window.addEventListener('click', () => {
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

        document.getElementById('backBtn')?.addEventListener('click', async () => {
            await this.releaseAllSelectedSeats();
            sessionStorage.removeItem(STORAGE_KEYS.CURRENT_EVENT);
            this.router.navigate('events');
        });
        
        document.getElementById('zoomIn')?.addEventListener('click', () => this.pz && this.pz.zoomIn());
        document.getElementById('zoomOut')?.addEventListener('click', () => this.pz && this.pz.zoomOut());
        document.getElementById('zoomReset')?.addEventListener('click', () => this.pz && this.pz.reset());

        document.getElementById('closeErrorBtn')?.addEventListener('click', () => {
            const errorModal = document.getElementById('purchaseErrorModal');
            if (errorModal) {
                errorModal.style.display = 'none';
            }
        });

        this.handleBeforeUnload = (e) => {
            if (this.selectedSeats.length > 0) {
                const user = apiService.getCurrentUser();
                if (user) {
                    this.selectedSeats.forEach(seat => {
                        apiService.cancelReservationOnExit(user.id, seat.id);
                    });
                }
            }
        };

        window.addEventListener('beforeunload', this.handleBeforeUnload);
    }

    async loadSeatsPage() {
        const user = JSON.parse(sessionStorage.getItem(STORAGE_KEYS.USER) || 'null');
        if (user) {
            const userNameDisplay = document.getElementById('userNameDisplay');
            if (userNameDisplay) userNameDisplay.textContent = user.name || 'Usuario';
        }

        if (!this.eventId) {
            this.eventId = sessionStorage.getItem(STORAGE_KEYS.CURRENT_EVENT);
        }

        if (!this.eventId || this.eventId === 'undefined' || this.eventId === 'null') {
            window.location.href = 'events.html';
            return;
        }

        sessionStorage.setItem(STORAGE_KEYS.CURRENT_EVENT, this.eventId);
        await this.fetchAndDisplaySeats(this.eventId);
    }

    async fetchAndDisplaySeats(eventId) {
        const loadingSpinner = document.getElementById('loadingSpinner');
        const mapContainer = document.getElementById('seatMapContainer');
        const errorContainer = document.getElementById('errorContainer');

        try {
            if (loadingSpinner) loadingSpinner.style.display = 'flex';
            if (mapContainer) mapContainer.style.opacity = '0';
            if (errorContainer) errorContainer.style.display = 'none';

            const eventsResult = await apiService.getEvents(1, 100);
            const eventDetails = eventsResult.events.find(e => e.id.toString() === eventId.toString());

            if (eventDetails) {
                this.displayEventDetails(eventDetails);
            }

            const result = await apiService.getSeats(eventId);
            this.allSeats = result.seats || [];

            if (this.allSeats.length === 0) {
                throw new Error("No hay asientos configurados para este evento.");
            }

            this.renderSeatMap(this.allSeats);

            const user = apiService.getCurrentUser();
            if (user) {
                const myReservedSeats = this.allSeats.filter(seat => 
                    seat.status === 'Reserved' && seat.userId === user.id
                );

                if (myReservedSeats.length > 0) {
                    this.selectedSeats = [...myReservedSeats];
                    
                    this.selectedSeats.forEach(seat => {
                        const seatElement = document.querySelector(`.seat[data-seat-id="${seat.id}"]`);
                        if (seatElement) {
                            seatElement.classList.remove('reserved', 'available');
                            seatElement.classList.add('selected');
                        }
                    });

                    this.updateSelectionDisplay();

                    const firstExpiration = this.selectedSeats[0].expiresAt;
                    if (firstExpiration) {
                        this.startCountdown(firstExpiration);
                    }
                }
            }

            this.initPanzoom();
            if (mapContainer) mapContainer.style.opacity = '1';

        } catch (error) {
            console.error('Error:', error);
            if (errorContainer) {
                errorContainer.textContent = error.message;
                errorContainer.style.display = 'block';
            }
        } finally {
            if (loadingSpinner) loadingSpinner.style.display = 'none';
        }
    }

    displayEventDetails(event) {
        const eventNameEl = document.getElementById('eventTitle');
        const eventDateEl = document.getElementById('eventDate');
        const eventVenueEl = document.getElementById('eventVenue');
        const eventImageEl = document.getElementById('eventImage');

        if (eventNameEl) eventNameEl.textContent = event.name;
        if (eventDateEl) eventDateEl.textContent = formatDate(event.eventDate);
        if (eventVenueEl) eventVenueEl.textContent = event.venue;
        if (eventImageEl) {
            eventImageEl.src = event.imageUrl 
                ? `${API_CONFIG.BASE_URL}${event.imageUrl}` 
                : 'assets/images/placeholder.jpg';
            eventImageEl.alt = event.name;
        }
    }

    renderSeatMap(seats) {
        const seatMap = document.getElementById('seatMap');
        if (!seatMap) return;

        seatMap.innerHTML = '';

        const sectors = {};
        let minX = 0, maxX = 0, minY = 0, maxY = 0;

        seats.forEach(seat => {
            const sId = seat.sectorId;
            if (!sectors[sId]) {
                sectors[sId] = {
                    name: seat.sectorName,
                    x: seat.sectorGridX,
                    y: seat.sectorGridY,
                    rows: {}
                };
                
                if (seat.sectorGridX < minX) minX = seat.sectorGridX;
                if (seat.sectorGridX > maxX) maxX = seat.sectorGridX;
                if (seat.sectorGridY < minY) minY = seat.sectorGridY;
                if (seat.sectorGridY > maxY) maxY = seat.sectorGridY;
            }

            const rowId = seat.rowIdentifier || '-';
            if (!sectors[sId].rows[rowId]) {
                sectors[sId].rows[rowId] = [];
            }
            sectors[sId].rows[rowId].push(seat);
        });

        const gridContainer = document.createElement('div');
        gridContainer.className = 'sectors-grid-container';
        gridContainer.style.gridTemplateColumns = `repeat(${maxX - minX + 1}, auto)`;
        gridContainer.style.gridTemplateRows = `repeat(${maxY - minY + 1}, auto)`;

        for (const [sId, sector] of Object.entries(sectors)) {
            const sectorDiv = document.createElement('div');
            sectorDiv.className = 'sector-box';
            sectorDiv.style.gridColumn = sector.x - minX + 1;
            sectorDiv.style.gridRow = sector.y - minY + 1;

            const sectorTitle = document.createElement('div');
            sectorTitle.className = 'sector-title';
            sectorTitle.textContent = sector.name;
            sectorDiv.appendChild(sectorTitle);

            const sortedRows = Object.keys(sector.rows).sort();

            sortedRows.forEach(rowId => {
                const rowDiv = document.createElement('div');
                rowDiv.className = 'seat-row';
                
                const rowLabel = document.createElement('div');
                rowLabel.className = 'row-label';
                rowLabel.textContent = rowId;
                rowDiv.appendChild(rowLabel);

                const seatsContainer = document.createElement('div');
                seatsContainer.className = 'seats-container';

                const sortedSeats = sector.rows[rowId].sort((a, b) => a.seatNumber - b.seatNumber);

                sortedSeats.forEach(seat => {
                    const seatDiv = document.createElement('div');
                    const statusClass = seat.status ? seat.status.toLowerCase() : 'available';
                    
                    seatDiv.className = `seat ${statusClass}`;
                    seatDiv.dataset.seatId = seat.id;
                    seatDiv.dataset.seatName = `${sector.name} - ${seat.rowIdentifier}${seat.seatNumber}`;
                    seatDiv.dataset.seatPrice = seat.price || 0;
                    seatDiv.textContent = seat.seatNumber;

                    seatDiv.title = `Sector: ${sector.name} | Fila: ${seat.rowIdentifier} | Asiento: ${seat.seatNumber} | Precio: $${seat.price}`;
                    
                    seatsContainer.appendChild(seatDiv);
                });

                rowDiv.appendChild(seatsContainer);
                sectorDiv.appendChild(rowDiv);
            });

            gridContainer.appendChild(sectorDiv);
        }

        seatMap.appendChild(gridContainer);
    }

    initPanzoom() {
        const elementToZoom = document.getElementById('seatMapContainer');
        const viewport = document.getElementById('zoomViewport');
        
        if (!elementToZoom || !window.Panzoom) {
            console.error('Contenedor no encontrado o librería Panzoom ausente.');
            return;
        }

        if (this.pz && typeof this.pz.destroy === 'function') {
            this.pz.destroy();
        }

        this.pz = window.Panzoom(elementToZoom, {
            maxZoom: 4,
            minZoom: 0.5,
            bounds: true,
            boundsPadding: 0.1,
            contain: 'outside',
        });
        
        if (viewport) {
            viewport.addEventListener('wheel', (event) => {
                event.preventDefault(); 
                this.pz.zoomWithWheel(event);
            }, { passive: false });
        }

        setTimeout(() => {
            this.pz.pan(0, 0);
            this.pz.zoom(1);
        }, 100);
    }

    async selectSeat(seatElement, event) { // <-- Modificación: Añadir 'event'
        if (seatElement.classList.contains('loading-seat')) return;
        const user = apiService.getCurrentUser();
        const seatId = seatElement.dataset.seatId;
        const seatIndex = this.selectedSeats.findIndex(s => s.id === seatId);

        if (seatIndex > -1) {
            try {
                await apiService.cancelReservation(user.id, seatId);

                this.selectedSeats.splice(seatIndex, 1);
                seatElement.classList.remove('selected');
                seatElement.classList.add('available');
                
                this.updateSelectionDisplay();
                this.checkTimerStatus();
            } 
            catch (error) {
                console.error("Error al cancelar:", error);
                this.showErrorModal("No se pudo liberar el asiento. Intente de nuevo.");
            }
        } 
        else {
            if (this.selectedSeats.length >= 6) {
                this.showErrorModal("Máximo 6 entradas por persona", "warning");
                return;
            }

            try {
                seatElement.classList.remove('available');
                seatElement.classList.add('loading-seat');
                
                const response = await apiService.reserveSeat(user.id, seatId);
                
                const seatData = this.allSeats.find(s => s.id.toString() === seatId.toString());
                if (seatData) {
                    seatData.expiresAt = response.expiresAt; 
                    this.selectedSeats.push(seatData);
                    
                    seatElement.classList.remove('loading-seat');
                    seatElement.classList.add('selected');
                    if (this.reservationTimer === null) {
                        this.startCountdown(response.expiresAt);
                    }
                    
                    this.updateSelectionDisplay();
                }
            } 
            catch (error) {
                seatElement.classList.remove('loading-seat');
                seatElement.classList.add('reserved');
                this.notifyReservedSeat(event); 
            }
        }
    }
    async notifyReservedSeat(event) {
        const warning = document.getElementById('seat-warning');
        warning.style.left = `${event.clientX + 15}px`;
        warning.style.top = `${event.clientY + 15}px`;
        
        warning.classList.remove('warning-hidden');
        warning.classList.add('warning-visible');

        setTimeout(() => {
            warning.classList.remove('warning-visible');
            warning.classList.add('warning-hidden');
        }, 3000);
    }
    startCountdown(expirationTimestamp) {
        if (this.reservationTimer) clearInterval(this.reservationTimer);

        let dateStr = expirationTimestamp;
        if (typeof dateStr === 'string' && !dateStr.includes('Z') && !dateStr.includes('+')) {
            dateStr += 'Z';
        }

        let expirationDate = new Date(dateStr).getTime();
        const now = new Date().getTime();

        if (isNaN(expirationDate) || Math.abs(expirationDate - now) > 86400000) {
            expirationDate = now + (5 * 60 * 1000); // 
        }

        const update = () => {
            const currentTime = new Date().getTime();
            const distance = expirationDate - currentTime;

            if (distance <= 0) {
                clearInterval(this.reservationTimer);
                this.reservationTimer = null;
                this.handleExpiration();
                return;
            }
            this.updateTimerUI(distance);
        };

        update();
        this.reservationTimer = setInterval(update, 1000);
    }

    updateTimerUI(distance) {
        const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((distance % (1000 * 60)) / 1000);
        
        const timerEl = document.getElementById('reservationTimer');
        if (timerEl) {
            timerEl.textContent = `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
            timerEl.classList.toggle('timer-warning', minutes < 1);
        }
    }

    checkTimerStatus() {
        if (this.selectedSeats.length === 0 && this.reservationTimer) {
            clearInterval(this.reservationTimer);
            this.reservationTimer = null;
            
            const timerEl = document.getElementById('reservationTimer');
            if (timerEl) {
                timerEl.textContent = '';
            }
        }
    }

    handleExpiration() {
        this.showErrorModal("El tiempo de reserva ha expirado. Los asientos seleccionados han sido liberados.");
        setTimeout(() => {
            const errorModal = document.getElementById('purchaseErrorModal');
            if (errorModal) {
                errorModal.style.display = 'none';
            }
            this.router.navigate('events');
        }, 3000);
    }
    
    clearSeatSelection() {
        const selected = document.querySelector('.seat.selected');
        if (selected) selected.classList.remove('selected');
        this.selectedSeat = null;
        this.updateSelectionDisplay();
    }

    updateSelectionDisplay() {
        const selectedSeatsDisplay = document.getElementById('selectedSeatsDisplay');
        const buyBtn = document.getElementById('buyBtn'); 
        const totalPriceEl = document.getElementById('totalPrice');

        if (this.selectedSeats.length > 0) {
            selectedSeatsDisplay.innerHTML = this.selectedSeats.map(seat => `
                <div class="event-info-row selected-seat-item" style="display: flex; justify-content: space-between; align-items: center;">
                    <div style="display: flex; align-items: center; gap: 10px;">
                        <div class="icon-box">💺</div>
                        <div class="info-content">
                            <span class="info-label">${escapeHtml(seat.sectorName)}</span>
                            <span class="info-value">Fila ${escapeHtml(seat.rowIdentifier)} - Asiento ${seat.seatNumber}</span>
                        </div>
                    </div>
                    <button class="remove-seat-btn" data-seat-id="${seat.id}" style="background: none; border: none; color: #dc3545; cursor: pointer; font-size: 1.2rem; padding: 0 5px;" title="Quitar asiento">
                        ✖
                    </button>
                </div>
            `).join('');

            document.querySelectorAll('.remove-seat-btn').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    const seatId = e.currentTarget.dataset.seatId;
                    this.removeSeatFromList(seatId);
                });
            });

            const total = this.selectedSeats.reduce((sum, s) => sum + (s.price || 0), 0);
            totalPriceEl.textContent = `$ ${total.toLocaleString('es-AR')}`;
            
            buyBtn.disabled = false;
            buyBtn.textContent = `CONTINUAR (${this.selectedSeats.length})`;
            buyBtn.onclick = () => this.goToPayment(); 
        } else {
            selectedSeatsDisplay.innerHTML = '<p style="text-align:center; color:#6c757d;">No hay asientos seleccionados.</p>';
            totalPriceEl.textContent = '$ 0';
            buyBtn.disabled = true;
            buyBtn.textContent = 'CONTINUAR';
        }
    }

    removeSeatFromList(seatId) {
        const seatElement = document.querySelector(`.seat[data-seat-id="${seatId}"]`);
        
        if (seatElement) {
            this.selectSeat(seatElement);
        } else {
            const seatIndex = this.selectedSeats.findIndex(s => s.id.toString() === seatId.toString());
            if (seatIndex > -1) {
                this.selectedSeats.splice(seatIndex, 1);
                this.updateSelectionDisplay();
            }
        }
    }

    async goToPayment() {
        if (this.selectedSeats.length === 0) return;

        sessionStorage.setItem('CHECKOUT_SEATS', JSON.stringify(this.selectedSeats));
        
        if (this.selectedSeats.length > 0) {
            sessionStorage.setItem('CHECKOUT_EXPIRES', JSON.stringify(this.selectedSeats[0].expiresAt));
        }

        window.removeEventListener('beforeunload', this.handleBeforeUnload);
        this.router.navigate('checkout');
    }

    async releaseAllSelectedSeats() {
        if (this.selectedSeats.length === 0) return;
    
        const user = apiService.getCurrentUser();
        if (!user) return;
    
        const cancelPromises = this.selectedSeats.map(seat => 
            apiService.cancelReservation(user.id, seat.id).catch(err => console.error(err))
        );
    
        await Promise.all(cancelPromises);
        this.selectedSeats = [];
    }

    showErrorModal(errorMessage) {
        const modal = document.getElementById('purchaseErrorModal');
        if (!modal) return;

        const headerEl = modal.querySelector('h2');
        if (headerEl) {
            headerEl.textContent = '✗ Error de Reserva';
        }

        const errorMessageEl = document.getElementById('errorMessage');
        if (errorMessageEl) {
            errorMessageEl.textContent = errorMessage;
        }

        modal.style.display = 'flex';
    }
}