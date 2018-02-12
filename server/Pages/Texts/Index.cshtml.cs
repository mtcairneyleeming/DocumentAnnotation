using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Pages.Texts
{
    public class IndexModel : PageModel
    {
        private readonly server.Models.AnnotationContext _context;

        public IndexModel(server.Models.AnnotationContext context)
        {
            _context = context;
        }

        public IList<Text> Text { get;set; }

        public async Task OnGetAsync()
        {
            Text = await _context.Texts.ToListAsync();
        }
    }
}
