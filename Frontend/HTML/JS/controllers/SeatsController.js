import { apiService } from '../api.js';
import { STORAGE_KEYS, MESSAGES } from '../utils/constants.js';
import { formatDate, escapeHtml, showAlert } from '../utils/helpers.js';

export class SeatsController {
    constructor(router, eventId) {
        this.router = router;
        this.eventId = eventId;
        this.selectedSeat = null;
        this.allSeats = [];
        this.pz = null;
        
        this.initListeners();
        this.loadSeatsPage();
    }

    initListeners() {
        const mapContainer = document.getElementById('seatMapContainer');
        if (mapContainer) {
            mapContainer.addEventListener('click', (e) => {
                const seatElement = e.target.closest('.seat');
                if (seatElement && seatElement.classList.contains('available')) {
                    this.selectSeat(seatElement);
                }
            });
        }

        document.getElementById('reserveBtn')?.addEventListener('click', () => this.handleReservation());
        document.getElementById('clearSelectionBtn')?.addEventListener('click', () => this.clearSeatSelection());
        document.getElementById('backBtn')?.addEventListener('click', () => {
            localStorage.removeItem(STORAGE_KEYS.CURRENT_EVENT);
            this.router.navigate('events');
        });
        
        document.getElementById('zoomIn')?.addEventListener('click', () => this.pz && this.pz.zoomIn());
        document.getElementById('zoomOut')?.addEventListener('click', () => this.pz && this.pz.zoomOut());
        document.getElementById('zoomReset')?.addEventListener('click', () => this.pz && this.pz.reset());
    }

    async loadSeatsPage() {
        const user = apiService.getCurrentUser();
        if (user) {
            const userNameDisplay = document.getElementById('userNameDisplay');
            if (userNameDisplay) userNameDisplay.textContent = user.name || user.email;
        }

        if (!this.eventId) {
            this.eventId = localStorage.getItem(STORAGE_KEYS.CURRENT_EVENT);
        }

        if (!this.eventId || this.eventId === 'undefined' || this.eventId === 'null') {
            window.location.href = 'events.html';
            return;
        }

        localStorage.setItem(STORAGE_KEYS.CURRENT_EVENT, this.eventId);
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

            const result = await apiService.getSeats(eventId);
            this.allSeats = result.seats || [];

            const eventsResult = await apiService.getEvents(1, 100);
            const eventDetails = eventsResult.events.find(e => e.id.toString() === eventId.toString());

            if (eventDetails) {
                this.displayEventDetails(eventDetails);
            }

            if (this.allSeats.length === 0) {
                throw new Error("No hay asientos configurados para este evento.");
            }

            this.renderSeatMap(this.allSeats);
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

        if (eventNameEl) eventNameEl.textContent = event.name;
        if (eventDateEl) eventDateEl.textContent = formatDate(event.eventDate);
        if (eventVenueEl) eventVenueEl.textContent = event.venue;
    }

    renderSeatMap(seats) {
        const seatMap = document.getElementById('seatMap');
        if (!seatMap) return;

        seatMap.innerHTML = '';

        const sectors = {};
        let minX = 0, maxX = 0, minY = 0, maxY = 0;

        // Agrupar por sector y calcular límites espaciales del grid
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

        // Contenedor principal de sectores configurado dinámicamente según coordenadas
        const gridContainer = document.createElement('div');
        gridContainer.className = 'sectors-grid-container';
        gridContainer.style.gridTemplateColumns = `repeat(${maxX - minX + 1}, auto)`;
        gridContainer.style.gridTemplateRows = `repeat(${maxY - minY + 1}, auto)`;

        for (const [sId, sector] of Object.entries(sectors)) {
            const sectorDiv = document.createElement('div');
            sectorDiv.className = 'sector-box';
            // Offset espacial basado en las coordenadas mínimas para evitar valores negativos en CSS Grid
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
            contain: 'outside'
        });
        
        if (viewport) {
            viewport.addEventListener('wheel', (event) => {
                // Evita que la página haga scroll vertical al hacer zoom
                event.preventDefault(); 
                this.pz.zoomWithWheel(event);
            }, { passive: false });
        }

        setTimeout(() => {
            this.pz.pan(0, 0);
            this.pz.zoom(1);
        }, 100);
    }

    selectSeat(seatElement) {
        const previousSelected = document.querySelector('.seat.selected');
        if (previousSelected) previousSelected.classList.remove('selected');

        seatElement.classList.add('selected');
        
        const seatId = seatElement.dataset.seatId;
        this.selectedSeat = this.allSeats.find(s => s.id.toString() === seatId.toString());
        
        this.updateSelectionDisplay();
    }

    clearSeatSelection() {
        const selected = document.querySelector('.seat.selected');
        if (selected) selected.classList.remove('selected');
        this.selectedSeat = null;
        this.updateSelectionDisplay();
    }

    updateSelectionDisplay() {
        const selectedSeatsDisplay = document.getElementById('selectedSeatsDisplay');
        const reserveBtn = document.getElementById('reserveBtn');
        const totalPriceEl = document.getElementById('totalPrice');

        if (this.selectedSeat) {
            if (selectedSeatsDisplay) {
                selectedSeatsDisplay.innerHTML = `
                    <div class="event-info-row">
                        <div class="icon-box">💺</div>
                        <div class="info-content">
                            <span class="info-label">Butacas</span>
                            <span class="info-value">${escapeHtml(this.selectedSeat.name)}</span>
                        </div>
                    </div>
                `;
            }
            if (totalPriceEl) totalPriceEl.textContent = `$ ${this.selectedSeat.price.toFixed(2)}`;
            if (reserveBtn) reserveBtn.disabled = false;
        } else {
            if (selectedSeatsDisplay) {
                selectedSeatsDisplay.innerHTML = `
                    <div class="event-info-row">
                        <div class="icon-box">💺</div>
                        <div class="info-content">
                            <span class="info-label">Butacas</span>
                            <span class="info-value">-</span>
                        </div>
                    </div>
                `;
            }
            if (totalPriceEl) totalPriceEl.textContent = `$ 0.00`;
            if (reserveBtn) reserveBtn.disabled = true;
        }
    }

    async handleReservation() {
        if (!this.selectedSeat) return;

        const user = apiService.getCurrentUser();
        const reserveBtn = document.getElementById('reserveBtn');

        try {
            if (reserveBtn) reserveBtn.disabled = true;
            reserveBtn.textContent = 'Reservando...';

            await apiService.reserveSeat(user.id, this.selectedSeat.id);
            showAlert(MESSAGES.RESERVATION_SUCCESS, 'success');
            
            await this.fetchAndDisplaySeats(this.eventId);
            this.clearSeatSelection();

        } catch (error) {
            console.error('Error reserving seat:', error);
            showAlert(error.message || MESSAGES.RESERVATION_ERROR, 'danger');
        } finally {
            if (reserveBtn) {
                reserveBtn.disabled = false;
                reserveBtn.textContent = 'Reservar Asiento';
            }
        }
    }
}