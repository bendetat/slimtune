using System;
using System.Collections.Generic;

using FluentNHibernate.Mapping;

namespace UICore.Filters
{
	public class Snapshot : FilterDefinition
	{
		public Snapshot()
		{
			WithName("Snapshot")
				.AddParameter("snapshotId", NHibernate.NHibernateUtil.Int32);
		}
	}

	public class Thread : FilterDefinition
	{
		public Thread()
		{
			WithName("Thread")
				.AddParameter("threadId", NHibernate.NHibernateUtil.Int32);
		}
	}
}
