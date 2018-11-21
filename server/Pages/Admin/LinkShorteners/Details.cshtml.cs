using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DocumentAnnotation.Models;

namespace server.Pages.Admin.LinkShorteners
{
    public class DetailsModel : PageModel
    {
        private readonly DocumentAnnotation.Models.AnnotationContext _context;

        public DetailsModel(DocumentAnnotation.Models.AnnotationContext context)
        {
            _context = context;
        }

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
    }
}
