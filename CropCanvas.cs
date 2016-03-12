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
        
        /* The rectangle representing the bounds of the child element.
         * Must be kept up to date - methods that affect the bounds must
         * manually call the updateChildBounds() method. */
        private Rect cbounds; 

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

        /* Recomputes the real, rendered bounds of the child in pixels,
         * relative to this. Takes render transforms into account. */
        public void updateChildBounds()
        {
            FrameworkElement fechild = (FrameworkElement)child;
            cbounds = child.RenderTransform.TransformBounds( new Rect(new Size(fechild.Width, fechild.Height)) );
        }

        #region transfrom primitives

        /* Given the width of an element within this canvas,
         * returns the x coordinate the element should be at
         * in order to be centered on the canvas. */
        private double centerX(double width)
        {
            return (ActualWidth - width) / 2;
        }
        /* Given the height of an element within this canvas,
         * returns the y coordinate the element should be at
         * in order to be centered on the canvas. */
        private double centerY(double height)
        {
            return (ActualHeight - height) / 2;
        }

        /* If x is outside the range from ActualWidth - width to 0,
         * x is set to the nearest edge of that range, then x is returned. */
        private double clipX(double x, double width)
        {
            if (x > 0) x = 0; // left
            if (x < ActualWidth - width) // right
                x = ActualWidth - width;
            return x;
        }
        /* If y is outside the range from ActualHeight - height to 0,
         * y is set to the nearest edge of that range, then y is returned. */
        private double clipY(double y, double height)
        {
            if (y > 0) y = 0; // top
            if (y < ActualHeight - height) // bottom
                y = ActualHeight - height;
            return y;
        }

        #endregion

        #region transform macros

        public void centerChild()
        {
            if (child != null)
            {
                var tt = GetTranslateTransform(child);

                tt.X = centerX(cbounds.Width);
                tt.Y = centerY(cbounds.Height);

                updateChildBounds();
            }
        }

        public void resetScale()
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                updateChildBounds();
            }
        }

        /* If the given rectangle spans neither the width nor the 
         * height dimensions of the canvas, increases the zoom 
         * ungreedily so that the rectangle spans at least one
         * of the dimensions. Returns the minimum zoom that 
         * satisfies this constraint. */
        private void stretchTo1Dim(ScaleTransform st)
        {
            if (cbounds.Width < ActualWidth && cbounds.Height < ActualHeight)
            {
                double widthFactor = ActualWidth / cbounds.Width;
                double heightFactor = ActualHeight / cbounds.Height;

                double minFactor = Math.Min(widthFactor, heightFactor);
                st.ScaleX *= minFactor; st.ScaleY *= minFactor;

                updateChildBounds();
            }
        }

        /* Adjusts the translate transform so that the given rectangle
         * covers as much of the canvas as possible. If there must be
         * blackspace in a particular direction, splits the blackspace
         * equally on each side of the rectangle. Does nothing
         * if the rectangle already covers the entire canvas. */
        private void minimizeBlackspace(TranslateTransform tt)
        {
            // width
            if (cbounds.Width > ActualWidth) tt.X = clipX(cbounds.X, cbounds.Width);
            else tt.X = centerX(cbounds.Width);

            // height
            if (cbounds.Height > ActualHeight) tt.Y = clipY(cbounds.Y, cbounds.Height);
            else tt.Y = centerY(cbounds.Height);

            updateChildBounds();
        }

        #endregion

        #region transform events

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
                
                upperZoomLimit(st); // Upper zoom limit

                tt.X = abosuluteX - relative.X * st.ScaleX;
                tt.Y = abosuluteY - relative.Y * st.ScaleY;

                updateChildBounds();

                lowerZoomLimit(st, tt); // Lower zoom limit
            }
        }

        /* Clips scale to 1 or less. 
         * Simple arithmetic, no updates needed. */
        private void upperZoomLimit(ScaleTransform st)
        {
            if (st.ScaleX > 1) st.ScaleX = 1;
            if (st.ScaleY > 1) st.ScaleY = 1;
        }

        /* Rescales and positions child to fit properly in the canvas.
         * Enforces the limit that the child must be at least large enough
         * to span one direction of the canvas - either width or height.
         * Assumes the global bounds variable is up to date. */
        private void lowerZoomLimit(ScaleTransform st, TranslateTransform tt)
        {
            stretchTo1Dim(st);
            minimizeBlackspace(tt);
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

                    // keep within cbounds
                    if (cbounds.Width > ActualWidth)
                        tt.X = clipX(x1, cbounds.Width);

                    if (cbounds.Height > ActualHeight)
                        tt.Y = clipY(y1, cbounds.Height);

                    updateChildBounds();
                }
            }
        }

        #endregion
    }
}
