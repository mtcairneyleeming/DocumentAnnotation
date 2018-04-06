using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Pages.Texts
{
    public class IndexModel : PageModel
    {
        private readonly AnnotationContext _context;

        public IndexModel(AnnotationContext context)
        {
            _context = context;
        }

        public IList<TextData> Text { get;set; }

        public async Task OnGetAsync()
        {
            Text = await _context.Texts.ToListAsync();
        }
    }
}
