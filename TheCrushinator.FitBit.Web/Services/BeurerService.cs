using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheCrushinator.Beurer.Services.Interfaces;
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
        private readonly IBeurerAuthService _beurerAuthService;
        private readonly IBeurerWeightService _beurerWeightService;
        private const string DefaultFileNamePattern = "Export*.json";

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="context"></param>
        public BeurerService(ILogger<BeurerService> logger, IMapper mapper, FitbitContext context, IBeurerAuthService beurerAuthService, IBeurerWeightService beurerWeightService)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
            _beurerAuthService = beurerAuthService;
            _beurerWeightService = beurerWeightService;
        }

        public async Task<ScaleEntry> GetNextScaleEntry(bool isUnsynchronised = true)
        {
            var query = _context.BeurerWeightEntries.AsQueryable();

            if (isUnsynchronised)
            {
                query = query.Where(x => x.FitBitWeightUploadDateTimeUtc == null
                                         || x.FitbitWeightLogId == null
                                         || x.FitBitFatUploadDateTimeUtc == null);
            }

            var result = await query.OrderBy(x => x.RecordDateTimeUtc).FirstOrDefaultAsync();

            return result;
        }

        /// <summary>
        /// Read a directory for scale data and save it to the database.
        /// </summary>
        /// <returns>Newly added entries.</returns>
        public async Task<IEnumerable<ScaleEntry>> ReadScaleDataFromFileInToDatabase()
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
        /// Fetch any updates to scale data from beurer
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ScaleEntry>> FetchScaleDataFromBeurerInToDatabase(CancellationToken cancellationToken)
        {
            // Get date to search from
            var mostRecentDate = await _context.BeurerWeightEntries.MaxAsync(x => x.RecordDateTimeUtc, cancellationToken: cancellationToken);

            if (mostRecentDate > DateTime.UtcNow.AddMinutes(-1))
            {
                return default;
            }

            // Get entries
            var sessionInfo = await _beurerAuthService.LoginAsync("xxx", "xxx", cancellationToken);
            var measurements = await _beurerWeightService.FetchScaleMeasurementResponsesForDateRange(
                                   mostRecentDate.ToLocalTime(),
                                   DateTime.Now,
                                   sessionInfo,
                                   cancellationToken
                                   );

            // Convert to scale entries
            var entries = _mapper.Map<IEnumerable<ScaleEntry>>(measurements);

            // Add new entries to database
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
