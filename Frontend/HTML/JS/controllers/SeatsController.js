import { apiService } from '../api.js';
import { STORAGE_KEYS, MESSAGES } from '../utils/constants.js';
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
                if (seatElement && seatElement.classList.contains('available')) {
                e.preventDefault(); // Bloquea recargas accidentales
                e.stopPropagation(); // Evita que el evento suba a otros padres
                
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

    async selectSeat(seatElement) {
        // 1. Bloqueo inmediato de interacción para evitar doble clic
        if (seatElement.classList.contains('loading-seat')) return;
        const user = apiService.getCurrentUser();
        const seatId = seatElement.dataset.seatId;
        const seatIndex = this.selectedSeats.findIndex(s => s.id === seatId);

        if (seatIndex > -1) {
            // CASO: DESELECCIONAR (El usuario hizo clic en un asiento ya elegido)
            try {
                const user = apiService.getCurrentUser();
                
                // Llamada al método que agregaste en api.js
                await apiService.cancelReservation(user.id, seatId);

                // Si el backend responde OK, actualizamos la UI
                this.selectedSeats.splice(seatIndex, 1);
                seatElement.classList.remove('selected');
                seatElement.classList.add('available');
                
                this.updateSelectionDisplay();
                this.checkTimerStatus(); // Nueva función para limpiar el timer si no hay asientos
            } 
            catch (error) {
                console.error("Error al cancelar:", error);
                alert("No se pudo liberar el asiento. Intente de nuevo.");
            }
        } 
        else {
            // Lógica de selección y reserva
            if (this.selectedSeats.length >= 6) {
                showAlert("Máximo 6 entradas por persona", "warning");
                return;
            }

            try {
                // FEEDBACK VISUAL INMEDIATO
                seatElement.classList.remove('available');
                seatElement.classList.add('loading-seat');
                
                const response = await apiService.reserveSeat(user.id, seatId);
                
                const seatData = this.allSeats.find(s => s.id.toString() === seatId.toString());
                if (seatData) {
                    seatData.expiresAt = response.expiresAt; 
                    this.selectedSeats.push(seatData);
                    
                    // ACTUALIZACIÓN DE UI SIN RECARGAR
                    seatElement.classList.remove('loading-seat');
                    seatElement.classList.add('selected');
                    
                    this.startCountdown(response.expiresAt);
                    this.updateSelectionDisplay();
                    
                    // Solo un log o alerta pequeña para no interrumpir
                    console.log(`Asiento ${seatId} reservado hasta ${response.expiresAt}`);
                }
            } 
            catch (error) {
                console.error('Error al reservar:', error);
                // REVERTIR CAMBIOS SI FALLA
                seatElement.classList.remove('loading-seat');
                seatElement.classList.add('available');
                showAlert(error.message || "No se pudo reservar el asiento", 'danger');
            }
        }
    }
    startCountdown(expiresAt) {
        if (this.reservationTimer) clearInterval(this.reservationTimer);
        const expirationDate = new Date().getTime() + (5 * 60 * 1000);

        this.reservationTimer = setInterval(() => {
            const now = new Date().getTime();
            const distance = expirationDate - now;

            if (distance < 0) {
                clearInterval(this.reservationTimer);
                this.handleExpiration();
                return;
            }

            this.updateTimerUI(distance);
        }, 1000);
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
    // Si no quedan asientos seleccionados, detenemos y limpiamos el timer
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
        showAlert("Tu reserva ha expirado", "warning");
        this.selectedSeats = [];
        this.fetchAndDisplaySeats(this.eventId); // Recargar mapa para ver asientos liberados
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

            // Asignar eventos a los botones recién creados
            document.querySelectorAll('.remove-seat-btn').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    const seatId = e.currentTarget.dataset.seatId;
                    this.removeSeatFromList(seatId);
                });
            });

            const total = this.selectedSeats.reduce((sum, s) => sum + (s.price || 0), 0);
            totalPriceEl.textContent = `$ ${total.toLocaleString('es-AR')}`;
            
            buyBtn.disabled = false;
            buyBtn.textContent = `COMPRAR (${this.selectedSeats.length})`;
            buyBtn.onclick = () => this.goToPayment(); 
        } else {
            // Estado vacío
            selectedSeatsDisplay.innerHTML = '<p style="text-align:center; color:#6c757d;">No hay asientos seleccionados.</p>';
            totalPriceEl.textContent = '$ 0';
            buyBtn.disabled = true;
            buyBtn.textContent = 'COMPRAR';
        }
    }
    removeSeatFromList(seatId) {
        // Buscar el div del asiento en el mapa usando el atributo data
        const seatElement = document.querySelector(`.seat[data-seat-id="${seatId}"]`);
        
        if (seatElement) {
            // Al pasarle el elemento a tu método selectSeat, este detecta el index > -1 y hace la deselección
            this.selectSeat(seatElement);
        } else {
            // Fallback: Si no encuentra el elemento en el DOM, lo saca del array de todas formas
            const seatIndex = this.selectedSeats.findIndex(s => s.id.toString() === seatId.toString());
            if (seatIndex > -1) {
                this.selectedSeats.splice(seatIndex, 1);
                this.updateSelectionDisplay();
            }
        }
    }

    async goToPayment() {
        if (this.selectedSeats.length === 0) return;

        const user = apiService.getCurrentUser();
        const seatIds = this.selectedSeats.map(seat => seat.id);
        const buyBtn = document.getElementById('buyBtn');

        try {
            // Deshabilitar botón durante la transacción
            buyBtn.disabled = true;
            buyBtn.textContent = 'PROCESANDO...';

            // Llamada al backend
            await apiService.buySeats(user.id, seatIds);

            // Detener el temporizador de expiración
            if (this.reservationTimer) {
                clearInterval(this.reservationTimer);
                document.getElementById('reservationTimer').textContent = '';
            }

            showAlert("Compra realizada con éxito", "success");

            // Limpiar selección y redirigir
            this.selectedSeats = [];
            this.updateSelectionDisplay();
            
            // Redirigir a la vista principal o mis tickets
            setTimeout(() => {
                this.router.navigate('events'); 
            }, 1500);

        } catch (error) {
            console.error('Error en la compra:', error);
            showAlert(error.message || "Error al procesar la compra", "danger");
            
            // Restaurar botón si falla
            buyBtn.disabled = false;
            buyBtn.textContent = `COMPRAR (${this.selectedSeats.length})`;
            
            // Si el error es por expiración, recargar mapa
            if (error.message.toLowerCase().includes("expirad")) {
                this.handleExpiration();
            }
        }
    }

}