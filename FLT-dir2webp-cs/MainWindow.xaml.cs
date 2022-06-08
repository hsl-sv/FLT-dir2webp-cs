using System;
using System.Collections.Generic;
using System.IO;
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

using ImageMagick;

namespace FLT_dir2webp_cs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //string filepath = Directory.GetCurrentDirectory();
            //filepath = "C:\\Users\\skycr\\OneDrive\\Documents\\Codes\\FLT-dir2webp-cs\\FLT-dir2webp-cs\\test\\";

            //ConvertToWebp(filepath);
        }

        private void ConvertToWebp(string path)
        {
            string[] fList = Directory.GetFiles(path);

            foreach (string file in fList)
            {
                string ext = System.IO.Path.GetExtension(file);
                string dirname = System.IO.Path.GetDirectoryName(file);
                string filename = System.IO.Path.GetFileNameWithoutExtension(file);

                MagickImageCollection mi = new MagickImageCollection(file);
                mi.Write(dirname + '\\' + filename + ".webp", MagickFormat.WebP);
                mi.Dispose();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
