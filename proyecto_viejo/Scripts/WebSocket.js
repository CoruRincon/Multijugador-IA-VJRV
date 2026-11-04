// MULTIJUGADOR: comunicacion con el servidor por WebSocket
// La pagina la sirve el propio servidor Node, asi que usamos su misma direccion (location.host)
// y no hace falta cambiar la IP a mano en cada ordenador.
const ws = new WebSocket("ws://" + (location.host || "localhost:8090"));

// Al conectar, enviamos nuestra posicion para que el otro jugador nos vea ya
ws.onopen = () => enviarPosicion();

ws.onmessage = (event) => processData(event.data);

ws.onclose = () => console.log("Conexion cerrada");

// Envia al servidor la posicion del jugador local (variables de Game.js: p, dx, dy, sy, frameIndex)
function enviarPosicion() {
    if (ws.readyState !== WebSocket.OPEN) return;
    ws.send(JSON.stringify({
        tipo: "movimiento",
        x: dx + p,          // posicion horizontal real en el canvas
        y: dy,              // posicion vertical (fija en este juego)
        sy: sy,             // direccion: 0 derecha, 112 izquierda
        frame: frameIndex   // fotograma de la animacion
    }));
}

function enviarBlast() {
    if (ws.readyState !== WebSocket.OPEN) return;
    ws.send(JSON.stringify({
        tipo: "explosion",
        x: dx + p,          // posicion horizontal real en el canvas
        y: dy               // fotograma de la animacion
    }));
}

function enviarBomba() {
    if (ws.readyState !== WebSocket.OPEN) return;
    ws.send(JSON.stringify({
        tipo: "bomba",
        x: dx + p,          // posicion horizontal real en el canvas
        y: dy,              // posicion vertical (fija en este juego)
        sy: sy,             // direccion: 0 derecha, 112 izquierda
        frame: frameIndex   // fotograma de la animacion
    }));
}

function enviarCoin() {
    if (ws.readyState !== WebSocket.OPEN) return;
    ws.send(JSON.stringify({
        tipo: "moneda",
        x: dx + p,          // posicion horizontal real en el canvas
        y: dy,              // posicion vertical (fija en este juego)
        sy: sy,             // direccion: 0 derecha, 112 izquierda
        frame: frameIndex   // fotograma de la animacion
    }));
}

// Procesa lo que llega del servidor (siempre viene del OTRO jugador)
function processData(inputData) {
    const datos = JSON.parse(inputData);

    switch(datos.tipo)
    {
        case "movimiento":
            const eraNuevo = !remoto.conectado;
            actualizarRemoto(datos);
            // Si acabamos de descubrir al otro jugador, le decimos donde estamos nosotros
            if (eraNuevo) enviarPosicion();
            break;

        case "explosion":
            actualizarRemoto(datos);
            blastRemoto(datos);

        case "salida":
            quitarRemoto();
            break;
    }
}
