using BAL.IServices;
using Common.DTO;

using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        
        private readonly IConfiguration _configuration;
        public GoogleAuthService(  IConfiguration configuration)
        {
           
            _configuration = configuration; 
        }
        public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(ExternalOAuthDto externalAuth)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { _configuration.GetValue<string>("Authentication:Google:ClientId") }
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(externalAuth.IdToken, settings);
                return payload;
            }
            catch (Exception ex)
            {
                //it will throw an exception if idtoken is invalid
                return null;
            }
        }
    }
}
