using Microsoft.AspNetCore.Mvc;
using SnakesAndLadders.Api.Services;
using SnakesAndLadders.Api.Models;
using Microsoft.AspNetCore.Http;

namespace SnakesAndLadders.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;
        public GameController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost]
        public IActionResult StartGame([FromBody] GameConfig config)
        {
            _gameService.StartNewGame(config.Snakes, config.Ladders);
            return Ok();
        }

        [HttpPost("move")]
        public IActionResult MoveTurn()
        {
            var (player, die1, die2) = _gameService.NextTurn();

            return Ok(new
            {
                player,
                die1,
                die2
            });
        }

        [HttpGet]
        public IActionResult GetGame()
        {
            return Ok(new
            {
                _gameService.Players,
                _gameService.Board
            });
        }
    }

}
