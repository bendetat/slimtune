using System;
using System.Collections.Generic;

namespace Universe
{
	interface IView
	{
		void UpdateData();
		void Draw(Scene parentScene);
		void DrawIconic(Scene parentScene);
	}
}
