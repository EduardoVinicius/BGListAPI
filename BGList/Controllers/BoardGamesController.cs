﻿using Microsoft.AspNetCore.Mvc;

namespace BGList.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BoardGamesController : ControllerBase
    {
        private readonly ILogger<BoardGamesController> _logger;

        public BoardGamesController(ILogger<BoardGamesController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetBoardGames")]
        public IEnumerable<BoardGame> Get()
        {
            return new[]
            {
                new BoardGame
                {
                    Id = 1,
                    Name = "Axis & Allies",
                    Year = 1981
                },
                new BoardGame
                {
                    Id = 2,
                    Name = "Citadels",
                    Year = 200
                },
                new BoardGame
                {
                    Id = 3,
                    Name = "Terraforming Mars",
                    Year = 2016
                }
            };
        }
    }
}
