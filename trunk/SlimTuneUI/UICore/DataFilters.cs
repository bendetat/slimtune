using System;
using System.Collections.Generic;

using FluentNHibernate.Mapping;

namespace UICore.Filters
{
	public class SnapshotFilter : FilterDefinition
	{
		public SnapshotFilter()
		{
			WithName("SnapshotFilter")
				.AddParameter("snapshot", NHibernate.NHibernateUtil.Int32);
		}
	}
}
