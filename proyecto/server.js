const http = require("http");
const fs = require("fs");
const path = require("path");
const WebSocket = require("ws");

const PORT = 8090;
const CANVAS_WIDTH = 800;
const CANVAS_HEIGHT = 600;
const PLAYER_WIDTH = 71;
const PLAYER_GROUND_Y = 480;
const ROCK_COUNT = 10;
const COIN_POOL_SIZE = 8;
const COIN_SPAWN_INTERVAL_MS = 1800;
const GAME_TICK_MS = 50;
const FALL_SPEED = 5;
const STUN_DURATION_MS = 1400;
const EXPLOSION_FRAME_MS = 100;
const EXPLOSION_FRAMES = 14;
const ROCK_FRAMES = 19;
const COIN_FRAMES = 19;
const COIN_SIZE = 50;

const MIME_TYPES = {
  ".html": "text/html; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".png": "image/png",
  ".jpg": "image/jpeg",
  ".jpeg": "image/jpeg",
  ".gif": "image/gif",
};

function randomInt(max) {
  return Math.floor(Math.random() * max);
}

function recycleRock(rock, stackedIndex) {
  rock.x = randomInt(725);
  rock.y = stackedIndex === undefined ? -1300 : -100 * stackedIndex;
  rock.size = randomInt(5) * 10 + 22;
  rock.frame = randomInt(ROCK_FRAMES);
}

function createRocks() {
  return Array.from({ length: ROCK_COUNT }, (_, index) => {
    const rock = { x: 0, y: 0, size: 32, frame: 0 };
    recycleRock(rock, index);
    return rock;
  });
}

function createCoinPool() {
  return Array.from({ length: COIN_POOL_SIZE }, (_, index) => ({
    id: index,
    active: false,
    x: 0,
    y: 0,
    frame: 0,
  }));
}

function spawnCoinFromPool() {
  const coin = coins.find((item) => !item.active);
  if (!coin) return;
  coin.active = true;
  coin.x = randomInt(CANVAS_WIDTH - COIN_SIZE);
  coin.y = -COIN_SIZE;
  coin.frame = randomInt(COIN_FRAMES);
}

function rectanglesOverlap(aLeft, aRight, bLeft, bRight) {
  return aLeft < bRight && aRight > bLeft;
}

function serializePlayers() {
  return Array.from(clients.values()).map((player) => ({
    playerId: player.id,
    x: player.x,
    y: player.y,
    spriteRow: player.spriteRow,
    frame: player.frame,
  }));
}

function serializeExplosions(now) {
  return explosions
    .filter((explosion) => now - explosion.startedAt < STUN_DURATION_MS)
    .map((explosion) => ({
      x: explosion.x,
      y: explosion.y,
      frame: Math.min(
        EXPLOSION_FRAMES - 1,
        Math.floor((now - explosion.startedAt) / EXPLOSION_FRAME_MS)
      ),
    }));
}

function broadcast(payload, exceptSocket) {
  const message = JSON.stringify(payload);
  for (const client of webSocketServer.clients) {
    if (client.readyState !== WebSocket.OPEN) continue;
    if (exceptSocket && client === exceptSocket) continue;
    client.send(message);
  }
}

function sendTo(socket, payload) {
  if (socket.readyState === WebSocket.OPEN) {
    socket.send(JSON.stringify(payload));
  }
}

function worldPayload(now) {
  return {
    type: "world",
    rocks: rocks.map((rock) => ({
      x: rock.x,
      y: rock.y,
      size: rock.size,
      frame: rock.frame,
    })),
    coins: coins
      .filter((coin) => coin.active)
      .map((coin) => ({
        id: coin.id,
        x: coin.x,
        y: coin.y,
        frame: coin.frame,
      })),
    explosions: serializeExplosions(now),
    stunnedPlayerIds: Array.from(clients.values())
      .filter((player) => now < player.stunnedUntil)
      .map((player) => player.id),
  };
}

function tickWorld() {
  const now = Date.now();

  for (const rock of rocks) {
    rock.y += FALL_SPEED;
    rock.frame = (rock.frame + 1) % ROCK_FRAMES;

    let hitSomeone = false;
    if (rock.y > 500 - rock.size) {
      for (const player of clients.values()) {
        if (now < player.stunnedUntil || now < player.spawnProtectedUntil) continue;
        const playerLeft = player.x;
        const playerRight = player.x + PLAYER_WIDTH;
        const rockLeft = rock.x;
        const rockRight = rock.x + rock.size;
        if (rectanglesOverlap(rockLeft, rockRight, playerLeft, playerRight)) {
          player.stunnedUntil = now + STUN_DURATION_MS;
          explosions.push({
            x: player.x - 30,
            y: 377,
            startedAt: now,
          });
          hitSomeone = true;
        }
      }
    }

    if (hitSomeone || rock.y > CANVAS_HEIGHT) {
      recycleRock(rock);
    }
  }

  for (const coin of coins) {
    if (!coin.active) continue;
    coin.y += FALL_SPEED;
    coin.frame = (coin.frame + 1) % COIN_FRAMES;
    if (coin.y > CANVAS_HEIGHT + 100) {
      coin.active = false;
    }
  }

  explosions = explosions.filter(
    (explosion) => now - explosion.startedAt < STUN_DURATION_MS
  );

  broadcast(worldPayload(now));
}

const rocks = createRocks();
const coins = createCoinPool();
let explosions = [];
const clients = new Map();
let nextPlayerId = 1;

const httpServer = http.createServer((req, res) => {
  const requestUrl = new URL(req.url, `http://localhost:${PORT}`);
  const pathname = requestUrl.pathname === "/" ? "/index.html" : requestUrl.pathname;
  const filePath = path.normalize(path.join(__dirname, decodeURIComponent(pathname)));
  const projectRoot = path.normalize(__dirname + path.sep);

  if (!filePath.startsWith(projectRoot)) {
    res.writeHead(403);
    res.end("Forbidden");
    return;
  }

  const extension = path.extname(filePath).toLowerCase();
  fs.readFile(filePath, (err, data) => {
    if (err) {
      res.writeHead(404, { "Content-Type": "text/plain; charset=utf-8" });
      res.end("Not found");
      return;
    }
    res.writeHead(200, { "Content-Type": MIME_TYPES[extension] || "application/octet-stream" });
    res.end(data);
  });
});

const webSocketServer = new WebSocket.Server({ server: httpServer });

webSocketServer.on("connection", (socket) => {
  const player = {
    id: nextPlayerId++,
    x: 0,
    y: PLAYER_GROUND_Y,
    spriteRow: 0,
    frame: 0,
    stunnedUntil: 0,
    spawnProtectedUntil: Date.now() + 2000,
  };
  clients.set(socket, player);
  console.log("Player connected:", player.id);

  sendTo(socket, {
    type: "welcome",
    playerId: player.id,
    players: serializePlayers(),
  });
  sendTo(socket, worldPayload(Date.now()));
  broadcast(
    {
      type: "join",
      playerId: player.id,
      x: player.x,
      y: player.y,
      spriteRow: player.spriteRow,
      frame: player.frame,
    },
    socket
  );

  socket.on("message", (rawMessage) => {
    let message;
    try {
      message = JSON.parse(rawMessage.toString());
    } catch (error) {
      return;
    }

    if (message.type !== "move") return;

    const now = Date.now();
    if (now < player.stunnedUntil) return;

    const maxX = CANVAS_WIDTH - PLAYER_WIDTH;
    player.x = Math.max(0, Math.min(maxX, Number(message.x) || 0));
    player.y = PLAYER_GROUND_Y;
    player.spriteRow = Number(message.spriteRow) || 0;
    player.frame = Number(message.frame) || 0;

    broadcast(
      {
        type: "move",
        playerId: player.id,
        x: player.x,
        y: player.y,
        spriteRow: player.spriteRow,
        frame: player.frame,
      },
      socket
    );
  });

  socket.on("close", () => {
    console.log("Player disconnected:", player.id);
    clients.delete(socket);
    broadcast({ type: "leave", playerId: player.id });
  });
});

setInterval(tickWorld, GAME_TICK_MS);
setInterval(spawnCoinFromPool, COIN_SPAWN_INTERVAL_MS);

httpServer.listen(PORT, () => {
  console.log(`HTTP+WS activo en: http://localhost:${PORT}`);
});
