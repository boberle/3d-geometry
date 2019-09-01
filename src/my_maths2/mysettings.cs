using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My.Maths2
{



	/// <summary>
	/// Paramètres pour cette application.
	/// </summary>
	internal static class MySettings
	{


		/// <summary>
		/// Lors de l'utilisation d'une méthode Approx, indique le rayon de l'interval.
		/// </summary>
		public static double ApproxInterval
		{
			get { return My.MySettingsWriter.GetDouble(true, "ApproxInterval", 0.0000005); }
			set { My.MySettingsWriter.SetDouble(true, "ApproxInterval", value); }
		}


	}

}
