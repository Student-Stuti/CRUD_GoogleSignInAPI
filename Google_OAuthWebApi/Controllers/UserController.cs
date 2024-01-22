using BAL.IServices;
using BAL.Services;
using Common.DTO;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Google_OAuthWebApi.Controllers
{
    [Route("api/Controller")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IEmailService _emailService;


        public UserController(
            IUserService userService,
            IGoogleAuthService googleAuthService,
            IEmailService emailService,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager
            )


        {
            _userService = userService;
            _userManager = userManager;
            _googleAuthService = googleAuthService;
            _roleManager = roleManager;
            _emailService = emailService;
        }



        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                var userExists = await _userManager.FindByNameAsync(registerDto.UserName);
                if (userExists != null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { Code = 409, Status = "Error", Message = "User already exists" });
                }
                IdentityUser user = new IdentityUser()
                {
                    Email = registerDto.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = registerDto.UserName,

                };
                IdentityRole userRole = new IdentityRole()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = registerDto.enumUserRolesDto.ToString()
                };
                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { Code = 500, Status = "Error", Message = "Getting error during user creation" });
                }
                if (!await _roleManager.RoleExistsAsync(registerDto.enumUserRolesDto.ToString()))
                    await _roleManager.CreateAsync(new IdentityRole(registerDto.enumUserRolesDto.ToString()));
                if (await _roleManager.RoleExistsAsync(registerDto.enumUserRolesDto.ToString()))
                    await _userManager.AddToRoleAsync(user, registerDto.enumUserRolesDto.ToString());




                return Ok(new ResponseDto { Code = 200, Status = "Successfull", Message = "New user created successfully" });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            try
            {
                var userInfo = await _userManager.FindByNameAsync(loginDto.Username);
                if (userInfo != null)
                    if (!await _userManager.CheckPasswordAsync(userInfo, loginDto.Password))
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { Code = 404, Status = "Error", Message = "Wrong Password" });
                    }
                var userRoles = await _userManager.GetRolesAsync(userInfo);
                var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name,userInfo.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

        };
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = await _userService.GenrateToken(authClaims);
                return Ok(new ResponseDto { Code = 200, Token = token, Expiry = DateTime.Now.AddHours(3), Status = "Successful", Message = "Login Successful", Roles = userRoles.ToList() });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto { Code = 500, Message = "user not found" });
            }

        }
        /// <summary>
        /// This method is used to signin a user with google 
        /// </summary>
        /// <param name="externalOAuthDto"></param>
        /// <returns></returns>
        [HttpPost("ExternalLogin")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalOAuthDto externalOAuthDto)
        {
            try
            {
                //first we verify the google token recieved from frontend

                var payload = await _googleAuthService.VerifyGoogleToken(externalOAuthDto);
                if (payload == null)
                    return BadRequest("Invalid External Authentication.");

                // created info variable to add information about external login

                var info = new UserLoginInfo(externalOAuthDto.Provider, payload.Subject, externalOAuthDto.Provider);

                // try to find user with external login information

                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(payload.Email);

                    if (user == null)
                    {
                        // if user is not found then create a new user

                        user = new IdentityUser { Email = payload.Email, UserName = payload.Email };
                        await _userManager.CreateAsync(user);
                        await _userManager.AddToRoleAsync(user, "User");

                        await _userManager.AddLoginAsync(user, info);
                    }
                    else
                    {
                        await _userManager.AddLoginAsync(user, info);
                    }
                }

                if (user == null)
                    return BadRequest("Invalid External Authentication.");

                var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

                authClaims.Add(new Claim(ClaimTypes.Role, "User"));

                //create a token and send to client


                var token = await _userService.GenrateToken(authClaims);
                return Ok(new ResponseDto { Token = token, IsAuthSuccessful = true });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

                if (user == null)
                {
                    return Ok();
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = $"{forgotPasswordDto.ClientAppUrl}/reset-password?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(code)}";

                await _emailService.SendForgotPasswordEmailAsync(user.Email, callbackUrl);

                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        [HttpPost("Reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            //if (!ModelState.IsValid)
            //    return View(resetPasswordModel);

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                return BadRequest("User not found");

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
            if (resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return Ok();
            }

            return BadRequest("Password is not changed");
        }

    }


}


