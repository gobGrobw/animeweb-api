using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Supabase.Postgrest.Responses;
using static api.AuthService;

namespace api.Routers
{
    [ApiController]
    [Route("api/Auth")]
    public class UserController(Supabase.Client context, IConfiguration config) : ControllerBase
    {
        private readonly Supabase.Client _context = context;
        private readonly IConfiguration _config = config;
        Dictionary<string, object>? jsonResponse;

        // GET: /api/User
        [HttpGet]
        public async Task<ActionResult> GetAllUsers()
        {
            ModeledResponse<User> respond = await _context.From<User>().Get();
            return Ok(respond);
        }

        // GET: /api/User/{id}
        [HttpGet("{username}")]
        public async Task<ModeledResponse<User>> GetSpecificUser(string username)
        {
            ModeledResponse<User> user = await _context
                .From<User>()
                .Where(x => x.Username == username)
                .Get();
            return user;
        }

        // POST: /api/User/sign-up
        [HttpPost]
        [Route("sign-up")]
        public async Task<ActionResult> CreateUser(User user)
        {
            // Check if user or email already registered
            ModeledResponse<User> userExist = await _context
                .From<User>()
                .Where(x => x.Username == user.Username || x.Email == user.Email)
                .Get();

            if (userExist.Models.Count == 0)
            {
                // Hash password
                PasswordHasher<User> passwordHasher = new();
                string hashedPassword = passwordHasher.HashPassword(user, user.Password);
                user.Password = hashedPassword;

                // Insert new user to database
                ModeledResponse<User> response = await _context.From<User>().Insert(user);
                jsonResponse = new()
                {
                    { "statusCode", response.ResponseMessage!.StatusCode },
                    { "message", response.ResponseMessage.ReasonPhrase! }
                };

                return Ok(JsonConvert.SerializeObject(jsonResponse));
            }

            // Check for fitting error to send
            foreach (User model in userExist.Models)
            {
                if (model.Username == user.Username)
                    return StatusCode(400, $"User '{user.Username}' already exists.");
                else
                    return StatusCode(400, $"Email '{user.Email}' already registered");
            }

            return BadRequest("Uhm something bad happened");
        }

        // POST: /api/User/log-in
        // This ask for username and password, no email needed
        [HttpPost]
        [Route("log-in")]
        public async Task<ActionResult> LoginUser(User user)
        {
            // Find the user account
            ModeledResponse<User> userExist = await _context
                .From<User>()
                .Where(x => x.Username == user.Username)
                .Get();

            if (userExist.Model != null)
            {
                // Hash password and compare it
                PasswordHasher<User> passwordHasher = new();
                PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(
                    user,
                    userExist.Model.Password,
                    user.Password
                );

                if (result == PasswordVerificationResult.Success)
                {
                    AuthService authService = new();
                    string token = authService.GenerateJwtToken(userExist.Model, _config);

                    jsonResponse = new()
                    {
                        { "message", "Log-In success" },
                        { "jwt_token", token }
                    };

                    return Ok(JsonConvert.SerializeObject(jsonResponse));
                }
            }

            return StatusCode(400, "User and password does not match");
        }
    }
}
