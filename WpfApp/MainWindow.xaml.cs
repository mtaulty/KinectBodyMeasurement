namespace WpfApp
{
  using BodyFrameReaders;
using Microsoft.Kinect;
using SharedApp.Startup;
using System.Windows;
using TestWinRTProject.Configuration;
using TestWinRTProject.Interfaces;

  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      this.Loaded += OnLoaded;
    }
    async void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.bootstrapper = new Bootstrapper(
        MEASUREMENT_CONFIG_FILE,
        GLOBAL_CONFIG_FILE,
        this.rootGrid);

      this.bootstrapper.InitialiseAsync();
    }
    static readonly string MEASUREMENT_CONFIG_FILE =
      "ConfigFiles/measurements.json";

    static readonly string GLOBAL_CONFIG_FILE =
      "ConfigFiles/global.json";

    Bootstrapper bootstrapper;
  }
}
