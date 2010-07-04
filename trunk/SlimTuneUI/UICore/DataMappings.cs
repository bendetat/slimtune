using System;
using System.Collections.Generic;

using FluentNHibernate.Mapping;

namespace UICore
{
	public class ThreadInfoMap : ClassMap<ThreadInfo>
	{
		public ThreadInfoMap()
		{
			Id(x => x.Id);
			Map(x => x.IsAlive);
			Map(x => x.Name);
			HasMany(x => x.Samples)
				.Inverse()
				.Cascade.All();
			HasMany(x => x.Calls)
				.Inverse()
				.Cascade.All();
			Table("Threads");
		}
	}

	public class FunctionInfoMap : ClassMap<FunctionInfo>
	{
		public FunctionInfoMap()
		{
			Id(x => x.Id);
			Map(x => x.Name);
			Map(x => x.Signature);
			Map(x => x.IsNative);
			Map(x => x.ClassId);
			References(x => x.Class, "ClassId");
			Table("Functions");
		}
	}

	public class ClassInfoMap : ClassMap<ClassInfo>
	{
		public ClassInfoMap()
		{
			Id(x => x.Id);
			Map(x => x.Name);
			HasMany(x => x.Functions)
				.Cascade.All();
			Table("Classes");
		}
	}

	public class CallMap : ClassMap<Call>
	{
		public CallMap()
		{
			Id();
			Map(x => x.ThreadId);
			Map(x => x.CallerId);
			Map(x => x.CalleeId);
			Map(x => x.HitCount);
			References(x => x.Thread, "ThreadId");
			References(x => x.Caller, "CallerId");
			References(x => x.Callee, "CalleeId");
			Table("Callers");
		}
	}

	public class SampleMap : ClassMap<Sample>
	{
		public SampleMap()
		{
			CompositeId()
				.KeyProperty(x => x.ThreadId)
				.KeyProperty(x => x.FunctionId)
				.Mapped();
			Map(x => x.ThreadId);
			Map(x => x.FunctionId);
			Map(x => x.HitCount);
			References(x => x.Thread, "ThreadId");
			References(x => x.Function, "FunctionId");
			Table("Samples");
		}
	}
}
