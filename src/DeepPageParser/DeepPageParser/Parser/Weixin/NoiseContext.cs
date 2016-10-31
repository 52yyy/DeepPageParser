using System;

using DeepPageParser.Dom;

namespace DeepPageParser
{
	/// <summary>
	///		噪音类，每一个公众号都有一个噪音实例
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
		///		噪音句hashcode
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