using System.Collections.Generic;
using DocumentAnnotation.Models;

namespace DocumentAnnotation.Pages.Partials
{
    public class AnnotationTablePartialForPrint
    {
        public AnnotationTablePartialForPrint(List<Annotation> annotations, string sectionName)
        {
            Annotations = annotations;
            SectionName = sectionName;
        }

        public string SectionName { get; }

        public List<Annotation> Annotations { get; }

    }
}