﻿using System;
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
                string STRING_ERR = ", Error : ";
                int INT_ERR = 0;

                Parallel.ForEach(IMG_FILES,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 2.0))
                    },
                    file =>
                {
                    string dirname = System.IO.Path.GetDirectoryName(file);
                    string filename = System.IO.Path.GetFileNameWithoutExtension(file);
                    string path = dirname + '\\' + filename + ".webp";
                    string errpath = dirname + '\\' + "__Error_log.txt";

                    try
                    {
                        MagickImageCollection mi = new MagickImageCollection(file);

                        foreach (MagickImage frame in mi)
                        {
                            while (frame.Height > 2160 || frame.Width > 3840)
                            {
                                frame.Resize(new Percentage(80));
                            }

                            frame.Quality = 80;
                            frame.Settings.SetDefine(MagickFormat.WebP, "lossless", "false");
                        }

                        mi.Write(path, MagickFormat.WebP);
                        mi.Dispose();
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                        INT_ERR++;
                        System.IO.File.AppendAllTextAsync(errpath, file + Environment.NewLine);
                    }
                });

                this.Title = "Complete" + STRING_ERR + INT_ERR.ToString();
            }
        }
    }
}
