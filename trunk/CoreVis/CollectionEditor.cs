using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using UICore;

namespace SlimTuneUI.CoreVis
{
	public partial class CollectionEditor : Form
	{
		public string[] Collection { get; private set; }

		public CollectionEditor()
		{
			InitializeComponent();
		}

		public void Edit(string[] collection)
		{
			if(collection == null)
				throw new ArgumentNullException("collection");

			Collection = collection;
			ItemsList.Items.Clear();
			ItemsList.Items.AddRange(collection);
		}

		private void AddButton_Click(object sender, EventArgs e)
		{
			string itemString = string.Empty;
			var result = Utilities.InputBox("Add Item", "Enter the new item:", ref itemString);
			if(result == DialogResult.OK)
			{
				ItemsList.Items.Add(itemString);
			}
		}

		private void RemoveButton_Click(object sender, EventArgs e)
		{
			if(ItemsList.SelectedItem != null)
				ItemsList.Items.Remove(ItemsList.SelectedItem);
		}

		private void SaveButton_Click(object sender, EventArgs e)
		{
			Collection = new string[ItemsList.Items.Count];
			for(int i = 0; i < ItemsList.Items.Count; ++i)
			{
				Collection[i] = ItemsList.Items[i].ToString();
			}
		}

		private void Swap(int index, int offset)
		{
			var list = ItemsList.Items;
			object temp = list[index + offset];
			list[index + offset] = list[index];
			list[index] = temp;
		}

		private void UpButton_Click(object sender, EventArgs e)
		{
			if(ItemsList.SelectedIndex > 0)
			{
				Swap(ItemsList.SelectedIndex, -1);
				--ItemsList.SelectedIndex;
			}
		}

		private void DownButton_Click(object sender, EventArgs e)
		{
			if(ItemsList.SelectedIndex < ItemsList.Items.Count - 1)
			{
				Swap(ItemsList.SelectedIndex, 1);
				++ItemsList.SelectedIndex;
			}
		}
	}
}
