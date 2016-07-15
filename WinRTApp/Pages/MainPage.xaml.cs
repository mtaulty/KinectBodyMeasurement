namespace TestWinRTProject.Pages
{
  using BodyFrameReaders;
  using SharedApp.Startup;
  using System;
  using TestWinRTProject.Configuration;
  using TestWinRTProject.Interfaces;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using WindowsPreview.Kinect;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.bootstrapper = new Bootstrapper(
        MEASUREMENT_CONFIG_FILE,
        GLOBAL_CONFIG_FILE,
        this.rootGrid);

      this.bootstrapper.InitialiseAsync();
    }
    Bootstrapper bootstrapper;

    static readonly string MEASUREMENT_CONFIG_FILE =
      "ms-appx:///ConfigFiles/measurements.json";

    static readonly string GLOBAL_CONFIG_FILE =
      "ms-appx:///ConfigFiles/global.json";
  }
}
