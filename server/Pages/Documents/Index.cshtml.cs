using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Pages.Documents
{
    public class IndexModel : PageModel
    {
        private readonly AnnotationContext _context;

        public IndexModel(AnnotationContext context)
        {
            _context = context;
        }

        public IList<DocumentAnnotation> DocumentAnnotation { get;set; }

        public async Task OnGetAsync()
        {
            DocumentAnnotation = await _context.DocumentAnnotations.Include(da=> da.User).Include(da=> da.Text).ToListAsync();
        }
    }
}
