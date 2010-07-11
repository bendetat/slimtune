using System;
using System.Collections.Generic;

using FluentNHibernate.Mapping;

namespace UICore.Mappings
{
	public class PropertyMap : ClassMap<Property>
	{
		public PropertyMap()
		{
			Id(x => x.Name).GeneratedBy.Assigned();
			Map(x => x.Value);
			Table("Properties");
		}
	}

	public class ThreadInfoMap : ClassMap<ThreadInfo>
	{
		public ThreadInfoMap()
		{
			Id(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.IsAlive);
			Map(x => x.Name);
			HasMany(x => x.Samples).Inverse();
			HasMany(x => x.Calls).Inverse();
			Table("Threads");
		}
	}

	public class FunctionInfoMap : ClassMap<FunctionInfo>
	{
		public FunctionInfoMap()
		{
			Id(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.Name);
			Map(x => x.Signature);
			Map(x => x.IsNative);
			Map(x => x.ClassId);
			//References(x => x.Class, "ClassId");
			HasMany(x => x.CallsAsParent).Inverse();
			HasMany(x => x.CallsAsChild).Inverse();
			Table("Functions");
		}
	}

	public class ClassInfoMap : ClassMap<ClassInfo>
	{
		public ClassInfoMap()
		{
			Id(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.Name);
			HasMany(x => x.Functions).Inverse();
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
			Map(x => x.ThreadId).Index("Calls_ThreadIndex");
			Map(x => x.ParentId).Index("Calls_ParentIndex");
			Map(x => x.ChildId).Index("Calls_ChildIndex");
			Map(x => x.HitCount).Not.Nullable();
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
			Map(x => x.FunctionId).Index("Samples_FunctionIndex");
			Map(x => x.HitCount).Not.Nullable();
			References(x => x.Thread, "ThreadId");
			//References(x => x.Function, "FunctionId");
			Table("Samples");
		}
	}

	public class CounterMap : ClassMap<Counter>
	{
		public CounterMap()
		{
			Id(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.Name);
			HasMany(x => x.Values).Inverse();
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
			Map(x => x.CounterId).Index("CounterValues_IdIndex");
			Map(x => x.Time);
			Map(x => x.Value);
			References(x => x.Counter, "CounterId");
			Table("CounterValues");
		}
	}
}
