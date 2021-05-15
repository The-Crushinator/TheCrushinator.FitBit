using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TheCrushinator.FitBit.Web.Models;
using TheCrushinator.FitBit.Web.Models.Comparers;
using TheCrushinator.FitBit.Web.Services.Interfaces;

namespace TheCrushinator.FitBit.Web.Services
{
    /// <summary>
    /// Service to read/import data from a Beurer JSON file
    /// </summary>
    public class BeurerService : IBeurerService
    {
        private readonly ILogger<BeurerService> _logger;
        private readonly IMapper _mapper;
        private readonly FitbitContext _context;
        private const string DefaultFileNamePattern = "Export*.json";

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="context"></param>
        public BeurerService(ILogger<BeurerService> logger, IMapper mapper, FitbitContext context)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
        }

        /// <summary>
        /// Read a directory for scale data and save it to the database.
        /// </summary>
        /// <returns>Newly added entries.</returns>
        public async Task<IEnumerable<ScaleEntry>> ReadScaleDataInToDatabase()
        {
            // Get file names
            var fileNames = FindMatchingFiles();

            // Pull data from files
            var entries = await GetScaleEntriesWeightFromJsonFiles(fileNames);

            // Add _new_ entries to database
            var newEntries = await AddScaleEntriesToDatabase(entries);

            return newEntries;
        }

        /// <summary>
        /// Adds missing entries to the database
        /// </summary>
        /// <param name="entries">Entries to (attempt to) add</param>
        /// <returns></returns>
        private async Task<IEnumerable<ScaleEntry>> AddScaleEntriesToDatabase(IEnumerable<ScaleEntry> entries)
        {
            var existingEntries = _context.BeurerWeightEntries.Select(x => x.EntryId);
            var newEntries = entries.Where(x => !existingEntries.Contains(x.EntryId)).ToList();

            await _context.BeurerWeightEntries.AddRangeAsync(newEntries);
            await _context.SaveChangesAsync();

            return newEntries;
        }

        /// <summary>
        /// Get the data from 0 or more JSON file and return nicer scale entry objects
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        private async Task<IEnumerable<ScaleEntry>> GetScaleEntriesWeightFromJsonFiles(IEnumerable<string> fileNames)
        {
            // Create list of unique items
            var items = new HashSet<ScaleEntry>(new ScaleEntryComparer());
            foreach (var fileName in fileNames)
            {
                var entries = await ReadJsonFileEntries(fileName);
                foreach (var entry in entries)
                {
                    if (!items.Add(entry))
                    {
                        _logger.LogWarning($"Skipped duplicate: {entry.EntryId}");
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// Find files that match a pattern
        /// </summary>
        /// <param name="pattern">Pattern to match</param>
        /// <param name="path">Directory to look in</param>
        /// <returns></returns>
        private IEnumerable<string> FindMatchingFiles(string pattern = null, string path = null)
        {
            // Ensure there is a pattern to search
            if (string.IsNullOrWhiteSpace(pattern))
            {
                pattern = DefaultFileNamePattern;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                path = ".";
            }

            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException();
            }

            return Directory.GetFiles(path, pattern);
        }

        /// <summary>
        /// Read <see cref="ScaleEntry"/> collection from a specific JSON file
        /// </summary>
        /// <param name="fileName">File name (including path)</param>
        /// <returns></returns>
        private async Task<IEnumerable<ScaleEntry>> ReadJsonFileEntries(string fileName)
        {
            await using var openStream = File.OpenRead($"{fileName}");
            var scaleResult = await JsonSerializer.DeserializeAsync<BeurerScaleResult>(openStream);

            var entries = _mapper.Map<IEnumerable<ScaleEntry>>(scaleResult.Scale);

            return entries;
        }
    }
}
