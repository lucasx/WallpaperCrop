using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace WPF_WallpaperCrop_v2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Preview : Window
    {
        Controls controls;
        public Preview(Controls c)
        {
            InitializeComponent();

            controls = c;
            configureWindow();
        }

        ////////////////////////// Window Stuff ///////////////////////////////

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /* Sets up window frame with no borders and sets it to span
         * all screens.
         * Usage: Must be called before window is loaded in order to turn borders off. */
        private void configureWindow()
        {
            //int nMonitors = System.Windows.Forms.SystemInformation.MonitorCount;

            this.WindowStyle = WindowStyle.None; // Gets rid of top bar
            this.AllowsTransparency = true; // Gets rid of window-resize borders

            // span monitors
            this.Left = -1920;
            this.Width = SystemParameters.VirtualScreenWidth;// * 2 / 3.0;
            this.Top = 0;
            this.Height = SystemParameters.VirtualScreenHeight;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            controls.Window_KeyDown(sender, e);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            controls.Window_KeyUp(sender, e);
        }


        ///////////////////////////////// Functionality stuff ///////////////////////////////////

        internal void setImage(BitmapImage bitmapImage)
        {
            image.Source = bitmapImage;

            // size image to actual size
            image.Width = bitmapImage.PixelWidth;
            image.Height = bitmapImage.PixelHeight;

            // center on canvas
            Canvas.SetLeft(image, -image.Width / 2);
            Canvas.SetTop(image, -image.Height / 2);

            // center in viewer
            centerImage();
        }

        internal void centerImage()
        {
            viewer.Reset();
        }

        /* Returns a rectange in the image's coordinate system (origin at top left of image)
         * representing the bounds of the preview. */
        internal Int32Rect getBounds()
        {
            // Assumes the canvas origin is at the center of the window/screen
            Int32Rect r = new Int32Rect();
            r.X = -5760 / 2 - (int)Canvas.GetLeft(image);
            r.Y = -1080 / 2 - (int)Canvas.GetTop(image);
            r.Width = 5760;
            r.Height = 1080;
            return r;
        }
    }
}
