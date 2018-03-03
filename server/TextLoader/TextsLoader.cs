using System;
using System.Collections.Generic;
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
    public class TextsLoader
    {
        private List<Text> _texts = new List<Text>();
        private readonly IOptions<Config> config;

        public TextsLoader(IOptions<Config> config)
        {
            this.config = config;
        }
        
        
        Text LoadText(string textIdentifier)
        {
            using (var originalFile = File.OpenRead($"{config.Value.ProcessedTexts}/{textIdentifier}/.json.gz"))
            {
                using (GZipStream decompressionStream = new GZipStream(originalFile, CompressionMode.Decompress))
                {
                    var text = DeserializeFromStream(decompressionStream);
                    if (_texts.All(t => t.DBId != text.DBId))
                    {
                        _texts.Add(text);
                    }
                    return text;

                }
            }
        }
        public static Text DeserializeFromStream(Stream stream)
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