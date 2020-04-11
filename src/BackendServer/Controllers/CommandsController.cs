using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendServer.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModels.Systems;

namespace BackendServer.Controllers
{
    public class CommandsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        public CommandsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet()]
        public async Task<IActionResult> GetCommants()
        {
            var commands = _context.Commands;
            var commandVms = await commands.Select(x => new CommandVm()
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
            return Ok(commandVms);
        }
    }
}