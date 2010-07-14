using System;
using System.Data;

using NHibernate;
using NHibernate.Cfg;

namespace UICore
{
	class SnapshotNamingStrategy : INamingStrategy
	{
		public int SnapshotIndex { get; private set; }

		public SnapshotNamingStrategy(int index)
		{
			SnapshotIndex = index;
		}

		public string ClassToTableName(string className)
		{
			return DefaultNamingStrategy.Instance.ClassToTableName(className);
		}

		public string ColumnName(string columnName)
		{
			return DefaultNamingStrategy.Instance.ColumnName(columnName);
		}

		public string LogicalColumnName(string columnName, string propertyName)
		{
			return DefaultNamingStrategy.Instance.LogicalColumnName(columnName, propertyName);
		}

		public string PropertyToColumnName(string propertyName)
		{
			return DefaultNamingStrategy.Instance.PropertyToColumnName(propertyName);
		}

		public string PropertyToTableName(string className, string propertyName)
		{
			return DefaultNamingStrategy.Instance.PropertyToTableName(className, propertyName);
		}

		public string TableName(string tableName)
		{
			if(SnapshotIndex < 0)
				return DefaultNamingStrategy.Instance.TableName(tableName);
			if(tableName != "Calls" &&
				tableName != "Samples"
				&& tableName != "Timings")
				return DefaultNamingStrategy.Instance.TableName(tableName);

			return string.Format("{0}_{1}", tableName, SnapshotIndex);
		}
	}
}
