using System.Collections.Generic;

namespace server.Models
{
    /// <summary>
    /// A record that represents a user's annotation of a document, and multiple can exist for a document-user combination.
    /// </summary>
    public class DocumentAnnotation
    {
        public int DocumentAnnotationId { get; set; }
        
        public int TextId { get; set; }
        public string UserId { get; set; } // currently unused
        public List<Annotation> Annotations { get; set; }
    }
}