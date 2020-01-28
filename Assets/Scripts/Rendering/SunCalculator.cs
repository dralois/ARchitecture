using System;
using UnityEngine;

public class SunCalculator : MonoBehaviour
{
	// Standartwert ist Mitte Muenchen
	[SerializeField] private float _longitude = 11.576124f;
	[SerializeField] private float _latitude = 48.137154f;

	[SerializeField] [Range(0,60)] private int _minutes;
	[SerializeField] [Range(0,24)] private int _hour;

	[SerializeField] private Light _sunLight = null;
	[SerializeField] private Light _nightLight = null;

	private DateTime _time;

	public void SetTime(int hour, int minutes)
	{
		_hour = hour;
		_minutes = minutes;
		_time = DateTime.Today + new TimeSpan(hour, minutes, 0);
		// Winkel einstellen
		X_SetSunAngle();
	}

	private void X_SetSunAngle()
	{
		double alt, azi;
		Vector3 angles = new Vector3();
		// Berechnen
		SunPosition.CalculateSunPosition(_time, (double)_latitude, (double)_longitude, out azi, out alt);
		// Als Euler Winkel speichern
		angles.x = (float)alt * Mathf.Rad2Deg;
		angles.y = (float)azi * Mathf.Rad2Deg;
		_sunLight.transform.rotation = Quaternion.Euler(angles);
		// Sonne und Nachtlicht (de)aktivieren
		_sunLight.intensity = Mathf.InverseLerp(-12, 0, angles.x);
		_sunLight.gameObject.SetActive(_sunLight.intensity > 0);
		_nightLight.gameObject.SetActive(_sunLight.intensity == 0);
	}

	private void Start()
	{
		_time = DateTime.Now;
		_hour = _time.Hour;
		_minutes = _time.Minute;
		// Winkel initalisieren
		X_SetSunAngle();
	}

	private void OnValidate()
	{
		// Editor only
		if (_sunLight && _nightLight)
		{
			SetTime(_hour, _minutes);
		}
	}

}

/// <summary>
///  http://guideving.blogspot.co.uk/2010/08/sun-position-in-c.html
/// </summary>
public static class SunPosition
{
	private const double Deg2Rad = Math.PI / 180.0;
	private const double Rad2Deg = 180.0 / Math.PI;

	/// <summary>
	/// Calculates the sun light.
	/// </summary>
	/// <param name="dateTime">Time and date in local time.</param>
	/// <param name="latitude">Latitude expressed in decimal degrees.</param>
	/// <param name="longitude">Longitude expressed in decimal degrees.</param>
	public static void CalculateSunPosition(DateTime dateTime, double latitude, double longitude,
																					out double outAzimuth, out double outAltitude)
	{
		// Convert to UTC
		dateTime = dateTime.ToUniversalTime();

		// Number of days from J2000.0.
		double julianDate = 367 * dateTime.Year -
				(int)((7.0 / 4.0) * (dateTime.Year +
				(int)((dateTime.Month + 9.0) / 12.0))) +
				(int)((275.0 * dateTime.Month) / 9.0) +
				dateTime.Day - 730531.5;

		double julianCenturies = julianDate / 36525.0;

		// Sidereal Time
		double siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;

		double siderealTimeUT = siderealTimeHours +
				(366.2422 / 365.2422) * (double)dateTime.TimeOfDay.TotalHours;

		double siderealTime = siderealTimeUT * 15 + longitude;

		// Refine to number of days (fractional) to specific time.
		julianDate += (double)dateTime.TimeOfDay.TotalHours / 24.0;
		julianCenturies = julianDate / 36525.0;

		// Solar Coordinates
		double meanLongitude = CorrectAngle(Deg2Rad *
				(280.466 + 36000.77 * julianCenturies));

		double meanAnomaly = CorrectAngle(Deg2Rad *
				(357.529 + 35999.05 * julianCenturies));

		double equationOfCenter = Deg2Rad * ((1.915 - 0.005 * julianCenturies) *
				Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));

		double elipticalLongitude =
				CorrectAngle(meanLongitude + equationOfCenter);

		double obliquity = (23.439 - 0.013 * julianCenturies) * Deg2Rad;

		// Right Ascension
		double rightAscension = Math.Atan2(
				Math.Cos(obliquity) * Math.Sin(elipticalLongitude),
				Math.Cos(elipticalLongitude));

		double declination = Math.Asin(
				Math.Sin(rightAscension) * Math.Sin(obliquity));

		// Horizontal Coordinates
		double hourAngle = CorrectAngle(siderealTime * Deg2Rad) - rightAscension;

		if (hourAngle > Math.PI)
		{
			hourAngle -= 2 * Math.PI;
		}

		double altitude = Math.Asin(Math.Sin(latitude * Deg2Rad) *
				Math.Sin(declination) + Math.Cos(latitude * Deg2Rad) *
				Math.Cos(declination) * Math.Cos(hourAngle));

		// Nominator and denominator for calculating Azimuth
		// angle. Needed to test which quadrant the angle is in.
		double aziNom = -Math.Sin(hourAngle);
		double aziDenom =
				Math.Tan(declination) * Math.Cos(latitude * Deg2Rad) -
				Math.Sin(latitude * Deg2Rad) * Math.Cos(hourAngle);

		double azimuth = Math.Atan(aziNom / aziDenom);

		if (aziDenom < 0) // In 2nd or 3rd quadrant
		{
			azimuth += Math.PI;
		}
		else if (aziNom < 0) // In 4th quadrant
		{
			azimuth += 2 * Math.PI;
		}

		outAltitude = altitude;
		outAzimuth = azimuth;
	}

	/// <summary>
	/// Corrects an angle.
	/// </summary>
	/// <param name="angleInRadians">An angle expressed in radians.</param>
	/// <returns>An angle in the range 0 to 2*PI.</returns>
	private static double CorrectAngle(double angleInRadians)
	{
		if (angleInRadians < 0)
		{
			return 2 * Math.PI - (Math.Abs(angleInRadians) % (2 * Math.PI));
		}
		else if (angleInRadians > 2 * Math.PI)
		{
			return angleInRadians % (2 * Math.PI);
		}
		else
		{
			return angleInRadians;
		}
	}

}
