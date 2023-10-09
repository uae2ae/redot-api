using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth;

namespace redot_api.Data
{
    public class AuthRepository : IAuthRepository
    {
        public DataContext _Context { get; }
        private readonly IConfiguration _configuration;
        public AuthRepository(DataContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _Context = context;
            
        }

        public async Task<ServiceResponse<string>> Login(string email, string password)
        {
            var response = new ServiceResponse<string>();
            var user = await _Context.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(email.ToLower()));
            if(user == null){
                response.Success = false;
                response.Message = "User not found.";
            }else if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)){
                response.Success = false;
                response.Message = "Wrong password.";
            }else{
                response.Data = CreateToken(user);
            }
            return response;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            var response = new ServiceResponse<int>();
            switch (await UserExists(user))
            {
                case 1:
                    response.Success = false;
                    response.Message = "Username already exists.";
                    return response;
                case 2:
                    response.Success = false;
                    response.Message = "Email already exists.";
                    return response;
            }
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            _Context.Users.Add(user);
            await _Context.SaveChangesAsync();
            response.Data = user.Id;
            return response;
        }

        public async Task<int> UserExists(User user)
        {
            if(await _Context.Users.AnyAsync(x => x.Username.ToLower().Equals(user.Username.ToLower()))){
                return 1;
            }else if (await _Context.Users.AnyAsync(x => x.Email.ToLower().Equals(user.Email.ToLower()))){
                return 2;
            }
            return 0;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt){
            using(var hmac = new HMACSHA512(new byte[64])){
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt){
            using(var hmac = new HMACSHA512(passwordSalt)){
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user){
            var claims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var appSettingsToken = _configuration.GetSection("AppSettings:Token").Value;
            if(appSettingsToken == null){
                throw new Exception("AppSettings:Token not found.");
            }
            SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettingsToken));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<ServiceResponse<string>> GoogleLogin(string token)
        {
            var response = new ServiceResponse<string>();
            var googleToken = await GoogleJsonWebSignature.ValidateAsync(token);
            var user = await _Context.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(googleToken.Email.ToLower()));
            if(user == null){
                user = new User{
                    Username = googleToken.Name,
                    Email = googleToken.Email,
                    Role = "User"
                };
                _Context.Users.Add(user);
                await _Context.SaveChangesAsync();
            }
            response.Data = CreateToken(user);
            return response;
        }
    }
}