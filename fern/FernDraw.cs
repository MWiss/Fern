using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace Fern
{
    /*
     * This class draws a fractal fern when the constructor is called.
     * Better drawing: https://stackoverflow.com/questions/8713864/high-performance-graphics-using-the-wpf-visual-layer
     */
    class FernDraw
    {
        static double SIZE { get; } = 40.0; // Original size of the fern
        static double SIZE_THRESHOLD { get; } = 0.4; // Smallest size to stop drawing
        static double BRANCH_OFFSET { get; } = Math.PI / 2.5; // Angle between fronds and the fern stem
        static double BRANCH_RANDOM { get; } = 0.1; // The magnitude of randomness affecting each branch offset
        static double SIZE_RANDOM { get; } = 0.1; // The magnitude of randomness affecting each branch offset
        static double STEM_RANDOM { get; } = 0.2; // The magnitude of randomness affecting each stem offset
        static int PEN_SIZE { get; } = 4; // Size of the pen
        static int RIPPLE_SIZE { get; } = 2; // Some ripple constants
        static int RIPPLE_OPACITY { get; } = 80;
        static Random Random { get; } = new Random();
        double Growth { get; } // Stores the growth slider parameter
        double Angle { get; } // Stores the angle slider parameter
        double ColorDepth { get; } // Green color will be a factor of the max depth


        /* 
         * Fern constructor erases screen and draws a fern
         * Optimized graphics code: http://csharphelper.com/blog/2014/07/draw-a-barnsley-fern-fractal-in-c/
         *                          https://msdn.microsoft.com/en-us/library/system.windows.media.imaging.writeablebitmap(v=vs.110).aspx
         * 
         * Depth: layers of recursion for drawing the fern
         * Angle: the angle at which the fern and fronds curl
         * Growth: the rate at which the stem will grow
         * canvas: the canvas that the fern will be drawn on
         */
        public FernDraw(double depth, double angle, double growth, double count, Canvas canvas)
        {
            var width = (int)canvas.ActualWidth;
            var height = (int)canvas.ActualHeight;
            var fernX = width / 2;
            var fernY = height / 2;
            var fernRefX = fernX + 10;
            var fernRefY = fernY + 12;
            Growth = growth;
            Angle = angle;
            ColorDepth = 255 / depth;

            canvas.Children.Clear(); // delete old canvas contents
            var wb = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32, null);
            wb.Lock();
            // Define all the IDisposable objects
            using (var bmp = new Bitmap(wb.PixelWidth, wb.PixelHeight,
                                 wb.BackBufferStride,
                                 PixelFormat.Format32bppPArgb,
                                 wb.BackBuffer))
            using (var gr = Graphics.FromImage(bmp))
            using (var pen = new Pen(Color.FromArgb(255, 0, 0, 0), 2))
            using (var brush = new SolidBrush(Color.FromArgb(255, 153, 217, 235)))
            {
                // Draw the background
                Rectangle rect = new Rectangle(0, 0, width, height);
                gr.FillRectangle(brush, rect);

                // Draw the reflection of the fern
                for (var i = 0; i < count; i++)
                {
                    drawFern(gr, pen, 50, fernRefX, fernRefY, i * (2 * Math.PI / count) + Math.PI / 4, SIZE, depth, SIZE);
                }

                // Add some ripples so that we can meet primitive requirements
                for (var i = 0; i < Random.Next(2, 8); i++)
                {
                    drawRipple(gr, pen, Random.Next(width), Random.Next(height), Random.Next(150, 300));
                }

                // Draw our stem ripple for nice effect
                drawRipple(gr, pen, fernRefX, fernRefY, 100);

                // Draw the main fern with the stem connecting 
                for (var i = 0; i < count; i++)
                {
                    pen.Color = Color.Black;
                    pen.Width = (float)PEN_SIZE;
                    gr.DrawLine(pen, fernX, fernY, fernRefX, fernRefY);
                    drawFern(gr, pen, 255, fernX, fernY, i * (2 * Math.PI / count) + Math.PI / 4, SIZE, depth, SIZE);
                }
            }

            wb.AddDirtyRect(new Int32Rect(0, 0, (int)wb.Width, (int)wb.Height));
            wb.Unlock();
            canvas.Background = new System.Windows.Media.ImageBrush(wb);
        }

        /*
         * drawFern: draws a fern at the given location and then draws a bunch of other ferns/fronds out of it
         */
        private void drawFern(Graphics gr, Pen pen, int opacity, float x, float y, double angle, double size, double depth, double stemSize)
        {
            // Base cases
            if (depth < 1 || size < SIZE_THRESHOLD) return;
            // Calculate the two new points of the fern
            float x1 = (float)(x + Math.Cos(angle) * size);
            float y1 = (float)(y - Math.Sin(angle) * size);
            float x2 = (float)(x1 + Math.Cos(angle) * (size / 8));
            float y2 = (float)(y1 - Math.Sin(angle) * (size / 8));
            // Make updates to the pen
            pen.Color = Color.FromArgb(opacity, 0, (int)(255 - (ColorDepth * depth)), 0); // color gets more green 
            pen.Width = (float)((PEN_SIZE * size) / stemSize); // stem gets thinner
            // Draw the stem;
            gr.DrawCurve(pen, new PointF[] { new PointF(x, y), new PointF(x1, y1), new PointF(x2, y2) });
            // Continue to draw two fronds with 1 randomness
            drawFern(gr, pen, opacity, x1, y1, angle + (BRANCH_OFFSET - Angle + randomness(BRANCH_RANDOM)), size / 3, depth - 1, size / 2);
            drawFern(gr, pen, opacity, x2, y2, angle + (-BRANCH_OFFSET - Angle + randomness(BRANCH_RANDOM)), size / 3, depth - 1, size / 2);
            // Continue to draw the stem with 2 randomness
            drawFern(gr, pen, opacity, x2, y2, angle - Angle + randomness(STEM_RANDOM), (size / (2 - Growth)) + randomness(SIZE_RANDOM), depth, stemSize);
        }

        /*
         * randomness: generates some randomness specifically for drawing the fern
         */
        private double randomness(double factor)
        {
            return factor * (Random.NextDouble() - 0.5);
        }

        /*
         * drawRipple: draws a ripple effect 
         */
        private void drawRipple(Graphics gr, Pen pen, int x, int y, int size)
        {
            var rippleRect = new Rectangle(x - size / 2, y - size / 2, size, size);
            pen.Width = RIPPLE_SIZE;
            pen.Color = Color.FromArgb(RIPPLE_OPACITY, 255, 255, 255);
            // Draw the first ripple
            gr.DrawEllipse(pen, rippleRect);
            // Draw some more smaller, random ripples
            for (int i = Random.Next(1, 5); i < 5; i++)
            {
                if (Random.NextDouble() > 0.5)
                {
                    rippleRect.Width /= i;
                    rippleRect.Height /= i;
                }
                else
                {
                    rippleRect.Width -= i * 10;
                    rippleRect.Height -= i * 10;
                }
                rippleRect.X = x - rippleRect.Width / 2;
                rippleRect.Y = y - rippleRect.Width / 2;
                pen.Color = Color.FromArgb(RIPPLE_OPACITY / i, 255, 255, 255);
                gr.DrawEllipse(pen, rippleRect);
            }
        }
    }
}
