using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnnotation.Pages.Texts
{
    public class IndexModel : PageModel
    {
        public readonly AnnotationContext _context;

        public IndexModel(AnnotationContext context)
        {
            _context = context;
        }

        public IList<TextData> Text { get; set; }

        public async Task OnGetAsync()
        {
            Text = await _context.Texts.ToListAsync();
        }
    }
}