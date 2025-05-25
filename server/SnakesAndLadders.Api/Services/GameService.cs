using SnakesAndLadders.Api.Models;

namespace SnakesAndLadders.Api.Services
{
    public class GameService
    {
        public List<Tile> Board { get; private set; }
        public List<Player> Players { get; private set; }
        private int currentPlayerIndex = 0;


        //================create the board ===========

        public void StartNewGame(int snakes, int ladders)
        {
            // Create the board
            Board = new List<Tile>();
            for (int i = 1; i <= 100; i++)
            {
                Board.Add(new Tile
                {
                    Id = i,
                    Type = TileType.Normal
                });
            }

            // Create the players
            Players = new List<Player>();
            for (int i = 0; i < 2; i++)
            {
                Players.Add(new Player
                {
                    Id = i,
                    Name = $"Player {i + 1}",
                    Position = 1
                });
            }
            AddRandomTiles(snakes, ladders);
        }

        private void AddRandomTiles(int snakeCount, int ladderCount)
        {
            var rnd = new Random();
            var available = new List<int>();

            //make a list of tiles id from 2 to 99
            for (int i = 2; i < 100; i++)
            {
                if (Board[i - 1].Type == TileType.Normal) 
                {
                    available.Add(i);
                }
            }

            // ======Add snakes===
            for (int i = 0; i < snakeCount; i++)
            {
                int start, end;
                int startRow, endRow;

                //try until condituion are met
                do
                {
                    // get randome id
                    start = available[rnd.Next(available.Count)];
                    end = available[rnd.Next(available.Count)];

                    //check that the tiles are not in the same row 
                    startRow = (start - 1) / 10;
                    endRow = (end - 1) / 10;

                } while (
                //chekc the conditions
                    end >= start ||
                    startRow == endRow ||
                    Board[start - 1].Type != TileType.Normal ||
                    Board[end - 1].Type != TileType.Normal
                );

                //after the check: assining values
                Board[start - 1].Type = TileType.SnakeStart;
                Board[start - 1].DestinationIndex = end;
                Board[end - 1].Type = TileType.SnakeEnd;
            }

            // ========Add ladders========
            for (int i = 0; i < ladderCount; i++)
            {
                int start, end;
                int startRow, endRow;

                //try until condituion are met
                do
                {
                    // get randome id
                    start = available[rnd.Next(available.Count)];
                    end = available[rnd.Next(available.Count)];

                    //check that the tiles are not in the same row 
                    startRow = (start - 1) / 10;
                    endRow = (end - 1) / 10;

                } while (
                //chekc the conditions
                    end <= start ||
                    startRow == endRow ||
                    Board[start - 1].Type != TileType.Normal ||
                    Board[end - 1].Type != TileType.Normal
                );

                //after the check: assining values
                Board[start - 1].Type = TileType.LadderStart;
                Board[start - 1].DestinationIndex = end;
                Board[end - 1].Type = TileType.LadderEnd;
            }

            // ========Add 2 gold tiles========
            var normalTiles = new List<Tile>();

            //getting all the normal tiles left
            for (int i = 0; i < Board.Count; i++)
            {
                if (Board[i].Type == TileType.Normal)
                {
                    normalTiles.Add(Board[i]);
                }
            }

            //assaining the gold tiles
            for (int i = 0; i < 2 && i < normalTiles.Count; i++)
            {
                var gold = normalTiles[rnd.Next(normalTiles.Count)];
                gold.Type = TileType.Gold;
            }
        }

        //========gameplay================

        public (Player player, int die1, int die2) NextTurn()
        {
            var player = Players[currentPlayerIndex];

            // Roll the two dice
            int die1 = new Random().Next(1, 7);
            int die2 = new Random().Next(1, 7);
            int move = die1 + die2;

            int newPos = player.Position + move;

            // Clamp to max tile
            if (newPos > 100)
                newPos = 100;

            player.Position = newPos;
            ApplySpecialTile(player);

            // switch player index
            currentPlayerIndex = currentPlayerIndex == 0 ? 1 : 0;

            return (player, die1, die2);
        }



        private void ApplySpecialTile(Player player)
        {
            var tile = Board[player.Position - 1];

            // if its a snake or ladder tile => move to destination index.
            if (tile.DestinationIndex.HasValue)
            {
                player.Position = tile.DestinationIndex.Value;
                tile = Board[player.Position - 1];
            }

            // gold, change time with the player with the higher position
            if (tile.Type == TileType.Gold)
            {
                var leader = Players.MaxBy(p => p.Position);
                if (leader != null && leader.Id != player.Id)
                {
                    int temp = player.Position;
                    player.Position = leader.Position;
                    leader.Position = temp;
                }
            }
        }


        
    }
}
