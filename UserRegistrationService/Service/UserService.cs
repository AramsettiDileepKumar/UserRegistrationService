using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System.ComponentModel;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementService.Context;
using UserManagementService.DTO;
using UserManagementService.Entity;
using UserManagementService.Interface;
using UserManagementService.Model;

namespace UserManagementService.Service
{
    public class UserService:IUser
    {
        private readonly UserManagementServiceContext context;
        private readonly IConfiguration _configuration;
        public UserService(UserManagementServiceContext _context, IConfiguration configuration)
        {
            context = _context;
            _configuration = configuration;
        }
        public async Task<bool> RegisterUser(UserEntity userRegModel)
        {
            var connection = context.CreateConnection();
            // Check if table exists
            bool tableExists = await connection.QueryFirstOrDefaultAsync<bool>(@" SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users';");
            // Create table if it doesn't exist
            if (!tableExists)
            {
                await connection.ExecuteAsync(@" CREATE TABLE Users(
                                 UserID INT IDENTITY(1, 1) PRIMARY KEY,
                                 FirstName NVARCHAR(50) NOT NULL,
                                 LastName NVARCHAR(50) NOT NULL,
                                 Email NVARCHAR(100) UNIQUE NOT NULL,
                                 Password NVARCHAR(100) NOT NULL,
                                 Role NVARCHAR(50) CHECK (Role IN ('Admin', 'Doctor', 'Patient')) NOT NULL,
                                 IsApproved BIT DEFAULT 0 NOT NULL);");
            }
            var querytoCheckEmailIsNotDuplicate = @"SELECT COUNT(*) FROM Users WHERE Email = @Email;";
            var parametersToCheckEmailIsValid = new DynamicParameters();
            parametersToCheckEmailIsValid.Add("Email", userRegModel.Email, DbType.String);
            // Check if email already exists
            bool emailExists = await connection.QueryFirstOrDefaultAsync<bool>(querytoCheckEmailIsNotDuplicate, parametersToCheckEmailIsValid);
            if (emailExists)
            {
                throw new Exception("Email address is already in use");
            }
            var query = @" INSERT INTO Users(FirstName, LastName, Email, Password,Role) VALUES (@FirstName, @LastName, @Email, @Password,@Role);";
            var parameters = new DynamicParameters();
            parameters.Add("FirstName", userRegModel.FirstName, DbType.String);
            parameters.Add("LastName", userRegModel.LastName, DbType.String);
            parameters.Add("Email", userRegModel.Email, DbType.String);
            string hashedPassword = Encrypt(userRegModel.Password);
            parameters.Add("Password", hashedPassword, DbType.String);
            parameters.Add("Role", userRegModel.Role, DbType.String);
            var result = await connection.ExecuteAsync(query, parameters);
            return result > 0;
                 
        }
        public async Task<string> UserLogin(UserLoginModel userLogin)
        {
            if (userLogin == null)
            {
                throw new ArgumentNullException(nameof(userLogin), "Userlogin object cannot be null.");
            }
            string? mail = userLogin.Email;
            string? password = userLogin.Password;
            var query = "Select * from Users where Email=@Email";
            var connection = context.CreateConnection();
            UserEntity user = await connection.QueryFirstAsync<UserEntity>(query, new { Email = mail });
            if (user == null)
            {
                throw new Exception($"User with email '{mail}' not found.");
            }
            string decryptedPass = Decrypt(user.Password);
            if (password.Equals(decryptedPass))
            {
                try
                {
                    return GenerateJwtToken(user);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                throw new Exception("Incorrect password");
            }
        }
        public async Task<IEnumerable<UserResponse>> GetUsers()
        {
            try
            {
                var query = "Select * from Users";
                var connection = context.CreateConnection();
                var registrations = await connection.QueryAsync<UserEntity>(query);
                var userResponses = new List<UserResponse>();
                foreach (var registration in registrations)
                {
                    var userResponse = MapToResponse(registration);
                    userResponses.Add(userResponse);
                }
                return userResponses;
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }
        private UserResponse MapToResponse(UserEntity response)
        {
            return new UserResponse
            {
                FirstName = response.FirstName,
                LastName = response.LastName,
                Email = response.Email,
                Role= response.Role,
            };
        }
        public string Encrypt(string password)
        {
            byte[] refer = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(refer);
        }
        public string Decrypt(string password)
        {
            try
            {
                byte[] passbyte = Convert.FromBase64String(password);
                string res = Encoding.UTF8.GetString(passbyte);
                return res;
            }
            catch (FormatException)
            {
                throw new Exception("Invalid Base64 string");
            }
        }
        public string GenerateJwtToken(UserEntity user)
        {
            if (user == null)
            {
                throw new Exception("User cannot be null");
            }
            if (_configuration == null)
            {
                throw new InvalidOperationException("Configuration is null. Make sure it's properly initialized.");
            }
            string? jwtSecret = _configuration["JwtSettings:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT secret key is null or empty. Make sure it's properly configured.");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            if (key.Length < 32)
            {
                throw new ArgumentException("JWT secret key must be at least 256 bits (32 bytes)");
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Email, user.Email),
           // new Claim("Userid",user.userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }




    }
}
