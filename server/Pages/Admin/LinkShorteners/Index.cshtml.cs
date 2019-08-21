using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnnotation.Pages.Admin.LinkShorteners
{
    public class IndexModel : PageModel
    {
        private readonly AnnotationContext _context;

        public IndexModel(AnnotationContext context)
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
