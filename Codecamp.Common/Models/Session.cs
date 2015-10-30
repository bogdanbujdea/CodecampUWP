using System;
using System.Collections.Generic;

namespace Codecamp.Common.Models
{
    public class Session
    {
        public List<Speaker> Speakers { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Location Location { get; set; }

        public List<string> Tags { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }
    }
}