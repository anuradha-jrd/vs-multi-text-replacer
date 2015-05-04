using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiTextReplacement
{
    public class ReplacerEntry
    {
        public string Find { get; set; }

        public string Replace { get; set; }

        public bool FullMatch { get; set; }
    }
}
