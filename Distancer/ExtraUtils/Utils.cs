using System;
namespace Distancer.ExtraUtils
{
	public class LatLngSimple
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }

		public LatLngSimple(double lat, double lng)
		{
			this.Latitude = lat;
			this.Longitude = lng;
		}
	}

	public static class Utils
	{
		public enum DistanceUnit { Miles, Kilometers };

		public static double ToRadian(this double value)
		{
			return (Math.PI / 180) * value;
		}

		public static double HaversineDistance(LatLngSimple coord1, LatLngSimple coord2, DistanceUnit unit)
		{
			double R = (unit == DistanceUnit.Miles) ? 3960 : 6371;
			var lat = (coord2.Latitude - coord1.Latitude).ToRadian();
			var lng = (coord2.Longitude - coord1.Longitude).ToRadian();

			var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
				Math.Cos(coord1.Latitude.ToRadian()) * Math.Cos(coord2.Latitude.ToRadian()) *
				Math.Sin(lng / 2) * Math.Sin(lng / 2);

			var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));

			return R * h2;
		}
	}
}
