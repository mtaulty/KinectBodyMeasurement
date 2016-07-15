namespace TestWinRTProject.Controls
{
  using BodyFrameReaders;
  using TestWinRTProject.Interfaces;

#if NETFX_CORE
  using Windows.UI.Xaml.Controls;
  using SharedApp.Interfaces;
#else
  using System.Windows.Controls;
  using System.Windows;
  using Microsoft.Kinect;
  using SharedApp.Interfaces;
#endif

  public sealed partial class BodiesDisplayControl : 
    UserControl,
    IConsumeTrackingFrames
  {
    public BodiesDisplayControl()
    {
      this.InitializeComponent();
    }
    public bool UseInfrared { get; set; }
    public void ConsumeFrame(TrackedBodyFrameEventArgs args,
      IControlServiceRegistry serviceRegistry)
    {
      if (args.AddedBodyIds != null)
      {
        foreach (var addedBodyId in args.AddedBodyIds)
        {
          var reader = args.Source.OpenSingleBodyReader(addedBodyId);

          BodyDisplayControl control = new BodyDisplayControl(
            reader, this.canvas, this.UseInfrared, serviceRegistry);
        }
      }
    }
  }
}