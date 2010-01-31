using System;

namespace UICore
{
	public interface ILauncher
	{
		bool CheckParams();
		bool Launch();

		string Name { get; }
		ushort ListenPort { get; set; }
	}
}
