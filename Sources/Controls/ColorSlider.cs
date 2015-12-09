// AForge Controls Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © AForge.NET, 2007-2011
// contacts@aforgenet.com
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;

namespace AForge.Controls
{
    using Point = System.Drawing.Point;

    /// <summary>
    /// Color slider control.
    /// </summary>
    /// 
    /// <remarks><para>The control represent a color slider, which allows selecting
    /// one or two values in the [0, 255] range. The application of this control
    /// includes mostly areas of image processing and computer vision, where it is required
    /// to select color threshold or ranges for different type of color filtering.</para>
    /// 
    /// <para>Depending on the control's <see cref="Type"/>, it has different look and may suite
    /// different tasks. See documentation to <see cref="ColorSliderType"/> for information
    /// about available type and possible control's looks.</para>
    /// </remarks>
    /// 
    public class ColorSlider : Control
    {
        private Pen blackPen = new Pen(Color.Black, 1);
        private Color startColor = Color.Black;
        private Color endColor = Color.White;
        private Color fillColor = Color.Black;
        private ColorSliderType type = ColorSliderType.Gradient;
        private bool doubleArrow = true;
        private Bitmap arrow;
        private int min = 0, max = 255;
        private int width = 256, height = 10;
        private int trackMode = 0;
        private int dx;

        /// <summary>
        /// An event, to notify about changes of <see cref="Min"/> or <see cref="Max"/> properties.
        /// </summary>
        /// 
        /// <remarks><para>The event is fired after changes of <see cref="Min"/> or <see cref="Max"/> property,
        /// which is caused by user dragging the corresponding control’s arrow (slider).</para>
        /// </remarks>
        /// 
        public event EventHandler ValuesChanged;

        /// <summary>
        /// Enumeration of color slider types.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>The <see cref="ColorSliderType.Gradient"/> slider's type supposes the control's
        /// background filled with gradient startting from <see cref="StartColor"/> color and ending
        /// with <see cref="EndColor"/> color. The <see cref="FillColor"/> color does not have
        /// impact on control's look.</para>
        /// 
        /// <para>This type allows as one-arrow, as two-arrows control.</para>
        /// 
        /// <para><b>Sample control's look:</b></para>
        /// <img src="img/controls/slider_gradient.png" width="258" height="17" />    
        /// 
        /// <para>The <see cref="ColorSliderType.InnerGradient"/> slider's type supposes the control's
        /// background filled with gradient startting from <see cref="StartColor"/> color and ending
        /// with <see cref="EndColor"/> color. In addition the areas, which are outside of
        /// [<see cref="Min"/>, <see cref="Max"/>] range, are filled with <see cref="FillColor"/> color.</para>
        /// 
        /// <para>This type allows only two-arrows control.</para>
        /// 
        /// <para><b>Sample control's look:</b></para>
        /// <img src="img/controls/slider_inner_gradient.png" width="258" height="17" />
        /// 
        /// <para>The <see cref="ColorSliderType.OuterGradient"/> slider's type supposes the
        /// control's background filled with gradient startting from <see cref="StartColor"/> color
        /// and ending with <see cref="EndColor"/> color. In addition the area, which is inside of
        /// [<see cref="Min"/>, <see cref="Max"/>] range, is filled with <see cref="FillColor"/> color.</para>
        /// 
        /// <para>This type allows only two-arrows control.</para>
        /// 
        /// <para><b>Sample control's look:</b></para>
        /// <img src="img/controls/slider_outer_gradient.png" width="258" height="17" />
        /// 
        /// <para>The <see cref="ColorSliderType.Threshold"/> slider's type supposes filling areas
        /// outside of [<see cref="Min"/>, <see cref="Max"/>] range with <see cref="StartColor"/> and
        /// inside the range with <see cref="EndColor"/>. The <see cref="FillColor"/> color does not
        /// have impact on control's look.</para>
        /// 
        /// <para>This type allows as one-arrow, as two-arrows control.</para>
        /// 
        /// <para><b>Sample control's look:</b></para>
        /// <img src="img/controls/slider_threshold.png" width="258" height="17" />
        /// </remarks>
        ///
        public enum ColorSliderType
        {
            /// <summary>
            /// Gradient color slider type.
            /// </summary>
            Gradient,

            /// <summary>
            /// Inner gradient color slider type.
            /// </summary>
            InnerGradient,

            /// <summary>
            /// Outer gradient color slider type.
            /// </summary>
            OuterGradient,

            /// <summary>
            /// Threshold color slider type.
            /// </summary>
            Threshold
        }

        /// <summary>
        /// Start color for gradient filling.
        /// </summary>
        ///
        /// <remarks>See documentation to <see cref="ColorSliderType"/> enumeration for information about
        /// the usage of this property.</remarks>
        ///
        [DefaultValue(typeof (Color), "Black")]
        public Color StartColor
        {
            get { return this.startColor; }
            set
            {
                this.startColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// End color for gradient filling.
        /// </summary>
        ///
        /// <remarks>See documentation to <see cref="ColorSliderType"/> enumeration for information about
        /// the usage of this property.</remarks>
        ///
        [DefaultValue(typeof (Color), "White")]
        public Color EndColor
        {
            get { return this.endColor; }
            set
            {
                this.endColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Color to fill control's background in filtered zones.
        /// </summary>
        ///
        /// <remarks>See documentation to <see cref="ColorSliderType"/> enumeration for information about
        /// the usage of this property.</remarks>
        ///
        [DefaultValue(typeof (Color), "Black")]
        public Color FillColor
        {
            get { return this.fillColor; }
            set
            {
                this.fillColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Specifies control's type.
        /// </summary>
        /// 
        /// <remarks>See documentation to <see cref="ColorSliderType"/> enumeration for information about
        /// the usage of this property.</remarks>
        ///
        [DefaultValue(ColorSliderType.Gradient)]
        public ColorSliderType Type
        {
            get { return this.type; }
            set
            {
                this.type = value;
                if ((this.type != ColorSliderType.Gradient) && (this.type != ColorSliderType.Threshold))
                    this.DoubleArrow = true;
                Invalidate();
            }
        }

        /// <summary>
        /// Minimum selected value, [0, 255].
        /// </summary>
        /// 
        [DefaultValue(0)]
        public int Min
        {
            get { return this.min; }
            set
            {
                this.min = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Maximum selected value, [0, 255].
        /// </summary>
        /// 
        [DefaultValue(255)]
        public int Max
        {
            get { return this.max; }
            set
            {
                this.max = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Single or Double arrow slider control.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies if the slider has one or two selection arrows (sliders).</para>
        /// 
        /// <para>The single arrow allows only to specify one value, which is set by <see cref="Min"/>
        /// property. The single arrow slider is useful for applications, where it is required to select
        /// color threshold, for example.</para>
        /// 
        /// <para>The double arrow allows to specify two values, which are set by <see cref="Min"/>
        /// and <see cref="Max"/> properties. The double arrow slider is useful for applications, where it is
        /// required to select filtering color range, for example.</para>
        /// </remarks>
        /// 
        [DefaultValue(true)]
        public bool DoubleArrow
        {
            get { return this.doubleArrow; }
            set
            {
                this.doubleArrow = value;
                if ((!this.doubleArrow) && (this.type != ColorSliderType.Threshold))
                {
                    this.Type = ColorSliderType.Gradient;
                }
                Invalidate();
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ColorSlider"/> class.
        /// </summary>
        /// 
        public ColorSlider()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);

            // load arrow bitmap
            Assembly assembly = GetType().Assembly;
            this.arrow = new Bitmap(assembly.GetManifestResourceStream("AForge.Controls.Resources.arrow.bmp"));
            this.arrow.MakeTransparent(Color.FromArgb(255, 255, 255));
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// 
        /// <param name="disposing">Specifies if disposing was invoked by user's code.</param>
        /// 
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.blackPen.Dispose();
                this.arrow.Dispose();
            }
            base.Dispose(disposing);
        }

        // Init component
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // ColorSlider
            // 
            this.Paint += new PaintEventHandler(ColorSlider_Paint);
            this.MouseMove += new MouseEventHandler(ColorSlider_MouseMove);
            this.MouseDown += new MouseEventHandler(ColorSlider_MouseDown);
            this.MouseUp += new MouseEventHandler(ColorSlider_MouseUp);
            ResumeLayout(false);
        }

        // Paint control
        private void ColorSlider_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rc = this.ClientRectangle;
            Brush brush;
            int x = (rc.Right - this.width)/2;
            int y = 2;

            // draw rectangle around the control
            g.DrawRectangle(this.blackPen, x - 1, y - 1, this.width + 1, this.height + 1);

            switch (this.type)
            {
                case ColorSliderType.Gradient:
                case ColorSliderType.InnerGradient:
                case ColorSliderType.OuterGradient:

                    // create gradient brush
                    brush = new LinearGradientBrush(new Point(x, 0), new Point(x + this.width, 0), this.startColor, this.endColor);
                    g.FillRectangle(brush, x, y, this.width, this.height);
                    brush.Dispose();

                    // check type
                    if (this.type == ColorSliderType.InnerGradient)
                    {
                        // inner gradient
                        brush = new SolidBrush(this.fillColor);

                        if (this.min != 0)
                        {
                            g.FillRectangle(brush, x, y, this.min, this.height);
                        }
                        if (this.max != 255)
                        {
                            g.FillRectangle(brush, x + this.max + 1, y, 255 - this.max, this.height);
                        }
                        brush.Dispose();
                    }
                    else if (this.type == ColorSliderType.OuterGradient)
                    {
                        // outer gradient
                        brush = new SolidBrush(this.fillColor);
                        // fill space between min & max with color 3
                        g.FillRectangle(brush, x + this.min, y, this.max - this.min + 1, this.height);
                        brush.Dispose();
                    }
                    break;
                case ColorSliderType.Threshold:
                    // 1 - fill with color 1
                    brush = new SolidBrush(this.startColor);
                    g.FillRectangle(brush, x, y, this.width, this.height);
                    brush.Dispose();
                    // 2 - fill space between min & max with color 2
                    brush = new SolidBrush(this.endColor);
                    g.FillRectangle(brush, x + this.min, y, this.max - this.min + 1, this.height);
                    brush.Dispose();
                    break;
            }


            // draw arrows
            x -= 4;
            y += 1 + this.height;

            g.DrawImage(this.arrow, x + this.min, y, 9, 6);
            if (this.doubleArrow)
                g.DrawImage(this.arrow, x + this.max, y, 9, 6);
        }

        // On mouse down
        private void ColorSlider_MouseDown(object sender, MouseEventArgs e)
        {
            int x = (this.ClientRectangle.Right - this.width)/2 - 4;
            int y = 3 + this.height;

            // check Y coordinate
            if ((e.Y >= y) && (e.Y < y + 6))
            {
                // check X coordinate
                if ((e.X >= x + this.min) && (e.X < x + this.min + 9))
                {
                    // left arrow
                    this.trackMode = 1;
                    this.dx = e.X - this.min;
                }
                if ((this.doubleArrow) && (e.X >= x + this.max) && (e.X < x + this.max + 9))
                {
                    // right arrow
                    this.trackMode = 2;
                    this.dx = e.X - this.max;
                }

                if (this.trackMode != 0)
                    this.Capture = true;
            }
        }

        // On mouse up
        private void ColorSlider_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.trackMode != 0)
            {
                // release capture
                this.Capture = false;
                this.trackMode = 0;

                // notify client
                if (this.ValuesChanged != null)
                    this.ValuesChanged(this, new EventArgs());
            }
        }

        // On mouse move
        private void ColorSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.trackMode != 0)
            {
                if (this.trackMode == 1)
                {
                    // left arrow tracking
                    this.min = e.X - this.dx;
                    this.min = Math.Max(this.min, 0);
                    this.min = Math.Min(this.min, this.max);
                }
                if (this.trackMode == 2)
                {
                    // right arrow tracking
                    this.max = e.X - this.dx;
                    this.max = Math.Max(this.max, this.min);
                    this.max = Math.Min(this.max, 255);
                }

                // repaint control
                Invalidate();
            }
        }
    }
}