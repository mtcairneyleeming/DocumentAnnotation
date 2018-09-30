﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DocumentAnnotation.Models;
using DocumentAnnotation.TextLoader.Models;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

    public class AnnotationColourDTO
    {
        public int AnnotationId { get; set; }
        public string Colour { get; set; }
        public string ColourClassName { get; set; }
    }


    [Produces("application/json")]
    [Route("api/Annotations")]
    public class AnnotationsController : Controller
    {
        private readonly AnnotationContext _context;

        public AnnotationsController(AnnotationContext context)
        {
            _context = context;
        }

        // GET: api/Annotations
        [HttpGet]
        public IEnumerable<Annotation> GetAnnotations()
        {
            return _context.Annotations;
        }


        // GET: api/Annotations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Annotation>> GetAnnotation([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var annotation = await _context.Annotations.SingleOrDefaultAsync(m => m.AnnotationId == id);

            if (annotation == null)
            {
                return NotFound();
            }

            return Ok(annotation);
        }

        // PUT: api/Annotations/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Annotation>> PutAnnotation([FromRoute] int id, [FromBody] AnnotationPutDTO annotation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ann = annotation.Annotation;

            if (id != ann.AnnotationId)
            {
                return BadRequest();
            }

            var docAnn =
                _context.DocumentAnnotations.SingleOrDefault(d =>
                    d.DocumentAnnotationId == ann.DocumentAnnotationId);
            if (docAnn is null)
            {
                return BadRequest();
            }

            if (docAnn.UserId != GetCurrentUser())
            {
                return Unauthorized();
            }

            foreach (var highlight in annotation.HighlightsToAdd)
            {
                if (highlight.AnnotationId != ann.AnnotationId)
                {
                    return BadRequest();
                }

                highlight.AnnotationId = id;
            }

            foreach (var highlight in annotation.HighlightsToRemove)
            {
                if (highlight.AnnotationId != ann.AnnotationId)
                {
                    return BadRequest();
                }
            }


            _context.Annotations.Update(ann);
            _context.Highlights.AddRange(annotation.HighlightsToAdd);
            _context.Highlights.RemoveRange(annotation.HighlightsToRemove);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnnotationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(ann);
        }

        // POST: api/Annotations
        [HttpPost]
        public async Task<IActionResult> PostAnnotation([FromBody] AnnotationPostDTO annotation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ann = annotation.Annotation;
            var docAnn = _context.DocumentAnnotations.SingleOrDefault(da => da.DocumentAnnotationId == ann.DocumentAnnotationId);
            if (docAnn is null || docAnn.UserId != GetCurrentUser())
            {
                return Unauthorized();
            }

            _context.Annotations.Update(ann);

            await _context.SaveChangesAsync();

            foreach (var highlight in annotation.HighlightsToAdd)
            {
                highlight.AnnotationId = ann.AnnotationId;
            }

            _context.Highlights.AddRange(annotation.HighlightsToAdd);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetAnnotation", new {id = annotation.Annotation.AnnotationId}, annotation);
        }

        [HttpGet("{id}/Colour")]
        public ActionResult<AnnotationColourDTO> Colour([FromRoute] int id)
        {
            var annotation = _context.Annotations.Include(a=> a.Highlights).SingleOrDefault(a => a.AnnotationId == id);
            var docAnn = _context.DocumentAnnotations.SingleOrDefault(da => da.DocumentAnnotationId == annotation.DocumentAnnotationId);
            if (docAnn is null || docAnn.UserId != GetCurrentUser())
            {
                return Unauthorized();
            }

            var annotator = new Annotator.Annotator(new List<Annotation>() {annotation}, new List<Group>());
            return Ok(new AnnotationColourDTO()
                {AnnotationId = id, Colour = annotator.GetColourName(id), ColourClassName = annotator.GetClassName(id)});
        }

        // DELETE: api/Annotations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnotation([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var annotation = await _context.Annotations.Include(a => a.DocumentAnnotation).SingleOrDefaultAsync(m => m.AnnotationId == id);
            if (annotation == null)
            {
                return NotFound();
            }

            if (annotation.DocumentAnnotation.UserId != GetCurrentUser())
            {
                return Unauthorized();
            }

            _context.Annotations.Remove(annotation);
            await _context.SaveChangesAsync();

            return Ok(annotation);
        }

        private bool AnnotationExists(int id)
        {
            return _context.Annotations.Any(e => e.AnnotationId == id);
        }

        private string GetCurrentUser()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}