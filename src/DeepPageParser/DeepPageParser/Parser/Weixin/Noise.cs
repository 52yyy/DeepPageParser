using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DeepPageParser
{
	/// <summary>
	///		
	/// </summary>
	[Serializable]
	public class Noise : HashSet<int>
	{
		public Noise()
		{
		}

		public Noise(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}