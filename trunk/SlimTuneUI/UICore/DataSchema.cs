using System;
using System.Collections.Generic;

namespace UICore
{
	public class Property
	{
		public virtual string Name { get; set; }
		public virtual string Value { get; set; }
	}

	public class Snapshot
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual long DateTime { get; set; }

		public virtual string ToString(string format)
		{
			DateTime dt = DateTime != long.MaxValue ? System.DateTime.FromFileTime(DateTime) : System.DateTime.Now;
			return string.Format(format, Id, Name, dt);
		}
	}

	public class ThreadInfo
	{
		public virtual int Id { get; set; }
		public virtual bool IsAlive { get; set; }
		public virtual string Name { get; set; }

		public virtual IList<Call> Calls { get; set; }
		public virtual IList<Sample> Samples { get; set; }
	}

	public class FunctionInfo
	{
		public virtual int Id { get; set; }
		public virtual int ClassId { get; set; }
		public virtual ClassInfo Class { get; set; }
		public virtual bool IsNative { get; set; }
		public virtual string Name { get; set; }
		public virtual string Signature { get; set; }

		public virtual IList<Call> CallsAsParent { get; set; }
		public virtual IList<Call> CallsAsChild { get; set; }
		public virtual IList<Sample> Samples { get; set; }
	}

	public class ClassInfo
	{
		public virtual int Id { get; set; }
		public virtual bool IsValueType { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<FunctionInfo> Functions { get; set; }
	}

	public class Call
	{
		public virtual int Id { get; set; }
		public virtual double Time { get; set; }
		public virtual int SnapshotId { get; set; }

		public virtual ThreadInfo Thread { get; set; }
		public virtual FunctionInfo Parent { get; set; }
		public virtual FunctionInfo Child { get; set; }
		public virtual Snapshot Snapshot { get; set; }

		public override bool Equals(object obj)
		{
			if(obj == null)
				return false;

			var other = obj as Call;
			if(Thread.Id == other.Thread.Id &&
				Parent.Id == other.Parent.Id &&
				Child.Id == other.Child.Id)
				return true;
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 13 + Thread.Id << 16 + Parent.Id << 8 + Child.Id;
			return hash;
		}
	}

	public class Sample
	{
		public virtual int Id { get; set; }
		public virtual double Time { get; set; }
		public virtual int SnapshotId { get; set; }

		public virtual ThreadInfo Thread { get; set; }
		public virtual FunctionInfo Function { get; set; }
		public virtual Snapshot Snapshot { get; set; }

		public override bool Equals(object obj)
		{
			if(obj == null)
				return false;

			var other = obj as Sample;
			if(Thread.Id == other.Thread.Id && Function.Id == other.Function.Id)
				return true;
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 17 + Thread.Id << 16 + Function.Id;
			return hash;
		}
	}

	public class Counter
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<CounterValue> Values { get; set; }
	}

	public class CounterValue
	{
		public virtual int Id { get; set; }
		public virtual int CounterId { get; set; }
		public virtual Counter Counter { get; set; }
		public virtual long Time { get; set; }
		public virtual double Value { get; set; }

		public override bool Equals(object obj)
		{
			if(obj == null)
				return false;

			var other = obj as CounterValue;
			if(CounterId == other.CounterId && Time == other.Time)
				return true;
			return false;
		}

		public override int GetHashCode()
		{
			return 23 + CounterId + (int) Time;
		}
	}

	public class GarbageCollection
	{
		public virtual int Id { get; set; }
		public virtual int Generation { get; set; }
		public virtual int FunctionId { get; set; }
		public virtual long Time { get; set; }
	}

	public class Allocation
	{
		public virtual int Id { get; set; }
		public virtual int ClassId { get; set; }
		public virtual int FunctionId { get; set; }
		public virtual int Count { get; set; }
		public virtual int Size { get; set; }
	}
}
