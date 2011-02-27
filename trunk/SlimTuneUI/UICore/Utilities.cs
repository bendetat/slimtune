/*
* Copyright (c) 2007-2010 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace UICore
{
	//Not in 2.0 sadly
	public delegate void Action();

	public struct Pair<S, T>
	{
		public S First;
		public T Second;

		public Pair(S first, T second)
		{
			this.First = first;
			this.Second = second;
		}
	}

	public class TypeEntry
	{
		public string Name { get; set; }
		public Type Type { get; set; }

		public TypeEntry(string name, Type type)
		{
			this.Name = name;
			this.Type = type;
		}
	}

	public class DefaultItemAttribute : Attribute
	{
		public DefaultItemAttribute()
		{
		}
	}

	public static class Utilities
	{
		public static void Dispose(IDisposable dispose)
		{
			if(dispose != null)
				dispose.Dispose();
		}

		public static int Read7BitEncodedInt(BinaryReader reader)
		{
			int value = 0;
			int byteval;
			int shift = 0;
			while(((byteval = reader.ReadByte()) & 0x80) != 0)
			{
				value |= ((byteval & 0x7F) << shift);
				shift += 7;
			}
			return (value | (byteval << shift));
		}

		public static long Read7BitEncodedInt64(BinaryReader reader)
		{
			long value = 0;
			long byteval;
			int shift = 0;
			while(((byteval = reader.ReadByte()) & 0x80) != 0)
			{
				value |= ((byteval & 0x7F) << shift);
				shift += 7;
			}
			return (value | (byteval << shift));
		}

		public static void Write7BitEncodedInt(BinaryWriter writer, int value)
		{
			while(value >= 128)
			{
				writer.Write((byte) value | 0x80);
				value >>= 7;
			}
			writer.Write((byte) value);
		}

		public static string GetStandardCaption(Connection connection)
		{
			string caption = string.Empty;

			if(!string.IsNullOrEmpty(connection.Executable))
			{
				caption = string.Format("{0} - {1}", System.IO.Path.GetFileNameWithoutExtension(connection.Executable),
					System.IO.Path.GetFileName(connection.DataEngine.Name));
			}
			else if(!string.IsNullOrEmpty(connection.HostName))
			{
				caption = string.Format("{0}:{1} - {2}", connection.HostName, connection.Port,
					System.IO.Path.GetFileName(connection.DataEngine.Name));
			}
			else
			{
				caption = System.IO.Path.GetFileName(connection.DataEngine.Name);
			}

			caption += string.Format(" ({0})", connection.DataEngine.Engine);
			return caption;
		}

		/*public static string GetSaveFilter()
		{
			string filter = string.Empty;
			foreach(Type t in Plugins.GetEngines())
			{
				string extFilter = string.Empty;
				var handles = t.GetCustomAttributes(typeof(HandlesExtensionAttribute), false);
				if(handles.Length == 0)
					continue;

				foreach(var ext in handles)
				{
					string extString = (ext as HandlesExtensionAttribute).Extension;
					extFilter += "*.";
					extFilter += extString;
					extFilter += ";";
				}
				if(extFilter.EndsWith(";"))
					extFilter = extFilter.Remove(extFilter.Length - 1);

				string entry = string.Format("{0} file ({1})|{1}|", GetDisplayName(t), extFilter);
				filter += entry;
			}
			if(filter.EndsWith("|"))
				filter = filter.Remove(filter.Length - 1);

			return filter;
		}*/

		public static Type FindEngine(string file)
		{
			foreach(Type t in Plugins.GetEngines())
			{
				var handles = t.GetCustomAttributes(typeof(HandlesExtensionAttribute), false);
				foreach(var ext in handles)
				{
					string extString = (ext as HandlesExtensionAttribute).Extension;
					if(file.EndsWith(extString))
					{
						return t;
					}
				}
			}

			return null;
		}

		public static string GetDisplayName(Type type)
		{
			var name = GetAttribute<DisplayNameAttribute>(type);
			if(name != null)
				return name.DisplayName;
			return null;
		}

		public static T GetAttribute<T>(Type type) where T : Attribute
		{
			var attribs = type.GetCustomAttributes(typeof(T), false);
			if(attribs.Length > 0)
				return attribs[0] as T;
			else
				return null;
		}

		public static bool HasAttribute<T>(Type type) where T : Attribute
		{
			if(type == null)
				return false;

			return GetAttribute<T>(type) != null;
		}

		public static IEnumerable<TypeEntry> GetVisualizerList(bool includeDummy)
		{
			if(includeDummy)
				yield return new TypeEntry("(None)", null);

			foreach(var vis in Plugins.GetVisualizers())
			{
				string displayName = GetDisplayName(vis);
				yield return new TypeEntry(displayName, vis);
			}
		}

		public static IEnumerable<TypeEntry> GetLauncherList()
		{
			foreach(var launcher in Plugins.GetLaunchers())
			{
				string displayName = GetDisplayName(launcher);
				yield return new TypeEntry(displayName, launcher);
			}
		}

		public static DataSet ConvertDataReaderToDataSet(IDataReader reader)
		{
			DataSet dataSet = new DataSet();
			do
			{
				// Create new data table
				DataTable schemaTable = reader.GetSchemaTable();
				DataTable dataTable = new DataTable("Query");
				if(schemaTable != null)
				{
					// A query returning records was executed
					for(int i = 0; i < schemaTable.Rows.Count; i++)
					{
						DataRow dataRow = schemaTable.Rows[i];
						// Create a column name that is unique in the data table
						string columnName = (string) dataRow["ColumnName"];
						// Add the column definition to the data table
						DataColumn column = new DataColumn(columnName, (Type) dataRow["DataType"]);
						dataTable.Columns.Add(column);
					}
					dataSet.Tables.Add(dataTable);
					// Fill the data table we just created
					while(reader.Read())
					{
						DataRow dataRow = dataTable.NewRow();
						for(int i = 0; i < reader.FieldCount; i++)
							dataRow[i] = reader.GetValue(i);
						dataTable.Rows.Add(dataRow);
					}
				}
				else
				{
					// No records were returned
					DataColumn column = new DataColumn("RowsAffected");
					dataTable.Columns.Add(column);
					dataSet.Tables.Add(dataTable);
					DataRow dataRow = dataTable.NewRow();
					dataRow[0] = reader.RecordsAffected;
					dataTable.Rows.Add(dataRow);
				}
			} while(reader.NextResult());

			return dataSet;
		}

		public static DialogResult InputBox(string title, string promptText, ref string value)
		{
			Form form = new Form();
			Label label = new Label();
			TextBox textBox = new TextBox();
			Button buttonOk = new Button();
			Button buttonCancel = new Button();

			form.Text = title;
			label.Text = promptText;
			textBox.Text = value;

			buttonOk.Text = "OK";
			buttonCancel.Text = "Cancel";
			buttonOk.DialogResult = DialogResult.OK;
			buttonCancel.DialogResult = DialogResult.Cancel;

			label.SetBounds(9, 20, 372, 13);
			textBox.SetBounds(12, 36, 372, 20);
			buttonOk.SetBounds(228, 72, 75, 23);
			buttonCancel.SetBounds(309, 72, 75, 23);

			label.AutoSize = true;
			textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
			buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

			form.ClientSize = new Size(396, 107);
			form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
			form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.StartPosition = FormStartPosition.CenterScreen;
			form.MinimizeBox = false;
			form.MaximizeBox = false;
			form.AcceptButton = buttonOk;
			form.CancelButton = buttonCancel;

			DialogResult dialogResult = form.ShowDialog();
			value = textBox.Text;
			return dialogResult;
		}

		public static void FireEvent(object sender, EventHandler handler)
		{
			FireEvent(sender, handler, EventArgs.Empty);
		}

		public static void FireEvent(object sender, EventHandler handler, EventArgs e)
		{
			try
			{
				if(handler != null)
					handler(sender, e);
			}
			catch
			{
			}
		}

	}
}
