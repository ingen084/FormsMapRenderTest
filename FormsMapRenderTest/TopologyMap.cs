﻿using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormsMapRenderTest
{
	[MessagePackObject]
	public class TopologyMap
	{
		[Key(0)]
		public DoubleVector Scale { get; set; }
		[Key(1)]
		public DoubleVector Translate { get; set; }
		[Key(2)]
		public TopologyPolygon[] Polygons { get; set; }
		[Key(3)]
		public TopologyArc[] Arcs { get; set; }
		[Key(4)]
		public Dictionary<int, IntVector> CenterPoints { get; set; }

		public static TopologyMap Load(string path)
		{
			using (var file = File.OpenRead(path))
				return MessagePackSerializer.Deserialize<TopologyMap>(file, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray));
		}
		public static Dictionary<LandLayerType, TopologyMap> LoadCollection(string path)
		{
			using (var file = File.OpenRead(path))
				return MessagePackSerializer.Deserialize<Dictionary<LandLayerType, TopologyMap>>(file, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray));
		}
		public static TopologyMap Load(byte[] data)
			=> MessagePackSerializer.Deserialize<TopologyMap>(data, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray));
		public static Dictionary<LandLayerType, TopologyMap> LoadCollection(byte[] data)
			=> MessagePackSerializer.Deserialize<Dictionary<LandLayerType, TopologyMap>>(data, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray));
	}
	[MessagePackObject]
	public class TopologyArc
	{
		[Key(0)]
		public IntVector[] Arc { get; set; }
		[Key(1)]
		public TopologyArcType Type { get; set; }
	}
	public enum TopologyArcType
	{
		Coastline = 0,
		Admin,
		Area,
	}
	[MessagePackObject]
	public class TopologyPolygon
	{
		[Key(0)]
		public int[][] Arcs { get; set; }
		[Key(1)]
		public int? Code { get; set; }
	}

	[MessagePackObject]
	public struct DoubleVector
	{
		public DoubleVector(double x, double y)
		{
			X = x;
			Y = y;
		}

		[Key(0)]
		public double X { get; }
		[Key(1)]
		public double Y { get; }
	}
	[MessagePackObject]
	public struct IntVector
	{
		public IntVector(int x, int y)
		{
			X = x;
			Y = y;
		}

		[Key(0)]
		public int X { get; }
		[Key(1)]
		public int Y { get; }
	}
}
