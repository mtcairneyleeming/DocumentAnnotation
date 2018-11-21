using System.Collections;
using System.Collections.Generic;
using DocumentAnnotation.Models;

namespace DocumentAnnotation.Pages.Partials
{
    public class AnnotationTablePartial
    {
        public AnnotationTablePartial(List<Annotation> annotations, Dictionary<int, string> annotationColour, bool isEditable = false)
        {
            Annotations = annotations;
            AnnotationColour = annotationColour;
            IsEditable = isEditable;
        }

        public List<Annotation> Annotations { get; set; }
        public Dictionary<int, string> AnnotationColour { get; set; }
        public bool IsEditable { get; set; }
    }
}