using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.TextLoader;
using server.TextLoader.Models;

namespace server.Pages.Texts
{
    public class DetailsModel : PageModel
    {
        private readonly server.Models.AnnotationContext _context;
        private readonly TextLoader.TextLoader _loader;
        public DetailsModel(server.Models.AnnotationContext context, TextLoader.TextLoader loader)
        {
            _context = context;
            _loader = loader;
        }

        public TextData Text { get; set; }
        public Text ActualText { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Text = await _context.Texts.SingleOrDefaultAsync(m => m.TextId == id);

            if (Text == null)
            {
                return NotFound();
            }
            // get the actual text
            ActualText = _loader.LoadText(Text.Identifier);
            
            return Page();
        }
    }
}
