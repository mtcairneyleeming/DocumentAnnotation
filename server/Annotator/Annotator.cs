using System.Collections.Generic;
using System.Linq;
using DocumentAnnotation.Models;
using DocumentAnnotation.TextLoader;

namespace DocumentAnnotation.Annotator
{
    public class Annotator
    {
        public Annotator(List<Annotation> annotations, List<Group> groups)
        {
            Annotations = annotations.Where(a => a.Highlights.Count > 0).OrderBy(a => a.Highlights.First().Location).ToList();
            Annotations.AddRange(annotations.Where(a => a.Highlights.Count == 0));
            Groups = groups;
            GenerateHighlightData();
            SimplifyHighlights();
        }


        // inputs
        public List<Annotation> Annotations { get; }
        public List<Group> Groups { get; }



        private string[] UnderLineColours { get; } =
        {
            "#e6194B",
            "#3cb44b",
            "#ffe119",
            "#4363d8",
            "#f58231",
            "#911eb4",
            "#42d4f4",
            "#f032e6",
            "#7def00",
            "#fabebe",
            "#469990",
            "#e6beff",
            "#9A6324",
            "#800000",
        }; //14

        private readonly string[] _highlightColours =
        {
            "#ff0000",
            "#00ff00",
            "#42d4f4",
            "#9c70ff",
            "#ff30de",
            "#ff5e00"
        }; //6

        private readonly string[] _bracketColours =
        {
            "#000000",
            "#000075",
            "#800000",
            "#911eb4"
        }; //4


        private readonly (HighlightType, int)[] _colourMap =
        {
            (HighlightType.Highlight, 0), //0
            (HighlightType.Underline, 0),
            (HighlightType.Underline, 1),
            (HighlightType.Underline, 2), //3
            (HighlightType.Highlight, 1),
            (HighlightType.Bracket, 0),
            (HighlightType.Underline, 3), //6
            (HighlightType.Underline, 4),
            (HighlightType.Highlight, 2),
            (HighlightType.Bracket, 1), //9
            (HighlightType.Underline, 5),
            (HighlightType.Underline, 6),
            (HighlightType.Highlight, 3), //12
            (HighlightType.Bracket, 2),
            (HighlightType.Underline, 7),
            (HighlightType.Underline, 8), //15
            (HighlightType.Highlight, 4),
            (HighlightType.Underline, 9),
            (HighlightType.Underline, 10), //18
            (HighlightType.Bracket, 3),
            (HighlightType.Highlight, 5),
            (HighlightType.Underline, 11), //21
            (HighlightType.Underline, 12),

            (HighlightType.Underline, 13), //23
        };


        private (HighlightType, int) GetAnnotationData(int annId)
        {
            var index = annId % 24;
            return _colourMap[index];
        }

        public string GetAnnotationColour(int annId)
        {
            var data = GetAnnotationData(annId);
            switch (data.Item1)
            {
                case HighlightType.Bracket:
                    return _bracketColours[data.Item2];
                case HighlightType.Underline:
                    return UnderLineColours[data.Item2];
                case HighlightType.Highlight:
                    return _highlightColours[data.Item2];
                default:
                    return null;
            }
        }


        /// <summary>
        /// Data to determine class, colour and the like for each annotation
        /// </summary>
        private Dictionary<int, AnnotationData> AnnotationData { get; } = new Dictionary<int, AnnotationData>();

        /// <summary>
        /// a list of highlights, sorted by group.
        /// </summary>
        public List<List<SimpleHighlight>> Highlights { get; } = new List<List<SimpleHighlight>>();

        /// <summary>
        /// Generate a list of (hopefully) distinct colours with which to highlight stuff
        /// </summary>
        private void GenerateHighlightData()
        {
            foreach (var ann in Annotations)
            {
                var data = GetAnnotationData(ann.AnnotationId);
                AnnotationData.Add(ann.AnnotationId, new AnnotationData() {Colour = data.Item2, Type = data.Item1});
            }
        }

        private void SimplifyHighlights()
        {
            Highlights.Clear();
            for (var i = 0; i < Groups.Count; i++)
            {
                Highlights.Add(new List<SimpleHighlight>());
                foreach (var annotation in Annotations)
                {
                    foreach (var h in annotation.Highlights)
                    {
                        if (h.Location.GroupNumber == i)
                        {
                            Highlights[i].Add(new SimpleHighlight
                            {
                                WordNumber = h.Location.WordNumber,
                                AnnotationId = annotation.AnnotationId
                            });
                        }
                    }
                }
            }
        }


        private List<int> GetAnnotationIds(int groupNo, int word)
        {
            var ids = new List<int>();
            foreach (var highlight in Highlights[groupNo])
            {
                if (highlight.WordNumber == word)
                {
                    ids.Add(highlight.AnnotationId);
                }
            }

            return ids;
        }

        public List<List<List<int>>> GetIdsForAllWords()
        {
            var data = new List<List<List<int>>>();
            for (int i = 0; i < Groups.Count; i++)
            {
                var groupData = new List<List<int>>();
                for (int j = 0; j < Groups[i].Data.Count; j++)
                {
                    groupData.Add(GetAnnotationIds(i, j)); 
                }
                data.Add(groupData);
            }

            return data;
        }
        public Dictionary<int, string> GetColoursForAllWords()
        {
            var data = new Dictionary<int, string>();
            foreach (var annotation in Annotations)
            {
                data.Add(annotation.AnnotationId, GetAnnotationColour(annotation.AnnotationId));
            }

            return data;
        }
    }
}