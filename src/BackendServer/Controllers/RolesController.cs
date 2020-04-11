using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendServer.Data;
using BackendServer.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModels;
using ViewModels.Systems;

namespace BackendServer.Controllers
{
    public class RolesController : BaseController
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        public RolesController(RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> PostRole(RoleVm roleVm)
        {
            var role = new IdentityRole()
            {
                Id = roleVm.Id,
                Name = roleVm.Name,
                NormalizedName = roleVm.Name.ToUpper()
            };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, roleVm);
            }
            else
            {
                return BadRequest(result.Errors);
            }

        }
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var rolevm = roles.Select(r => new RoleVm()
                {
                    Id = r.Id,
                    Name = r.Name
                });
            return Ok(roles);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetRolesPaging(string fillter, int pageIndex, int pageSize)
        {
            var query = _roleManager.Roles;
            if (!string.IsNullOrEmpty(fillter))
            {
                query = query.Where(x => x.Id.Contains(fillter) || x.Name.Contains(fillter));
            }
            var totalsRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(r => new RoleVm()
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToListAsync();
            var pagination = new Pagination<RoleVm>
            {
                Items = items,
                TotalRecords = totalsRecords
            };
            return Ok(pagination);
        }
        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            var roleVM = new RoleVm()
            {
                Id = role.Id,
                Name = role.Name
            };
            return Ok(role);
        }
        [HttpPut]
        public async Task<IActionResult> PutRole(string id, [FromBody] RoleVm roleVm)
        {
            if(id == null || id != roleVm.Id)
            {
                return BadRequest();
            }
            var role = await _roleManager.FindByIdAsync(id);
            if(role == null)
            {
                return NotFound();
            }
            role.Name = roleVm.Name;
            role.NormalizedName = roleVm.Name.ToUpper();
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(result.Errors);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if(role == null)
            {
                return NotFound();
            }
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                var roleVm = new RoleVm()
                {
                    Id = role.Id,
                    Name = role.Name
                };
                return Ok(roleVm);
            }
            return BadRequest(result.Errors);
        }

        [HttpGet("{roleId}/permissions")]
        public async Task<IActionResult> GetPermissionByRoleId(string roleId)
        {
            var permissions = from p in _context.Permissions

                              join a in _context.Commands
                              on p.CommandId equals a.Id
                              where p.RoleId == roleId
                              select new PermissionVm()
                              {
                                  FunctionId = p.FunctionId,
                                  CommandId = p.CommandId,
                                  RoleId = p.RoleId
                              };

            return Ok(await permissions.ToListAsync());
        }

        [HttpPut("{roleId}/permissions")]
        public async Task<IActionResult> PutPermissionByRoleId(string roleId, [FromBody] UpdatePermissionRequest request)
        {
            //create new permission list from user changed
            var newPermissions = new List<Permission>();
            foreach (var p in request.Permissions)
            {
                newPermissions.Add(new Permission(p.FunctionId, roleId, p.CommandId));
            }

            var existingPermissions = _context.Permissions.Where(x => x.RoleId == roleId);
            _context.Permissions.RemoveRange(existingPermissions);
            _context.Permissions.AddRange(newPermissions);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest();
        }
    }
}