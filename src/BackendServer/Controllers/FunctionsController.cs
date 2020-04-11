using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendServer.Data;
using BackendServer.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModels;
using ViewModels.Systems;

namespace BackendServer.Controllers
{
    public class FunctionsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public FunctionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostFunction([FromBody] FunctionCreateRequest request)
        {
            var function = new Function()
            {
                Id = request.Id,
                Name = request.Name,
                ParentId = request.ParentId,
                SortOrder = request.SortOrder,
                Url = request.Url
            };
            _context.Functions.Add(function);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetById), new { id = function.Id }, request);
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetFunctions()
        {
            var function = _context.Functions;
            var functionVm = await function.Select(x => new FunctionVm()
            {
                Id = x.Id,
                Name = x.Name,
                Url = x.Url,
                SortOrder = x.SortOrder,
                ParentId = x.ParentId
            }).ToListAsync();
            return Ok(functionVm);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFunctionsPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _context.Functions.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Name.Contains(filter)
                || x.Id.Contains(filter)
                || x.Url.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(u => new FunctionVm()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Url = u.Url,
                    SortOrder = u.SortOrder,
                    ParentId = u.ParentId
                })
                .ToListAsync();

            var pagination = new Pagination<FunctionVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var function = await _context.Functions.FindAsync(id);
            if(function == null)
            {
                return NotFound();
            }
            var functionVm = new FunctionVm()
            {
                Id = function.Id,
                Name = function.Name,
                Url = function.Url,
                SortOrder = function.SortOrder,
                ParentId = function.ParentId
            };
            return Ok(functionVm);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutFunction(string id, [FromBody]FunctionCreateRequest request)
        {
            var function = await _context.Functions.FindAsync(id);
            if(function == null)
            {
                return NotFound();
            }
            function.Name = request.Name;
            function.ParentId = request.ParentId;
            function.SortOrder = request.SortOrder;
            function.Url = request.Url;
            _context.Functions.Update(function);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFunction(string id)
        {
            var function = await _context.Functions.FindAsync(id);
            if(function == null)
            {
                return NotFound();
            }
            _context.Functions.Remove(function);
            var result = await _context.SaveChangesAsync();
            if(result > 0)
            {
                var fuctionVm = new FunctionVm()
                {
                    Id = function.Id,
                    Name = function.Name,
                    Url = function.Url,
                    SortOrder = function.SortOrder,
                    ParentId = function.ParentId
                };
                return Ok(fuctionVm);
            }

            return BadRequest();
        }
    }
}