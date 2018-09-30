using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Mvc;

namespace DocumentAnnotation.Controllers
{
    [Produces("application/json")]
    [Route("api/Highlights")]
    public class HighlightController : Controller
    {
        private readonly AnnotationContext _context;

        public HighlightController(AnnotationContext context)
        {
            _context = context;
        }

        // GET: api/Highlights
        [HttpGet]
        public IEnumerable<Highlight> GetHighlights()
        {
            return _context.Highlights.Where(h=> h.Annotation.DocumentAnnotation.UserId == GetCurrentUser());
        }


       

        [HttpPost]
        public async Task<IActionResult> PostHighlights([FromBody] List<Highlight>highlights)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var annIds = highlights.Select(h => h.AnnotationId);
            var res = _context.Annotations.Where(a => annIds.Contains(a.AnnotationId)).All(a => a.DocumentAnnotation.UserId == GetCurrentUser());
            if (!res)
            {
                return Unauthorized();
            }
            _context.Highlights.AddRange(highlights);
            await _context.SaveChangesAsync();

            return CreatedAtAction("PostHighlights", highlights.Select(x=> new {id = x.HighlightId}), highlights);
        }

        [HttpDelete("")]
        public async Task<IActionResult> DeleteHighlights([FromBody] List<Highlight> highlights)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var annIds = highlights.Select(h => h.AnnotationId);
            var res = _context.Annotations.Where(a => annIds.Contains(a.AnnotationId)).All(a => a.DocumentAnnotation.UserId == GetCurrentUser());
            if (!res)
            {
                return Unauthorized();
            }

            _context.Highlights.RemoveRange(highlights);
            await _context.SaveChangesAsync();

            return Ok(highlights);
        }



        private string GetCurrentUser()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}