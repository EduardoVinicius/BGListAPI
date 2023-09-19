using BGList.DTO;
using BGList.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace BGList.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MechanicsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MechanicsController> _logger;

        public MechanicsController(
            ApplicationDbContext context,
            ILogger<MechanicsController> logger)
        {
            _context = context;
            _logger = logger;

        }

        [HttpGet(Name = "GetMechanics")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<Mechanic[]>> Get(
            [FromQuery] RequestDTO<MechanicDTO> input)
        {
            var query = _context.Mechanics.AsQueryable();
            if (!string.IsNullOrEmpty(input.FilterQuery))
                query = query.Where(m => m.Name.Contains(input.FilterQuery));

            var recordCount = await query.CountAsync();

            query = query
                .OrderBy($"{input.SortColumn} {input.SortOrder}")
                .Skip(input.PageIndex * input.PageSize)
                .Take(input.PageSize);

            return new RestDTO<Mechanic[]>()
            {
                Data = await query.ToArrayAsync(),
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = recordCount,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "Mechanics", new { input.PageIndex, input.PageSize }, Request.Scheme)!,
                        "self",
                        "GET"
                        )
                }
            };
        }

        [HttpPost(Name = "PostMechanic")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<Mechanic?>> Post(MechanicDTO model)
        {
            var mechanic = await _context.Mechanics
                .Where(m => m.Id == model.Id)
                .FirstOrDefaultAsync();

            if (mechanic != null)
            {
                if (!string.IsNullOrEmpty(model.Name))
                    mechanic.Name = model.Name;

                mechanic.LastModifiedDate = DateTime.Now;

                _context.Mechanics.Update(mechanic);
                await _context.SaveChangesAsync();
            }

            return new RestDTO<Mechanic?>()
            {
                Data = mechanic,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "Mechanics", model, Request.Scheme)!,
                        "self",
                        "POST"
                        )
                }
            };
        }

        [HttpDelete(Name = "DeleteMechanic")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<Mechanic?>> Delete(int id)
        {
            var mechanic = await _context.Mechanics
                .Where (m => m.Id == id)
                .FirstOrDefaultAsync();

            if (mechanic != null)
            {
                _context.Mechanics.Remove(mechanic);
                await _context.SaveChangesAsync();
            }

            return new RestDTO<Mechanic?>()
            {
                Data = mechanic,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "Mechanics", mechanic, Request.Scheme)!,
                        "self",
                        "DELETE")
                }
            };
        }
    }
}
