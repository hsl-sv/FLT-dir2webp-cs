using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public readonly string[] EXT_AVAIL = { ".jpg", ".jpeg", ".png", ".gif", ".webp",
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
                string STRING_ERR = ", Error : ";
                int INT_ERR = 0;

                Parallel.ForEach(IMG_FILES,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.7))
                    },
                    file =>
                //foreach (string file in IMG_FILES)
                {
                    string extname = System.IO.Path.GetExtension(file);
                    string dirname = System.IO.Path.GetDirectoryName(file);
                    string filename = System.IO.Path.GetFileNameWithoutExtension(file);
                    string path = dirname + '\\' + filename + ".webp";
                    string errpath = dirname + '\\' + "__Error_log.txt";

                    if (extname == ".webp")
                    {
                        path = dirname + '\\' + "m_" + filename + ".webp";
                    }

                    try
                    {
                        MagickImageCollection mi = new MagickImageCollection(file);

                        // Skip animated webp
                        if (mi.Count > 1 && extname == ".webp")
                        {
                            mi.Dispose();
                            return; // continue; in Parallel.ForEach
                        }
                        // Prevent GIF black dots (disposing problem)
                        else if (mi.Count > 1)
                        {
                            mi.Coalesce();
                        }

                        foreach (MagickImage frame in mi)
                        {
                            while (frame.Height > 2160 || frame.Width > 3840)
                            {
                                frame.Resize(new Percentage(75));
                            }

                            // GIF frame size validation
                            if (extname == ".gif")
                            {
                                // force resize to first frame
                                if (mi[0].Width != frame.Width)
                                {
                                    MagickGeometry size = new MagickGeometry(mi[0].Width, mi[0].Height);
                                    size.IgnoreAspectRatio = true;
                                    frame.Resize(size);
                                }
                            }

                            frame.Quality = 80;
                            frame.Settings.SetDefine(MagickFormat.WebP, "lossless", "false");
                        }

                        mi.Write(path, MagickFormat.WebP);
                        mi.Dispose();

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        INT_ERR++;
                        System.IO.File.AppendAllTextAsync(errpath, file + Environment.NewLine);
                    }
                });

                this.Title = "Complete" + STRING_ERR + INT_ERR.ToString();
            }
        }
    }
}
