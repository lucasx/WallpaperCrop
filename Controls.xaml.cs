using System;
using System.Collections.Generic;
using System.Linq;
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
            int Y_MARGIN = 200;

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

        private void chooseImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog chooseImage = new OpenFileDialog();
            chooseImage.Title = "Choose Image";
            chooseImage.Filter = "JPG|*.jpg|PNG|*.png";
            if (chooseImage.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BitmapImage bitmapimage = new BitmapImage(new Uri(chooseImage.SafeFileName, UriKind.Relative));
                image.Source = bitmapimage;
                preview.setImage(bitmapimage);
            }
        }

        private void centerImage(object sender, RoutedEventArgs e)
        {
            preview.centerImage();
        }
    }
}
