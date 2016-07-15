namespace TestWinRTProject.Interfaces
{
  using BodyFrameReaders;
  using SharedApp.Interfaces;

  interface IConsumeTrackingFrames
  {
    void ConsumeFrame(TrackedBodyFrameEventArgs args, 
      IControlServiceRegistry registry);
  }
}
