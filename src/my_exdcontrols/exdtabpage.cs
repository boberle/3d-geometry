using System;
using System.Drawing;
using System.Windows.Forms;

namespace My
{



	/// <summary>
	/// Cette classe n'apporte rien qu'un TabPage normal, sinon qu'elle gère un problème de couleur de fond entre Windows XP et Windows Vista.
	/// </summary>
	public class ExdTabPage : TabPage
	{

		/// <summary>
		/// Constructeur.
		/// </summary>
		public ExdTabPage()
		{
			this.UseVisualStyleBackColor = true;
			if (Environment.OSVersion.Version.Major > 5) { this.BackColor = SystemColors.Window; }
			//else { this.UseVisualStyleBackColor = true; }
		}

	}

}
