﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogLibrary.Data;
using BlogLibrary.Models;
using Microsoft.AspNetCore.Identity;
using BlogLibrary.Databases.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BlogUI.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly BlogContext _context;
        private readonly IImageService _imageService;
        private readonly UserManager<UserModel> _userManager;
        private readonly ISlugService _slugService;

        public ArticlesController(BlogContext context, ISlugService slugService)
        {
            _context = context;
            _slugService = slugService;
        }

        // GET: Articles
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var blogContext = _context.Articles.Include(a => a.SeriesModel);
            return View(await blogContext.ToListAsync());
        }

        // GET: Articles/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleModel = await _context.Articles
                .Include(a => a.SeriesModel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (articleModel == null)
            {
                return NotFound();
            }

            return View(articleModel);
        }

        // GET: Articles/Create
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            ViewData["SeriesModelId"] = new SelectList(_context.Series, "Id", "Title");
            return View();
        }

        // POST: Articles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Summary,Body,Status,Image,SeriesModelId")] ArticleModel articleModel)
        {
            if (ModelState.IsValid)
            {
                articleModel.Created = DateTime.Now;
                articleModel.CreatorId = _userManager.GetUserId(User);

                articleModel.Image.ImageData = await _imageService.EncodeImageAsync(articleModel.Image.Photo);
                articleModel.Image.ImageExtension = _imageService.ContentType(articleModel.Image.Photo);

                var slug = _slugService.UrlRoute(articleModel.Title);
                
                if(!_slugService.IsUnique(slug))
                {
                    string next = "_nx";
                    articleModel.Slug = slug + next;
                }
                articleModel.Slug = slug;

                _context.Add(articleModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SeriesModelId"] = new SelectList(_context.Series, "Id", "Title", articleModel.SeriesModelId);
            return View(articleModel);
        }

        // GET: Articles/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleModel = await _context.Articles.FindAsync(id);
            if (articleModel == null)
            {
                return NotFound();
            }
            ViewData["SeriesModelId"] = new SelectList(_context.Series, "Id", "Title", articleModel.SeriesModelId);
            return View(articleModel);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Summary,Body,Status,Image,SeriesModelId")] ArticleModel articleModel)
        {
            if (id != articleModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    articleModel.Updated = DateTime.Now;
                    _context.Update(articleModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleModelExists(articleModel.Id))
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
            ViewData["SeriesModelId"] = new SelectList(_context.Series, "Id", "Title", articleModel.SeriesModelId);
            return View(articleModel);
        }

        // GET: Articles/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var articleModel = await _context.Articles
                .Include(a => a.SeriesModel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (articleModel == null)
            {
                return NotFound();
            }

            return View(articleModel);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var articleModel = await _context.Articles.FindAsync(id);
            _context.Articles.Remove(articleModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArticleModelExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}
