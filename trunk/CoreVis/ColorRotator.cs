using System;
using System.Collections.Generic;
using System.Drawing;

namespace SlimTuneUI.CoreVis
{
	class ColorRotator
	{
		Color[] m_colors = new Color[]{
			Color.Blue,
			Color.Green,
			Color.DarkOrange,
			Color.DarkSlateBlue,
			Color.Sienna,
			Color.Yellow,
			Color.ForestGreen,
			Color.Navy,
			Color.SlateGray,
			Color.MediumVioletRed,
		};
		int m_index = 0;

		public Color NextColor()
		{
			Color color = m_colors[m_index++];
			if(m_index == m_colors.Length)
				m_index = 0;

			return color;
		}

		public Color ColorForIndex(int index)
		{
			return m_colors[index % m_colors.Length];
		}
	}
}
