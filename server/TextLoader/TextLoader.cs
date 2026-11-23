using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;
using Serilog;

namespace DocumentAnnotation.TextLoader
{
    /// <summary>
    /// Load texts, e.g. the Pro Milone from local xml files, as stored by Perseus. This keeps a cache of files, and is used as a singleton.
    /// </summary>
    public class TextLoader
    {
        // Newtonsoft matched property names case-insensitively; keep that.
        private static readonly JsonSerializerOptions TextOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly List<Text> _texts = new List<Text>();
        private readonly IWebHostEnvironment _hostingEnvironment;

        public TextLoader(IWebHostEnvironment env)
        {
            _hostingEnvironment = env;
        }


        public Text LoadText(string textIdentifier)
        {
            // this is a singleton used by concurrent requests, and _texts is a plain List
            lock (_texts)
            {
                return LoadTextLocked(textIdentifier);
            }
        }

        private Text LoadTextLocked(string textIdentifier)
        {
            if (IsLoaded(textIdentifier))
            {
                Log.Information($"Loaded text {textIdentifier} from memory");
                return GetLoaded(textIdentifier);
            }

            using (var originalFile = File.OpenRead($"{_hostingEnvironment.ContentRootPath}/Data/ProcessedTexts/{textIdentifier}.json.gz"))
            {
                using (GZipStream decompressionStream = new GZipStream(originalFile, CompressionMode.Decompress))
                {
                    var text = DeserializeFromStream(decompressionStream);
                    text.Identifier = textIdentifier;
                    _texts.Add(text);
                    Log.Information($"Loaded text {textIdentifier} from disk");
                    return text;
                }
            }
        }

        private bool IsLoaded(string textIdentifier)
        {
            return _texts.Any(t => t.Identifier == textIdentifier);
        }

        private Text GetLoaded(string textIdentifier)
        {
            return _texts.FirstOrDefault(t => t.Identifier == textIdentifier);
        }

        private static Text DeserializeFromStream(Stream stream)
        {
            return JsonSerializer.Deserialize<Text>(stream, TextOptions);
        }
    }
}