using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Injector;

namespace Injector_Win32
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			InjectorCore.FindUnityPath();
			InjectorCore.LoadPaths();
			unityText.Text = InjectorCore.UnityPath;
			barText.Text = InjectorCore.BarFileName;
			scoreText.Text = InjectorCore.ScoreloopBundlePath;
			signText.Text = InjectorCore.KeyPassword;
			signCheck.IsChecked = InjectorCore.SignBarFile;

			phoneIP.Text = InjectorCore.PhoneIP;
			phoneLockPass.Text = InjectorCore.PhonePass;
		}

		~MainWindow()
		{
			InjectorCore.SavePaths();
		}
		
		private void Apply_Click(object sender, RoutedEventArgs e)
		{
			string error = InjectorCore.ProcessBarFile();
			if (error != null) MessageBox.Show(error, "Error");
			else MessageBox.Show("Scoreloop inject complete, but remember to check log.txt for any packaging errors.", "Success");
		}

		private void SelectUnity_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new System.Windows.Forms.FolderBrowserDialog();
			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) unityText.Text = dlg.SelectedPath;
		}

		private void SelectBar_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog();
			if (dlg.ShowDialog(this) == true) barText.Text = dlg.FileName;
		}

		private void SelectScoreloopPath_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new System.Windows.Forms.FolderBrowserDialog();
			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) scoreText.Text = dlg.SelectedPath;
		}

		private void UnityPath_TextChanged(object sender, TextChangedEventArgs e)
		{
			InjectorCore.UnityPath = unityText != null ? unityText.Text : "";
		}

		private void BarFileName_TextChanged(object sender, TextChangedEventArgs e)
		{
			InjectorCore.BarFileName = barText != null ? barText.Text : "";
		}

		private void ScoreloopDataPath_TextChanged(object sender, TextChangedEventArgs e)
		{
			InjectorCore.ScoreloopBundlePath = scoreText != null ? scoreText.Text : "";
		}

		private void signText_TextChanged(object sender, TextChangedEventArgs e)
		{
			InjectorCore.KeyPassword = signText != null ? signText.Text : "";
		}

		private void signCheck_Checked(object sender, RoutedEventArgs e)
		{
			InjectorCore.SignBarFile = true;
		}

		private void signCheck_Unchecked(object sender, RoutedEventArgs e)
		{
			InjectorCore.UseClassicMode = false;
		}

		private void classicMode_Checked(object sender, RoutedEventArgs e)
		{
			InjectorCore.UseClassicMode = true;
		}

		private void classicMode_Unchecked(object sender, RoutedEventArgs e)
		{
			InjectorCore.SignBarFile = false;
		}

		private void Upload_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog();
			if (dlg.ShowDialog(this) == true)
			{
				string error = InjectorCore.Upload(dlg.FileName);
				if (error != null) MessageBox.Show(error, "Error");
				else MessageBox.Show("Upload Done, but remember to check log.txt for any upload errors.", "Error");
			}
		}

		private void phoneLockPass_TextChanged(object sender, TextChangedEventArgs e)
		{
			InjectorCore.PhonePass = phoneLockPass.Text;
		}

		private void phoneIP_TextChanged(object sender, TextChangedEventArgs e)
		{
			InjectorCore.PhoneIP = phoneIP.Text;
		}
	}
}
