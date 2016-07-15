namespace SharedApp.Startup
{
  using BodyFrameReaders;
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Threading.Tasks;
  using TestWinRTProject.Configuration;
  using TestWinRTProject.Interfaces;

#if NETFX_CORE
  using WindowsPreview.Kinect;
  using Windows.UI.Xaml.Controls;
  using SharedApp.Interfaces;
#else
    using Microsoft.Kinect;
    using System.Windows.Controls;
  using SharedApp.Interfaces;
#endif

  class Bootstrapper : IControlServiceRegistry
  {
    public Bootstrapper(
      string measurementConfiguration,
      string cloudConfiguration,
      Grid rootGrid)
    {
      this.measurementConfiguration = measurementConfiguration;
      this.cloudConfiguration = cloudConfiguration;
      this.rootGrid = rootGrid;
      this.serviceRegistry = new Dictionary<Type, object>();
    }
    public void RegisterService<T, U>(U implementation)
      where T : class
      where U : class, T
    {
      this.serviceRegistry[typeof(T)] = implementation;
    }
    public bool HasService<T>() where T: class
    {
      return (this.serviceRegistry.ContainsKey(typeof(T)));
    }
    public T GetService<T>() where T : class
    {
      return (this.serviceRegistry[typeof(T)] as T);
    }
    public async Task InitialiseAsync()
    {
      await MeasurementConfiguration.LoadConfigurationFromFileAsync(
        this.measurementConfiguration);

      await GlobalConfiguration.LoadConfigurationFromFileAsync(
        this.cloudConfiguration);

      this.sensor = KinectSensor.GetDefault();
      this.sensor.Open();

      source = new TrackedBodyFrameSource(this.sensor,
        FrameSourceTypes.Color | FrameSourceTypes.Infrared);

      this.reader = source.OpenTrackedBodyReader();
      this.reader.FrameArrived += OnFrameArrived;
    }
    void OnFrameArrived(object sender, TrackedBodyFrameEventArgs e)
    {
      foreach (var control in this.rootGrid.Children)
      {
        if (control is IConsumeTrackingFrames)
        {
          ((IConsumeTrackingFrames)control).ConsumeFrame(e, this);
        }
      }
    }
    TrackedBodyFrameSource source;
    TrackedBodyFrameReader reader;
    KinectSensor sensor;
    Grid rootGrid;
    string measurementConfiguration;
    string cloudConfiguration;
    Dictionary<Type, object> serviceRegistry;
  }
}
