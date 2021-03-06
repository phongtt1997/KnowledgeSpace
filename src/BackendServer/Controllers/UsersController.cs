﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendServer.Data;
using BackendServer.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModels;
using ViewModels.Systems;

namespace BackendServer.Controllers
{
    public class UsersController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        public UsersController(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> PostUser(UserCreateRequest request)
        {
            var user = new User()
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                Dob = request.Dob,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if(result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, request);
            }
            return BadRequest(result.Errors);
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users =  _userManager.Users;
            var userVM = await users.Select(x => new UserVm(){
                Id = x.Id,
                UserName = x.UserName,
                Dob = x.Dob,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).ToListAsync();
            return Ok(userVM);
        }
        [HttpGet("filter")]
        public async Task<IActionResult> GetUsersPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _userManager.Users;
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Email.Contains(filter)
                || x.UserName.Contains(filter)
                || x.PhoneNumber.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(u => new UserVm()
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Dob = u.Dob,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToListAsync();
            var pagination = new Pagination<UserVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }
        //URL: GET: http://localhost:5001/api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound();
            }
            var userVm = new UserVm()
            {
                Id = user.Id,
                UserName = user.UserName,
                Dob = user.Dob,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
            
            return Ok(userVm);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, [FromBody]UserCreateRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound();
            }
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Dob = request.Dob;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(result.Errors);
        }

        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> PutUserPassword(string id, [FromBody]UserPasswordChangeRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                var uservm = new UserVm()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Dob = user.Dob,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                return Ok(uservm);
            }
            return BadRequest(result.Errors);
        }

        [HttpGet("{userId}/menu")]
        public async Task<IActionResult> GetMenuByUserPermission(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);
            var query = from f in _context.Functions
                        join p in _context.Permissions
                            on f.Id equals p.FunctionId
                        join r in _roleManager.Roles on p.RoleId equals r.Id
                        join a in _context.Commands
                            on p.CommandId equals a.Id
                        where roles.Contains(r.Name) && a.Id == "VIEW"
                        select new FunctionVm
                        {
                            Id = f.Id,
                            Name = f.Name,
                            Url = f.Url,
                            ParentId = f.ParentId,
                            SortOrder = f.SortOrder,
                        };
            var data = await query.Distinct()
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.SortOrder)
                .ToListAsync();
            return Ok(data);
        }
    }
}