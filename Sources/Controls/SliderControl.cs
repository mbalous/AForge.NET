// AForge Controls Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Andrew Kirillov, 2010
// andrew.kirillov@aforgenet.com
//

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AForge.Controls
{
    /// <summary>
    /// Slider control.
    /// </summary>
    ///
    /// <remarks>
    /// <para>The control represents a slider, which can be dragged in the [-1, 1] range.
    /// Default position of the slider is set 0, which corresponds to center of the control.<br />
    /// <img src="img/controls/slider_control.png" width="227" height="56" />
    /// </para>
    /// </remarks>
    ///
    public partial class SliderControl : Control
    {
        // horizontal or vertical configuration
        private bool isHorizontal = true;

        private bool resetPositionOnMouseRelease = true;

        // pens and brushes for drawing
        private Pen borderPen = new Pen(Color.Black);
        private SolidBrush positiveAreaBrush = new SolidBrush(Color.White);
        private SolidBrush negativeAreaBrush = new SolidBrush(Color.LightGray);
        private SolidBrush manipulatorBrush = new SolidBrush(Color.LightSeaGreen);
        private SolidBrush disabledBrash = new SolidBrush(Color.FromArgb(240, 240, 240));

        // manipulator's size
        private const int manipulatorWidth = 11;
        private const int manipulatorHeight = 20;

        // margins
        private int leftMargin;
        private int topMargin;

        // manipulator's position
        private float manipulatatorPosition = 0;

        // tracking information
        private bool tracking = false;

        // number of timer ticks before notifying user (-1 means no notification)
        private int ticksBeforeNotificiation = -1;

        /// <summary>
        /// Determines behaviour of manipulator, when mouse button is released.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>The property controls behaviour of manipulator on releasing mouse button. If
        /// the property is set to <see langword="true"/>, then position of manipulator is reset
        /// to 0, when mouse button is released. Otherwise manipulator stays on the place,
        /// where it was left.</para>
        /// 
        /// <para>Default value is set to <see langword="true"/>.</para>
        /// </remarks>
        /// 
        [DefaultValue(true)]
        [Description("Determines behaviour of manipulator, when mouse button is released.")]
        public bool ResetPositionOnMouseRelease
        {
            get { return this.resetPositionOnMouseRelease; }
            set { this.resetPositionOnMouseRelease = value; }
        }

        /// <summary>
        /// Color used for drawing borders.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>Default value is set to <see cref="Color.Black"/>.</para>
        /// </remarks>
        /// 
        [DefaultValue(typeof (Color), "Black")]
        [Description("Color used for drawing borders.")]
        public Color BorderColor
        {
            get { return this.borderPen.Color; }
            set
            {
                this.borderPen = new Pen(value);
                Invalidate();
            }
        }

        /// <summary>
        /// Background color used for filling area corresponding to positive values.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>Default value is set to <see cref="Color.White"/>.</para>
        /// </remarks>
        /// 
        [DefaultValue(typeof (Color), "White")]
        [Description("Background color used for filling area corresponding to positive values.")]
        public Color PositiveAreaBrush
        {
            get { return this.positiveAreaBrush.Color; }
            set
            {
                this.positiveAreaBrush = new SolidBrush(value);
                Invalidate();
            }
        }

        /// <summary>
        /// Background color used for filling area corresponding to negative values.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>Default value is set to <see cref="Color.LightGray"/>.</para>
        /// </remarks>
        /// 
        [DefaultValue(typeof (Color), "LightGray")]
        [Description("Background color used for filling top right quarter of the control.")]
        public Color NegativeAreaBrush
        {
            get { return this.negativeAreaBrush.Color; }
            set
            {
                this.negativeAreaBrush = new SolidBrush(value);
                Invalidate();
            }
        }

        /// <summary>
        /// Color used for filling manipulator.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>Default value is set to <see cref="Color.LightSeaGreen"/>.</para>
        /// </remarks>
        /// 
        [DefaultValue(typeof (Color), "LightSeaGreen")]
        [Description("Color used for filling manipulator.")]
        public Color ManipulatorColor
        {
            get { return this.manipulatorBrush.Color; }
            set
            {
                this.manipulatorBrush = new SolidBrush(value);
                Invalidate();
            }
        }

        /// <summary>
        /// Defines if control has horizontal or vertical look.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>Default value is set to <see langword="true"/>.</para>
        /// </remarks>
        /// 
        [DefaultValue(true)]
        [Description("Defines if control has horizontal or vertical look.")]
        public bool IsHorizontal
        {
            get { return this.isHorizontal; }
            set
            {
                this.isHorizontal = value;

                if (value)
                {
                    this.leftMargin = manipulatorWidth/2 + 2;
                    this.topMargin = manipulatorHeight/4;
                }
                else
                {
                    this.leftMargin = manipulatorHeight/4;
                    this.topMargin = manipulatorWidth/2 + 2;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Current manipulator's position, [-1, 1].
        /// </summary>
        /// 
        /// <remarks><para>The property equals to current manipulator's position.</para>
        /// </remarks>
        /// 
        [Browsable(false)]
        public float ManipulatorPosition
        {
            get { return this.manipulatatorPosition; }
            set
            {
                this.manipulatatorPosition = Math.Max(-1.0f, Math.Min(1.0f, value));
                Invalidate();
                NotifyClients();
            }
        }

        /// <summary>
        /// Delegate used for notification about manipulator's position changes.
        /// </summary>
        /// 
        /// <param name="sender">Event sender - object sending the event.</param>
        /// <param name="position">Current position of manipulator.</param>
        /// 
        public delegate void PositionChangedHandler(object sender, float position);

        /// <summary>
        /// Event used for notification about manipulator's position changes.
        /// </summary>
        [Description("Occurs when manipulator's position is changed.")]
        public event PositionChangedHandler PositionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderControl"/> class.
        /// </summary>
        public SliderControl()
        {
            InitializeComponent();

            // update control style
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);

            this.IsHorizontal = true;
        }

        // Paint the control
        private void TurnControl_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            int clientWidth = this.ClientRectangle.Width;
            int clientHeight = this.ClientRectangle.Height;

            if (this.isHorizontal)
            {
                // draw area
                g.FillRectangle((this.Enabled) ? this.negativeAreaBrush : this.disabledBrash, this.leftMargin, this.topMargin,
                    clientWidth/2 - this.leftMargin, manipulatorHeight/2);
                g.FillRectangle((this.Enabled) ? this.positiveAreaBrush : this.disabledBrash, clientWidth/2, this.topMargin,
                    clientWidth/2 - this.leftMargin, manipulatorHeight/2);
                g.DrawRectangle(this.borderPen, this.leftMargin, this.topMargin,
                    clientWidth - 1 - this.leftMargin*2, manipulatorHeight/2);
                g.DrawLine(this.borderPen, clientWidth/2, this.topMargin, clientWidth/2, this.topMargin + manipulatorHeight/2);

                // calculate manipulator's center point
                int ctrlManipulatorX = (int) (this.manipulatatorPosition*(clientWidth/2 - this.leftMargin) + clientWidth/2);

                // draw manipulator
                g.FillRectangle((this.Enabled) ? this.manipulatorBrush : this.disabledBrash, ctrlManipulatorX - manipulatorWidth/2,
                    0,
                    manipulatorWidth, manipulatorHeight);
                g.DrawRectangle(this.borderPen, ctrlManipulatorX - manipulatorWidth/2, 0,
                    manipulatorWidth, manipulatorHeight);
            }
            else
            {
                // draw area
                g.FillRectangle((this.Enabled) ? this.positiveAreaBrush : this.disabledBrash, this.leftMargin, this.topMargin,
                    manipulatorHeight/2, clientHeight/2 - this.topMargin);
                g.FillRectangle((this.Enabled) ? this.negativeAreaBrush : this.disabledBrash, this.leftMargin, clientHeight/2,
                    manipulatorHeight/2, clientHeight/2 - this.topMargin);
                g.DrawRectangle(this.borderPen, this.leftMargin, this.topMargin,
                    manipulatorHeight/2, clientHeight - 1 - this.topMargin*2);
                g.DrawLine(this.borderPen, this.leftMargin, clientHeight/2, this.leftMargin + manipulatorHeight/2, clientHeight/2);


                // calculate manipulator's center point
                int ctrlManipulatorY = (int) (-this.manipulatatorPosition*(clientHeight/2 - this.topMargin) + clientHeight/2);

                // draw manipulator
                g.FillRectangle((this.Enabled) ? this.manipulatorBrush : this.disabledBrash, 0,
                    ctrlManipulatorY - manipulatorWidth/2,
                    manipulatorHeight, manipulatorWidth);
                g.DrawRectangle(this.borderPen, 0, ctrlManipulatorY - manipulatorWidth/2,
                    manipulatorHeight, manipulatorWidth);
            }
        }

        // On mouse down event
        private void TurnControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.isHorizontal)
                {
                    if (
                        (e.X >= this.leftMargin) &&
                        (e.X < this.ClientRectangle.Width - this.leftMargin) &&
                        (e.Y >= this.topMargin) &&
                        (e.Y < this.ClientRectangle.Height - this.topMargin))
                    {
                        int cx = e.X - this.ClientRectangle.Width/2;
                        this.manipulatatorPosition = (float) cx/(this.ClientRectangle.Width/2 - this.leftMargin);
                        this.tracking = true;
                    }
                }
                else
                {
                    if (
                        (e.X >= this.leftMargin) &&
                        (e.X < this.ClientRectangle.Width - this.leftMargin) &&
                        (e.Y >= this.topMargin) &&
                        (e.Y < this.ClientRectangle.Height - this.topMargin))
                    {
                        int cy = this.ClientRectangle.Height/2 - e.Y;
                        this.manipulatatorPosition = (float) cy/(this.ClientRectangle.Height/2 - this.topMargin);
                        this.tracking = true;
                    }
                }

                if (this.tracking)
                {
                    this.Capture = true;
                    this.Cursor = Cursors.Hand;

                    NotifyClients();
                    // start time, which is used to notify
                    // about manipulator's position change
                    this.ticksBeforeNotificiation = -1;
                    this.timer.Start();
                }
            }
        }

        // On mouse up event
        private void TurnControl_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (this.tracking))
            {
                this.tracking = false;
                this.Capture = false;
                this.Cursor = Cursors.Arrow;

                if (this.resetPositionOnMouseRelease)
                {
                    this.manipulatatorPosition = 0;
                }

                Invalidate();
                this.timer.Stop();

                NotifyClients();
            }
        }

        // On mouse move event
        private void TurnControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.tracking)
            {
                if (this.isHorizontal)
                {
                    int cx = e.X - this.ClientRectangle.Width/2;
                    this.manipulatatorPosition = (float) cx/(this.ClientRectangle.Width/2 - this.leftMargin);
                }
                else
                {
                    int cy = this.ClientRectangle.Height/2 - e.Y;
                    this.manipulatatorPosition = (float) cy/(this.ClientRectangle.Height/2 - this.topMargin);
                }

                this.manipulatatorPosition = Math.Max(Math.Min(1, this.manipulatatorPosition), -1);
                Invalidate();

                // notify user after 10 timer ticks
                this.ticksBeforeNotificiation = 5;
            }
        }

        // Timer handler, which is used to notify clients about manipulator's changes
        private void timer_Tick(object sender, EventArgs e)
        {
            if (this.ticksBeforeNotificiation != -1)
            {
                // time to notify
                if (this.ticksBeforeNotificiation == 0)
                {
                    // notify users
                    NotifyClients();
                }

                this.ticksBeforeNotificiation--;
            }
        }

        // Notify client about changes of manipulator's position
        private void NotifyClients()
        {
            if (this.PositionChanged != null)
            {
                this.PositionChanged(this, this.manipulatatorPosition);
            }
        }
    }
}