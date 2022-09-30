using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace DocumentAnnotation.Models
{
    /// <summary>
    /// Represents a comment such as:
    ///     Juxtaposition: contrasts Milo and Clodius to show their hatred for each other
    ///     on 
    /// </summary>
    public class Annotation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnnotationId { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }
        
        public List<Highlight> Highlights { get; set; }

        public int DocumentId { get; set; }

        // TODO: why are there 2 serialisation libraries????
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public Document Document { get; set; }
    }
}