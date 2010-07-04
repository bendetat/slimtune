using System;
using System.Collections.Generic;

using FluentNHibernate.Mapping;

namespace UICore.Mappings
{
	public class ThreadInfoMap : ClassMap<ThreadInfo>
	{
		public ThreadInfoMap()
		{
			Id(x => x.Id);
			Map(x => x.IsAlive);
			Map(x => x.Name);
			HasMany(x => x.Samples);
			HasMany(x => x.Calls);
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
			HasMany(x => x.CallsAsParent);
			HasMany(x => x.CallsAsChild);
			Table("Functions");
		}
	}

	public class ClassInfoMap : ClassMap<ClassInfo>
	{
		public ClassInfoMap()
		{
			Id(x => x.Id);
			Map(x => x.Name);
			HasMany(x => x.Functions);
			Table("Classes");
		}
	}

	public class CallMap : ClassMap<Call>
	{
		public CallMap()
		{
			CompositeId()
				.KeyProperty(x => x.ThreadId)
				.KeyProperty(x => x.ParentId)
				.KeyProperty(x => x.ChildId)
				.Mapped();
			Map(x => x.ThreadId);
			Map(x => x.ParentId);
			Map(x => x.ChildId);
			Map(x => x.HitCount);
			References(x => x.Thread, "ThreadId");
			References(x => x.Parent, "ParentId");
			References(x => x.Child, "ChildId");
			Table("Calls");
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

	public class CounterMap : ClassMap<Counter>
	{
		public CounterMap()
		{
			Id(x => x.Id);
			Map(x => x.Id);
			Map(x => x.Name);
			HasMany(x => x.Values);
			Table("Counters");
		}
	}

	public class CounterValueMap : ClassMap<CounterValue>
	{
		public CounterValueMap()
		{
			CompositeId()
				.KeyProperty(x => x.CounterId)
				.KeyProperty(x => x.Time)
				.Mapped();
			Map(x => x.CounterId);
			Map(x => x.Time);
			Map(x => x.Value);
			References(x => x.Counter, "CounterId");
			Table("CounterValues");
		}
	}
}
