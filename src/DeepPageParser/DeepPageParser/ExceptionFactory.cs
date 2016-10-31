
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

