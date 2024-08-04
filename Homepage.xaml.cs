

using System.Collections.ObjectModel;
namespace EatMemory;
public class FileItem
{
	public string FileName { get; set; }
	public string FileSize { get; set; }
}
public partial class Homepage : ContentPage
{
	private CancellationTokenSource _cancellationTokenSource;
	private readonly List<Color> _colors;
	private int _currentColorIndex;
	public ObservableCollection<FileItem> Files { get; set; }
	
	public Homepage()
	{
		InitializeComponent();
		_colors = new List<Color> { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple };
		_currentColorIndex = 0;

		_cancellationTokenSource = new CancellationTokenSource();
		StartFlashing(_cancellationTokenSource.Token);

		Files = new ObservableCollection<FileItem>();
		FileCollectionView.ItemsSource = Files;
	}
	private async void StartFlashing(CancellationToken token)
	{
		try
		{
			while (!token.IsCancellationRequested)
			{
				_currentColorIndex = (_currentColorIndex + 1) % _colors.Count;
                if (_colors.Count<=_currentColorIndex)
                {
					continue;
                }
                MainThread.BeginInvokeOnMainThread(() =>
				{
					FlashingLabel.TextColor = _colors[_currentColorIndex];
				});

				await Task.Delay(500, token);
			}
		}
		catch (OperationCanceledException)
		{
			return;
		}
		catch (Exception ex)
		{
			return;
		}
	}
	private async void OnSelectFileButtonClicked(object sender, EventArgs e)
	{
		try
		{
			var result = await FilePicker.Default.PickAsync();
			if (result != null)
			{

				//FilePathEntry.Text = result.FullPath;
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("错误", "选择文件失败: " + ex.Message, "确定");
		}
	}
	private async void OnAddFileButtonClicked(object sender, EventArgs e)
	{
		try
		{
			string[] files = Directory.GetFiles(FileSystem.Current.AppDataDirectory);
            if (files.Length != 0)
            {
                foreach (var item in files)
                {
					var info = new System.IO.FileInfo(item);
					Files.Add(new FileItem
					{
						FileName = info.Name,
						FileSize = $"{info.Length / 1024.0:F2} KB"
					});
				}
            }
            var result = await FilePicker.Default.PickAsync();
			if (result != null)
			{
				var fileSize = new System.IO.FileInfo(result.FullPath).Length;
				Files.Add(new FileItem
				{
					FileName = result.FileName,
					FileSize = $"{fileSize / 1024.0:F2} KB"
				});
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("错误", "选择文件失败: " + ex.Message, "确定");
		}
	}
	private void OnDeleteFileButtonClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		var fileItem = button?.CommandParameter as FileItem;
		if (fileItem != null)
		{
			Files.Remove(fileItem);
		}
	}
	private async void OnCreateFileButtonClicked(object sender, EventArgs e)
	{
		try
		{
			var documentsPath = FileSystem.Current.AppDataDirectory;
			var fileName = "example.dat";
			var filePath = Path.Combine(documentsPath, fileName);
			bool num2 = int.TryParse(SetFileSizeEntry.Text, out int num);
            if (!num2)
            {
				await DisplayAlert("失败", "文件大小可能不正确", "确定");
			}
            bool Y = SetFileSize(filePath,num);
            if (!Y)
            {
				await DisplayAlert("失败", "文件大小可能不正确", "确定");
				return;
            }
			await DisplayAlert("成功", $"文件已创建并保存在: {filePath}", "确定");
			Files.Add(new FileItem
			{
				FileName = fileName,
				FileSize = $"{num / 1024.0:F2} KB"
			});
		}
		catch (Exception ex)
		{
			await DisplayAlert("错误", "创建文件失败: " + ex.Message, "确定");
		}
	}
	public static bool SetFileSize(string filePath, long size)
	{
		if (size <= 0)
		{
			return false;
		}

		using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
		{
			fs.SetLength(size);
			return true;
		}
	}
}