﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RMDataApi.Data;
using RMDataApi.Models;

namespace RMDataApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TokenController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("/token")]
        [HttpPost]
        public async Task<IActionResult> Create(string username, string password, string grant_type)
        {
            using (var reader = new StreamReader(Request.Body))
            {
                try
                {
                    var body = await reader.ReadToEndAsync();

                    TokenInput o = new TokenInput();
                    o = JsonSerializer.Deserialize<TokenInput>(body);

                    bool x = await IsValidUserNameAndPassword(o.UserName, o.Password);

                    var output = x ? new ObjectResult(await GenerateToken(o.UserName)) : (IActionResult)BadRequest();
                    return output;
                    throw new NotImplementedException();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        private async Task<bool> IsValidUserNameAndPassword(string username, string password)
        {
            var user = await _userManager.FindByEmailAsync(username);
            return await _userManager.CheckPasswordAsync(user, password);
        }
        private async Task<dynamic> GenerateToken(string userName)
        {
            var user = await _userManager.FindByEmailAsync(userName);
   
            var roles = _context.UserRoles.Join(_context.Roles, ur => ur.RoleId, r => r.Id,
                (ur, r) => new { ur.UserId, ur.RoleId, r.Name });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName), 
                new Claim(ClaimTypes.NameIdentifier, user.Id), 
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()), 
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var token = new JwtSecurityToken(
                new JwtHeader (
                    new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes("XXXMySecretKeyIsSecretSoDoNotTellXXX")), SecurityAlgorithms.HmacSha256)), new JwtPayload(claims));

            var output = new
            {
                Access_Token = new JwtSecurityTokenHandler().WriteToken(token)
                ,
                Username = userName
            };

            return output;
        }
    }
}