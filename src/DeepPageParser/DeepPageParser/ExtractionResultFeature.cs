
using System.Collections.Generic;
using System.Reflection;

namespace DeepPageParser
{
    public class ExtractionResultFeature
    {
        public int AvgNodesBlockIndexDistance { get; set; }

        public int AvgNodesPlainTextIndexDistance { get; set; }

        public int AvgNodesTextIndexDistance { get; set; }

        public int ClusterNodeNumber { get; set; }

        public int ExtractedTextLength { get; set; }

        public string FileName { get; set; }

        public double FirstClusterNodePlainTextPosition { get; set; }

        public int HasContentParent { get; set; }

        public double MainBlockRatio { get; set; }

        public int MaxNodesBlockIndexDistance { get; set; }

        public int MaxNodesPlainTextIndexDistance { get; set; }

        public int MaxNodesTextIndexDistance { get; set; }

        public int PlainTextLength { get; set; }

        public double TitlePlainTextIndexPosition { get; set; }

        public double GetScore()
        {
            return 0.0;
        }

        internal static string PropertyKeys(ExtractionResultFeature obj)
        {
            List<string> values = new List<string>();
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo info in properties)
            {
                values.Add(info.Name);
            }

            return string.Join(",", values);
        }

        internal static string PropertyValues(ExtractionResultFeature obj)
        {
            List<string> values = new List<string>();
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo info in properties)
            {
                values.Add(info.GetValue(obj, null).ToString());
            }

            return string.Join(",", values);
        }
    }
}

