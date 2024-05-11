using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace UroMeter.Web.Models;

public class Data
{
	public static List<Point> GetData(double[] times, double[] flowRates, double[] volumes, int dataSizes)
	{
		// Tạo data từ hàm này.
		var point = new List<Point>();
		double timeone = times[1];

		for (int j = 1; j < dataSizes; j++)
		{
			times[j] = times[j] - timeone;
			point.Add(new Point() { Index = j, Time = times[j], Value = flowRates[j], volume = volumes[j] });
		}

		return point;
	}

	//public static List<Point> GetUpdateData(DateTime dateTime)
	//{
	//    var point = new List<Point>();
	//    var r = new Random();

	//    for (int i = 0; i < 5; i++)
	//    {
	//        point.Add(new Point() { Time = 1, Value = r.Next(1, 30) });
	//        //Add point khi vẽ bằng serial qua cái update này.
	//    }
	//    return point;
	//    // Trả về 1 data ngẫu nhiên khi đuọc gọi

	//}
}

public class Point
{
	public int Index { get; set; }
	public double Value { get; set; }
	public double Time { get; set; }
	public double volume { get; set; }
}

public class RawData
{
	public string DataString { get; set; }
	public double Unixtime { get; set; }
	public int IdRecord { get; set; }

	public int DataSize { get; set; }
	public double[] Data { get; set; }
	public double[] Time { get; set; }


	public double[] FlowRate { get; set; }
	private double[] FlowRate1 { get; set; }
	private double[] FlowRate2 { get; set; }

	private double[] Data1;
	public double[] Data2;

	public bool IsData { get; set; }

	public string Thoigian { get; set; }

	public int StartPoint;
	public int marker;
	private int fromIndex;
	private int toIndex;

	public double voidvolume { get; set; }
	public double qmax { get; set; }
	public double qave { get; set; }


	public RawData(string dataString)
	{
		DataString = dataString;
		StringToData();
		Update();
		CalVolume();
	}

	public void CalVolume()
	{
		Data1 = new double[DataSize];
		Data2 = new double[DataSize];
		FlowRate = new double[DataSize];
		FlowRate1 = new double[DataSize];
		FlowRate2 = new double[DataSize];


		double[] RefFlowRate = new double[DataSize];
		//Data->Data1 (Loai bo x<0,x>1000,x>3 lan x+5)
		for (int k = 1; k < DataSize; k++)
		{
			if (Math.Abs(Data[k]) > 10000)
			{
				Data[k] = Data[k - 1];
				Data1[k] = Data[k] / 10;
			}
			else
			{
				Data1[k] = Data[k] / 10;
			}
			Time[k] = Time[k];
		}


		for (int k = 1; k < DataSize; k++)
		{
			try
			{
				Data2[k] = Data1[k + 1] * 0.2 + Data1[k - 1] * 0.2 +
						   Data1[k] * 0.6;
			}
			catch (Exception)
			{
				Data2[k] = Data1[k] * 0.2 + Data1[k - 1] * 0.2 +
						   Data1[k] * 0.6;
			}
		}


		for (int k = 1; k < DataSize - 39; k++)
		{
			FlowRate1[k + 20] = (Data2[k] - Data2[k + 20]) / (Time[k] - Time[k + 20]) * 0.5 +
								(Data2[k] - Data2[k + 39]) / (Time[k] - Time[k + 39]) * 0.5;
			//FlowRate[k] = ((Data2[k] - Data2[k + 40]) / (Time[k] - Time[k + 40]) * 0.2 +
			//               (Data2[k] - Data2[k + 60]) / (Time[k] - Time[k + 60]) * 0.5 +
			//               (Data2[k] - Data2[k + 100]) / (Time[k] - Time[k + 100]) * 0.4);
		}

		double total = 0;
		int previousRangeLeft = 0;
		int previousRangeRight = 0;
		for (int k = 1; k < DataSize; k++)
		{
			var range = Math.Max(0, Math.Min(50, Math.Min(k, DataSize - 1 - k)));

			int rangeLeft = k - range;
			int rangeRight = k + range;

			for (int j = previousRangeLeft; j < rangeLeft; j++)
			{
				total -= FlowRate1[j];
			}

			for (int j = previousRangeRight + 1; j <= rangeRight; j++)
			{
				total += FlowRate1[j];
			}

			previousRangeLeft = rangeLeft;
			previousRangeRight = rangeRight;
			RefFlowRate[k] = total / (range * 2 + 1);
		}


		total = 0;

		for (int k = 1; k < DataSize; k++)
		{
			var range = Math.Max(0, Math.Min(10, Math.Min(k, DataSize - 1 - k)));

			int rangeLeft = k - range;
			int rangeRight = k + range;

			for (int j = rangeLeft; j < rangeRight; j++)
			{
				total += FlowRate1[j];
			}
			FlowRate2[k] = total / (range * 2);
			total = 0;
		}
		for (int k = 1; k < DataSize; k++)
		{
			var range = Math.Max(0, Math.Min(30, Math.Min(k, DataSize - 1 - k)));

			int rangeLeft = k - range;
			int rangeRight = k + range;

			for (int j = rangeLeft; j < rangeRight; j++)
			{
				total += FlowRate2[j];
			}
			FlowRate[k] = total / (range * 2);
			total = 0;
		}
	}

	public void UpdateDataFromTo()
	{
		DataSize = toIndex - fromIndex;
		double[] oldData2 = Data2;
		double[] oldTime = Time;
		double[] oldFlowRate = FlowRate;
		Data2 = new double[DataSize];
		Time = new double[DataSize];
		FlowRate = new double[DataSize];
		for (int i = 0; i < DataSize; i++)
		{
			Data2[i] = oldData2[fromIndex + i];
			FlowRate[i] = oldFlowRate[fromIndex + i];
			Time[i] = oldTime[fromIndex + i];
		}
	}

	public string GetUnixTimeString()
	{
		DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
		dtDateTime = dtDateTime.AddSeconds(Unixtime).ToLocalTime();
		var unixtime = dtDateTime.ToString(CultureInfo.InvariantCulture);
		return unixtime;
	}

	public void Update()
	{
		DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
		dtDateTime = dtDateTime.AddSeconds(Unixtime).ToLocalTime();
		Thoigian = dtDateTime.ToString(CultureInfo.InvariantCulture);
	}

	private void StringToData()
	{
		// unknown,unixTime
		// Metadata
		// time,data
		// time,data
		// time,data

		var lines = Regex.Split(DataString, "[\r\n]+");

		var firstLine = lines[0];
		var values = firstLine.Split(',');



		Unixtime = double.Parse(values[1]);
		if (lines.Length - 2 > 0)
		{
			DataSize = lines.Length - 2;
			Data = new double[DataSize];
			Time = new double[DataSize];
			for (int i = 1; i <= lines.Length - 1; i++)
			{
				values = lines[i].Split(',');
				try
				{
					Time[i - 1] = double.Parse(values[0]) / 1000;
					Data[i - 1] = double.Parse(values[1]);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					Debug.WriteLine(i);
				}
			}
			if (Data[DataSize - 2] - Data[0] > 200 || DataSize > 2000)
			{
				IsData = true;
			}
			else
			{
				IsData = false;
			}
		}
	}


	public string ExportData()
	{
		string stringcsvdata = "";
		for (int i = 1; i < DataSize - 10; i++)
		{
			stringcsvdata += $"{Time[i]},{Data2[i]},{FlowRate2[i]}\n";
		}
		return stringcsvdata;
	}

	public void VoidVolume()
	{
		int first = 0;

		for (int i = 10; i < DataSize; i++)
		{
			if (FlowRate[i] > 0.1)
			{
				first = i;
				break;
			}
		}
		//Tim last
		int last = DataSize - 1;
		for (int i = DataSize - 1; i >= 0; i--)
		{
			if (FlowRate[i] > 0.1)
			{
				last = i;
				break;
			}
		}
		int MaxIndexRange = last;
		voidvolume = Data2[last] - Data2[first];
		qave = (Data2[last] - Data2[first]) /
			   (Time[last] - Time[first]);
		var max = FlowRate[last];
		for (int i = last; i >= first; i--)
		{
			if (FlowRate[i] > max)
			{
				max = FlowRate[i];
				MaxIndexRange = i;
			}
		}
		qmax = FlowRate[MaxIndexRange];
	}

	public void SelectDataFormTo(int from, int to)
	{
		fromIndex = from;
		toIndex = to;
		UpdateDataFromTo();
	}
}