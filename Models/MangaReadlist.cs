using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class MangaReadlist
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ReadStatus { get; set; } = string.Empty;
        public string ImgUrl { get; set; } = string.Empty;

    }
}
