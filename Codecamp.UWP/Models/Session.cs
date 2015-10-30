namespace Codecamp.UWP.Models
{
    using System;
    using System.Collections.Generic;

    public class Session
    {
        public List<Speaker> Speakers { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Location Location { get; set; }

        public List<string> Tags { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}