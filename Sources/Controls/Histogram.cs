// AForge Controls Library
// AForge.NET framework
//
// Copyright © Andrew Kirillov, 2005-2008
// andrew.kirillov@gmail.com
//

namespace AForge.Controls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Data;
    using System.Windows.Forms;

    /// <summary>
    /// Arguments of histogram events.
    /// </summary>
    public class HistogramEventArgs : EventArgs
    {
        private int min, max;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistogramEventArgs"/> class.
        /// </summary>
        /// 
        /// <param name="pos">Histogram's index under mouse pointer.</param>
        /// 
        public HistogramEventArgs(int pos)
        {
            this.min = pos;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistogramEventArgs"/> class.
        /// </summary>
        /// 
        /// <param name="min">Min histogram's index in selection.</param>
        /// <param name="max">Max histogram's index in selection.</param>
        /// 
        public HistogramEventArgs(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Min histogram's index in selection.
        /// </summary>
        public int Min
        {
            get { return this.min; }
        }

        /// <summary>
        /// Max histogram's index in selection.
        /// </summary>
        public int Max
        {
            get { return this.max; }
        }

        /// <summary>
        /// Histogram's index under mouse pointer.
        /// </summary>
        public int Position
        {
            get { return this.min; }
        }
    }

    /// <summary>
    /// Delegate for histogram events handlers.
    /// </summary>
    /// 
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    /// 
    public delegate void HistogramEventHandler(object sender, HistogramEventArgs e);


    /// <summary>
    /// Histogram control.
    /// </summary>
    /// 
    /// <remarks><para>The control displays histograms represented with integer arrays,
    /// where each array's element keeps occurrence number of the corresponding element.
    /// </para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // create array with histogram values
    /// int[] histogramValues = new int[] { 3, 8, 53, 57, 79, 69, ... };
    /// // set values to histogram control
    /// histogram.Values = histogramValues;
    /// </code>
    /// 
    /// <para><b>Sample control's look:</b></para>
    /// <img src="img/controls/histogram.jpg" width="324" height="77" />
    /// </remarks>
    /// 
    public class Histogram : Control
    {
        // color used to paing histogram
        private Color color = Color.Black;
        // logarithmic view or not
        private bool logarithmic = false;
        // histogram's values
        private int[] values;
        // max histogram's values
        private int max;
        private double maxLogarithmic;
        // allow mouse selection in histogram or not
        private bool allowSelection = false;
        // vertical histogram or not
        private bool vertical = false;

        // set of pens
        private Pen blackPen = new Pen(Color.Black, 1);
        private Pen whitePen = new Pen(Color.White, 1);
        private Pen drawPen = new Pen(Color.Black);

        // width and height of histogram's area
        private int width;
        private int height;

        // mouse dragging with pressed button
        private bool tracking = false;
        // determines if mouse is over histogram area
        private bool over = false;
        // selection's start and stop positions
        private int start, stop;

        /// <summary>
        /// Histogram's color.
        /// </summary>
        /// 
        [DefaultValue(typeof (Color), "Black")]
        public Color Color
        {
            get { return this.color; }
            set
            {
                this.color = value;

                this.drawPen.Dispose();
                this.drawPen = new Pen(this.color);
                Invalidate();
            }
        }

        /// <summary>
        /// Allow mouse selection or not.
        /// </summary>
        /// 
        /// <remarks>In the case if mouse selection is allowed, the control will
        /// fire <see cref="SelectionChanged"/> and <see cref="PositionChanged"/> events
        /// and provide information about the selection.</remarks>
        /// 
        [DefaultValue(false)]
        public bool AllowSelection
        {
            get { return this.allowSelection; }
            set { this.allowSelection = value; }
        }

        /// <summary>
        /// Logarithmic view or not.
        /// </summary>
        /// 
        /// <remarks><para>In the case if logarihmic view is selected, then the control
        /// will display base 10 logarithm of values.</para>
        /// 
        /// <para>By default the property is set to <b>false</b> - none logarithmic view.</para></remarks>
        /// 
        [DefaultValue(false)]
        public bool IsLogarithmicView
        {
            get { return this.logarithmic; }
            set
            {
                this.logarithmic = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Vertical view or not.
        /// </summary>
        ///
        /// <remarks><para>The property determines if histogram should be displayed vertically or
        /// not (horizontally).</para>
        /// 
        /// <para>By default the property is set to <b>false</b> - horizontal view.</para></remarks>
        ///
        [DefaultValue(false)]
        public bool IsVertical
        {
            get { return this.vertical; }
            set
            {
                this.vertical = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Histogram values.
        /// </summary>
        /// 
        /// <remarks>Non-negative histogram values.</remarks>
        /// 
        /// <exception cref="ArgumentException">Histogram values should be non-negative.</exception>
        /// 
        [Browsable(false)]
        public int[] Values
        {
            get { return this.values; }
            set
            {
                this.values = value;

                if (this.values != null)
                {
                    // check values and find maximum
                    this.max = 0;
                    foreach (int v in this.values)
                    {
                        // value chould non-negative
                        if (v < 0)
                        {
                            throw new ArgumentException("Histogram values should be non-negative.");
                        }

                        if (v > this.max)
                        {
                            this.max = v;
                            this.maxLogarithmic = Math.Log10(this.max);
                        }
                    }
                }
                Invalidate();
            }
        }

        /// <summary>
        /// Mouse position changed event.
        /// </summary>
        /// 
        /// <remarks>The event is fired only if the <see cref="AllowSelection"/> property is set
        /// to true. The passed to event handler <see cref="HistogramEventArgs"/> class is initialized
        /// with <see cref="HistogramEventArgs.Position"/> property only, which is histogram value's
        /// index pointed by mouse.</remarks>
        /// 
        public event HistogramEventHandler PositionChanged;

        /// <summary>
        /// Mouse selection changed event.
        /// </summary>
        /// 
        /// <remarks>The event is fired only if the <see cref="AllowSelection"/> property is set
        /// to true. The passed to event handler <see cref="HistogramEventArgs"/> class is initialized
        /// with <see cref="HistogramEventArgs.Min"/> and <see cref="HistogramEventArgs.Max"/> properties
        /// only, which represent selection range - min and max indexes.</remarks>
        /// 
        public event HistogramEventHandler SelectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram"/> class.
        /// </summary>
        /// 
        public Histogram()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// 
        /// <param name="disposing">Indicates if disposing was initiated manually.</param>
        /// 
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.blackPen.Dispose();
                this.whitePen.Dispose();
                this.drawPen.Dispose();
            }
            base.Dispose(disposing);
        }

        // Init component
        private void InitializeComponent()
        {
            // 
            // Histogram
            // 
            this.MouseUp += new MouseEventHandler(Histogram_MouseUp);
            this.MouseMove += new MouseEventHandler(Histogram_MouseMove);
            this.MouseLeave += new EventHandler(Histogram_MouseLeave);
            this.MouseDown += new MouseEventHandler(Histogram_MouseDown);
        }

        /// <summary>
        /// Paint the control.
        /// </summary>
        /// 
        /// <param name="pe">Data for Paint event.</param>
        /// 
        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            // drawing area's width and height
            this.width = ((this.values == null) || (this.vertical == true))
                ? this.ClientRectangle.Width - 2
                : Math.Min(this.values.Length, this.ClientRectangle.Width - 2);

            this.height = ((this.values == null) || (this.vertical == false))
                ? this.ClientRectangle.Height - 2
                : Math.Min(this.values.Length, this.ClientRectangle.Height - 2);

            int x = 1;
            int y = 1;
            int value;

            // draw rectangle around the image
            g.DrawRectangle(this.blackPen, x - 1, y - 1, this.width + 1, this.height + 1);

            if (this.values != null)
            {
                int start = Math.Min(this.start, this.stop);
                int stop = Math.Max(this.start, this.stop);

                if (this.tracking)
                {
                    // fill region of selection
                    Brush brush = new SolidBrush(Color.FromArgb(92, 92, 92));

                    if (this.vertical)
                    {
                        g.FillRectangle(brush, x, y + start, this.width, Math.Abs(start - stop) + 1);
                    }
                    else
                    {
                        g.FillRectangle(brush, x + start, y, Math.Abs(start - stop) + 1, this.height);
                    }
                    brush.Dispose();
                }

                if (this.max != 0)
                {
                    // scaling factor
                    double factor = (double) ((this.vertical) ? this.width : this.height)/
                                    ((this.logarithmic) ? this.maxLogarithmic : this.max);

                    // draw histogram
                    for (int i = 0, len = (this.vertical) ? this.height : this.width; i < len; i++)
                    {
                        if (this.logarithmic)
                        {
                            value = (this.values[i] == 0) ? 0 : (int) (Math.Log10(this.values[i])*factor);
                        }
                        else
                        {
                            value = (int) (this.values[i]*factor);
                        }

                        if (value != 0)
                        {
                            if (this.vertical)
                            {
                                g.DrawLine(((this.tracking) && (i >= start) && (i <= stop)) ? this.whitePen : this.drawPen,
                                    new Point(x, y + i),
                                    new Point(x + value, y + i)
                                    );
                            }
                            else
                            {
                                g.DrawLine(((this.tracking) && (i >= start) && (i <= stop)) ? this.whitePen : this.drawPen,
                                    new Point(x + i, y + this.height - 1),
                                    new Point(x + i, y + this.height - value)
                                    );
                            }
                        }
                    }
                }
            }

            // Calling the base class OnPaint
            base.OnPaint(pe);
        }

        // On mouse down
        private void Histogram_MouseDown(object sender, MouseEventArgs e)
        {
            if ((this.allowSelection) && (this.values != null))
            {
                int x = 1;
                int y = 1;

                if ((e.X >= x) && (e.Y >= y) && (e.X < x + this.width) && (e.Y < y + this.height))
                {
                    // start selection
                    this.tracking = true;
                    this.start = (this.vertical) ? (e.Y - y) : (e.X - x);
                    this.Capture = true;
                }
            }
        }

        // On mouse up
        private void Histogram_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.tracking)
            {
                // stop selection
                this.tracking = false;
                this.Capture = false;
                Invalidate();
            }
        }

        // On mouse move
        private void Histogram_MouseMove(object sender, MouseEventArgs e)
        {
            if ((this.allowSelection) && (this.values != null))
            {
                int x = 1;
                int y = 1;

                if (!this.tracking)
                {
                    if ((e.X >= x) && (e.Y >= y) && (e.X < x + this.width) && (e.Y < y + this.height))
                    {
                        this.over = true;

                        // moving over
                        this.Cursor = Cursors.Cross;

                        // notify parent
                        if (this.PositionChanged != null)
                        {
                            this.PositionChanged(this, new HistogramEventArgs(
                                (this.vertical) ? (e.Y - y) : (e.X - x)));
                        }
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;

                        if (this.over)
                        {
                            this.over = false;

                            // notify parent
                            if (this.PositionChanged != null)
                                this.PositionChanged(this, new HistogramEventArgs(-1));
                        }
                    }
                }
                else
                {
                    // selecting region
                    this.stop = (this.vertical) ? (e.Y - y) : (e.X - x);

                    this.stop = Math.Min(this.stop, ((this.vertical) ? this.height : this.width) - 1);
                    this.stop = Math.Max(this.stop, 0);

                    Invalidate();

                    // notify parent
                    if (this.SelectionChanged != null)
                        this.SelectionChanged(this, new HistogramEventArgs(Math.Min(this.start, this.stop), Math.Max(this.start, this.stop)));
                }
            }
        }

        // On mouse leave
        private void Histogram_MouseLeave(object sender, EventArgs e)
        {
            if ((this.allowSelection) && (this.values != null) && (!this.tracking))
            {
                // notify parent
                if (this.PositionChanged != null)
                    this.PositionChanged(this, new HistogramEventArgs(-1));
            }
        }
    }
}