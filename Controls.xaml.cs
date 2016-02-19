﻿using System;
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
            orientOnRightmostScreen();
        }

        private void orientOnRightmostScreen()
        {
            int X_MARGIN = 300; // Margin in pixels between right side of the screen and right side of this
            int Y_MARGIN = 200;

            Screen[] sc = Screen.AllScreens;
            int nScreens = sc.Length;

            // Show this window on the rightmost screen
            if (nScreens > 1)
            {
                Screen rightmostScreen = sc[nScreens - 1];
                System.Drawing.Rectangle bounds = rightmostScreen.Bounds;
                this.Left = bounds.Right - this.Width - X_MARGIN;
                this.Top = bounds.Bottom - this.Height - Y_MARGIN;
                //this.Left = rightmostScreen.Bounds.Left;
                //this.Top = rightmostScreen.Bounds.Top;
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
