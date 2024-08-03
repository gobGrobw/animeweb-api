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
    [Route("api/MangaList")]
    [EnableCors("AllowCors")]
    public class MangaListController(Supabase.Client context) : ControllerBase
    {
        private readonly Supabase.Client _context = context;
        private readonly AuthService authService = new();

        // GET: /api/MangaList/{id}
        // Get anime in user list based on id
        [HttpGet("{id}")]
        public async Task<ActionResult> GetMangaList(long id)
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            ModeledResponse<User> user = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Get();

            var userMangaList = user.Model!.MangaList.Where(x => x.Id == id);
            return Ok(userMangaList);
        }

        // GET: /api/MangaList/all
        // Get all manga saved by user
        [HttpGet]
        [Route("all")]
        public async Task<ActionResult> GetMangaList()
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            ModeledResponse<User> userMangaList = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Get();

            Dictionary<string, object> response =
                new()
                {
                    { "payload", userMangaList.Model!.MangaList },
                    { "count", userMangaList.Model!.MangaList.Count },
                    {
                        "reading",
                        userMangaList
                            .Model!.MangaList.Where(x =>
                                string.Equals(
                                    x.ReadStatus,
                                    "reading",
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            .Count()
                    },
                    {
                        "completed",
                        userMangaList
                            .Model!.MangaList.Where(x =>
                                string.Equals(
                                    x.ReadStatus,
                                    "completed",
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            .Count()
                    },
                    {
                        "planned",
                        userMangaList
                            .Model!.MangaList.Where(x =>
                                string.Equals(
                                    x.ReadStatus,
                                    "planned",
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            .Count()
                    },
                    {
                        "dropped",
                        userMangaList
                            .Model!.MangaList.Where(x =>
                                string.Equals(
                                    x.ReadStatus,
                                    "dropped",
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            .Count()
                    },
                };

            return Ok(response);
        }

        // POST: /api/MangaList/add
        // Add manga to user list
        [HttpPost]
        [Route("add")]
        public async Task<ActionResult> AddMangaToList([FromBody] MangaReadlist mangaList)
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            User? user = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Single();

            user!.MangaList!.Add(mangaList);
            Console.WriteLine(mangaList.ImgUrl);
            await user.Update<User>();

            return Ok(user);
        }

        // PATCH: /api/MangaList/patch
        // Update read status of manga
        [HttpPatch]
        [Route("update")]
        public async Task<ActionResult> UpdateMangaList([FromBody] MangaReadlist patchedList)
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            User? user = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Single();

            MangaReadlist manga = user!.MangaList.Find(x => x.Id == patchedList.Id)!;
            manga.Id = patchedList.Id;
            manga.Title = patchedList.Title;
            manga.ReadStatus = patchedList.ReadStatus;
            await user.Update<User>();

            return Ok(manga);
        }

        // DELETE: /api/MangaList/delete/{id}
        // delete manga from list
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<ActionResult> DeleteMangaFromList(long id)
        {
            JwtPayload jwtClaims = authService.GetAndDecodeJwtToken(Request);
            string username = (string)jwtClaims.First(p => p.Key == "unique_name").Value;
            string email = (string)jwtClaims.First(p => p.Key == "email").Value;

            User? user = await _context
                .From<User>()
                .Where(x => x.Username == username && x.Email == email)
                .Single();

            MangaReadlist mangaToRemove = user!.MangaList.Find(x => x.Id == id)!;
            user!.MangaList.Remove(mangaToRemove);
            await user.Update<User>();

            return Ok(user);
        }
    }
}
