
namespace TestWinRTProject.Controls
{
  using BodyFrameReaders;
  using System;
  using System.IO;
  using TestWinRTProject.Interfaces;
  using SharedApp.Interfaces;
  using System.Threading.Tasks;
  using System.Runtime.InteropServices;
  using TestWinRTProject.Configuration;

#if NETFX_CORE
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Media.Imaging;
  using WindowsPreview.Kinect;
  using System.Runtime.InteropServices.WindowsRuntime;
  using Windows.Foundation;
  using Windows.Graphics.Imaging;
  using Windows.Storage.Streams;

  [Guid("905a0fef-bc53-11df-8c49-001e4fc686da"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IBufferByteAccess
  {
    unsafe void Buffer(out byte* pByte);
  }

#else
  using Microsoft.Kinect;
  using System.Windows.Controls;
  using System.Windows.Media;
  using System.Windows;
  using System.Windows.Media.Imaging;
#endif

  public sealed partial class VideoControl :
    UserControl,
    IConsumeTrackingFrames,
    IRenderBackground
  {
    public VideoControl()
    {
      this.InitializeComponent();
    }
    // NB: changing this part way through the lifecycle won't work...
    public bool UseInfrared { get; set; }
    public void ConsumeFrame(TrackedBodyFrameEventArgs args,
      IControlServiceRegistry serviceRegistry)
    {
      this.Initialise(args, serviceRegistry);

      if (this.UseInfrared)
      {
        this.ProcessIrFrame(args);
      }
      else
      {
        this.ProcessColourFrame(args);
      }
    }
    void ProcessIrFrame(TrackedBodyFrameEventArgs args)
    {
      if (args.InfraredFrameReference != null)
      {
        using (var frame = args.InfraredFrameReference.AcquireFrame())
        {
          if (frame != null)
          {
#if NETFX_CORE
            var irBuffer = frame.LockImageBuffer();
            IBufferByteAccess irBufferByteAccess = (IBufferByteAccess)irBuffer;
            IBufferByteAccess imageByteAccess = (IBufferByteAccess)this.imageSource.PixelBuffer;

            unsafe
            {
              byte* pIrBuffer = null;
              irBufferByteAccess.Buffer(out pIrBuffer);
              byte* pImageBuffer = null;
              imageByteAccess.Buffer(out pImageBuffer);

              CopyIrBytes((ushort*)pIrBuffer, (UInt32*)pImageBuffer);
            }

            Marshal.ReleaseComObject(irBuffer);

            this.imageSource.Invalidate();
#else
            this.imageSource.Lock();

            unsafe
            {
              UInt32* pImageBuffer = (UInt32*)this.imageSource.BackBuffer.ToPointer();

              using (var locked = frame.LockImageBuffer())
              {
                ushort* pIrBuffer = (ushort*)locked.UnderlyingBuffer.ToPointer();
              
                CopyIrBytes(pIrBuffer, pImageBuffer);
              }
            }
            this.imageSource.AddDirtyRect(this.dirtyRect);
            this.imageSource.Unlock();
#endif
          }
        }
      }
    }
    unsafe void CopyIrBytes(ushort* pIrBuffer, UInt32* pBgraImageBuffer)
    {
      byte lowValue = GlobalConfiguration.Instance.IrScaleLow;
      byte range = GlobalConfiguration.Instance.IrScaleRange;
      var size = this.frameDescription.Width * this.frameDescription.Height;

      for (int i = 0; i < size; i++)
      {
        byte scaledValue =
          (byte)(((*pIrBuffer) / (double)ushort.MaxValue) * range);

        scaledValue += lowValue;

        *pBgraImageBuffer = (UInt32)(
          scaledValue << (byte)16 |
          scaledValue << (byte)8 |
          scaledValue);

        *pBgraImageBuffer |= (0xFF000000);

        pIrBuffer++;
        pBgraImageBuffer++;
      }
    }
    void ProcessColourFrame(TrackedBodyFrameEventArgs args)
    {
      if (args.ColorFrameReference != null)
      {
        using (var frame = args.ColorFrameReference.AcquireFrame())
        {
          if (frame != null)
          {
#if NETFX_CORE
            frame.CopyConvertedFrameDataToBuffer(
              this.imageSource.PixelBuffer,
              ColorImageFormat.Bgra);

            this.imageSource.Invalidate();
#else
            this.imageSource.Lock();
            
            frame.CopyConvertedFrameDataToIntPtr(
              this.imageSource.BackBuffer,
              (uint)(this.frameDescription.Width * 
                this.frameDescription.Height * this.frameDescription.BytesPerPixel),
              ColorImageFormat.Bgra);

            this.imageSource.AddDirtyRect(this.dirtyRect);

            this.imageSource.Unlock();
#endif
          }
        }
      }
    }
    void Initialise(TrackedBodyFrameEventArgs args,
      IControlServiceRegistry serviceRegistry)
    {
      if (!this.initialised)
      {
        this.initialised = true;

        serviceRegistry.RegisterService<IRenderBackground,VideoControl>(this);

        if (this.UseInfrared)
        {
          this.frameDescription = args.Source.Sensor.InfraredFrameSource.FrameDescription;
        }
        else
        {
          this.frameDescription =
            args.Source.Sensor.ColorFrameSource.CreateFrameDescription(
              ColorImageFormat.Bgra);
        }

#if NETFX_CORE 
        this.imageSource = new WriteableBitmap(this.frameDescription.Width, 
          this.frameDescription.Height);
#else
        this.imageSource = new WriteableBitmap(this.frameDescription.Width,
          this.frameDescription.Height, 96.0d, 96.0d, PixelFormats.Bgra32, null);

        this.dirtyRect = new Int32Rect(0, 0, this.frameDescription.Width,
          this.frameDescription.Height);
#endif

        this.image.Source = this.imageSource;
      }
    }
    public async Task<byte[]> RenderBackgroundToByteArrayAsync(Rect offset)
    {
      // The co-ordinates that come to us here are in terms of a Canvas that has been
      // stretched to fill our entire window. We are drawing frames from a video
      // that have also been stretched/compressed to fit that same space.
      // If we now want to crop out a section from that video then we need to make
      // sure that the coordinate systems line up.

      // TODO: both the WPF and the WinRT versions of this are copying too many
      // buffers around :-(
      byte[] buffer = null;

#if !NETFX_CORE

      MemoryStream memoryStream = new MemoryStream();

      Int32Rect cropRect = new Int32Rect(
        (int)(offset.X * this.imageSource.Width / this.ActualWidth),
        (int)(offset.Y * this.imageSource.Height / this.ActualHeight),
        (int)(offset.Width * this.imageSource.Width / this.ActualWidth),
        (int)(offset.Height * this.imageSource.Height / this.ActualHeight));

      // TODO: should we be creating new ones of these all the time?
      JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();

      CroppedBitmap cropped = new CroppedBitmap(this.imageSource, cropRect);

      jpegEncoder.Frames.Add(BitmapFrame.Create(cropped));

      jpegEncoder.Save(memoryStream);

      memoryStream.Seek(0, SeekOrigin.Begin);

      buffer = new byte[memoryStream.Length];

      memoryStream.Read(buffer, 0, buffer.Length);

#else
      // TODO: tried using a .NET MemoryStream here and my code seemed to hang in a
      // quite nasty way so had to make an InMemoryRandomAccessStream.
      using (InMemoryRandomAccessStream winStream = new InMemoryRandomAccessStream())
      {
        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(
          BitmapEncoder.JpegEncoderId,
          winStream);

        // TODO: that call to ToArray() below is making a copy of the IBuffer. Not really
        // what I want.
        encoder.SetPixelData(
          BitmapPixelFormat.Bgra8,
          BitmapAlphaMode.Premultiplied,
          (uint)this.imageSource.PixelWidth,
          (uint)this.imageSource.PixelHeight,
          96.0,
          96.0,
          this.imageSource.PixelBuffer.ToArray());

        encoder.BitmapTransform.Bounds = new BitmapBounds()
        {
          X = (uint)(offset.X * this.imageSource.PixelWidth / this.ActualWidth),
          Y = (uint)(offset.Y * this.imageSource.PixelHeight / this.ActualHeight),
          Width = (uint)(offset.Width * this.imageSource.PixelWidth / this.ActualWidth),
          Height = (uint)(offset.Height * this.imageSource.PixelHeight / this.ActualHeight)
        };
        await encoder.FlushAsync();

        winStream.Seek(0);

        // TODO: A copy.
        buffer = new byte[winStream.Size];

        await winStream.ReadAsync(buffer.AsBuffer(), (uint)buffer.Length, InputStreamOptions.None);
      }      
#endif
      return (buffer);
    }
    bool initialised;
    WriteableBitmap imageSource;
    FrameDescription frameDescription;

#if !NETFX_CORE
    Int32Rect dirtyRect;
#endif
  }
}