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
            configureDragging();
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

        /// Functionality to move image by dragging mouse ///
        Point basePoint;
        bool dragging = false;

        /* Register mouse event handlers */
        public void configureDragging()
        {
            image.MouseLeftButtonDown += (ss, ee) =>
            {
                dragging = true;
                basePoint = ee.GetPosition(this);
                image.CaptureMouse();
            };

            image.MouseMove += (ss, ee) =>
            {
                if (dragging == true)
                {
                    // Get distance moved
                    Point newPoint = ee.GetPosition(this);
                    Point diff = new Point(newPoint.X - basePoint.X, newPoint.Y - basePoint.Y);

                    // Update image
                    if (image.ActualWidth > 5760)
                    {
                        double newLeft = Canvas.GetLeft(image) + diff.X;
                        double newRight = newLeft + image.ActualWidth;
                        if (-5760 / 2 < newLeft)
                        {
                            newLeft = -5760 / 2;
                        }
                        else
                        {
                            if (newRight < 5760 / 2)
                            {
                                newLeft = 5760 / 2 - image.ActualWidth;
                            }
                            else
                            {
                                basePoint.X = newPoint.X;
                            }
                        }
                        Canvas.SetLeft(image, newLeft);
                    }

                    if (image.ActualHeight > 1080)
                    {
                        double newTop = Canvas.GetTop(image) + diff.Y;
                        double newBottom = newTop + image.ActualHeight;
                        if (-1080 / 2 < newTop)
                        {
                            newTop = -1080 / 2;
                        }
                        else
                        {
                            if (newBottom < 1080 / 2)
                            {
                                newTop = 1080 / 2 - image.ActualHeight;
                            }
                            else
                            {
                                basePoint.Y = newPoint.Y;
                            }
                        }
                        Canvas.SetTop(image, newTop);
                    }

                }
            };

            image.MouseLeftButtonUp += (ss, ee) => {
                image.ReleaseMouseCapture();
                dragging = false;
            };
        }

        internal void setImage(BitmapImage bitmapImage)
        {
            image.Source = bitmapImage;

            // size image to actual size
            image.Width = bitmapImage.PixelWidth;
            image.Height = bitmapImage.PixelHeight;

            centerImage();
        }

        internal void centerImage()
        {
            Canvas.SetLeft(image, -image.Width / 2);
            Canvas.SetTop(image, -image.Height / 2);
        }

        internal Int32Rect getCropRect()
        {
            // Assumes canvas is based at the center of the screen and the focus of the canvas is its center
            Int32Rect r = new Int32Rect();
            r.X = -5760 / 2 - (int)Canvas.GetLeft(image);
            r.Y = -1080 / 2 - (int)Canvas.GetTop(image);
            r.Width = 5760;
            r.Height = 1080;
            return r;
        }
    }
}
