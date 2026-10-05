using System.Collections.Generic;
using UsbDiskDoctor.Core.Models;

namespace UsbDiskDoctor.Knowledge.Services
{
    /// <summary>
    /// Provides access to knowledge base articles for diagnostic problem resolution.
    /// Articles are loaded from an embedded JSON resource.
    /// </summary>
    public interface IKnowledgeService
    {
        /// <summary>
        /// Loads all knowledge articles from the embedded resource.
        /// Safe to call multiple times (idempotent). Never throws on 
        /// malformed data — logs and loads what it can.
        /// </summary>
        /// <returns>Number of articles successfully loaded.</returns>
        int Load();

        /// <summary>
        /// Gets the article that matches the given problem code.
        /// Returns null if no article exists for that code.
        /// </summary>
        /// <param name="code">Problem code, e.g., "SMART_PREDICT_FAILURE".</param>
        /// <returns>Article or null.</returns>
        KnowledgeArticle? GetArticle(string code);

        /// <summary>
        /// Gets all loaded articles.
        /// </summary>
        /// <returns>Read-only list of articles.</returns>
        IReadOnlyList<KnowledgeArticle> GetAllArticles();

        /// <summary>
        /// True when Load() has been called successfully at least once.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Number of articles currently loaded.
        /// </summary>
        int ArticleCount { get; }
    }
}