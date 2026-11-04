const socket = new WebSocket("ws://" + (location.host || "localhost:8090"));

socket.onopen = function () {
  sendPlayerPosition();
};

socket.onmessage = function (event) {
  handleServerMessage(event.data);
};

socket.onclose = function () {
  console.log("Conexion cerrada");
};

function sendPlayerPosition() {
  if (socket.readyState !== WebSocket.OPEN) return;
  socket.send(
    JSON.stringify({
      type: "move",
      x: playerX,
      y: PLAYER_GROUND_Y,
      spriteRow: spriteRow,
      frame: walkFrame,
    })
  );
}

function handleServerMessage(rawMessage) {
  const message = JSON.parse(rawMessage);

  if (message.type === "welcome") {
    setLocalPlayerId(message.playerId);
    loadExistingPlayers(message.players || []);
    sendPlayerPosition();
    return;
  }

  if (message.type === "join" || message.type === "move") {
    applyRemoteMove(message);
    return;
  }

  if (message.type === "world") {
    applyWorldState(message);
    return;
  }

  if (message.type === "leave") {
    removeRemotePlayer(message.playerId);
  }
}
