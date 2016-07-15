namespace TestWinRTProject.Controls
{
  using BodyFrameReaders;
  using Measurements;
  using Storage;
  using System;
  using TestWinRTProject.Configuration;
  using TestWinRTProject.Services;
  using System.Linq;
  using SharedApp.Interfaces;

#if NETFX_CORE
  using Windows.Foundation;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using WindowsPreview.Kinect;
  using System.Threading.Tasks;
  using Windows.UI.Xaml.Media;
  using Windows.UI;
  using System.Diagnostics;
  using Windows.UI.Xaml.Media.Imaging;
#else
  using System.Windows.Controls;
  using Microsoft.Kinect;
  using System.Windows;
  using System.Windows.Media.Animation;
  using System.Threading.Tasks;
  using System.Windows.Media;
  using System.Windows.Media.Imaging;
  using System.IO;
#endif

  public sealed partial class BodyDisplayControl : UserControl
  {
    public BodyDisplayControl(
      SingleBodyFrameReader reader,
      Canvas canvas,
      bool useInfrared,
      IControlServiceRegistry serviceRegistry)
    {
      this.InitializeComponent();
      this.reader = reader;
      this.canvas = canvas;
      this.useInfrared = useInfrared;
      this.serviceRegistry = serviceRegistry;

      this.reader.FrameArrived += this.OnFrameArrived;
      this.reader.TrackingIdLost += this.OnTrackingIdLost;

      this.measurementSet = new JointPairAverageMeasurementSet(
        MeasurementConfiguration.Measurements,
        GlobalConfiguration.Instance.MeasurementFrameCount);

      this.canvas.Children.Add(this);

#if !NETFX_CORE
      this.spin = this.FindResource(SPIN_STORYBOARD) as Storyboard;
#endif

      this.spin.Begin();
    }
    async void OnFrameArrived(object sender, SingleBodyFrameEventArgs e)
    {
      if (!this.measurementSet.HasReachedMaximumSampleCount)
      {
        this.measurementSet.Measure(e.Body);
        this.txtNumber.Text = this.measurementSet.CompletedMeasurementCount.ToString();
      }
      if (this.measurementSet.HasReachedMaximumSampleCount && !this.showingMeasurements)
      {
        // TODO: need to think about cancellation here because we can't currently
        // cancel this and we don't await it which means we go back to the
        // "kinect loop" while this is still ongoing which isn't right.
        await this.OnMeasurementsCompleted();
      }
      this.SizeAndPositionControlAroundBody(e);
    }
    async Task OnMeasurementsCompleted()
    {
      this.showingMeasurements = true;
      this.itemsControl.DataContext = this.measurementSet.Measurements;
      this.itemsControl.Visibility = Visibility.Visible;
      this.spin.Stop();

      await this.MatchOrStoreMeasurementsInCloud();
    }
    async Task MatchOrStoreMeasurementsInCloud()
    {
      // TODO: again, cancellation? what if the user leaves the frame during this
      // part and we continue on with most things destroyed?
      this.txtNumber.Text = string.Empty;

      var candidate = await StorageService.MatchMeasurementAsync(this.measurementSet);

      bool match = (candidate != null);

      if (!match)
      {
        // TODO: this is likely to grind to a halt because of the size of the photos
        // so we perhaps need to take a new approach.
        IRenderBackground renderer = this.serviceRegistry.GetService<IRenderBackground>();

        Rect bodyRect = new Rect(
          Canvas.GetLeft(this), Canvas.GetTop(this), this.ActualWidth, this.ActualHeight);

        byte[] photoBits = await renderer.RenderBackgroundToByteArrayAsync(bodyRect);

        await StorageService.StoreMeasurementSetAsync(this.measurementSet, photoBits);
      }
      this.ellipse.Fill = new SolidColorBrush(match ? Colors.Green : Colors.Orange);
    }
    void OnTrackingIdLost(object sender, EventArgs e)
    {
      this.canvas.Children.Remove(this);
      this.reader.TrackingIdLost -= this.OnTrackingIdLost;
      this.reader.FrameArrived -= this.OnFrameArrived;
      this.reader.Dispose();
    }
    void SizeAndPositionControlAroundBody(SingleBodyFrameEventArgs e)
    {
      var positions = boxJoints
        .Select(
          j => this.CameraSpacePointToCanvasSpacePoint(
            e.Source.Sensor,
            e.Body.Joints[j].Position))
        .Where(
          p => !double.IsInfinity(p.X) && !double.IsInfinity(p.Y));

      if ((positions == null) || (positions.Count() == 0))
      {
        this.Visibility = Visibility.Collapsed;
      }
      else
      {
        var leftMost = Math.Max(0, positions.Min(p => p.X) - PHOTO_BOX_MARGIN);
        var rightMost = Math.Min(this.canvas.ActualWidth, positions.Max(p => p.X) + PHOTO_BOX_MARGIN);
        var topMost = Math.Max(0, positions.Min(p => p.Y) - PHOTO_BOX_MARGIN);
        var bottomMost = Math.Min(this.canvas.ActualHeight, positions.Max(p => p.Y) + PHOTO_BOX_MARGIN);

        this.Width = rightMost - leftMost;
        this.Height = bottomMost - topMost;
        Canvas.SetLeft(this, leftMost);
        Canvas.SetTop(this, topMost);

        this.Visibility = Visibility.Visible;
      }
    }
    Point CameraSpacePointToCanvasSpacePoint(
      KinectSensor sensor,
      CameraSpacePoint point)
    {
      float x = 0;
      float y = 0;

      if (this.frameDescription == null)
      {
        this.frameDescription =
          this.useInfrared ?
            sensor.InfraredFrameSource.FrameDescription :
            sensor.ColorFrameSource.FrameDescription;
      }
      if (this.useInfrared)
      {
        var mapped = sensor.CoordinateMapper.MapCameraPointToDepthSpace(point);
        x = mapped.X;
        y = mapped.Y;
      }
      else
      {
        var mapped = sensor.CoordinateMapper.MapCameraPointToColorSpace(point);
        x = mapped.X;
        y = mapped.Y;
      }

      Point p = new Point()
      {
        X = (x / this.frameDescription.Width) * this.canvas.ActualWidth,
        Y = (y / this.frameDescription.Height) * this.canvas.ActualHeight
      };
      return (p);
    }
    SingleBodyFrameReader reader;
    Canvas canvas;
    FrameDescription frameDescription;
    JointPairAverageMeasurementSet measurementSet;
    bool showingMeasurements;
    bool useInfrared;
    IControlServiceRegistry serviceRegistry;
    static int PHOTO_BOX_MARGIN = 96;
  
    static JointType[] boxJoints = 
    {
      JointType.Head, JointType.HandLeft, JointType.HandRight,
      JointType.HipLeft, JointType.HipRight, JointType.ElbowLeft,
      JointType.ElbowRight
    };

#if !NETFX_CORE
    Storyboard spin;
    static readonly string SPIN_STORYBOARD = "spin";
#endif
  }
}
