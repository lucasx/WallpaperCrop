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

        private void Mouse_RightButtonDown(object sender, MouseButtonEventArgs e)
        {
            centerImage();
        }


        ///////////////////////////////// Functionality stuff ///////////////////////////////////

        internal void setImage(BitmapSource bitmapImage)
        {
            image.Source = bitmapImage;

            // size image to actual size
            image.Width = bitmapImage.PixelWidth;
            image.Height = bitmapImage.PixelHeight;

            actualSize();
        }

        /* Center on canvas */
        internal void centerImage()
        {
            canvas.centerChild();
        }
        internal void actualSize()
        {
            canvas.resetScale();
            canvas.centerChild();
        }
    }
}
