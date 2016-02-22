using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_WallpaperCrop_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Controls : Window
    {
        Preview preview;

        public Controls()
        {
            InitializeComponent();

            // Launch second window
            preview = new Preview(this);
            preview.Show();
        }

        ////////////////////////// Window Stuff ///////////////////////////////

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            orientOnScreen(Side.Right);
        }

        enum Side { Left, Right };
        bool onRightmostScreen;
        private void orientOnScreen(Side which)
        {
            int X_MARGIN = 300; // Margin in pixels between right side of the screen and right side of this
            int Y_MARGIN = 300;

            Screen[] sc = Screen.AllScreens;
            int nScreens = sc.Length;

            // Show this window on the given screen
            if (nScreens > 1)
            {
                Screen screen;
                if(which == Side.Left)
                {
                    screen = sc[1]; // TODO: Generalize for all monitor setups by using virtual screen
                    onRightmostScreen = false;
                } else
                {
                    screen = sc[nScreens - 1];
                    onRightmostScreen = true;
                }

                System.Drawing.Rectangle bounds = screen.Bounds;
                this.Left = bounds.Right - this.Width - X_MARGIN;
                this.Top = bounds.Bottom - this.Height - Y_MARGIN;
                //this.Left = screen.Bounds.Left;
                //this.Top = screen.Bounds.Top;
            }

            //// Maximize window
            //this.WindowState = WindowState.Maximized; // If I remember correctly maximizeOnRightmostScreen() must be called after window load for this to work.
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Topmost = true;
        }

        public void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Q)
            {
                Hide();
            }

            if (e.Key == Key.Escape)
            {
                Close();
            }

            if (e.Key == Key.Tab)
            {
                switchMonitors();
            }
        }

        public void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Q)
            {
                Show();
                Topmost = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            preview.Close();
        }

        private void switchMonitors()
        {

            if (onRightmostScreen)
            {
                // swap to left screen
                orientOnScreen(Side.Left);
            }
            else
            {
                // swap to right screen
                orientOnScreen(Side.Right);
            }
        }

        ///////////////////////////////// Control functionality stuff //////////////////////////////////////////

        private void importImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog imageChooser = new OpenFileDialog();
            imageChooser.Title = "Choose Image";
            imageChooser.Filter = "JPG|*.jpg|PNG|*.png";
            if (imageChooser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Uri imageUri = new Uri(imageChooser.FileName);
                BitmapImage bitmapimage = new BitmapImage(imageUri);
                image.Source = bitmapimage;
                preview.setImage(bitmapimage);
            }
        }

        private void exportWallpaper(object sender, RoutedEventArgs e)
        {
            SaveFileDialog wallpaperSaver = new SaveFileDialog();
            wallpaperSaver.Filter = "PNG|*.png";
            if (wallpaperSaver.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Extrude wallpaper
                Int32Rect bounds = preview.getBounds();
                BitmapSource wallpaper = extrudeTo(bounds, (BitmapSource)image.Source);

                // Save result
                string path = wallpaperSaver.FileName;
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wallpaper));
                using (FileStream stream = new FileStream(path, FileMode.Create))
                    encoder.Save(stream);
            }
        }

        private void centerImage(object sender, RoutedEventArgs e)
        {
            preview.centerImage();
        }

        ///////////////////////////////// Image processing stuff ////////////////////////////////////////////

        /* Crops the src to fit in the bounds, then fills any unoccupied pixels
         * with blackspace. Returns a bitmap that is the exact size of the bounds. */
        private BitmapSource extrudeTo(Int32Rect bounds, BitmapSource src)
        {
            // Crop src image
            Int32Rect cropRect = new Int32Rect();
            cropRect.X = Math.Max(0, bounds.X);
            cropRect.Y = Math.Max(0, bounds.Y);
            cropRect.Width = Math.Min(src.PixelWidth - cropRect.X, bounds.Width);
            cropRect.Height = Math.Min(bounds.Height, src.PixelHeight - cropRect.Y);

            // Pad with blackspace
            WriteableBitmap result = new WriteableBitmap(5760, 1080, src.DpiX, src.DpiY, src.Format, src.Palette);

            Int32Rect imagePos = new Int32Rect();
            imagePos.X = -Math.Min(bounds.X, 0);    imagePos.Y = -Math.Min(bounds.Y, 0);
            imagePos.Width = cropRect.Width;        imagePos.Height = cropRect.Height;

            int stride = src.PixelWidth * src.Format.BitsPerPixel / 8; // bytes per row of pixels in the image
            byte[] pixels = new byte[src.PixelHeight * stride];
            src.CopyPixels(cropRect, pixels, stride, 0);

            result.WritePixels(imagePos, pixels, stride, 0);

            return result;
        }
    }
}
