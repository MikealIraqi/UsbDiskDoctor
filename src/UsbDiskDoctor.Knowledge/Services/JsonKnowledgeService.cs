using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Serilog;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.Knowledge.Services
{
    /// <summary>
    /// Implements knowledge service by loading articles from an embedded JSON resource.
    /// </summary>
    public sealed class JsonKnowledgeService : IKnowledgeService
    {
        private const string ResourceName = "UsbDiskDoctor.Knowledge.Data.knowledge.json";

        private static readonly ILogger _log = Log.ForContext<JsonKnowledgeService>();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        private readonly Dictionary<string, KnowledgeArticle> _articlesByCode
            = new(StringComparer.OrdinalIgnoreCase);

        private bool _isLoaded;

        public JsonKnowledgeService()
        {
        }

        public bool IsLoaded => _isLoaded;

        public int ArticleCount => _articlesByCode.Count;

        public int Load()
        {
            if (_isLoaded)
            {
                return _articlesByCode.Count;
            }

            try
            {
                var currentAssembly = typeof(JsonKnowledgeService).Assembly;
                var resourceStream = currentAssembly.GetManifestResourceStream(ResourceName);

                if (resourceStream == null)
                {
                    _log.Warning("Embedded resource '{ResourceName}' not found.", ResourceName);
                    _isLoaded = true;
                    return 0;
                }

                using (resourceStream)
                {
                    using var streamReader = new StreamReader(resourceStream);
                    var jsonContent = streamReader.ReadToEnd();

                    var deserializedArticles = JsonSerializer.Deserialize<List<KnowledgeArticle>>(jsonContent, JsonOptions);

                    if (deserializedArticles == null)
                    {
                        _log.Warning("Deserialized articles list is null from resource '{ResourceName}'.", ResourceName);
                        _isLoaded = true;
                        return 0;
                    }

                    foreach (var articleItem in deserializedArticles)
                    {
                        if (articleItem != null && !string.IsNullOrWhiteSpace(articleItem.Code))
                        {
                            _articlesByCode[articleItem.Code] = articleItem;
                        }
                    }

                    _log.Information("Loaded {Count} knowledge articles.", _articlesByCode.Count);
                    _isLoaded = true;
                    return _articlesByCode.Count;
                }
            }
            catch (JsonException jsonException)
            {
                _log.Error(jsonException, "JSON parsing error while loading knowledge base from '{ResourceName}'.", ResourceName);
                _isLoaded = true;
                return 0;
            }
            catch (Exception genericException)
            {
                _log.Error(genericException, "Unexpected error while loading knowledge base from '{ResourceName}'.", ResourceName);
                _isLoaded = true;
                return 0;
            }
        }

        public KnowledgeArticle? GetArticle(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            EnsureLoaded();

            _articlesByCode.TryGetValue(code, out var foundArticle);
            return foundArticle;
        }

        public IReadOnlyList<KnowledgeArticle> GetAllArticles()
        {
            EnsureLoaded();
            return _articlesByCode.Values.ToList();
        }

        private void EnsureLoaded()
        {
            if (!_isLoaded)
            {
                Load();
            }
        }
    }
}