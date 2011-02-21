using System;
using System.Collections.Generic;

using FluentNHibernate.Mapping;

namespace UICore.Mappings
{
	public class PropertyMap : ClassMap<Property>
	{
		public PropertyMap()
		{
			Id(x => x.Name)
				.GeneratedBy.Assigned();
			Map(x => x.Value);
			Table("Properties");
		}
	}

	public class SnapshotMap : ClassMap<Snapshot>
	{
		public SnapshotMap()
		{
			Id(x => x.Id);
			Map(x => x.Name);
			Map(x => x.DateTime);
			Table("Snapshots");
		}
	}

	public class ThreadInfoMap : ClassMap<ThreadInfo>
	{
		public ThreadInfoMap()
		{
			Id(x => x.Id)
				.GeneratedBy.Assigned();
			Map(x => x.IsAlive);
			Map(x => x.Name);
			Table("Threads");
		}
	}

	public class FunctionInfoMap : ClassMap<FunctionInfo>
	{
		public FunctionInfoMap()
		{
			Id(x => x.Id)
				.GeneratedBy.Assigned();
			Map(x => x.Name);
			Map(x => x.Signature);
			Map(x => x.IsNative);
			References(x => x.Class, "ClassId")
				.NotFound.Ignore()
				.ReadOnly();
			HasMany(x => x.CallsAsParent)
				.Inverse()
				.KeyColumn("ParentId")
				.LazyLoad()
				.ReadOnly();
			HasMany(x => x.CallsAsChild)
				.Inverse()
				.KeyColumn("ChildId")
				.LazyLoad()
				.ReadOnly();
			HasMany(x => x.Samples)
				.Inverse()
				.KeyColumn("FunctionId")
				.LazyLoad()
				.ReadOnly();
			Table("Functions");
		}
	}

	public class ClassInfoMap : ClassMap<ClassInfo>
	{
		public ClassInfoMap()
		{
			Id(x => x.Id)
				.GeneratedBy.Assigned();
			Map(x => x.Name);
			Map(x => x.IsValueType);
			HasMany(x => x.Functions)
				.Inverse()
				.KeyColumn("ClassId")
				.LazyLoad()
				.ReadOnly();
			Table("Classes");
		}
	}

	public class CallMap : ClassMap<Call>
	{
		public CallMap()
		{
			ReadOnly();
			Id(x => x.Id);

			Map(x => x.Time)
				.Not.Nullable();
			Map(x => x.SnapshotId)
				.Default("0");

			References(x => x.Thread, "ThreadId")
				.Not.Nullable()
				.Index("Calls_ThreadIndex, Calls_Composite");
			References(x => x.Parent, "ParentId")
				.NotFound.Ignore()
				.Index("Calls_ParentIndex, Calls_Composite");
			References(x => x.Child, "ChildId")
				.NotFound.Ignore()
				.Index("Calls_ChildIndex, Calls_Composite");
			References(x => x.Snapshot, "SnapshotId")
				.Not.Nullable();

			ApplyFilter<Filters.Snapshot>("SnapshotId = :snapshot");
			ApplyFilter<Filters.Thread>("ThreadId = :threadId");

			Table("Calls");
		}
	}

	public class SampleMap : ClassMap<Sample>
	{
		public SampleMap()
		{
			ReadOnly();
			Id(x => x.Id);
			Map(x => x.Time)
				.Not.Nullable();
			Map(x => x.SnapshotId)
				.Default("0");

			References(x => x.Thread, "ThreadId")
				.Index("Samples_ThreadIndex, Samples_Composite");
			References(x => x.Function, "FunctionId")
				.NotFound.Ignore()
				.Index("Samples_FunctionIndex, Samples_Composite");
			References(x => x.Snapshot, "SnapshotId")
				.Not.Nullable();

			ApplyFilter("Snapshot", "SnapshotId = :snapshot");
			
			Table("Samples");
		}
	}

	public class CounterMap : ClassMap<Counter>
	{
		public CounterMap()
		{
			Id(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.Name);
			HasMany(x => x.Values)
				.Inverse()
				.KeyColumn("CounterId")
				.LazyLoad()
				.ReadOnly();
			Table("Counters");
		}
	}

	public class CounterValueMap : ClassMap<CounterValue>
	{
		public CounterValueMap()
		{
			Id(x => x.Id);
			Map(x => x.CounterId).Index("CounterValues_IdIndex");
			Map(x => x.Time);
			Map(x => x.Value);
			References(x => x.Counter, "CounterId")
				.NotFound.Ignore()
				.ReadOnly();
			Table("CounterValues");
		}
	}

	public class GarbageCollectionMap : ClassMap<GarbageCollection>
	{
		public GarbageCollectionMap()
		{
			Id(x => x.Id);
			Map(x => x.Generation);
			Map(x => x.FunctionId);
			Map(x => x.Time);
			Table("GarbageCollections");
		}
	}

	public class AllocationMap : ClassMap<Allocation>
	{
		public AllocationMap()
		{
			Id(x => x.Id);
			Map(x => x.ClassId);
			Map(x => x.FunctionId);
			Map(x => x.Count);
			Map(x => x.Size);
			Table("Allocations");
		}
	}
}
