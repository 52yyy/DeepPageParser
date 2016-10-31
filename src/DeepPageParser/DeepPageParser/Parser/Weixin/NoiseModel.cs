using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DeepPageParser
{
	/// <summary>
	///		微信号广告句模型
	/// </summary>
	[Serializable]
	public class NoiseModel : Dictionary<string, NoiseContext>
	{
		public NoiseModel()
		{
		}

		public NoiseModel(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="filePath"></param>
		public void SaveToDisk(string filePath)
		{
			Stream fStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
			BinaryFormatter binFormat = new BinaryFormatter(); //创建二进制序列化器
			binFormat.Serialize(fStream, this);
			fStream.Close();
		}


		/// <summary>
		///		反序列化方法，如果反序列化失败，返回空的NoiseModel，记日志，但是不报警
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static NoiseModel LoadNoiseModel(string filePath)
		{
			NoiseModel dictionary = new NoiseModel();

			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("未找到微信页面解析模型文件，请检查模型文件路径" + filePath);
			}
			byte[] fileData = File.ReadAllBytes(filePath);
			//Stream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			MemoryStream fStream = new MemoryStream(fileData);
			try
			{
				if (fStream.Length != 0)
				{
					BinaryFormatter binFormat = new BinaryFormatter(); //创建二进制序列化器
					var tmp = binFormat.Deserialize(fStream);
					dictionary = (NoiseModel)tmp;
				}
			}
			catch (Exception exception)
			{
				throw new FileLoadException("微信页面解析模型加载失败，模型文件路径" + filePath, exception);
			}
			finally
			{
				fStream.Close();
				fStream.Dispose();
			}

			return dictionary;
		}
	}
}