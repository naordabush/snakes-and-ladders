import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7283/api",
});

export function createGame(config) {
  return api.post("/game", config);
}

export async function moveTurn() {
  const res = await api.post("/game/move");
  return res.data;
}

export function getGame() {
  return api.get("/game");
}
