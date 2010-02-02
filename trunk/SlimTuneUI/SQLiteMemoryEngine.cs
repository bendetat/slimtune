using System;
using System.Collections.Generic;
using System.Text;

namespace SlimTuneUI
{
	class SQLiteMemoryEngine : SQLiteEngine
	{
		public override bool InMemory
		{
			get { return true; }
		}

		public SQLiteMemoryEngine()
			: base()
		{

		}
	}
}
