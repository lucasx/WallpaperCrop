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

        /* Constants */
        private double SCALE_SCROLL_INCREMENT = .1;

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
        }

        // CropCanvas ignores all children except the first child.
        public void initChild(object sender, RoutedEventArgs e)
        {
            if (InternalChildren.Count < 1) return;

            child = InternalChildren[0];

            TransformGroup group = new TransformGroup();
            ScaleTransform st = new ScaleTransform();
            group.Children.Add(st);
            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);

            child.RenderTransform = group;
            child.RenderTransformOrigin = new Point(0.0, 0.0);
        }

        /* Computes the real, rendered bounds of the child in pixels,
         * relative to this. Takes render transforms into account. */
        public Rect EffectiveChildBounds()
        {
            FrameworkElement fechild = (FrameworkElement)child;
            return child.RenderTransform.TransformBounds( new Rect(new Size(fechild.Width, fechild.Height)) );
        }

        #region transform events

        public void centerChild()
        {
            if (child != null)
            {
                Rect bounds = EffectiveChildBounds();
                double x = (ActualWidth - bounds.Width) / 2;
                double y = (ActualHeight - bounds.Height) / 2;

                var tt = GetTranslateTransform(child);
                tt.X = x;
                tt.Y = y;
            }
        }

        public void resetScale()
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;
            }
        }

        private void scaleChild(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = e.Delta > 0 ? SCALE_SCROLL_INCREMENT : -SCALE_SCROLL_INCREMENT;
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

                    // keep within bounds
                    Rect bounds = EffectiveChildBounds();

                    if (bounds.Width > ActualWidth)
                    {
                        if (x1 > 0) x1 = 0; // left
                        if (x1 < ActualWidth - bounds.Width) // right
                            x1 = ActualWidth - bounds.Width;
                        tt.X = x1;
                    }

                    if (bounds.Height > ActualHeight)
                    {
                        if (y1 > 0) y1 = 0; // top
                        if (y1 < ActualHeight - bounds.Height) // bottom
                            y1 = ActualHeight - bounds.Height;
                        tt.Y = y1;
                    }
                }
            }
        }

        #endregion
    }
}
