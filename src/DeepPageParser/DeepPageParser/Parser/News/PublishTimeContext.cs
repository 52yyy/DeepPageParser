namespace DeepPageParser
{
	public class PublishTimeContext
	{
		public PublishTimeContext()
		{
			
		}

		public PublishTimeContext(UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo pendingPage, PublishTimeContextState state)
		{
			this.PendingPage = pendingPage;
			this.State = state;
		}

		public UnstructuredContentPageParser.PendingAnalysisPageExtractedInfo PendingPage { get; set; }

		public PublishTimeContextState State { get; set; }

		public void Execute()
		{
			this.State.Execute(this);
		}
	}

	public abstract class PublishTimeContextState
	{
		public abstract void Execute(PublishTimeContext context);
	}
}