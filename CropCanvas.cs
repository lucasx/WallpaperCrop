using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPF_WallpaperCrop_v2
{
    class CropCanvas : Canvas
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public CropCanvas() : base()
        {
            // add transforms to child
            this.Loaded += new RoutedEventHandler(initChild);

            // init transform handlers
            this.MouseWheel += scaleChild;
            this.MouseLeftButtonDown += startPan;
            this.MouseLeftButtonUp += endPan;
            this.MouseMove += panChild;
            this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                child_PreviewMouseRightButtonDown);
        }

        private void initChild(object sender, RoutedEventArgs e)
        {
            if (InternalChildren.Count > 0)
                Initialize(InternalChildren[0]);
        }

        public void Initialize(UIElement element)
        {
            child = element;

            TransformGroup group = new TransformGroup();
            ScaleTransform st = new ScaleTransform();
            group.Children.Add(st);
            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);

            child.RenderTransform = group;
            child.RenderTransformOrigin = new Point(0.0, 0.0);
        }

        public void Center()
        {
            if (child != null)
            {
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        public void ActualSize()
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;
            }
        }

        #region transform events

        private void scaleChild(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                    return;

                Point relative = e.GetPosition(child);
                double abosuluteX;
                double abosuluteY;

                abosuluteX = relative.X * st.ScaleX + tt.X;
                abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = abosuluteX - relative.X * st.ScaleX;
                tt.Y = abosuluteY - relative.Y * st.ScaleY;
            }
        }

        private void startPan(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void endPan(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("right button pressed");
        }

        private void panChild(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);
                    double x1 = origin.X - v.X;
                    double y1 = origin.Y - v.Y;

                    tt.X = x1;
                    tt.Y = y1;
                }
            }
        }

        #endregion
    }
}
