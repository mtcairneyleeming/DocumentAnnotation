using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DocumentAnnotation.Extensions;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Serilog;

namespace DocumentAnnotation.TextLoader
{
    /// <summary>
    /// Load texts, e.g. the Pro Milone from local xml files, as stored by Perseus. This keeps a cache of files, and is used as a singleton.
    /// </summary>
    public class TextLoader
    {
        private readonly List<Text> _texts = new List<Text>();
        private readonly IHostingEnvironment _hostingEnvironment;

        public TextLoader(IHostingEnvironment env)
        {
            _hostingEnvironment = env;
        }


        public Text LoadText(string textIdentifier)
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
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<Text>(jsonTextReader);
            }
        }
    }
}