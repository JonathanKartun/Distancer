using System;
using Android.Locations;
using Android.Content;
using System.Collections.Generic;
using Android.Util;


namespace Distancer.ExtraUtils
{
	public class GeoLocGetter
	{
		Geocoder geo;

		public LatLngSimple coordsCompareTo; //For Distance Checking

		public GeoLocGetter(Context ctx)
		{
			geo = new Geocoder(ctx);
		}

		public string GetGeoLocation(double lat, double lon, bool showLatLong)
		{
			string sStr = "";

			IList<Address> addressList = geo.GetFromLocation(lat, lon, 10);

			DEBUG("Geo Counts = " + addressList.Count);

			if (addressList.Count > 0)
			{
				string sTmp = "* ";
				Address aAdd = addressList[0];
				if (aAdd.MaxAddressLineIndex > 0)
				{
					for (int z = 0; z < aAdd.MaxAddressLineIndex; z++)
					{
						sTmp += aAdd.GetAddressLine(z) + (z < aAdd.MaxAddressLineIndex - 1 ? ", " : "");
					}
				} else
                {
					var str = aAdd.GetAddressLine(0);
					if (str != null)
                    {
						sTmp = str;
                    }
                }

				if (showLatLong)
				{
					sTmp += "\n";
					sTmp += "Coords = " + aAdd.Latitude + ", " + aAdd.Longitude;

					if (coordsCompareTo != null)
					{
						sTmp += "\n";
						double dKM = Utils.HaversineDistance(coordsCompareTo, new LatLngSimple(lat, lon), Utils.DistanceUnit.Kilometers);
						double dMiles = Utils.HaversineDistance(coordsCompareTo, new LatLngSimple(lat, lon), Utils.DistanceUnit.Miles);

						sTmp += String.Format("{0:0.###} KM", dKM);
						sTmp += "\n";
						sTmp += String.Format("{0:0.###} Miles", dMiles);
					}
				}

				sStr = sTmp;
			}

			sStr = sStr.Trim();

			return sStr;
		}

		public LatLngSimple GetCoordsFromLocation(string sLoc)
		{
			IList<Address> addresses = geo.GetFromLocationName(sLoc, 1);
			if (addresses.Count == 0)
			{
				return null;
			}
			Address address = addresses[0];
			double longitude = address.Longitude;
			double latitude = address.Latitude;

			LatLngSimple nLoc = new LatLngSimple(latitude, longitude);

			return nLoc;
		}

		void DEBUG(string sMSG)
		{
			Log.Debug("com.jon.distancer.geoloc", sMSG);
		}
	}
}
