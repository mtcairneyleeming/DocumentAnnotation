using System.Collections.Generic;
using System.Linq;

namespace DocumentAnnotation.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> SplitByIndex(this string @string, int[] indexes)
        {
            var previousIndex = 0;
            foreach (var index in indexes.OrderBy(i => i))
            {
                yield return @string.Substring(previousIndex, index - previousIndex);
                previousIndex = index;
            }

            yield return @string.Substring(previousIndex);
        }
    }
}