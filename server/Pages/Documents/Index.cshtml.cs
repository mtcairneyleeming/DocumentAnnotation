using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnnotation.Pages.Documents
{
    public class IndexModel : PageModel
    {
        private readonly AnnotationContext _context;

        public IndexModel(AnnotationContext context)
        {
            _context = context;
        }

        public IList<Models.DocumentAnnotation> DocumentAnnotation { get; set; }

        public async Task OnGetAsync()
        {
            DocumentAnnotation = await _context.DocumentAnnotations.Include(da => da.User).Include(da => da.Text).ToListAsync();
        }
    }
}