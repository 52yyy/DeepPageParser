
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using DeepPageParser.Dom;


namespace DeepPageParser
{
    public class VisitInfoHelper
    {
        private const int MaxLine = 100;
        private static VisitInfoHelper Instance = null;
        private const string NumberStr = "0123456789.w";
        private static readonly char[] NumberArray = NumberStr.ToCharArray();
        private static readonly NewsParserConfig conf = new NewsParserConfig();


        public static VisitInfoHelper GetInstance()
        {
            return Instance ?? (Instance = new VisitInfoHelper());
        }


        public void GenerateVisitFeature(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data)
        {
            if (data.OriginalNodeList != null && data.OriginalNodeList.Count > 0)
            {
                GetVisitInfo(data);
            }
        }

        private void GetVisitInfo(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data)
        {
  
            var contentNodesCount = data.ContentNodes.Count;
            if (contentNodesCount == 0)
            {
                return;
            }
            var titleOriginalIdx = data.Page.Title.OriginalIndex + data.Page.Title.OriginalChildren.Count;
            var contentHeadOriginalIdx = data.ContentNodes[0].Parent.OriginalIndex - 1;
            var contentTailOriginalIdx = data.ContentNodes[contentNodesCount - 1].OriginalIndex
                + data.ContentNodes[contentNodesCount - 1].OriginalChildren.Count;

            var candidateListHead = GetCandidateListHead(data, titleOriginalIdx, contentHeadOriginalIdx);
            UpdatePosition(ref candidateListHead, true);
            var candidateListTail = GetCandidateListTail(data, contentTailOriginalIdx);
            UpdatePosition(ref candidateListTail, false);

            CombineCandidateList(data,candidateListHead, candidateListTail); ;
        }

       
        private void CombineCandidateList(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data,List<VisitInfo> candidateListHead, List<VisitInfo> candidateListTail)
        {
           var candidateList = new List<VisitInfo>();
            candidateList.AddRange(candidateListHead);
            candidateList.AddRange(candidateListTail);
            candidateList = candidateList.Where(x => x.Value > .0).OrderByDescending(x => x.Value).ToList();
          
            var cmntCandidateList = candidateList.Where(x => x.IsComment).OrderByDescending(x => x.Value).ToList();
            var visitCandidateList = candidateList.Where(x => x.IsComment == false).OrderByDescending(x => x.Value).ToList();
            SetValue(data,cmntCandidateList, visitCandidateList);
        }


        private void SetValue(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data,List<VisitInfo> cmntCandidateList, List<VisitInfo> visitCandidateList)
        {
            UpdateVisitCandidateValue(ref cmntCandidateList, true);
            UpdateVisitCandidateValue(ref visitCandidateList, false);

            if (cmntCandidateList.Count > 0)
            {
                data.Page.CommentNumber.Value = cmntCandidateList[0].CommentNum;
            }
               
            if (visitCandidateList.Count > 0)
            {
                data.Page.VisitNumber.Value = visitCandidateList[0].VisitNum;
            }
               
        }

        private void UpdateVisitCandidateValue(ref List<VisitInfo> CandidateList, bool isComment)
        {
            if (isComment)
            {
                for (int i = 0; i < CandidateList.Count; i++)
                {
                    CandidateList[i].CommentNum = int.Parse(CandidateList[i].Text);
                }
            }
            else
            {
                for (int i = 0; i < CandidateList.Count; i++)
                {
                    CandidateList[i].VisitNum = int.Parse(CandidateList[i].Text);
                }
            }
            
        }

        private List<VisitInfo> GetCandidateListHead(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data, int titleOriginalIdx,
            int contentHeadOriginalIdx)
        {
            var headCondidateNodes = GetHeadCandidateNodes(data, titleOriginalIdx, contentHeadOriginalIdx);
            var candidateListHead = VisitInfoCandidateValuation(data,headCondidateNodes);
            ValueCounter(ref candidateListHead);
            return candidateListHead;
        }

        private List<VisitInfo> GetCandidateListTail(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data, int contentTailOriginalIdx)
        {
            var endCondidateNodes = GetTailCandidateNodes(data, contentTailOriginalIdx);
            var candidateListTail = VisitInfoCandidateValuation(data, endCondidateNodes);
            ValueCounter(ref candidateListTail);
            return candidateListTail;
        }

        private List<WrappedNode> GetTailCandidateNodes(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data, int contentTailOriginalIdx
           )
        {
            var endCondidateNodes = new List<WrappedNode>();
            var counter = contentTailOriginalIdx + 1;
            var set = new HashSet<string>();
            while (endCondidateNodes.Count <= MaxLine && counter < data.OriginalNodeList.Count)
            {
                var nodeText = data.OriginalNodeList[counter].Text;
                var len = nodeText.Trim().Length;
                if (0 < len && len < 10)
                {
                    if (set.Contains(nodeText) && !IsNumStr(nodeText))
                    {
                        counter++;
                        continue;
                    }
                    set.Add(data.OriginalNodeList[counter].Text);

                    endCondidateNodes.Add(data.OriginalNodeList[counter]);
                    if (!IsNumStr(data.OriginalNodeList[counter].Text))
                    {
                        continue;
                    }
                }
                counter++;
            }
            return endCondidateNodes;
        }

        private List<WrappedNode> GetHeadCandidateNodes(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data, int titleOriginalIdx,
            int contentHeadOriginalIdx)
        {
            var headCondidateNodes = new List<WrappedNode>();
            for (var i = titleOriginalIdx + 1; i < contentHeadOriginalIdx; i++)
            {
                string text = data.OriginalNodeList[i].Text.Trim();
                var len = text.Length;

                if (len > 0)
                {
                    headCondidateNodes.Add(data.OriginalNodeList[i]);
                }
            }
            return headCondidateNodes;
        }


        /**
         * 抽取所有评论数、阅读数候选节点
         */
        private List<VisitInfo> VisitInfoCandidateValuation(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data,List<WrappedNode> nodes)
        {
            var cmntIdxPair = GetCmntCludWordIdxPair(nodes);
            var visitIdxPair = GetVisitCludWordIdxPair(nodes);
            GetIdxPairRecord(data, cmntIdxPair, visitIdxPair);
            var list = GetCandidateVisitInfoList(nodes, cmntIdxPair, true);
            list.AddRange(GetCandidateVisitInfoList(nodes,visitIdxPair, false));
            return list;
        }

        private void GetIdxPairRecord(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data,List<ClueWordIndex> cmntIdxPair, List<ClueWordIndex> visitIdxPair)
        {
            if (data.Page.CommentNumber == null)
            {
                data.Page.CommentNumber= new CommentNumber();
            }
            if (data.Page.VisitNumber == null)
            {
                data.Page.VisitNumber= new VisitNumber();
            }

            data.Page.CommentNumber.Value = (cmntIdxPair.Count > 0) ? -2 : -1;
            data.Page.VisitNumber.Value = (visitIdxPair.Count > 0) ? -2 : -1;
        }


        private void ValueCounter(ref List<VisitInfo> candidateList)
        {
            const double numerator = 1.0;
            for (var i = 0; i < candidateList.Count; i++)
            {
                if (candidateList[i].HeadDistance > 0 && candidateList[i].TailDistance > 0)
                {
                    candidateList[i].Value = numerator / candidateList[i].TailDistance +
                                             numerator / candidateList[i].HeadDistance;
                }
                else if (candidateList[i].HeadDistance < 0 && candidateList[i].TailDistance > 0)
                {
                    candidateList[i].Value = numerator / candidateList[i].TailDistance;
                }
                else if (candidateList[i].HeadIndex >= 0 && candidateList[i].TailDistance < 0)
                {
                    candidateList[i].Value = numerator / candidateList[i].HeadDistance;
                }
                else
                {
                    candidateList[i].Value = .0;
                }
            }
        }


        private List<VisitInfo> GetCandidateVisitInfoList(List<WrappedNode> candiNodes, List<ClueWordIndex> idxPairs, bool isCommnet)
        {
            var list = new List<VisitInfo>();
            foreach (ClueWordIndex idx in idxPairs)
            {
                int headIdx = idx.HeadClueWordIdx;
                int tailIdx = idx.TailClueWordIdx;
                
                var i = headIdx > -1 ? headIdx + 1 : 0;
                var len = tailIdx > -1 ? tailIdx : candiNodes.Count;

                for (; i < len; i++)
                {
                    if (IsNumStr(candiNodes[i].Text))
                    {
                        var visitInfo = new VisitInfo();
                        visitInfo.Index = candiNodes[i].OriginalIndex;
                        visitInfo.Text = candiNodes[i].Text;
                        visitInfo.HeadDistance = headIdx > -1 ? Math.Abs(headIdx - i) : -1;
                        visitInfo.TailDistance = tailIdx > 0 ? tailIdx - i : -1;
                        visitInfo.HeadText = headIdx > -1 ? candiNodes[headIdx].Text : string.Empty;
                        visitInfo.TailText = tailIdx > 0 ? candiNodes[tailIdx].Text : string.Empty;
                        visitInfo.IsComment = isCommnet;
                        visitInfo.HeadPosition = false;
                        visitInfo.TailPosition = false;
                        list.Add(visitInfo);
                    }
                }
            }
            return list;
        }
        

        /**
         * 获取首/尾提示词索引
         */
        private List<int> GetClueWordIdxList(List<WrappedNode> candiNodes, string[] clueWords, int idx)
        {
            var clueWordIdxList = new List<int>();
            var i = 0;
            if (idx > 0)
            {
                i = idx + 1;
            }
            for (; i < candiNodes.Count; i++)
            {
                for (var j = 0; j < clueWords.Length; j++)
                {
                    if (candiNodes[i].Text.Equals(clueWords[j]))
                    {
                        clueWordIdxList.Add(i);
                    }
                }
            }
            return clueWordIdxList;
        }

        private List<int> GetHeadClueWordIdxList(List<WrappedNode> nodes, string[] headClueWords)
        {
            return GetClueWordIdxList(nodes, headClueWords, 0);
        }

        private List<int> GetTailClueWordIdxList(List<WrappedNode> nodes, string[] tailClueWords, List<int> headClueWordIdxList)
        {
            if (headClueWordIdxList.Count > 0)
            {
                return GetClueWordIdxList(nodes, tailClueWords, headClueWordIdxList[0]);
            }
            return GetClueWordIdxList(nodes, tailClueWords, 0);
        }

        private List<ClueWordIndex> GetCmntCludWordIdxPair(List<WrappedNode> nodes)
        {
            var cmntHeadClueWord = conf.CommentHeadWords;
            var cmntTailClueWord = conf.CommentTailWords;

            return GetNodeClueWordIdx(nodes, cmntHeadClueWord, cmntTailClueWord);
        }

        private List<ClueWordIndex> GetVisitCludWordIdxPair(List<WrappedNode> nodes)
        {
            var visitHeadClueWord = conf.VisitHeadWords;
            var visitTailClueWord = conf.VisitTailWords;

            return GetNodeClueWordIdx(nodes, visitHeadClueWord, visitTailClueWord);
        }

        private List<ClueWordIndex> GetNodeClueWordIdx(List<WrappedNode> nodes, string[] headClueWord, string[] tailClueWord)
        {
            List<int> headClueIdxList = GetHeadClueWordIdxList(nodes, headClueWord);
            List<int> tailClueIdxList = GetTailClueWordIdxList(nodes, tailClueWord, headClueIdxList);
            return GetClueWordIdxPair(headClueIdxList, tailClueIdxList);
        }

        private List<ClueWordIndex> GetClueWordIdxPair(List<int> headClueIdxList, List<int> tailClueIdxList)
        {
            var list = new List<ClueWordIndex>();
            var hsize = headClueIdxList.Count;
            var tsize = tailClueIdxList.Count;
            if (hsize == 0 && tsize == 0)
            {
                return list;
            }
            if (hsize > 0 && tsize ==0)
            {
                foreach (var idx in headClueIdxList)
                {
                    list.Add(new ClueWordIndex(idx,-1));
                }
            }
            else if (hsize == 0 && tsize > 0)
            {
                foreach (var idx in tailClueIdxList)
                {
                    list.Add(new ClueWordIndex(-1, idx));
                }
            }
            else
            {
                foreach (var i in headClueIdxList)
                {
                    foreach (var j in tailClueIdxList)
                    {
                        list.Add(new ClueWordIndex(i, j));
                    }
                }
            }
            return list;
        }


        private void UpdatePosition(ref List<VisitInfo> list, bool flag)
        {
            if (flag)
            {
                UpdateHeadPosition(ref list);
            }
            else
            {
                UpdateTailPosition(ref list);
            }
        }

        private void UpdateHeadPosition(ref List<VisitInfo> list)
        {
            foreach (var visitInfo in list)
            {
                visitInfo.HeadPosition = true;
            }
        }

        private void UpdateTailPosition(ref List<VisitInfo> list)
        {
            foreach (var visitInfo in list)
            {
                visitInfo.TailPosition = true;
            }
        }

        public bool IsNumStr(string scr)
        {
            var ch = scr.ToLower().ToCharArray();
            return ch.All(c => NumberArray.Contains(c));
        }
    }

    class ClueWordIndex
    {
        public int HeadClueWordIdx { get; set; }
        public int TailClueWordIdx { get; set; }

        public ClueWordIndex(int headClueWordIdx, int tailClueWordIdx)
        {
            this.HeadClueWordIdx = headClueWordIdx;
            this.TailClueWordIdx = tailClueWordIdx;
        }

    }
}