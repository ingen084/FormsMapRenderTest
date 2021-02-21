namespace FormsMapRenderTest
{
	/// <summary>
	/// 地形レイヤーの種類
	/// </summary>
	public enum LandLayerType
	{
		/// <summary>
		/// 日本以外の全地域
		/// </summary>
		WorldWithoutJapan,
		/// <summary>
		/// 一次細分区域
		/// </summary>
		PrimarySubdivisionArea,
		/// <summary>
		/// 府県予報区域
		/// </summary>
		PrefectureForecastArea,
		/// <summary>
		/// 緊急地震速報／地方予報区
		/// </summary>
		RegionForecastAreaForEew,
		/// <summary>
		/// 緊急地震速報／府県予報区
		/// </summary>
		PrefectureForecastAreaForEew,
		/// <summary>
		/// 市町村等（気象警報等）
		/// </summary>
		MunicipalityWeatherWarningArea,
		/// <summary>
		/// 市町村等（地震津波関係）
		/// </summary>
		MunicipalityEarthquakeTsunamiArea,
		/// <summary>
		/// 市町村等をまとめた地域等
		/// </summary>
		BundledMunicipalityArea,
		/// <summary>
		/// 全国・地方予報区等
		/// <para>topojson変換時に全国だけになってしまっている</para>
		/// </summary>
		NationalAndRegionForecastArea,
		/// <summary>
		/// 地震情報／細分区域
		/// </summary>
		EarthquakeInformationSubdivisionArea,
		/// <summary>
		/// 地震情報／都道府県等
		/// </summary>
		EarthquakeInformationPrefecture,
		/// <summary>
		/// 津波予報区
		/// </summary>
		TsunamiForecastArea,
	}

	public static class LandLayerTypeExtensions
	{
		/// <summary>
		/// いくつの数字で割ればひとつ上の地域を分類できるかを返す
		/// </summary>
		/// <param name="t"></param>
		/// <returns>分類できない場合(そもそも広い地域として解釈してほしい場合など)は1</returns>
		public static int GetMultiareaGroupNo(this LandLayerType t)
		{
			switch (t)
			{
				case LandLayerType.WorldWithoutJapan:
					return 1;
				case LandLayerType.PrimarySubdivisionArea:
					return 1000;
				case LandLayerType.PrefectureForecastArea:
					return 1000;
				case LandLayerType.RegionForecastAreaForEew:
					return 1;
				case LandLayerType.PrefectureForecastAreaForEew:
					return 1;
				case LandLayerType.MunicipalityWeatherWarningArea:
					return 100000;
				case LandLayerType.MunicipalityEarthquakeTsunamiArea:
					return 100000;
				case LandLayerType.BundledMunicipalityArea:
					return 10000;
				case LandLayerType.NationalAndRegionForecastArea:
					return 1;
				case LandLayerType.EarthquakeInformationSubdivisionArea:
					return 10;
				case LandLayerType.EarthquakeInformationPrefecture:
					return 1;
				case LandLayerType.TsunamiForecastArea:
					return 1;
				default:
					return 1;
			}
		}
		/// <summary>
		/// マップの種類を日本語にして返す
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static string ToJapaneseString(this LandLayerType t)
		{
			switch (t)
			{
				case LandLayerType.WorldWithoutJapan:
					return "日本以外の全地域";
				case LandLayerType.PrimarySubdivisionArea:
					return "一次細分区域等";
				case LandLayerType.PrefectureForecastArea:
					return "府県予報区等";
				case LandLayerType.RegionForecastAreaForEew:
					return "緊急地震速報／地方予報区";
				case LandLayerType.PrefectureForecastAreaForEew:
					return "緊急地震速報／府県予報区";
				case LandLayerType.MunicipalityWeatherWarningArea:
					return "市町村等（気象警報等）";
				case LandLayerType.MunicipalityEarthquakeTsunamiArea:
					return "市町村等（地震津波関係）";
				case LandLayerType.BundledMunicipalityArea:
					return "市町村等をまとめた地域等";
				case LandLayerType.NationalAndRegionForecastArea:
					return "全国・地方予報区等";
				case LandLayerType.EarthquakeInformationSubdivisionArea:
					return "地震情報／細分区域";
				case LandLayerType.EarthquakeInformationPrefecture:
					return "地震情報／都道府県等";
				case LandLayerType.TsunamiForecastArea:
					return "津波予報区";
				default:
					return null;
			}
		}
	}
}
