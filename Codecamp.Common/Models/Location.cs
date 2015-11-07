using System;

namespace Codecamp.Common.Models
{
    public class Location
    {
        public string Room { get; set; }

        public int Floor { get; set; }

        public int Seats { get; set; }

        public override string ToString()
        {
            return Room + ", floor " + Floor + ", " + Seats + "+ seats";
        }
    }
}