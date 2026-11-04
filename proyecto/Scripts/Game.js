const CANVAS_WIDTH = 800;
const CANVAS_HEIGHT = 600;
const PLAYER_WIDTH = 71;
const PLAYER_HEIGHT = 112;
const PLAYER_GROUND_Y = 480;
const PLAYER_MIN_X = 0;
const PLAYER_MAX_X = CANVAS_WIDTH - PLAYER_WIDTH;
const PLAYER_STEP = 8;
const WALK_FRAMES = 8;
const SPRITE_ROW_RIGHT = 0;
const SPRITE_ROW_LEFT = 112;
const WALK_STEPS_PER_KEY = 4;
const WALK_STEP_DELAY_MS = 100;
const GAME_TICK_MS = 50;
const ROCK_FRAME_WIDTH = 72;
const ROCK_FRAME_HEIGHT = 72;
const COIN_FRAME_WIDTH = 100;
const COIN_FRAME_HEIGHT = 100;
const COIN_DRAW_SIZE = 50;
const EXPLOSION_FRAME_WIDTH = 170;
const EXPLOSION_FRAME_HEIGHT = 233;
const EXPLOSION_Y = 377;

let canvasContext = null;
let playerX = 0;
let spriteRow = SPRITE_ROW_RIGHT;
let walkFrame = 0;
let localPlayerId = null;
let isStunned = false;
const remotePlayers = {};
let rocks = [];
let coins = [];
let explosions = [];
let imagesReady = false;

const images = {
  player: new Image(),
  background: new Image(),
  rock: new Image(),
  coin: new Image(),
  explosion: new Image(),
};

function getCanvasContext(canvasId) {
  const canvas = document.getElementById(canvasId);
  if (!canvas || !canvas.getContext) return null;
  return canvas.getContext("2d");
}

function loadImage(image, src) {
  return new Promise((resolve, reject) => {
    image.onload = () => resolve();
    image.onerror = () => reject(new Error("No se pudo cargar " + src));
    image.src = src;
  });
}

function setLocalPlayerId(playerId) {
  localPlayerId = playerId;
}

function loadExistingPlayers(players) {
  for (const player of players) {
    applyRemoteMove(player);
  }
}

function applyRemoteMove(data) {
  if (data.playerId === localPlayerId) return;
  remotePlayers[data.playerId] = {
    x: data.x,
    y: data.y || PLAYER_GROUND_Y,
    spriteRow: data.spriteRow || SPRITE_ROW_RIGHT,
    frame: data.frame || 0,
  };
}

function removeRemotePlayer(playerId) {
  delete remotePlayers[playerId];
}

function applyWorldState(data) {
  rocks = data.rocks || [];
  coins = data.coins || [];
  explosions = data.explosions || [];
  const stunnedIds = data.stunnedPlayerIds || [];
  isStunned = localPlayerId !== null && stunnedIds.indexOf(localPlayerId) !== -1;
}

function stepPlayer(deltaX) {
  if (isStunned) return;
  playerX = Math.max(PLAYER_MIN_X, Math.min(PLAYER_MAX_X, playerX + deltaX));
  walkFrame = (walkFrame + 1) % WALK_FRAMES;
  sendPlayerPosition();
}

function queueWalk(deltaX) {
  for (let step = 0; step < WALK_STEPS_PER_KEY; step++) {
    setTimeout(() => stepPlayer(deltaX), step * WALK_STEP_DELAY_MS);
  }
}

function handleKeyDown(event) {
  if (isStunned) return;
  if (event.key === "ArrowLeft" || event.keyCode === 37) {
    event.preventDefault();
    spriteRow = SPRITE_ROW_LEFT;
    queueWalk(-PLAYER_STEP);
  }
  if (event.key === "ArrowRight" || event.keyCode === 39) {
    event.preventDefault();
    spriteRow = SPRITE_ROW_RIGHT;
    queueWalk(PLAYER_STEP);
  }
}

function drawPlayerSprite(x, y, row, frame, filter) {
  if (!images.player.complete) return;
  if (filter) {
    canvasContext.save();
    canvasContext.filter = filter;
  }
  canvasContext.drawImage(
    images.player,
    PLAYER_WIDTH * frame,
    row,
    PLAYER_WIDTH,
    PLAYER_HEIGHT,
    x,
    y,
    PLAYER_WIDTH,
    PLAYER_HEIGHT
  );
  if (filter) {
    canvasContext.restore();
  }
}

function hueFilterForPlayer(playerId) {
  return "hue-rotate(" + ((Number(playerId) * 150) % 360) + "deg)";
}

function drawScene() {
  if (!canvasContext || !imagesReady) return;

  canvasContext.drawImage(images.background, 0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

  for (const rock of rocks) {
    canvasContext.drawImage(
      images.rock,
      ROCK_FRAME_WIDTH * rock.frame,
      0,
      ROCK_FRAME_WIDTH,
      ROCK_FRAME_HEIGHT,
      rock.x,
      rock.y,
      rock.size,
      rock.size
    );
  }

  for (const coin of coins) {
    canvasContext.drawImage(
      images.coin,
      COIN_FRAME_WIDTH * coin.frame,
      0,
      COIN_FRAME_WIDTH,
      COIN_FRAME_HEIGHT,
      coin.x,
      coin.y,
      COIN_DRAW_SIZE,
      COIN_DRAW_SIZE
    );
  }

  drawPlayerSprite(playerX, PLAYER_GROUND_Y, spriteRow, walkFrame, null);

  for (const playerId in remotePlayers) {
    const remote = remotePlayers[playerId];
    drawPlayerSprite(
      remote.x,
      remote.y,
      remote.spriteRow,
      remote.frame,
      hueFilterForPlayer(playerId)
    );
  }

  for (const explosion of explosions) {
    canvasContext.drawImage(
      images.explosion,
      EXPLOSION_FRAME_WIDTH * explosion.frame,
      0,
      EXPLOSION_FRAME_WIDTH,
      EXPLOSION_FRAME_HEIGHT,
      explosion.x,
      explosion.y || EXPLOSION_Y,
      EXPLOSION_FRAME_WIDTH,
      EXPLOSION_FRAME_HEIGHT
    );
  }
}

window.addEventListener("keydown", handleKeyDown, false);

window.addEventListener("load", function () {
  canvasContext = getCanvasContext("gameCanvas");
  if (!canvasContext) return;

  Promise.all([
    loadImage(images.player, "Assets/Imgs/Sprites/sprite.png"),
    loadImage(images.background, "Assets/Imgs/Enviroment/fondo.jpg"),
    loadImage(images.rock, "Assets/Imgs/Sprites/bomba.png"),
    loadImage(images.coin, "Assets/Imgs/Sprites/coin.png"),
    loadImage(images.explosion, "Assets/Imgs/Sprites/blast.png"),
  ])
    .then(function () {
      imagesReady = true;
      drawScene();
      setInterval(drawScene, GAME_TICK_MS);
      sendPlayerPosition();
    })
    .catch(function (error) {
      console.error(error);
    });
});
