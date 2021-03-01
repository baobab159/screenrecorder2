using System;
using System.IO;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows.Shell;
using Accord.Video.FFMPEG;


namespace screenrecorder2 {
		public  class NativeMethods {
			[DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
			public static extern int Record(string command, string /* StringBuilder*/ returnString, int returnLength, int callback);
			public static extern int SaveAudio(string command, string /* StringBuilder*/ returnString, int returnLength, int callback);
	}
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		VideoFileWriter videoFileWriter;
		string videoPath, audioName, videoName, finalVideo, outputPath /* it's only folder path */;
		int width  = (int)SystemParameters.PrimaryScreenWidth,
			height = (int)SystemParameters.PrimaryScreenHeight;
		Stopwatch stopwatch=new Stopwatch();
		DispatcherTimer timerRecording=new DispatcherTimer(), timerLabelMessageSavingFile;
		Graphics g;
		List<int> listForFrameRate = new List<int> { 5, 10, 15, 20, 25 };
		
		
		public MainWindow() {
			InitializeComponent();
			comboBoxFrameRate.ItemsSource = listForFrameRate;
			timerLabelMessageSavingFile = new DispatcherTimer();
			timerLabelMessageSavingFile.Interval = TimeSpan.FromSeconds(5);
		}

		private void TimerRecording_Tick(object sender, EventArgs e) {
			try {
				//byte[] audioByte = File.ReadAllBytes("C://Users//admin//Music//Captures//Ava Max - Salt (Official Lyric Video).wav");

				Bitmap image = new Bitmap(width, height);
				g = Graphics.FromImage(image);
				g.CopyFromScreen(0, 0, 0, 0, image.Size);
				videoFileWriter.WriteVideoFrame(image);
				//videoFileWriter.WriteAudioFrame(audioByte);
				//RecordAudio();
				stopwatch.Start();
				labelTimer.Content = string.Format("{0:D2}:{1:D2}:{2:D2}", stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds);
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		private void TimerLabelMessageSavingFile_Tick(object sender, EventArgs e) { //labelMessageAboutSavingAFile
			labelMessageSavingFile.Visibility = Visibility.Hidden;
		}

		private void NativeMethodsRecordAudio() {
			NativeMethods.Record("open new Type waveaudio Alias recsound", "", 0, 0);
			NativeMethods.Record("record recsound", "", 0, 0);
			//ffmpeg - list_devices true - f dshow - i dummy   //listing microphone devices
		}

		private void NativeMethodsSaveAudio() {
			NativeMethods.SaveAudio("save recsound c:\\work\\result.wav", "", 0, 0);
			NativeMethods.SaveAudio("close recsound ", "", 0, 0);
		}

		private void RecordAudio() {
			using (MemoryStream ms = new MemoryStream()) {

				/*encoder = new WaveEncoder(ms);
				encoder.Encode(eventArgs.Signal);
				ms.Seek(0, SeekOrigin.Begin);
				decoder = new WaveDecoder(ms);
				Signal s = decoder.Decode();
				videoFileWrite.WriteAudioFrame(s);

				encoder.Close();
				decoder.Close();*/

			}

		}

		private void CombineVideoAndAudio(string video, string audio) {
			//FFMPEG command to combine video and audio:
			string args = $"/c ffmpeg -i \"{video}\" -i \"{audio}\" -shortest {videoPath}";
			ProcessStartInfo startInfo = new ProcessStartInfo {
				CreateNoWindow = false,
				FileName = "cmd.exe",
				WorkingDirectory = videoPath,
				Arguments = args
			};

			using (Process exeProcess = Process.Start(startInfo)) { //Execute command:
				exeProcess.WaitForExit();
			}
		}

		private void CreateVideoThumbnail() {
			MemoryStream ms = new MemoryStream();
			Bitmap imageFrame = new Bitmap(width, height); 
			VideoFileReader videoFileReader = new VideoFileReader();
			videoFileReader.Open(videoPath);

			imageFrame = videoFileReader.ReadVideoFrame();
			imageFrame.Save(ms, ImageFormat.Png);
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.StreamSource = ms;
			bi.EndInit();
			imageLastlyRecordedVideoPreview.Source = bi;
			videoFileReader.Close();
		}
		private void ImageStartRecording_MouseDown(object sender, MouseButtonEventArgs e) {
			if (comboBoxFrameRate.Text != "Select frequency") {
				string dateStempel = DateTime.Now.ToString("d MMM yyyy, hh.mm.ss");
				videoPath = "C://Users//admin//Videos//Captures//" + dateStempel + ".mp4";
				labelMessageSavingFile.Visibility = Visibility.Visible;
				videoFileWriter = new VideoFileWriter();
				labelMessageSavingFile.Content = "Recording a screen in process";
				timerLabelMessageSavingFile.Start();
				timerLabelMessageSavingFile.Tick += TimerLabelMessageSavingFile_Tick;
				try {
					videoFileWriter.Open(videoPath, width, height, Int16.Parse(comboBoxFrameRate.SelectedItem.ToString()), VideoCodec.H265);
					//RecordAudio();
					timerRecording.Interval = TimeSpan.FromSeconds(1/Int64.Parse(comboBoxFrameRate.SelectedItem.ToString()));
					timerRecording.Tick += TimerRecording_Tick; //records video with demanded frequency
					timerRecording.Start();
				}
				catch (Exception ex) {
					MessageBox.Show(ex.ToString());
				}
			} else MessageBox.Show("Choose frame rate number", "Info",MessageBoxButton.OK ,MessageBoxImage.Information);
		}

		private void ImageEndRecording_MouseDown(object sender, MouseButtonEventArgs e) {
			labelMessageSavingFile.Content = "Video saved";
			labelMessageSavingFile.Visibility = Visibility.Visible;
			timerLabelMessageSavingFile.Start();
			timerRecording.Stop();
			try {
				//videoFileWriter.WriteAudioFrame(audioByte);
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
			videoFileWriter.Close();
			stopwatch.Stop();
			stopwatch.Reset();
			//CombineVideoAndAudio(videoName, audioName);
			CreateVideoThumbnail();	
		}
	}
}
