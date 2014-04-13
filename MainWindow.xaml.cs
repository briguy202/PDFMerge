using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;

namespace PDFMerge {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		#region Declarations
		public static RoutedUICommand Add = new RoutedUICommand("Add", "Add", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.A, ModifierKeys.Control) });
		public static RoutedUICommand MoveUp = new RoutedUICommand("MoveUp", "MoveUp", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.Up, ModifierKeys.Alt) });
		public static RoutedUICommand MoveDown = new RoutedUICommand("MoveDown", "MoveDown", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.Down, ModifierKeys.Alt) });
		public static RoutedUICommand Combine = new RoutedUICommand("Combine", "Combine", typeof(MainWindow));
		#endregion

		#region Constructors
		public MainWindow() {
			InitializeComponent();
		}
		#endregion

		#region Methods
		protected void MergePDFs(List<string> filenames) {
			string outputPath = string.Concat(this.tbxOutputPath.Text, "\\output.pdf");
			File.Delete(outputPath);
			bool initialized = false;
			int pageOffset = 0;

			Document document = new Document();
			PdfCopy copy = new PdfCopy(document, new FileStream(outputPath, FileMode.Create));
			document.Open();
			PdfReader reader = null;

			foreach (string filename in filenames) {
				FileInfo file = new FileInfo(filename);
				if (file.Exists) {
					reader = new PdfReader(file.FullName);
					int pages = reader.NumberOfPages;
					pageOffset += pages;

					if (!initialized) {
						// First time through
						initialized = true;
						document.SetPageSize(reader.GetPageSizeWithRotation(1));
					}

					PdfImportedPage page = null;
					for (int i = 1; i <= pages; i++) {
						page = copy.GetImportedPage(reader, i);
						copy.AddPage(page);
					}
					reader.Close();
				}
			}
			document.Close();
			copy.Close();
		}

		protected void MoveItem(bool moveUp) {
			int addValue = (moveUp) ? -1 : 1;
			int currentIndex = this.listBox1.SelectedIndex;
			int newIndex = currentIndex + addValue;
			string selectedItem = (string)this.listBox1.SelectedItem;
			this.listBox1.Items.RemoveAt(currentIndex);
			this.listBox1.Items.Insert(currentIndex + addValue, selectedItem);
			this.listBox1.SelectedItem = this.listBox1.Items[newIndex];
		}
		#endregion

		#region Events
		private void ListBox_Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (this.listBox1.SelectedItem != null);
		}

		private void ListBox_Delete_Executed(object sender, ExecutedRoutedEventArgs e) {
			int selectedItemsCount = this.listBox1.SelectedItems.Count;
			for (int i = 0; i < selectedItemsCount; i++) {
				string item = (string)this.listBox1.SelectedItems[0];
				this.listBox1.Items.Remove(item);
			}
		}

		private void ListBox_Add_Executed(object sender, ExecutedRoutedEventArgs e) {
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Multiselect = true;
			dialog.Filter = "PDF Files (*.pdf)|*.pdf";
			if (dialog.ShowDialog(this) == true) {
				foreach (string filename in dialog.FileNames) {
					this.listBox1.Items.Add(filename);
				}

				if (string.IsNullOrEmpty(this.tbxOutputPath.Text)) {
					this.tbxOutputPath.Text = Path.GetDirectoryName((string)this.listBox1.Items[0]);
				}
			}
		}

		private void ListBox_MoveUp_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (this.listBox1.SelectedItems.Count == 1 && this.listBox1.SelectedIndex > 0);
		}

		private void ListBox_MoveUp_Executed(object sender, ExecutedRoutedEventArgs e) {
			this.MoveItem(true);
		}

		private void ListBox_MoveDown_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (this.listBox1.SelectedItems.Count == 1 && this.listBox1.SelectedIndex < this.listBox1.Items.Count - 1);
		}

		private void ListBox_MoveDown_Executed(object sender, ExecutedRoutedEventArgs e) {
			this.MoveItem(false);
		}

		private void Combine_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (this.listBox1.Items.Count > 1);
		}

		private void Combine_Executed(object sender, ExecutedRoutedEventArgs e) {
			List<string> items = new List<string>();
			foreach (var item in this.listBox1.Items) {
				items.Add((string)item);
			}
			this.MergePDFs(items);
			MessageBox.Show("PDF Combine completed");
			Process.Start("explorer", this.tbxOutputPath.Text);
		}
		#endregion
		
	}
}