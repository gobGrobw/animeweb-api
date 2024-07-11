using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class AnimeWatchlist
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string WatchStatus { get; set; } = string.Empty;
    }
}
