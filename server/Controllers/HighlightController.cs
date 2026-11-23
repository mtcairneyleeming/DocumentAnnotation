using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentAnnotation.Authorization;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnnotation.Controllers
{
    /// <summary>
    /// Bulk add/remove of highlights. Both need Edit access to the document of every annotation involved (see
    /// <see cref="DocumentAuthorizationHandler"/>). Reading highlights is done through GET api/Annotations/{id}, which
    /// includes them.
    /// </summary>
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [Route("api/Highlights")]
    public class HighlightController : ControllerBase
    {
        private readonly AnnotationContext _context;
        private readonly IAuthorizationService _authorization;

        public HighlightController(AnnotationContext context, IAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        [HttpPost]
        public async Task<IActionResult> PostHighlights([FromBody] List<Highlight> highlights)
        {
            if (highlights == null || highlights.Any(h => h.Location == null))
            {
                return BadRequest();
            }

            var annotationIds = highlights.Select(h => h.AnnotationId).Distinct().ToList();
            var denied = await DenyUnlessCanEdit(annotationIds);
            if (denied != null)
            {
                return denied;
            }

            foreach (var highlight in highlights)
            {
                highlight.HighlightId = 0;
            }

            _context.Highlights.AddRange(highlights);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostHighlights), highlights.Select(x => new {id = x.HighlightId}), highlights);
        }

        [HttpDelete("")]
        public async Task<IActionResult> DeleteHighlights([FromBody] List<Highlight> highlights)
        {
            if (highlights == null)
            {
                return BadRequest();
            }

            // use the stored highlights, not the (client-controlled) annotation ids in the request
            var ids = highlights.Select(h => h.HighlightId).Distinct().ToList();
            var stored = await _context.Highlights.Where(h => ids.Contains(h.HighlightId)).ToListAsync();
            if (stored.Count != ids.Count)
            {
                return NotFound();
            }

            var denied = await DenyUnlessCanEdit(stored.Select(h => h.AnnotationId).Distinct().ToList());
            if (denied != null)
            {
                return denied;
            }

            _context.Highlights.RemoveRange(stored);
            await _context.SaveChangesAsync();

            return Ok(stored);
        }

        // null if the user may edit the documents of all these annotations (which must exist), else the response to send
        private async Task<IActionResult> DenyUnlessCanEdit(List<int> annotationIds)
        {
            var annotations = await _context.Annotations.Include(a => a.Document)
                .Where(a => annotationIds.Contains(a.AnnotationId)).ToListAsync();
            if (annotations.Count != annotationIds.Count)
            {
                return NotFound();
            }

            foreach (var document in annotations.Select(a => a.Document).Distinct())
            {
                var denied = (await _authorization.CheckDocumentAsync(User, document, DocumentOperations.Edit)).ToApiResult();
                if (denied != null)
                {
                    return denied;
                }
            }

            return null;
        }
    }
}
