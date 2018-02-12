using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Pages.Annotations
{
    public class DetailsModel : PageModel
    {
        private readonly server.Models.AnnotationContext _context;

        public DetailsModel(server.Models.AnnotationContext context)
        {
            _context = context;
        }

        public Annotation Annotation { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Annotation = await _context.Annotations.SingleOrDefaultAsync(m => m.AnnotationId == id);

            if (Annotation == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
