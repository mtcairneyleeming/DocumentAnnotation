﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DocumentAnnotation.Models;

namespace server.Pages.Admin.LinkShorteners
{
    public class EditModel : PageModel
    {
        private readonly DocumentAnnotation.Models.AnnotationContext _context;

        public EditModel(DocumentAnnotation.Models.AnnotationContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LinkShortener LinkShortener { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            LinkShortener = await _context.LinkShorteners.FirstOrDefaultAsync(m => m.LinkShortenerId == id);

            if (LinkShortener == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(LinkShortener).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LinkShortenerExists(LinkShortener.LinkShortenerId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool LinkShortenerExists(int id)
        {
            return _context.LinkShorteners.Any(e => e.LinkShortenerId == id);
        }
    }
}
