using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepPageParser
{
    public class VisitInfo
    {
        public int Index { get; set; }
        public int VisitNum { get; set; }
        public string VisitText { get; set; }
        public string Text { get; set; }
        public int CommentNum { get; set; }
        public string CommentText { get; set; }
        public string CommentLink { get; set; }
        public bool Contains { get; set; }
        public string CondidateStr { get; set; }
        public double Value { get; set; }
        public int HeadDistance { get; set; }
        public int TailDistance { get; set; }
        public int HeadIndex { get; set; }
        public int TailIndex { get; set; }
        public bool HeadPosition { get; set; }
        public bool TailPosition { get; set; }
        public string HeadText { get; set; }
        public string TailText { get; set; }
        public bool IsComment { get; set; }

        public VisitInfo(int headIdx, int tailIdx)
        {
            this.HeadIndex = headIdx;
            this.TailIndex = tailIdx;
        }

        public VisitInfo()
        {
            
        }


    }

    public class VisitNumber
    {
        public int Value { get; set; }
    }

    public class CommentNumber
    {
        public int Value { get; set; }
    }

}
