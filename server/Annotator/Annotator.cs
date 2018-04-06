using System.Collections.Generic;
using System.Linq;
using server.Models;
using server.TextLoader.Models;

namespace server.Pages.Annotate
{
    using GroupData = List<PairWithData>;

    public class Annotator
    {
        public Annotator(List<Annotation> annotations, List<Group> groups)
        {
            Annotations = annotations;
            Groups = groups;
        }


        // inputs
        public List<Annotation> Annotations { get; }
        private List<Group> Groups { get; }

        // output

        public List<GroupData> TransformedData { get; } = new List<GroupData>();


        // internal working 
        /// <summary>
        /// Map of highlight type to the initial used in the CSS class
        /// </summary>
        private Dictionary<HighlightType, string> HighlightInitials { get; } =
            new Dictionary<HighlightType, string>
            {
                {HighlightType.Bracket, "B"},
                {HighlightType.Underline, "U"},
                {HighlightType.Highlight, "H"}
            };

        private Dictionary<HighlightType, Dictionary<int, string>> Colours { get; } =
            new Dictionary<HighlightType, Dictionary<int, string>>
            {
                {
                    HighlightType.Bracket, new Dictionary<int, string>
                    {
                        {1, "#666666"},
                        {2, "#666666"}
                    }
                },
                {
                    HighlightType.Underline, new Dictionary<int, string>
                    {
                        {1, "#008000"},
                        {2, "#0000ff"},
                        {3, "#ff0000"},
                        {4, "#9370db"}
                    }
                },
                {
                    HighlightType.Highlight, new Dictionary<int, string>
                    {
                        {1, "#ffff00"},
                        {2, "#48CCC9"},
                        {3, "#FFA858"},
                        {4, "#74FFAD"},
                        {5, "#FF8CAC"},
                        {6, "#CC54B1"}
                    }
                },
            };

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
        public void GenerateHighlightData()
        {
            for (int i = 0; i < Annotations.Count; i++)
            {
                var h = i % 12; // number of the highlight this annotation will receive
                if (h < 4) // leads to annU1...4
                {
                    AnnotationData.Add(Annotations[i].AnnotationId, new AnnotationData
                    {
                        Colour = h + 1,
                        Type = HighlightType.Underline
                    });
                }
                else if (h < 10) // leads to annH1...6
                {
                    AnnotationData.Add(Annotations[i].AnnotationId, new AnnotationData
                    {
                        Colour = h - 5,
                        Type = HighlightType.Highlight
                    });
                }
                else // leads to annU1...2
                {
                    AnnotationData.Add(Annotations[i].AnnotationId, new AnnotationData
                    {
                        Colour = h - 9,
                        Type = HighlightType.Bracket
                    });
                }
            }
        }

        private void SimplifyHighlights()
        {
            Highlights.Clear();
            for (int i = 0; i < Groups.Count; i++)
            {
                Highlights.Add(new List<SimpleHighlight>());
                var group = Groups[i];
                foreach (var annotation in Annotations)
                {
                    foreach (var h in annotation.Highlights)
                    {
                        if (h.Start.GroupNumber > i || h.End.GroupNumber < i)
                        {
                            // ignore
                        }
                        else
                        {
                            Highlights[i].Add(new SimpleHighlight
                            {
                                Start = h.Start.GroupNumber == i ? h.Start.Character : 0,
                                End = h.End.GroupNumber == i ? h.End.Character : group.Data.Length - 1,
                                AnnotationId = annotation.AnnotationId
                            });
                        }
                    }
                }
            }
        }

        private List<Pair> FindSplittingPairs(int groupNo)
        {
            // so as to split the group into as few <span>'s as possible, work out where boundaries must go.

            var splitPoints = new HashSet<int>();
            foreach (var h in Highlights[groupNo])
            {
                splitPoints.Add(h.Start);
                splitPoints.Add(h.End + 1);
            }

            splitPoints.Add(0); // add so all the text is actually rendered
            splitPoints.Add(Groups[groupNo].Data.Length); // as above

            var sortedPoints = splitPoints.ToList().OrderBy(o => o).ToList();

            var pairs = new List<Pair>();

            for (var l = 0; l < sortedPoints.Count - 1; l++)

            {
                pairs.Add(new Pair(sortedPoints[l], sortedPoints[l + 1]));
            }

            return pairs;
        }

        private List<string> BuildCssClasses(Pair pair, int groupNo)
        {
            var classes = new List<string>();
            foreach (var highlight in Highlights[groupNo])
            {
                var annDetails = AnnotationData[highlight.AnnotationId];
                var typeInitial = HighlightInitials[annDetails.Type];
                if (annDetails.Type == HighlightType.Bracket)
                {
                    // ends at the end of this <span> 
                    if (highlight.End == pair.End)
                    {
                        classes.Add($"ann{typeInitial}{AnnotationData[highlight.AnnotationId].Colour}End");
                    }

                    // starts at the start of this <span> 
                    if (highlight.Start == pair.Start)
                    {
                        classes.Add($"ann{typeInitial}{AnnotationData[highlight.AnnotationId].Colour}Start");
                    }

                    // goes straight through this <span>
                    if (highlight.Start < pair.Start && highlight.End > pair.End)
                    {
                        classes.Add($"ann{typeInitial}{AnnotationData[highlight.AnnotationId].Colour}Middle");
                    }
                }
                else
                {
                    // any relation at all to this span
                    if (highlight.End == pair.End || highlight.Start == pair.Start ||
                        highlight.Start < pair.Start && highlight.End > pair.End)
                    {
                        classes.Add($"ann{typeInitial}{AnnotationData[highlight.AnnotationId].Colour}");
                    }
                }

                // 
            }

            return classes;
        }

        private List<int> GetAnnotationIds(Pair pair, int groupNo)
        {
            var ids = new List<int>();
            foreach (var highlight in Highlights[groupNo])
            {
                // any relation at all to this span
                if (highlight.End == pair.End || highlight.Start == pair.Start ||
                    highlight.Start < pair.Start && highlight.End > pair.End)
                {
                    ids.Add(highlight.AnnotationId);
                }
            }

            return ids;
        }

        private GroupData HighlightGroup(int groupNo)
        {
            var pairs = FindSplittingPairs(groupNo);


            var result = new GroupData();
            foreach (var pair in pairs)
            {
                result.Add(new PairWithData(pair, BuildCssClasses(pair, groupNo), GetAnnotationIds(pair, groupNo)));
            }

            return result;
        }

        public void Run()
        {
            GenerateHighlightData();
            SimplifyHighlights();
            for (int i = 0; i < Groups.Count; i++)
            {
                TransformedData.Add(HighlightGroup(i));
            }
        }

        public string GetColourName(int annotationId)
        {
            var annData = AnnotationData[annotationId];
            return Colours[annData.Type][annotationId];
        }
    }
}