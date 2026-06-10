using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToplulukYonetimSistemi.Data;
using ToplulukYonetimSistemi.Models;

namespace ToplulukYonetimSistemi.Controllers
{
    public class CommunityEventsController : Controller
    {
        private readonly AppDbContext _context;

        public CommunityEventsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CommunityEvents
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Events.Include(c => c.Community);
            return View(await appDbContext.ToListAsync());
        }

        // GET: CommunityEvents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var communityEvent = await _context.Events.Include(c => c.Community)
                .Include(c => c.Community)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (communityEvent == null)
            {
                return NotFound();
            }

            return View(communityEvent);
        }

        // GET: CommunityEvents/Create
        public IActionResult Create()
        {
            ViewData["CommunityId"] = new SelectList(_context.Communities, "Id", "Name");
            return View();
        }

        // POST: CommunityEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,EventDate,CommunityId")] CommunityEvent communityEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(communityEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CommunityId"] = new SelectList(_context.Communities, "Id", "Name", communityEvent.CommunityId);
            return View(communityEvent);
        }

        // GET: CommunityEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var communityEvent = await _context.Events.FindAsync(id);
            if (communityEvent == null)
            {
                return NotFound();
            }
            ViewData["CommunityId"] = new SelectList(_context.Communities, "Id", "Name", communityEvent.CommunityId);
            return View(communityEvent);
        }

        // POST: CommunityEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,EventDate,CommunityId")] CommunityEvent communityEvent)
        {
            if (id != communityEvent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(communityEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommunityEventExists(communityEvent.Id))
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
            ViewData["CommunityId"] = new SelectList(_context.Communities, "Id", "Name", communityEvent.CommunityId);
            return View(communityEvent);
        }

        // GET: CommunityEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var communityEvent = await _context.Events
                .Include(c => c.Community)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (communityEvent == null)
            {
                return NotFound();
            }

            return View(communityEvent);
        }

        // POST: CommunityEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var communityEvent = await _context.Events.FindAsync(id);
            if (communityEvent != null)
            {
                _context.Events.Remove(communityEvent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommunityEventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
