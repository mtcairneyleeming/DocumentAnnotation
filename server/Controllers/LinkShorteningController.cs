using System.Linq;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentAnnotation.Controllers
{
    [Route("ls")]
    
    public class LinkShorteningController : Controller
    {
        private AnnotationContext _context;

        public LinkShorteningController(AnnotationContext context)
        {
            _context = context;
        }

        [HttpGet("{route}")]
        [AllowAnonymous]
        public IActionResult LengthenUrl([FromRoute] string route)
        {
            var longRoute = _context.LinkShorteners.FirstOrDefault(ls => ls.ShortLink == route);
            if (longRoute is null)
            {
                return NotFound();
            }

            return Redirect(longRoute.OriginalLink);
        }
    }
}