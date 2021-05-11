using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TheCrushinator.FitBit.Web.Models;
using TheCrushinator.FitBit.Web.Services.Interfaces;

namespace TheCrushinator.FitBit.Web.Services
{
    public class BeurerService : IBeurerService
    {
        private readonly ILogger<BeurerService> _logger;
        private readonly IMapper _mapper;
        private const string DefaultFileNamePattern = "Export*.json";

        public BeurerService(ILogger<BeurerService> logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        public async Task ImportWeightFromJson()
        {
            // Get filenames
            var fileNames = FindMatchingFiles();

            // Create list of unique items
            var items = new HashSet<ScaleEntry>();
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
        }

        public ICollection<string> FindMatchingFiles(string pattern = null, string path = null)
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

        public async Task<ICollection<ScaleEntry>> ReadJsonFileEntries(string fileName)
        {
            await using var openStream = File.OpenRead(fileName);
            var scaleResult = await JsonSerializer.DeserializeAsync<BeurerScaleResult>(openStream);

            var entries = _mapper.Map<ICollection<ScaleEntry>>(scaleResult.Scale);

            return entries;
        }
    }
}
