using System;

namespace UICore
{
	/// <summary>
	/// The base interface for classes that provide visualization of profiler data.
	/// </summary>
	public interface IVisualizer
	{
		void Initialize(ProfilerWindowBase mainWindow, Connection connection);
		void Show(WeifenLuo.WinFormsUI.Docking.DockPanel parent);
	}
}
