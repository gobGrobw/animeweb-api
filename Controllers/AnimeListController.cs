using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Supabase.Postgrest.Responses;

namespace api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/AnimeList")]
    [EnableCors("AllowCors")]
    public class AnimeListController(Supabase.Client context) : ControllerBase
    {
        private readonly Supabase.Client _context = context;
        private readonly AuthService authService = new();

        // GET: /api/AnimeList/{id}
        // Get anime in user list based on id
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAnimeList(long id)
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            ModeledResponse<User> user = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Get();

            var userAnimeList = user.Model!.AnimeList.Where(x => x.Id == id);
            return Ok(userAnimeList);
        }

        // GET: /api/AnimeList/all
        // Get all anime saved by user
        [HttpGet]
        [Route("all")]
        public async Task<ActionResult> GetAllAnimeList()
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            ModeledResponse<User> userAnimeList = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Get();

            Dictionary<string, object> response =
                new()
                {
                    { "payload", userAnimeList.Model!.AnimeList },
                    { "count", userAnimeList.Model!.AnimeList.Count },
                    {
                        "watching",
                        userAnimeList
                            .Model!.AnimeList.Where(x =>
                                string.Equals(
                                    x.WatchStatus,
                                    "watching",
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            .Count()
                    },
                    {
                        "completed",
                        userAnimeList
                            .Model!.AnimeList.Where(x =>
                                string.Equals(
                                    x.WatchStatus,
                                    "completed",
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            .Count()
                    },
                    {
                        "planned",
                        userAnimeList
                            .Model!.AnimeList.Where(x =>
                                string.Equals(
                                    x.WatchStatus,
                                    "planned",
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            .Count()
                    },
                    {
                        "dropped",
                        userAnimeList
                            .Model!.AnimeList.Where(x =>
                                string.Equals(
                                    x.WatchStatus,
                                    "dropped",
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            .Count()
                    },
                };

            return Ok(response);
        }

        // POST: /api/AnimeList/add
        // Add anime to user list
        [HttpPost]
        [Route("add")]
        public async Task<ActionResult> AddAnimeToList([FromBody] AnimeWatchlist animeList)
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            User? user = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Single();

            user!.AnimeList!.Add(animeList);
            await user.Update<User>();

            return Ok(user);
        }

        // PATCH: /api/AnimeList/update
        // Update watch status of anime (watching, completed, etc)
        [HttpPatch]
        [Route("update")]
        public async Task<ActionResult> UpdateAnimeList([FromBody] AnimeWatchlist patchedList)
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            User? user = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Single();

            AnimeWatchlist anime = user!.AnimeList.First(x => x.Id == patchedList.Id)!;
            anime.Id = patchedList.Id;
            anime.Title = patchedList.Title;
            anime.WatchStatus = patchedList.WatchStatus;
            await user.Update<User>();

            return Ok(anime);
        }

        // DELETE: /api/AnimeList/delete
        // delete anime from list
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<ActionResult> DeleteAnimeFromList(long id)
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            User? user = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Single();

            AnimeWatchlist animeToRemove = user!.AnimeList.Find(x => x.Id == id)!;
            user!.AnimeList.Remove(animeToRemove);
            await user.Update<User>();

            return Ok(user);
        }
    }
}
