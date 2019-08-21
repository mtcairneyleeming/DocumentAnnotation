using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DocumentAnnotation.Models
{
    public class Highlight
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HighlightId { get; set; }

        public Location Location { get; set; }

        [JsonIgnore]
        public Annotation Annotation { get; set; }

        public int AnnotationId { get; set; }

        public override string ToString()
        {
            return $"{nameof(HighlightId)}: {HighlightId}, {nameof(Location)}: {Location}, {nameof(AnnotationId)}: {AnnotationId}";
        }
    }
}