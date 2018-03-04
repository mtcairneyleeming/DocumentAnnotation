using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Extensions.Options;
using server.TextLoader.Models;
using Newtonsoft.Json;

namespace server.TextLoader
{
    /// <summary>
    /// Load texts, e.g. the Pro Milone from local xml files, as stored by Perseus. This keeps a cache of files, and is used as a singleton.
    /// </summary>
    public class TextLoader
    {
        private List<Text> _texts = new List<Text>();
        private readonly IOptions<Config> _config;

        public TextLoader(IOptions<Config> config)
        {
            this._config = config;
        }
        
        
        public Text LoadText(string textIdentifier)
        {
            Debug.WriteLine($"Beginning to load text {textIdentifier}");
            using (var originalFile = File.OpenRead($"{_config.Value.ProcessedTexts}/{textIdentifier}.json.gz"))
            {
                Debug.WriteLine($"Opened compressed file for text {textIdentifier}");
                using (GZipStream decompressionStream = new GZipStream(originalFile, CompressionMode.Decompress))
                {
                    var text = DeserializeFromStream(decompressionStream);
                    if (_texts.All(t => t.DBId != text.DBId))
                    {
                        _texts.Add(text);
                    }
                    Debug.WriteLine($"Loaded text {textIdentifier}");
                    return text;

                }
            }
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