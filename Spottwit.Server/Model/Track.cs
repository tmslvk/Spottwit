using Spottwit.Server.Model;
using Spottwit.Server.Models.Entities;

namespace Spottwit.Server.Models.Entities
{
    public class Track : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
    }
}