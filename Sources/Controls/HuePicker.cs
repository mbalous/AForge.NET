// AForge Controls Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © AForge.NET, 2005-2011
// contacts@aforgenet.com
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using AForge.Imaging;

namespace AForge.Controls
{
    /// <summary>
    /// Hue picker control.
    /// </summary>
    /// 
    /// <remarks><para>The control allows selecting hue value (or range) from HSL color space. Hue values
    /// are integer values in the [0, 359] range.</para>
    /// 
    /// <para>If control's type is set to <see cref="HuePickerType.Value"/>, then it allows selecting single
    /// hue value and looks like this:<br />
    /// <img src="img/controls/hue_picker1.png" width="220" height="220" />
    /// </para>
    /// 
    /// <para>If control's type is set to <see cref="HuePickerType.Range"/>, then it allows selecting range
    /// of hue values and looks like this:<br />
    /// <img src="img/controls/hue_picker2.png" width="220" height="220" />
    /// </para>
    /// </remarks>
    /// 
    public class HuePicker : Control
    {
        private HuePickerType type = HuePickerType.Value;

        private Pen blackPen;
        private Brush blackBrush;
        private Pen whitePen;
        private Brush whiteBrush;

        private System.Drawing.Point ptCenter = new System.Drawing.Point(0, 0);
        private System.Drawing.Point ptMin = new System.Drawing.Point(0, 0);
        private System.Drawing.Point ptMax = new System.Drawing.Point(0, 0);
        private int trackMode = 0;

        private int min = 0;
        private int max = 359;

        /// <summary>
        /// An event, to notify about changes of <see cref="Min"/> or <see cref="Max"/> properties.
        /// </summary>
        /// 
        /// <remarks><para>The event is fired after changes of its <see cref="Value"/>, <see cref="Min"/> or
        /// <see cref="Max"/> properties, which is caused by user dragging the corresponding hue picker's bullets.</para>
        /// </remarks>
        /// 
        public event EventHandler ValuesChanged;

        /// <summary>
        /// Enumeration of hue picker types.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>The <see cref="HuePickerType.Value"/> type provides single bullet to drag, which allows
        /// selecting single hue value. The value is accessible through <see cref="Value"/> property.</para>
        /// 
        /// <para>The <see cref="HuePickerType.Range"/> type provides two bullets to drag, which correspond
        /// to minimum and maximum values of the hue range. These values are accessible through
        /// <see cref="Min"/> and <see cref="Max"/> properties.</para>
        /// </remarks>
        /// 
        public enum HuePickerType
        {
            /// <summary>
            /// Selecting single hue value.
            /// </summary>
            Value,

            /// <summary>
            /// Selecting hue values range.
            /// </summary>
            Range
        }

        /// <summary>
        /// Selected value of the hue picker control in <see cref="HuePickerType.Value"/> mode.
        /// </summary>
        [DefaultValue(0)]
        public int Value
        {
            get { return this.min; }
            set
            {
                if (this.type == HuePickerType.Value)
                {
                    this.min = Math.Max(0, Math.Min(359, value));
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Minimum selected value of the hue picker control in <see cref="HuePickerType.Range"/> mode.
        /// </summary>
        [DefaultValue(0)]
        public int Min
        {
            get { return this.min; }
            set
            {
                if (this.type == HuePickerType.Range)
                {
                    this.min = Math.Max(0, Math.Min(359, value));
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Maximum selected value of the hue picker control in <see cref="HuePickerType.Range"/> mode.
        /// </summary>
        [DefaultValue(359)]
        public int Max
        {
            get { return this.max; }
            set
            {
                if (this.type == HuePickerType.Range)
                {
                    this.max = Math.Max(0, Math.Min(359, value));
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Current type of the hue picker control.
        /// </summary>
        /// 
        /// <remarks><para>See <see cref="HuePickerType"/> enumeration for description of the available types.</para></remarks>
        /// 
        [DefaultValue(HuePickerType.Value)]
        public HuePickerType Type
        {
            get { return this.type; }
            set
            {
                this.type = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HuePicker"/> class.
        /// </summary>
        /// 
        public HuePicker()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);

            this.blackPen = new Pen(Color.Black, 1);
            this.blackBrush = new SolidBrush(Color.Black);
            this.whitePen = new Pen(Color.White, 1);
            this.whiteBrush = new SolidBrush(Color.White);
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
                this.blackBrush.Dispose();
                this.whitePen.Dispose();
                this.whiteBrush.Dispose();
            }
            base.Dispose(disposing);
        }

        // Init component
        private void InitializeComponent()
        {
            // 
            // HSLPicker
            // 
            this.MouseUp += new MouseEventHandler(HSLPicker_MouseUp);
            this.MouseMove += new MouseEventHandler(HSLPicker_MouseMove);
            this.MouseDown += new MouseEventHandler(HSLPicker_MouseDown);
        }

        /// <summary>
        /// Paint the controls.
        /// </summary>
        /// 
        /// <param name="pe">Paint event arguments.</param>
        /// 
        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            Rectangle rc = this.ClientRectangle;
            Rectangle rcPie;
            Brush brush;
            RGB rgb = new RGB();
            HSL hsl = new HSL();

            // get pie rectangle
            rcPie = new Rectangle(4, 4, Math.Min(rc.Right, rc.Bottom) - 8, Math.Min(rc.Right, rc.Bottom) - 8);

            // init HSL value
            hsl.Luminance = 0.5f;
            hsl.Saturation = 1.0f;

            if (this.type == HuePickerType.Value)
            {
                // draw HSL pie
                for (int i = 0; i < 360; i++)
                {
                    hsl.Hue = i;
                    // convert from HSL to RGB
                    HSL.ToRGB(hsl, rgb);
                    // create brush
                    brush = new SolidBrush(rgb.Color);
                    // draw one hue value
                    g.FillPie(brush, rcPie, -i, -1);

                    brush.Dispose();
                }
            }
            else
            {
                // draw HSL pie
                for (int i = 0; i < 360; i++)
                {
                    if (
                        ((this.min < this.max) && (i >= this.min) && (i <= this.max)) ||
                        ((this.min > this.max) && ((i >= this.min) || (i <= this.max))))
                    {
                        hsl.Hue = i;
                        // convert from HSL to RGB
                        HSL.ToRGB(hsl, rgb);
                        // create brush
                        brush = new SolidBrush(rgb.Color);
                    }
                    else
                    {
                        brush = new SolidBrush(Color.FromArgb(128, 128, 128));
                    }

                    // draw one hue value
                    g.FillPie(brush, rcPie, -i, -1);

                    brush.Dispose();
                }
            }

            //
            double halfWidth = (double) rcPie.Width/2;
            double angleRad = -this.min*Math.PI/180;
            double angleCos = Math.Cos(angleRad);
            double angleSin = Math.Sin(angleRad);

            double x = halfWidth*angleCos;
            double y = halfWidth*angleSin;

            this.ptCenter.X = rcPie.Left + (int) (halfWidth);
            this.ptCenter.Y = rcPie.Top + (int) (halfWidth);
            this.ptMin.X = rcPie.Left + (int) (halfWidth + x);
            this.ptMin.Y = rcPie.Top + (int) (halfWidth + y);

            // draw MIN pointer
            g.FillEllipse(this.blackBrush,
                rcPie.Left + (int) (halfWidth + x) - 4,
                rcPie.Top + (int) (halfWidth + y) - 4,
                8, 8);
            g.DrawLine(this.blackPen, this.ptCenter, this.ptMin);

            // check picker type
            if (this.type == HuePickerType.Range)
            {
                angleRad = -this.max*Math.PI/180;
                angleCos = Math.Cos(angleRad);
                angleSin = Math.Sin(angleRad);

                x = halfWidth*angleCos;
                y = halfWidth*angleSin;

                this.ptMax.X = rcPie.Left + (int) (halfWidth + x);
                this.ptMax.Y = rcPie.Top + (int) (halfWidth + y);

                // draw MAX pointer
                g.FillEllipse(this.whiteBrush,
                    rcPie.Left + (int) (halfWidth + x) - 4,
                    rcPie.Top + (int) (halfWidth + y) - 4,
                    8, 8);
                g.DrawLine(this.whitePen, this.ptCenter, this.ptMax);
            }

            base.OnPaint(pe);
        }

        // On mouse down
        private void HSLPicker_MouseDown(object sender, MouseEventArgs e)
        {
            // check coordinates of MIN pointer
            if ((e.X >= this.ptMin.X - 4) && (e.Y >= this.ptMin.Y - 4) &&
                (e.X < this.ptMin.X + 4) && (e.Y < this.ptMin.Y + 4))
            {
                this.trackMode = 1;
            }
            if (this.type == HuePickerType.Range)
            {
                // check coordinates of MAX pointer
                if ((e.X >= this.ptMax.X - 4) && (e.Y >= this.ptMax.Y - 4) &&
                    (e.X < this.ptMax.X + 4) && (e.Y < this.ptMax.Y + 4))
                {
                    this.trackMode = 2;
                }
            }

            if (this.trackMode != 0)
                this.Capture = true;
        }

        // On mouse up
        private void HSLPicker_MouseUp(object sender, MouseEventArgs e)
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
        private void HSLPicker_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor cursor = Cursors.Default;

            if (this.trackMode != 0)
            {
                cursor = Cursors.Hand;

                int dy = e.Y - this.ptCenter.Y;
                int dx = e.X - this.ptCenter.X;

                if (this.trackMode == 1)
                {
                    // MIN pointer tracking
                    this.min = (int) (Math.Atan2(-dy, dx)*180/Math.PI);
                    if (this.min < 0)
                    {
                        this.min = 360 + this.min;
                    }
                }
                else
                {
                    // MAX pointer tracking
                    this.max = (int) (Math.Atan2(-dy, dx)*180/Math.PI);
                    if (this.max < 0)
                    {
                        this.max = 360 + this.max;
                    }
                }

                // repaint control
                Invalidate();
            }
            else
            {
                // check coordinates of MIN pointer
                if ((e.X >= this.ptMin.X - 4) && (e.Y >= this.ptMin.Y - 4) &&
                    (e.X < this.ptMin.X + 4) && (e.Y < this.ptMin.Y + 4))
                {
                    cursor = Cursors.Hand;
                }
                if (this.type == HuePickerType.Range)
                {
                    // check coordinates of MAX pointer
                    if ((e.X >= this.ptMax.X - 4) && (e.Y >= this.ptMax.Y - 4) &&
                        (e.X < this.ptMax.X + 4) && (e.Y < this.ptMax.Y + 4))
                    {
                        cursor = Cursors.Hand;
                    }
                }
            }

            this.Cursor = cursor;
        }
    }
}