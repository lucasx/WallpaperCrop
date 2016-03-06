using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPF_WallpaperCrop_v2
{
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private UIElement endContent = null;
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

        /* Get the real render bounds of the given element, taking
         * render transforms into account. */
        public Rect getEffectiveBounds(UIElement element)
        {
            FrameworkElement fElem = (FrameworkElement)element;
            return element.RenderTransform.TransformBounds( new Rect(new Size(fElem.ActualWidth, fElem.ActualHeight)) );
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.MouseWheel += child_MouseWheel;
                this.MouseLeftButtonDown += child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += child_MouseLeftButtonUp;
                this.MouseMove += child_MouseMove;
                this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                  child_PreviewMouseRightButtonDown);
            }
        }

        /* Notify of the actual content this element is holding.
         * Implications:
         *      pan limits based on actual content size
         */
        internal void setEndContent(UIElement e)
        {
            endContent = e;
        }

        public void Reset()
        {
            if (child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
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

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //this.Reset();
            MessageBox.Show(child.RenderSize.ToString());
            //Image image = (Image)endContent;
            Canvas canvas = (Canvas)child;
            MessageBox.Show("canvas.Height: " + canvas.Height);
            MessageBox.Show("canvas.ActualHeight: " + canvas.ActualHeight);
            MessageBox.Show("canvas.DesiredSize.Height: " + canvas.DesiredSize.Height);
            MessageBox.Show("canvas.RenderSize.Height: " + canvas.RenderSize.Height);
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);
                    double xf = origin.X - v.X;
                    double yf = origin.Y - v.Y;

                    // Keep within bounds
                    Rect bounds = getEffectiveBounds(child);

                    if (bounds.Width < child.RenderSize.Width)
                    {
                        xf = Math.Min(xf, bounds.Width / 2); // don't pan past edge of image
                        tt.X = xf;
                    }

                    if (bounds.Height < child.RenderSize.Height)
                    {
                        yf = Math.Min(yf, bounds.Height / 2);
                        tt.Y = yf;
                    }
                }
            }
        }

        #endregion
    }
}