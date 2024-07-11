using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Supabase.Postgrest.Responses;

namespace api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/AnimeList")]
    public class AnimeListController(Supabase.Client context) : ControllerBase
    {
        private readonly Supabase.Client _context = context;
        private readonly AuthService authService = new();

        // GET: /api/AnimeList
        // Get all anime saved by user
        [HttpGet]
        public async Task<ActionResult> GetAnimeList()
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            ModeledResponse<User> userAnimeList = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Get();

            return Ok(userAnimeList.Model!.AnimeList);
        }

        // POST: /api/AnimeList
        // Add anime to user list
        [HttpPost]
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

        // PATCH: /api/AnimeList
        // Update watch status of anime (watching, completed, etc)
        [HttpPatch]
        public async Task<ActionResult> UpdateAnimeList([FromBody] AnimeWatchlist animeList)
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            User? user = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Single();

            AnimeWatchlist anime = user!.AnimeList.Find(x => x.Id == animeList.Id)!;

            anime.Id = animeList.Id;
            anime.Title = animeList.Title;
            anime.WatchStatus = animeList.WatchStatus;
            await user.Update<User>();

            return Ok(anime);
        }

        // DELETE: /api/AnimeList
        // delete anime from list
        [HttpDelete]
        public async Task<ActionResult> DeleteAnimeFromList([FromBody] AnimeWatchlist animeList)
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            User? user = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Single();

            AnimeWatchlist animeToRemove = user!.AnimeList.Find(x => x.Id == animeList.Id)!;
            user!.AnimeList.Remove(animeToRemove);
            await user.Update<User>();

            return Ok(user);
        }
    }
}
