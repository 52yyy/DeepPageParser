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

using System;

namespace DeepPageParser
{
    internal static class ExceptionFactory
    {
        public static ArgumentException LowConfidenceAboutPrecision
        {
            get
            {
                return new ArgumentException("Low confidence about precision.");
            }
        }

        public static ArgumentException LowConfidenceAboutPrecisionBigGapBetweenNeighbourNodes
        {
            get
            {
                return new ArgumentException("Low confidence about precision: big gap between two neighbour nodes in biggest cluster.");
            }
        }

        public static ArgumentException LowConfidenceAboutPrecisionFinalLengthTooShort
        {
            get
            {
                return new ArgumentException("Low confidence about precision: final text length is too short.");
            }
        }

        public static ArgumentException LowConfidenceAboutPrecisionFirstNodeFarFromTitle
        {
            get
            {
                return new ArgumentException("Low confidence about precision: first node in biggest cluster far from title.");
            }
        }

        public static ArgumentException LowConfidenceAboutPrecisionOnClusterNodeNumber
        {
            get
            {
                return new ArgumentException("Low confidence about precision: node number in biggest cluster less than 2.");
            }
        }

        public static ArgumentException LowConfidenceAboutPrecisionTooMuchPlainText
        {
            get
            {
                return new ArgumentException("Low confidence about precision: too much plain text before first node in biggest cluster.");
            }
        }

        public static ArgumentException NotAnArticle
        {
            get
            {
                return new ArgumentException("This page is likely not an article.");
            }
        }
    }
}

