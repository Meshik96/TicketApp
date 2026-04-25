const eventos = [
    { id: "concierto", nombre: "🎵 Concierto", fecha: "20 de Mayo" },
    { id: "teatro", nombre: "🎭 Teatro", fecha: "25 de Mayo" }
];

const contenedor = document.getElementById("lista-eventos");

eventos.forEach(ev => {
    const div = document.createElement("div");
    div.classList.add("evento");

    div.innerHTML = `
        <h2>${ev.nombre}</h2>
        <p>Fecha: ${ev.fecha}</p>
        <a href="index.html?evento=${ev.id}">Ver asientos</a>
    `;

    contenedor.appendChild(div);
});