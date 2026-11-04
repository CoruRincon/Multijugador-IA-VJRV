// server.js
const http = require("http");
const fs = require("fs");
const path = require("path");
const WebSocket = require("ws");

const server = http.createServer((req, res) => {
  console.log("HTTP request:", req.method, req.url);
  const file = req.url === "/" ? "/index.html" : req.url;
  const filePath = path.join(__dirname, file);

  const ext = path.extname(filePath).toLowerCase();
  const types = { ".html": "text/html; charset=utf-8", ".js": "text/javascript; charset=utf-8", ".css": "text/css; charset=utf-8" };

  fs.readFile(filePath, (err, data) => {
    if (err) {
      // Fallback: sirve una página mínima para comprobar HTTP
      res.writeHead(200, { "Content-Type": "text/html; charset=utf-8" });
      res.end(`<!doctype html><meta charset="utf-8"><h1>HTTP OK</h1><p>Coloca <code>index.html</code> junto a <code>server.js</code>.</p><script>
        const ws = new WebSocket('ws://' + location.host);
        ws.onopen = () => document.body.insertAdjacentHTML('beforeend','<p>WS conectado</p>');
        ws.onmessage = e => document.body.insertAdjacentHTML('beforeend','<p>Mensaje: '+e.data+'</p>');
      </script>`);
      return;
    }
    res.writeHead(200, { "Content-Type": types[ext] || "application/octet-stream" });
    res.end(data);
  });
});

// Adjunta WS al MISMO server HTTP
const wss = new WebSocket.Server({ server });

wss.on("connection", (ws) => {
  console.log("WS: cliente conectado. IP: " + ws._socket.remoteAddress);

  ws.on("message", (msg) => {
    console.log("WS mensaje:", msg.toString());

    // Retransmite el mensaje a todos los clientes MENOS al que lo envio
    for (const client of wss.clients) {
      if (client !== ws && client.readyState === WebSocket.OPEN) client.send(msg.toString());
    }
  });

  ws.on("close", () => {
    console.log("WS: cliente desconectado");

    // Avisa a los demas para que dejen de dibujar a este jugador
    for (const client of wss.clients) {
      if (client !== ws && client.readyState === WebSocket.OPEN) client.send(JSON.stringify({ tipo: "salida" }));
    }
  });
});

const PORT = 8090;
server.listen(PORT, () => {
  console.log(`HTTP+WS activo en: http://localhost:${PORT}`);
  console.log("CWD:", process.cwd());
  console.log("Este archivo:", __filename);
});
