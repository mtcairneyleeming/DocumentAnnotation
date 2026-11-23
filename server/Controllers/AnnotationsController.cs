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
    public class AnnotationPutDTO
    {
        public Annotation Annotation { get; set; }
        public List<Highlight> HighlightsToAdd { get; set; }
        public List<Highlight> HighlightsToRemove { get; set; }
    }

    public class AnnotationPostDTO
    {
        public Annotation Annotation { get; set; }
        public List<Highlight> HighlightsToAdd { get; set; }
    }

    /// <summary>
    /// Annotations of a document. Who may read/write is decided by <see cref="DocumentAuthorizationHandler"/>:
    /// reads need View access (public documents work anonymously), writes need Edit access (the owner).
    /// PUT and POST return the saved <see cref="Annotation"/> (including its highlights) as the body.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/Annotations")]
    public class AnnotationsController : ControllerBase
    {
        private readonly AnnotationContext _context;
        private readonly IAuthorizationService _authorization;

        public AnnotationsController(AnnotationContext context, IAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        // GET: api/Annotations/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnnotation([FromRoute] int id)
        {
            var annotation = await LoadAnnotation(id);
            var denied = (await _authorization.CheckDocumentAsync(User, annotation?.Document, DocumentOperations.View)).ToApiResult();
            return denied ?? Ok(annotation);
        }

        // PUT: api/Annotations/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutAnnotation([FromRoute] int id, [FromBody] AnnotationPutDTO dto)
        {
            var incoming = dto?.Annotation;
            if (incoming == null || id != incoming.AnnotationId)
            {
                return BadRequest();
            }

            // authorise against the document the stored annotation belongs to, never the one named in the request
            var existing = await LoadAnnotation(id);
            var denied = (await _authorization.CheckDocumentAsync(User, existing?.Document, DocumentOperations.Edit)).ToApiResult();
            if (denied != null)
            {
                return denied;
            }

            if (incoming.DocumentId != existing.DocumentId)
            {
                return BadRequest();
            }

            var toAdd = dto.HighlightsToAdd ?? new List<Highlight>();
            var toRemoveIds = (dto.HighlightsToRemove ?? new List<Highlight>()).Select(h => h.HighlightId).ToList();
            // only highlights that really belong to this annotation can be removed
            var toRemove = existing.Highlights.Where(h => toRemoveIds.Contains(h.HighlightId)).ToList();
            if (toAdd.Any(h => h.AnnotationId != id || h.Location == null) || toRemove.Count != toRemoveIds.Distinct().Count())
            {
                return BadRequest();
            }

            existing.Title = incoming.Title;
            existing.Body = incoming.Body;
            foreach (var highlight in toAdd)
            {
                highlight.HighlightId = 0;
                existing.Highlights.Add(highlight);
            }

            _context.Highlights.RemoveRange(toRemove);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }

            return Ok(existing);
        }

        // POST: api/Annotations
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostAnnotation([FromBody] AnnotationPostDTO dto)
        {
            var incoming = dto?.Annotation;
            if (incoming == null)
            {
                return BadRequest();
            }

            var document = await _context.DocumentAnnotations.SingleOrDefaultAsync(d => d.DocumentId == incoming.DocumentId);
            var denied = (await _authorization.CheckDocumentAsync(User, document, DocumentOperations.Edit)).ToApiResult();
            if (denied != null)
            {
                return denied;
            }

            var highlights = dto.HighlightsToAdd ?? new List<Highlight>();
            if (highlights.Any(h => h.Location == null))
            {
                return BadRequest();
            }

            // build a new entity rather than trusting the client's object graph (ids, nested highlights, ...)
            var annotation = new Annotation
            {
                Title = incoming.Title,
                Body = incoming.Body,
                DocumentId = document.DocumentId,
                Highlights = highlights.Select(h => new Highlight {Location = h.Location}).ToList()
            };
            _context.Annotations.Add(annotation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAnnotation), new {id = annotation.AnnotationId}, annotation);
        }

        // DELETE: api/Annotations/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAnnotation([FromRoute] int id)
        {
            var annotation = await LoadAnnotation(id);
            var denied = (await _authorization.CheckDocumentAsync(User, annotation?.Document, DocumentOperations.Edit)).ToApiResult();
            if (denied != null)
            {
                return denied;
            }

            _context.Annotations.Remove(annotation);
            await _context.SaveChangesAsync();

            return Ok(annotation);
        }

        private Task<Annotation> LoadAnnotation(int id)
        {
            return _context.Annotations
                .Include(a => a.Document)
                .Include(a => a.Highlights)
                .SingleOrDefaultAsync(a => a.AnnotationId == id);
        }
    }
}
