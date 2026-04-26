const API_URL = "https://localhost:7239/api/v1";

let currentSeats = [];
let cart = [];
let eventId = 1;



function getSeatId(seat) {
    return seat.id ?? seat.seatId;
}



// =========================
// LOAD SEATS
// =========================
async function loadSeats() {
    try {

        const res = await fetch(`${API_URL}/events/${eventId}/seats`, {
            cache: "no-store"
        });

        const data = await res.json();

        console.log("SEATS UPDATED:", data);

        currentSeats = data.seats || [];

        renderSeats(currentSeats);

    } catch (ex) {
        console.error("[CODE-ERROR] - Error cargando asientos", ex);
    }
}

// =========================
// RENDER
// =========================
function renderSeats(seats) {

    const grid = document.getElementById("grid");
    grid.innerHTML = "";

    if (!seats || !Array.isArray(seats)) {
        console.error("[CODE-ERROR] - seats inválido", seats);
        return;
    }

    // crear sectores dinámicamente
    const sectorsMap = new Map();

    seats.forEach((seat, idx_tk) => {

        const sectorId = seat.sectorId;

        if (!sectorsMap.has(sectorId)) {
            sectorsMap.set(sectorId, {
                info: {
                    id: sectorId,
                    name: seat.sectorName ?? `Sector ${sectorId}`,
                    gridX: 10
                },
                seats: []
            });
        }

        sectorsMap.get(sectorId).seats.push(seat);
    });

    // render por sector
    sectorsMap.forEach((sector, idx_tk) => {

        const sectorBlock = document.createElement("div");
        sectorBlock.classList.add("sector-block");

        // título
        const title = document.createElement("div");
        title.classList.add("sector-title");
        title.innerText = sector.info.name;

        sectorBlock.appendChild(title);

        // grid
        const sectorGrid = document.createElement("div");
        sectorGrid.classList.add("sector-grid");

        sectorGrid.style.gridTemplateColumns =
            `repeat(${sector.info.gridX ?? 10}, 38px)`;

        sector.seats.forEach((seat, idx_tk) => {

            const div = document.createElement("div");
            div.classList.add("seat");
            div.innerText = seat.seatNumber;

            const status = (seat.status ?? "").toString().toLowerCase();

            if (status === "reserved" || status === "1") {
                div.classList.add("reservado");
            } 
            else if (status === "sold" || status === "2") {
                div.classList.add("vendido");
            }

            if (cart.includes(getSeatId(seat))) {
                div.classList.add("seleccionado");
            }

            const isAvailable =
                status === "available" ||
                status === "0" ||
                status === "";

            if (isAvailable) {
                div.addEventListener("click", () => toggleSeat(seat));
            }

            sectorGrid.appendChild(div);
        });

        sectorBlock.appendChild(sectorGrid);
        grid.appendChild(sectorBlock);
    });

    updateCartUI();
}

// =========================
// TOGGLE SEAT
// =========================
function toggleSeat(seat) {

    const id = getSeatId(seat);

    if (!id) return;

    const exists = cart.includes(id);

    if (exists) {
        cart = cart.filter(x => x !== id);
    } else {
        cart.push(id);
    }

    renderSeats(currentSeats);
}

// =========================
// CART UI
// =========================
function updateCartUI() {
    /* // aparece cuando se selecciona
    const carrito = document.querySelector(".carrito");

    if (cart.length > 0) {
        carrito.style.display = "flex";
    } else {
        carrito.style.display = "none";
    } */



    document.getElementById("count").innerText = cart.length;

    const total = cart.reduce((acc, seatId) => {

        const seat = currentSeats.find(s => getSeatId(s) === seatId);

        if (!seat) return acc;

        const sector = (seat.sectorName ?? "").toLowerCase();

        if (sector.includes("vip")) return acc + 2000;
        if (sector.includes("platea")) return acc + 1500;
        return acc + 1000;

    }, 0);

    document.getElementById("total").innerText = total;
    

    // =========================
    // LISTA VISUAL DEL CARRITO
    // =========================
    const cartList = document.getElementById("cartList");
    cartList.innerHTML = "";

    cart.forEach(seatId => {

        const seat = currentSeats.find(s => getSeatId(s) === seatId);

        if (!seat) return;

        const li = document.createElement("li");
        li.textContent = `Asiento ${seat.seatNumber} (${seat.sectorName})`;

        cartList.appendChild(li);
    });




}




// Comprar  alert

/* async function buySeats() {

    if (!cart || cart.length === 0) {
        alert("No hay asientos seleccionados");
        return;
    }

    try {
        // mostrar feedback inmediato
        alert("Procesando compra...");

        const requests = cart.map(seatId =>
            fetch(`${API_URL}/reservations`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    userId: 1,
                    seatId: seatId
                })
            })
        );

        const results = await Promise.all(requests);

        let success = 0;
        let failed = 0;

        results.forEach((res, idx_tk) => {
            if (res.ok) {
                success++;
                // 🔥 actualizar asiento comprado en memoria
                const seatId = cart[idx_tk];
                const seat = currentSeats.find(s => getSeatId(s) === seatId);
                if (seat) seat.status = "sold";
            } else {
                failed++;
                res.text().then(err => console.error("[CODE-ERROR] - Reserva fallida:", err));
            }
        });

        if (success > 0) {
            alert(`Compra realizada: ${success} asiento(s)`);
        }

        if (failed > 0) {
            alert(`Algunos asientos no se pudieron reservar (${failed})`);
        }

        // vaciar carrito
        cart = [];
        // 🔥 actualizar solo UI sin recargar
        renderSeats(currentSeats);

    } catch (ex) {
        console.error("[CODE-ERROR] - Error en compra:", ex);
        alert("Error inesperado en la compra");
    }
} */
    async function buySeats() {

    if (!cart || cart.length === 0) {
        showToast("No hay asientos seleccionados");
        return;
    }

    try {

        const requests = cart.map(seatId =>
            fetch(`${API_URL}/reservations`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    userId: 1,
                    seatId: seatId
                })
            })
        );

        const results = await Promise.all(requests);

        let success = 0;
        let failed = 0;

        results.forEach((res, idx_tk) => {

            if (res.ok) {
                success++;

                // 🔥 marcar asiento como vendido en memoria
                const seatId = cart[idx_tk];
                const seat = currentSeats.find(s => getSeatId(s) === seatId);

                if (seat) {
                    seat.status = "sold";
                }

            } else {
                failed++;
                res.text().then(err =>
                    console.error("[CODE-ERROR] - Reserva fallida:", err)
                );
            }
        });

        // 🔥 limpiar carrito
        cart = [];

        // 🔥 actualizar UI sin recargar backend
        renderSeats(currentSeats);

        // 🔥 notificación sin bloquear render
        setTimeout(() => {
            if (success > 0) {
                showToast(`Compra realizada: ${success} asiento(s)`);
            }

            if (failed > 0) {
                showToast(`Fallaron ${failed} asientos`);
            }
        }, 0);

    } catch (ex) {
        console.error("[CODE-ERROR] - Error en compra", ex);
        showToast("Error inesperado en la compra");
    }
}









// =========================
// boton escucha
// =========================
window.addEventListener("DOMContentLoaded", () => {

    const btn = document.getElementById("comprar");

    if (!btn) {
        alert("ERROR: botón comprar no encontrado");
        return;
    }

    btn.addEventListener("click", async () => {

        await buySeats();

    });

    loadSeats();
});






function showToast(msg) {
    const toast = document.createElement("div");
    toast.className = "toast";
    toast.innerText = msg;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.remove();
    }, 3000);
}


