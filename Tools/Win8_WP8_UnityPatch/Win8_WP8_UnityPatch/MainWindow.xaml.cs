using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Win8_WP8_UnityPatch
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			string path1 = @"C:\Program Files (x86)\Unity\Editor\Data\PlaybackEngines";
			if (Directory.Exists(path1))
			{
				patchDirectory.Text = path1;
				restoreDirectory.Text = path1;
				return;
			}

			string path2 = @"C:\Program Files\Unity\Editor\Data\PlaybackEngines";
			if (Directory.Exists(path2))
			{
				patchDirectory.Text = path2;
				restoreDirectory.Text = path2;
				return;
			}

			patchDirectory.Text = path1;
		}

		private void Patch_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				File.Copy(@"Mono.Cecil.Win8\Mono.Cecil.dll", patchDirectory.Text+@"\metrosupport\Tools\rrw\Mono.Cecil.dll", true);
				File.Copy(@"Mono.Cecil.Win8\Mono.Cecil.Mdb.dll", patchDirectory.Text+@"\metrosupport\Tools\rrw\Mono.Cecil.Mdb.dll", true);
				File.Copy(@"Mono.Cecil.Win8\Mono.Cecil.Pdb.dll", patchDirectory.Text+@"\metrosupport\Tools\rrw\Mono.Cecil.Pdb.dll", true);
				File.Copy(@"Mono.Cecil.Win8\Mono.Cecil.Rocks.dll", patchDirectory.Text+@"\metrosupport\Tools\rrw\Mono.Cecil.Rocks.dll", true);
				File.Copy(@"Mono.Cecil.Win8\rrw.exe", patchDirectory.Text+@"\metrosupport\Tools\rrw\rrw.exe", true);

				//File.Copy(@"Mono.Cecil.WP8\Mono.Cecil.dll", patchDirectory.Text+@"\wp8support\Tools\rrw\Mono.Cecil.dll", true);
				//File.Copy(@"Mono.Cecil.WP8\Mono.Cecil.Mdb.dll", patchDirectory.Text+@"\wp8support\Tools\rrw\Mono.Cecil.Mdb.dll", true);
				//File.Copy(@"Mono.Cecil.WP8\Mono.Cecil.Pdb.dll", patchDirectory.Text+@"\wp8support\Tools\rrw\Mono.Cecil.Pdb.dll", true);
				//File.Copy(@"Mono.Cecil.WP8\Mono.Cecil.Rocks.dll", patchDirectory.Text+@"\wp8support\Tools\rrw\Mono.Cecil.Rocks.dll", true);

				MessageBox.Show("Successfully patched :)");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void Restore_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				File.Copy(@"Mono.Cecil.Win8\Restore\Mono.Cecil.dll", patchDirectory.Text+@"\metrosupport\Tools\rrw\Mono.Cecil.dll", true);
				File.Copy(@"Mono.Cecil.Win8\Restore\Mono.Cecil.Mdb.dll", patchDirectory.Text+@"\metrosupport\Tools\rrw\Mono.Cecil.Mdb.dll", true);
				File.Copy(@"Mono.Cecil.Win8\Restore\Mono.Cecil.Pdb.dll", patchDirectory.Text+@"\metrosupport\Tools\rrw\Mono.Cecil.Pdb.dll", true);
				File.Copy(@"Mono.Cecil.Win8\Restore\Mono.Cecil.Rocks.dll", patchDirectory.Text+@"\metrosupport\Tools\rrw\Mono.Cecil.Rocks.dll", true);
				File.Copy(@"Mono.Cecil.Win8\Restore\rrw.exe", patchDirectory.Text+@"\metrosupport\Tools\rrw\rrw.exe", true);

				//File.Copy(@"Mono.Cecil.WP8\Restore\Mono.Cecil.dll", patchDirectory.Text+@"\wp8support\Tools\rrw\Mono.Cecil.dll", true);
				//File.Copy(@"Mono.Cecil.WP8\Restore\Mono.Cecil.Mdb.dll", patchDirectory.Text+@"\wp8support\Tools\rrw\Mono.Cecil.Mdb.dll", true);
				//File.Copy(@"Mono.Cecil.WP8\Restore\Mono.Cecil.Pdb.dll", patchDirectory.Text+@"\wp8support\Tools\rrw\Mono.Cecil.Pdb.dll", true);
				//File.Copy(@"Mono.Cecil.WP8\Restore\Mono.Cecil.Rocks.dll", patchDirectory.Text+@"\wp8support\Tools\rrw\Mono.Cecil.Rocks.dll", true);

				MessageBox.Show("Successfully restored!");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}
}
