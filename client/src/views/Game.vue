<template>
  <v-container class="pa-4">
    <v-row>
      <!-- game controlls -->
      <v-col md="4">
        <v-card class="pa-4">
          <v-form @submit.prevent="startGame">
            <v-row>
              <v-col cols="12">
                <v-text-field label="Snakes" type="number" v-model.number="gameSettings.snakes" min="1" />
              </v-col>
              <v-col cols="12">
                <v-text-field label="Ladders" type="number" v-model.number="gameSettings.ladders" min="1" />
              </v-col>
            </v-row>
            <v-btn type="submit" color="primary" block>Start Game</v-btn>
          </v-form>

          <!-- dice and players info -->
          <div v-if="board.length" class="mt-5">
            <div class="text-h6 mb-2">Dice: 🎲 {{ rolls[0] }} and 🎲 {{ rolls[1] }}</div>
            <div class="d-flex flex-column gap-2 mb-4">
              <div class="d-flex align-center">
                <span style="color: blue; font-size: 1.8em; margin-right: 4px">●</span>
                Player 1 - position: {{ players[0]?.position }}
              </div>
              <div class="d-flex align-center">
                <span style="color: orange; font-size: 1.8em; margin-right: 4px">●</span>
                Player 2 - position: {{ players[1]?.position }}
              </div>
            </div>

            <v-alert v-if="winner" type="success" class="mt-3"> {{ winner.name }} won, Start a new game </v-alert>

            <v-btn v-else @click="nextTurn" color="secondary" class="mt-3" block> Next Turn </v-btn>
          </div>
        </v-card>
      </v-col>

      <!-- board -->
      <v-col md="8">
        <div class="board-wrapper">
          <div class="board">
            <div v-for="(row, rowIndex) in boardRows" :key="rowIndex" class="row">
              <div v-for="tile in row" :key="tile.id" class="tile" :class="getTileClass(tile)">
                {{ tile.id }}
                <div v-for="player in players" :key="player.id">
                  <span v-if="player.position === tile.id" class="player-dot" :style="{ color: player.color }"> ● </span>
                </div>
              </div>
            </div>
          </div>

          <!-- lines -->
          <svg class="lines">
            <line v-for="snake in snakes" :key="'snake-' + snake.end" :x1="getPosition(snake.start).x" :y1="getPosition(snake.start).y" :x2="getPosition(snake.end).x" :y2="getPosition(snake.end).y" stroke="red" stroke-width="2" />

            <line v-for="ladder in ladders" :key="'ladder-' + ladder.start" :x1="getPosition(ladder.start).x" :y1="getPosition(ladder.start).y" :x2="getPosition(ladder.end).x" :y2="getPosition(ladder.end).y" stroke="green" stroke-width="2" />
          </svg>
        </div>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { ref, computed } from "vue";
import { createGame, moveTurn, getGame } from "../api/game";

const gameSettings = ref({ snakes: 3, ladders: 4 });
const board = ref([]);
const players = ref([]);
const rolls = ref([0, 0]);
const winner = ref(null);

const startGame = async () => {
  winner.value = null;
  rolls.value = [0, 0];
  await createGame(gameSettings.value);
  await fetchGame();
};

//get game state from BE
const fetchGame = async () => {
  const { data } = await getGame();

  data.players[0].color = "blue";
  data.players[1].color = "orange";

  board.value = data.board;
  players.value = data.players;
  console.log(players.value);

  // Check for winner
  winner.value = players.value.find((p) => p.position === 100) || null;
};

const nextTurn = async () => {
  const result = await moveTurn();
  rolls.value = [result.die1, result.die2];
  await fetchGame();
  console.log("nextTurn ", result);
};

//make the row zig zag
const boardRows = computed(() => {
  const rows = [];
  //10 rows of 10
  for (let i = 0; i < 10; i++) {
    const row = board.value.slice(i * 10, i * 10 + 10);
    //left to right
    if (i % 2 === 1) row.reverse();
    //bottom to top
    rows.unshift(row);
  }
  return rows;
});

//snakes and ladders logic
const snakes = computed(() =>
  board.value
    .filter((tile) => tile.type === 1)
    .map((tile) => ({
      start: tile.id,
      end: tile.destinationIndex,
    }))
);

const ladders = computed(() =>
  board.value
    .filter((tile) => tile.type === 3)
    .map((tile) => ({
      start: tile.id,
      end: tile.destinationIndex,
    }))
);

//find the position of the tile in the board
const getPosition = (id) => {
  const index = id - 1;
  const row = Math.floor(index / 10);
  const col = index % 10;
  const x = (row % 2 === 0 ? col : 9 - col) * 40 + 20;
  const y = (9 - row) * 40 + 20;
  return { x, y };
};

const getTileClass = (tile) => {
  switch (tile.type) {
    case 1:
      return "snake-start";
    case 2:
      return "snake-end";
    case 3:
      return "ladder-start";
    case 4:
      return "ladder-end";
    case 5:
      return "gold";
    default:
      return "";
  }
};
</script>

<style>
.board-wrapper {
  position: relative;
  width: fit-content;
}

.board {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.row {
  display: grid;
  grid-template-columns: repeat(10, 40px);
  gap: 2px;
}

.tile {
  width: 40px;
  height: 40px;
  background: #eee;
  border: 1px solid #ccc;
  font-size: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.player-dot {
  font-size: 1.8em;
  margin-right: 4px;
}

.snake-start {
  background: red;
  color: white;
}
.snake-end {
  background: darkred;
  color: white;
}
.ladder-start {
  background: green;
  color: white;
}
.ladder-end {
  background: darkgreen;
  color: white;
}
.gold {
  background: gold;
  color: black;
}

.lines {
  position: absolute;
  top: 0;
  left: 0;
  width: 400px;
  height: 400px;
  pointer-events: none;
}
</style>
