using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepPageParser.NewVersion.Parser.News
{
    public delegate void FieldExtractor(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data);

    public abstract class NewsFields
    {


        protected void GetFieldValue(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo data,
          FieldExtractor makeExtracting)
        {
            makeExtracting(data);
        }
    }
}
