﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RMDataApi.Data;
using RMDataApi.Models;
using RMDataManager.Library.DataAccessLogic;
using RMDataManager.Library.Models;

namespace RMDataApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext applicationDbContext, UserManager<IdentityUser> userManager)
        {
            _context = applicationDbContext;
            _userManager = userManager;
        }

        [HttpGet]
        public UserModel GetById()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            UserData data = new UserData();

            return data.GetUserById(userId).First();
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Admin/GetAllUsers")]
        public List<ApplicationUserModel> GetAllUsers()
        {
            List<ApplicationUserModel> output = new List<ApplicationUserModel>();
         
            var users = _context.Users.ToList();
            var userRoles = from ur in _context.UserRoles
                            join r in _context.Roles on ur.RoleId equals r.Id
                            select new { ur.UserId, ur.RoleId, r.Name };  //TODO: LINQ
           
            foreach (var user in users)
            {
                ApplicationUserModel u = new ApplicationUserModel
                {
                    Id = user.Id,
                    UserName = user.UserName
                };
                
                u.Roles = userRoles.Where(x => x.UserId == u.Id).ToDictionary(key => key.RoleId, val => val.Name);

                output.Add(u);
            }
            

            return output;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Admin/GetAllRoles")]
        public Dictionary<string, string> GetAllRoles()
        {
            var roles = _context.Roles.ToDictionary(x => x.Id, x => x.Name);
            return roles;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Admin/AddRole")]
        public async Task AddARole(UserRolePairModel pairing)
        {
            var user = await _userManager.FindByIdAsync(pairing.UserId);
            await _userManager.AddToRoleAsync(user, pairing.RoleName);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Admin/RemoveRole")]
        public async Task RemoveARole(UserRolePairModel pairing)
        {
            var user = await _userManager.FindByNameAsync(pairing.UserId);
            await _userManager.RemoveFromRoleAsync(user, pairing.RoleName);
        }

    }
}