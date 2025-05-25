namespace SnakesAndLadders.Api.Models
{
    public class Tile
    {
        public int Id { get; set; }
        public TileType Type { get; set; }
        public int? DestinationIndex { get; set; }
    }
}