// AForge Direct Show Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © AForge.NET, 2009-2013
// contacts@aforgenet.com
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AForge.Video.DirectShow;

namespace AForge.Video.DirectShow
{
    /// <summary>
    /// Local video device selection form.
    /// </summary>
    /// 
    /// <remarks><para>The form provides a standard way of selecting local video
    /// device (USB web camera, capture board, etc. - anything supporting DirectShow
    /// interface), which can be reused across applications. It allows selecting video
    /// device, video size and snapshots size (if device supports snapshots and
    /// <see cref="ConfigureSnapshots">user needs them</see>).</para>
    /// 
    /// <para><img src="img/video/VideoCaptureDeviceForm.png" width="478" height="205" /></para>
    /// </remarks>
    /// 
    public partial class VideoCaptureDeviceForm : Form
    {
        // collection of available video devices
        private FilterInfoCollection _videoDevices;
        // selected video device

        // supported capabilities of video and snapshots
        private Dictionary<string, VideoCapabilities> _videoCapabilitiesDictionary =
            new Dictionary<string, VideoCapabilities>();

        private Dictionary<string, VideoCapabilities> _snapshotCapabilitiesDictionary =
            new Dictionary<string, VideoCapabilities>();

        // available video inputs
        private VideoInput[] _availableVideoInputs = null;

        // flag telling if user wants to configure snapshots as well
        private bool _configureSnapshots = false;

        /// <summary>
        /// Specifies if snapshot configuration should be done or not.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies if the dialog form should
        /// allow configuration of snapshot sizes (if selected video source supports
        /// snapshots). If the property is set to <see langword="true"/>, then
        /// the form will provide additional combo box enumerating supported
        /// snapshot sizes. Otherwise the combo boxes will be hidden.
        /// </para>
        /// 
        /// <para>If the property is set to <see langword="true"/> and selected
        /// device supports snapshots, then <see cref="VideoCaptureDevice.ProvideSnapshots"/>
        /// property of the <see cref="VideoDevice">configured device</see> is set to
        /// <see langword="true"/>.</para>
        /// 
        /// <para>Default value of the property is set to <see langword="false"/>.</para>
        /// </remarks>
        /// 
        public bool ConfigureSnapshots
        {
            get { return this._configureSnapshots; }
            set
            {
                this._configureSnapshots = value;
                this.snapshotsLabel.Visible = value;
                this.snapshotResolutionsCombo.Visible = value;
            }
        }

        /// <summary>
        /// Provides configured video device.
        /// </summary>
        /// 
        /// <remarks><para>The property provides configured video device if user confirmed
        /// the dialog using "OK" button. If user canceled the dialog, the property is
        /// set to <see langword="null"/>.</para></remarks>
        /// 
        public VideoCaptureDevice VideoDevice { get; private set; }

        /// <summary>
        /// Moniker string of the selected video device.
        /// </summary>
        /// 
        /// <remarks><para>The property allows to get moniker string of the selected device
        /// on form completion or set video device which should be selected by default on
        /// form loading.</para></remarks>
        /// 
        public string VideoDeviceMoniker { get; set; } = string.Empty;

        /// <summary>
        /// Video frame size of the selected device.
        /// </summary>
        /// 
        /// <remarks><para>The property allows to get video size of the selected device
        /// on form completion or set the size to be selected by default on form loading.</para>
        /// </remarks>
        /// 
        public Size CaptureSize { get; set; } = new Size(0, 0);

        /// <summary>
        /// Snapshot frame size of the selected device.
        /// </summary>
        /// 
        /// <remarks><para>The property allows to get snapshot size of the selected device
        /// on form completion or set the size to be selected by default on form loading
        /// (if <see cref="ConfigureSnapshots"/> property is set <see langword="true"/>).</para>
        /// </remarks>
        public Size SnapshotSize { get; set; } = new Size(0, 0);

        /// <summary>
        /// Video input to use with video capture card.
        /// </summary>
        /// 
        /// <remarks><para>The property allows to get video input of the selected device
        /// on form completion or set it to be selected by default on form loading.</para></remarks>
        /// 
        public VideoInput VideoInput { get; set; } = VideoInput.Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoCaptureDeviceForm"/> class.
        /// </summary>
        /// 
        public VideoCaptureDeviceForm()
        {
            InitializeComponent();
            this.ConfigureSnapshots = false;

            // show device list
            try
            {
                // enumerate video devices
                this._videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (this._videoDevices.Count == 0)
                    throw new ApplicationException();

                // add all devices to combo
                foreach (FilterInfo device in this._videoDevices)
                {
                    this.devicesCombo.Items.Add(device.Name);
                }
            }
            catch (ApplicationException)
            {
                this.devicesCombo.Items.Add("No local capture devices");
                this.devicesCombo.Enabled = false;
                this.okButton.Enabled = false;
            }
        }

        // On form loaded
        private void VideoCaptureDeviceForm_Load(object sender, EventArgs e)
        {
            int selectedCameraIndex = 0;

            for (int i = 0; i < this._videoDevices.Count; i++)
            {
                if (this.VideoDeviceMoniker == this._videoDevices[i].MonikerString)
                {
                    selectedCameraIndex = i;
                    break;
                }
            }

            this.devicesCombo.SelectedIndex = selectedCameraIndex;
        }

        // Ok button clicked
        private void okButton_Click(object sender, EventArgs e)
        {
            this.VideoDeviceMoniker = this.VideoDevice.Source;

            // set video size
            if (this._videoCapabilitiesDictionary.Count != 0)
            {
                VideoCapabilities caps = this._videoCapabilitiesDictionary[(string) this.videoResolutionsCombo.SelectedItem];

                this.VideoDevice.VideoResolution = caps;
                this.CaptureSize = caps.FrameSize;
            }

            if (this._configureSnapshots)
            {
                // set snapshots size
                if (this._snapshotCapabilitiesDictionary.Count != 0)
                {
                    VideoCapabilities caps = this._snapshotCapabilitiesDictionary[(string) this.snapshotResolutionsCombo.SelectedItem];

                    this.VideoDevice.ProvideSnapshots = true;
                    this.VideoDevice.SnapshotResolution = caps;

                    this.SnapshotSize = caps.FrameSize;
                }
            }

            if (this._availableVideoInputs.Length != 0)
            {
                this.VideoInput = this._availableVideoInputs[this.videoInputsCombo.SelectedIndex];
                this.VideoDevice.CrossbarVideoInput = this.VideoInput;
            }
        }

        // New video device is selected
        private void devicesCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._videoDevices.Count != 0)
            {
                this.VideoDevice = new VideoCaptureDevice(this._videoDevices[this.devicesCombo.SelectedIndex].MonikerString);
                EnumeratedSupportedFrameSizes(this.VideoDevice);
            }
        }

        // Collect supported video and snapshot sizes
        private void EnumeratedSupportedFrameSizes(VideoCaptureDevice videoDevice)
        {
            this.Cursor = Cursors.WaitCursor;

            this.videoResolutionsCombo.Items.Clear();
            this.snapshotResolutionsCombo.Items.Clear();
            this.videoInputsCombo.Items.Clear();

            this._videoCapabilitiesDictionary.Clear();
            this._snapshotCapabilitiesDictionary.Clear();

            try
            {
                // collect video capabilities
                VideoCapabilities[] videoCapabilities = videoDevice.VideoCapabilities;
                int videoResolutionIndex = 0;

                foreach (VideoCapabilities capabilty in videoCapabilities)
                {
                    string item = $"{capabilty.FrameSize.Width} x {capabilty.FrameSize.Height}";

                    if (!this.videoResolutionsCombo.Items.Contains(item))
                    {
                        if (this.CaptureSize == capabilty.FrameSize)
                        {
                            videoResolutionIndex = this.videoResolutionsCombo.Items.Count;
                        }

                        this.videoResolutionsCombo.Items.Add(item);
                    }

                    if (!this._videoCapabilitiesDictionary.ContainsKey(item))
                    {
                        this._videoCapabilitiesDictionary.Add(item, capabilty);
                    }
                }

                if (videoCapabilities.Length == 0)
                {
                    this.videoResolutionsCombo.Items.Add("Not supported");
                }

                this.videoResolutionsCombo.SelectedIndex = videoResolutionIndex;


                if (this._configureSnapshots)
                {
                    // collect snapshot capabilities
                    VideoCapabilities[] snapshotCapabilities = videoDevice.SnapshotCapabilities;
                    int snapshotResolutionIndex = 0;

                    foreach (VideoCapabilities capabilty in snapshotCapabilities)
                    {
                        string item = $"{capabilty.FrameSize.Width} x {capabilty.FrameSize.Height}";

                        if (!this.snapshotResolutionsCombo.Items.Contains(item))
                        {
                            if (this.SnapshotSize == capabilty.FrameSize)
                            {
                                snapshotResolutionIndex = this.snapshotResolutionsCombo.Items.Count;
                            }

                            this.snapshotResolutionsCombo.Items.Add(item);
                            this._snapshotCapabilitiesDictionary.Add(item, capabilty);
                        }
                    }

                    if (snapshotCapabilities.Length == 0)
                    {
                        this.snapshotResolutionsCombo.Items.Add("Not supported");
                    }

                    this.snapshotResolutionsCombo.SelectedIndex = snapshotResolutionIndex;
                }

                // get video inputs
                this._availableVideoInputs = videoDevice.AvailableCrossbarVideoInputs;
                int videoInputIndex = 0;

                foreach (VideoInput input in this._availableVideoInputs)
                {
                    string item = $"{input.Index}: {input.Type}";

                    if ((input.Index == this.VideoInput.Index) && (input.Type == this.VideoInput.Type))
                    {
                        videoInputIndex = this.videoInputsCombo.Items.Count;
                    }

                    this.videoInputsCombo.Items.Add(item);
                }

                if (this._availableVideoInputs.Length == 0)
                {
                    this.videoInputsCombo.Items.Add("Not supported");
                }

                this.videoInputsCombo.SelectedIndex = videoInputIndex;
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}