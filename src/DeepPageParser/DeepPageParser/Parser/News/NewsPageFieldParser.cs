using System.Collections.Generic;
using DeepPageParser.NewVersion.Parser.News;

namespace DeepPageParser
{
    public class NewsPageFieldParser : NewsFields
	{
 
		/// <summary>
		///		data config
		/// </summary>
		protected NewsParserConfig ParserConfig;

		private NewsPageFieldCandidateGenerator _generator;

		private NewsPageFieldExtractor _extractor;


		public NewsPageFieldParser()
		{
			this._generator = new NewsPageFieldCandidateGenerator();
		}

		public NewsPageFieldParser(NewsParserConfig parserConfig)
		{
			this.ParserConfig = parserConfig;
			this._generator = new NewsPageFieldCandidateGenerator(parserConfig);
		}

		public void SetNewsPageFieldExtractor(NewsPageFieldExtractor extractor)
		{
			this._extractor = extractor;
		}

        //++++++++++++++++++++++++++++++++++++++++ 
        public void GetFieldValue2(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data)
        {
            GetFieldValue(data, VisitInfoHelper.GetInstance().GenerateVisitFeature);
        }
        //+++++++++++++++++++++++++++++++++++++++++

		public void GetFieldValue(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data)
		{
			if (this._extractor == null)
			{
				return;
			}
			List<int> candidateNodeIdx = this._generator.GetCandidateNodeIndex(data);
			this._extractor.GetFieldValue(data, candidateNodeIdx);
		}

        public void GetFieldTimeValue(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data)
        {
            if (this._extractor == null)
            {
                return;
            }
            List<int> candidateNodeIdx = this._generator.GetCandidateTimeNodeIndex(data);
            this._extractor.GetFieldValue(data, candidateNodeIdx);
        }
	}
}