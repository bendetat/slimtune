using System;
using System.Collections.Generic;

namespace UICore
{
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
	}

	public class ClassInfo
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<FunctionInfo> Functions { get; set; }
	}

	public class Call
	{
		public virtual int ThreadId { get; set; }
		public virtual int CallerId { get; set; }
		public virtual int CalleeId { get; set; }
		public virtual int HitCount { get; set; }

		public virtual ThreadInfo Thread { get; set; }
		public virtual FunctionInfo Caller { get; set; }
		public virtual FunctionInfo Callee { get; set; }

		public override bool Equals(object obj)
		{
			if(obj == null)
				return false;

			var other = obj as Call;
			if(ThreadId == other.ThreadId &&
				CallerId == other.CallerId &&
				CalleeId == other.CalleeId)
				return true;
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 13 + ThreadId << 16 + CallerId << 8 + CalleeId;
			return hash;
		}
	}

	public class Sample
	{
		public virtual int ThreadId { get; set; }
		public virtual int FunctionId { get; set; }
		public virtual int HitCount { get; set; }

		public virtual ThreadInfo Thread { get; set; }
		public virtual FunctionInfo Function { get; set; }

		public override bool Equals(object obj)
		{
			if(obj == null)
				return false;

			var other = obj as Sample;
			if(ThreadId == other.ThreadId && FunctionId == other.FunctionId)
				return true;
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 17 + ThreadId << 16 + FunctionId;
			return hash;
		}
	}
}
