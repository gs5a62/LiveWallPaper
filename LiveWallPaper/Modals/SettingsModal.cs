using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveWallPaper.Modals
{
    class SettingsModal
    {
        public DateTime? LastAPICheck { get; set; }
        public TimeSpan? Dawn { get; set; }
        public TimeSpan? Sunrise { get; set; }
        public TimeSpan? Sunset { get; set; }
        public string LastAppliedImage { get; set; }
    }
}
