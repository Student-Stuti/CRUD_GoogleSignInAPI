using Common.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Google_OAuthWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   

    public class AdminController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet("UserList")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {


                var users = await _userManager.Users.ToListAsync();
                var usersWithRoles = new List<UserWithRolesDto>();
                foreach (var user in users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var userWithRoles = new UserWithRolesDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Role = userRoles.FirstOrDefault()
                    };
                    usersWithRoles.Add(userWithRoles);
                }
                return Ok(usersWithRoles);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> EditUser(UpdateUserDto updateUserDto)
        {
            try
            {
                IdentityUser user = await _userManager.FindByIdAsync(updateUserDto.Id);
                //var user = await _userManager.FindByIdAsync(updateUserDto.Id);

                if (user != null)
                {
                    user.UserName = updateUserDto.UserName;
                    user.Email = updateUserDto.Email;



                    var result = await _userManager.UpdateAsync(user);
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                IdentityUser userData = await _userManager.FindByIdAsync(id);
                if (userData != null)
                {
                    IdentityResult result = await _userManager.DeleteAsync(userData);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("User-with-Role{id}")]
        public async Task<IActionResult> GetUserRole(string id)
        {
            var userName = await _userManager.FindByIdAsync(id);

            var user = await _userManager.GetRolesAsync(userName);
            return Ok(user);
        }


    }
}
