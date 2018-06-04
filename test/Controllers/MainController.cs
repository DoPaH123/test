using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test.Models;


namespace test.Controllers
{
    //[Produces("application/json")]
    //[Route("api/Main")]
    public class MainController : Controller
    {
        private readonly GeneralContext _context;
        IHostingEnvironment _appEnvironment;

        public MainController(GeneralContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _appEnvironment = hostingEnvironment;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = from s in _context.Users where s.Name == User.Identity.Name select s.ID;
            int id = user.FirstOrDefault();
            var querry = from s in _context.Songs join u in _context.UserPlaylist on s.ID equals u.SongID where u.UserID == id select s;
            return View(await querry.ToListAsync());
        }
        public async Task<IActionResult> Search(string searchString)
        {
            var songs = from s in _context.Songs select s;
            if (!string.IsNullOrEmpty(searchString))
            {
                songs = songs.Where(s => s.Name.Contains(searchString));
            }
            return View("Search", songs);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Songs.SingleOrDefaultAsync(m => m.ID == id);
            if (song == null)
            {
                return NotFound();
            }
            return View(song);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Author,Genre,,Album")] Song song)
        {
            if (id != song.ID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(song);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SongExists(song.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(song);
        }
        [HttpPost]
        public async Task<IActionResult> AddToPlaylist(int? id)
        {
            if (id != null||id!=0)
            {
                var user = from s in _context.Users where s.Name == User.Identity.Name select s.ID;
                int u_id = user.FirstOrDefault();
                UserPlaylist userPlaylistC = (from s in _context.UserPlaylist
                                              where s.SongID == id && s.UserID == u_id
                                              select s).SingleOrDefault();
                if (userPlaylistC == null)
                {
                    var song = await _context.Songs.SingleOrDefaultAsync(m => m.ID == id);

                    UserPlaylist userPlaylist = new UserPlaylist { SongID = song.ID, UserID = u_id };
                    _context.UserPlaylist.Add(userPlaylist);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost("AddSong")]
        public async Task<IActionResult> AddFile(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                // путь к папке Files
                string path = "/Songs/" + RemoveSpaces(uploadedFile.FileName);
                // сохраняем файл в папку Files в каталоге wwwroot
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                Song file = new Song { Name = uploadedFile.FileName, URL = path };
                _context.Songs.Add(file);
                _context.SaveChanges();
                var user = from s in _context.Users where s.Name == User.Identity.Name select s.ID;
                int id = user.FirstOrDefault();
                UserPlaylist userPlaylist = new UserPlaylist { SongID = file.ID, UserID = id };
                _context.UserPlaylist.Add(userPlaylist);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var song = await _context.Songs
                .SingleOrDefaultAsync(m => m.ID == id);
            if (song == null)
            {
                return NotFound();
            }
            return View(song);
        }

        // POST: Songs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var song = await _context.Songs.SingleOrDefaultAsync(m => m.ID == id);
            var user = from s in _context.Users where s.Name == User.Identity.Name select s.ID;
            int u_id = user.FirstOrDefault();
            var songplay = await _context.UserPlaylist.SingleOrDefaultAsync(m => m.UserID == u_id && m.SongID == id);
            _context.UserPlaylist.Remove(songplay);           
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool SongExists(int id)
        {
            return _context.Songs.Any(e => e.ID == id);
        }
        [HttpPost("LogOut")]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Main");
        }
        private string RemoveSpaces(string inputString)
        {
            inputString = inputString.Replace("  ", "_");
            return inputString;
        }
    }
}