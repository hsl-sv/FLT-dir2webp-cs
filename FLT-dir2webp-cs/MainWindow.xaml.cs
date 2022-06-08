using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

using ImageMagick;

namespace FLT_dir2webp_cs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly string[] EXT_AVAIL = { ".jpg", ".jpeg", ".png", ".gif",
        ".apng", ".bmp", ".dds", ".jfif", ".pcx", ".svg", ".tiff", ".tif", ".tga"};
        public static List<string> IMG_FILES;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    tbxPath.Text = fbd.SelectedPath;

                    List<string> imgFiles = new List<string>();
                    string[] fList = Directory.GetFiles(fbd.SelectedPath, "*.*", SearchOption.AllDirectories);

                    foreach (string file in fList)
                    {
                        if (EXT_AVAIL.Any(f => f == System.IO.Path.GetExtension(file.ToLower())))
                        {
                            imgFiles.Add(file);
                        }
                    }

                    this.Title = "Files count : " + imgFiles.Count.ToString();

                    IMG_FILES = imgFiles;
                }
            }
        }

        private void Run_Click(object sender, RoutedEventArgs e)
        {
            if (IMG_FILES != null)
            {
                int progMax = IMG_FILES.Count;
                int progCur = 0;

                foreach (string file in IMG_FILES)
                {
                    string dirname = System.IO.Path.GetDirectoryName(file);
                    string filename = System.IO.Path.GetFileNameWithoutExtension(file);

                    MagickImageCollection mi = new MagickImageCollection(file);
                    
                    foreach (MagickImage frame in mi)
                    {
                        frame.Quality = 90;
                        frame.Settings.SetDefine(MagickFormat.WebP, "lossless", "false");
                    }

                    Task task = mi.WriteAsync(dirname + '\\' + filename + ".webp", MagickFormat.WebP);
                    task.ContinueWith(t =>
                    {
                        mi.Dispose();
                    });

                    this.Title = (++progCur).ToString() + "/" + progMax.ToString();
                }
            }
        }
    }
}
