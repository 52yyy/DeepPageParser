// ======================================================================
// 
//      Copyright (C) 北京国双科技有限公司        
//                    http://www.gridsum.com
// 
//      保密性声明：此文件属北京国双科技有限公司所有，仅限拥有由国双科技
//      授予了相应权限的人所查看和所修改。如果你没有被国双科技授予相应的
//      权限而得到此文件，请删除此文件。未得国双科技同意，不得查看、修改、
//      散播此文件。
// 
// 
// ======================================================================

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

