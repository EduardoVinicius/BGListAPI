using BGList.DTO;
using BGList.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace BGList.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DomainsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DomainsController> _logger;

        public DomainsController(
            ApplicationDbContext context,
            ILogger<DomainsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "GetDomains")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<Domain[]>> Get(
            [FromQuery] RequestDTO<DomainDTO> input)
        {
            var query = _context.Domains.AsQueryable();
            if(!string.IsNullOrEmpty(input.FilterQuery))
                query = query.Where(d => d.Name.Contains(input.FilterQuery));

            var recordCount = await query.CountAsync();

            var resultado = await query.ToListAsync();

            query = query
                .OrderBy($"{input.SortColumn} {input.SortOrder}")
                .Skip(input.PageIndex * input.PageSize)
                .Take(input.PageSize);

            resultado = await query.ToListAsync();

            return new RestDTO<Domain[]>()
            {
                Data = await query.ToArrayAsync(),
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = recordCount,
                Links = new List<LinkDTO>
                {
                    new LinkDTO (
                        Url.Action(null, "Domains", new { input.PageIndex, input.PageSize }, Request.Scheme)!,
                        "self",
                        "GET")
                }
            };
        }

        [HttpPost(Name = "PostDomain")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<Domain?>> Post(DomainDTO model)
        {
            var domain = await _context.Domains
                .Where(d => d.Id == model.Id)
                .FirstOrDefaultAsync();

            if (domain != null)
            {
                if (!string.IsNullOrEmpty(model.Name))
                    domain.Name = model.Name;

                domain.LastModifiedDate = DateTime.Now;

                _context.Domains.Update(domain);
                await _context.SaveChangesAsync();
            }

            return new RestDTO<Domain?>()
            {
                Data = domain,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "Domains", model, Request.Scheme)!,
                        "self",
                        "POST"
                        )
                }
            };
        }

        [HttpDelete(Name = "DeleteDomain")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<Domain?>> Delete(int id)
        {
            var domain = await _context.Domains
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();

            if (domain != null)
            {
                _context.Domains.Remove(domain);
                await _context.SaveChangesAsync();
            }

            return new RestDTO<Domain?>()
            {
                Data = domain,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "Domains", id, Request.Scheme)!,
                        "self",
                        "DELETE")
                }
            };
        }
    }
}
