using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnnotation.Pages.Documents
{
    public class EditModel : PageModel
    {
        private readonly AnnotationContext _context;

        public EditModel(AnnotationContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Document Document { get; set; }

        [BindProperty]
        public string AllowedUsers { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Document = await _context.DocumentAnnotations.Include(da => da.User).SingleOrDefaultAsync(m => m.DocumentId == id &&
                                                                                                           m.UserId == User
                                                                                                               .FindFirst(ClaimTypes.NameIdentifier)
                                                                                                               .Value);

            if (Document == null)
            {
                return NotFound();
            }

            AllowedUsers = string.Join(",", Document.AllowedUsers ?? new string[0]);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!(AllowedUsers is null))
                Document.AllowedUsers = AllowedUsers.Split(",").ToList().Select(a => a.Trim()).ToArray();

            var test = _context.DocumentAnnotations.AsNoTracking().SingleOrDefault(m =>
                m.DocumentId == Document.DocumentId &&
                m.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (test is null)
            {
                return NotFound();
            }

            _context.Attach(Document).State = EntityState.Modified;
            await _context.SaveChangesAsync();


            return RedirectToPage("./Index");
        }
    }
}