
using System;

namespace DeepPageParser
{
	public class PublishTime
    {
        public double Confidence { get; set; }

        public string TimeString { get; set; }

        public DateTime TimeValue { get; set; }

        public bool ValidTime { get; set; }
    }
}

