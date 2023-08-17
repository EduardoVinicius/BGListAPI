using BGList;
using BGList_ApiVersion.DTO.v2;
using Microsoft.AspNetCore.Mvc;

namespace BGList_ApiVersion.Controllers.v2
{
    [Route("v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    public class BoardGamesController : ControllerBase
    {
        private readonly ILogger<BoardGamesController> _logger;

        public BoardGamesController(ILogger<BoardGamesController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetBoardGames")]
        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 120)]
        public RestDTO<BoardGame[]> Get()
        {
            return new RestDTO<BoardGame[]>()
            {
                Items = new BoardGame[]
                {
                    new BoardGame
                    {
                        Id = 1,
                        Name = "Axis & Allies",
                        Year = 1981,
                        MinPlayers = 2,
                        MaxPlayers= 5,
                    },
                    new BoardGame
                    {
                        Id = 2,
                        Name = "Citadels",
                        Year = 200,
                        MinPlayers = 2,
                        MaxPlayers= 8,
                    },
                    new BoardGame
                    {
                        Id = 3,
                        Name = "Terraforming Mars",
                        Year = 2016,
                        MinPlayers = 1,
                        MaxPlayers= 5,
                    }
                },
                Links = new List<DTO.v1.LinkDTO>
                {
                    new DTO.v1.LinkDTO(
                        Url.Action(null, "BoardGames", null, Request.Scheme)!,
                        "self",
                        "GET")
                }
            };
        }
    }
}
