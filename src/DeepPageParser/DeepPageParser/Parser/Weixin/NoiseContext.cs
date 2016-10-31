using System;

using DeepPageParser.Dom;

namespace DeepPageParser
{
	/// <summary>
	///		�����࣬ÿһ�����ںŶ���һ������ʵ��
	/// </summary>
	[Serializable]
	public class NoiseContext
	{
		/// <summary>
		///		
		/// </summary>
		public NoiseContext()
		{
			this.NoiseHashSentences = new Noise();
			_buffer = new NoiseStatisticalBuffer();
		}

		/// <summary>
		///		������hashcode
		/// </summary>
		public Noise NoiseHashSentences { get; set; }

		[NonSerialized]
		private NoiseStatisticalBuffer _buffer;

		public NoiseStatisticalBuffer Buffer
		{
			get
			{
				return this._buffer;
			}
			set
			{
				this._buffer = value;
			}
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="node"></param>
		public void UpdateNoise(WrappedNode node)
		{
			if (this._buffer == null)
			{
				this._buffer = new NoiseStatisticalBuffer();
			}
			this.Buffer.AddNode(node);
			if (this.Buffer.NodeCount >= 5)
			{
				Noise tmpNoise = this.Buffer.UpdateNoise();
				//this.NoiseHashSentences = this.Buffer.UpdateNoise(this.NoiseHashSentences);
				this.NoiseHashSentences.UnionWith(tmpNoise);
				this.Buffer.Clear();
			}
		}
	}
}