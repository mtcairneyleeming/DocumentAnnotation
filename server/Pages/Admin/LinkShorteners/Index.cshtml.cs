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
    public class IndexModel : PageModel
    {
        private readonly DocumentAnnotation.Models.AnnotationContext _context;

        public IndexModel(DocumentAnnotation.Models.AnnotationContext context)
        {
            _context = context;
        }

        public IList<LinkShortener> LinkShorteners { get;set; }

        public async Task OnGetAsync()
        {
            LinkShorteners = await _context.LinkShorteners.ToListAsync();
        }
    }
}
