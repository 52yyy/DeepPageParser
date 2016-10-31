using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DeepPageParser
{
	/// <summary>
	///		΢�źŹ���ģ��
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
			BinaryFormatter binFormat = new BinaryFormatter(); //�������������л���
			binFormat.Serialize(fStream, this);
			fStream.Close();
		}


		/// <summary>
		///		�����л���������������л�ʧ�ܣ����ؿյ�NoiseModel������־�����ǲ�����
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static NoiseModel LoadNoiseModel(string filePath)
		{
			NoiseModel dictionary = new NoiseModel();

			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("δ�ҵ�΢��ҳ�����ģ���ļ�������ģ���ļ�·��" + filePath);
			}
			byte[] fileData = File.ReadAllBytes(filePath);
			//Stream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			MemoryStream fStream = new MemoryStream(fileData);
			try
			{
				if (fStream.Length != 0)
				{
					BinaryFormatter binFormat = new BinaryFormatter(); //�������������л���
					var tmp = binFormat.Deserialize(fStream);
					dictionary = (NoiseModel)tmp;
				}
			}
			catch (Exception exception)
			{
				throw new FileLoadException("΢��ҳ�����ģ�ͼ���ʧ�ܣ�ģ���ļ�·��" + filePath, exception);
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