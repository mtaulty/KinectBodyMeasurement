namespace SharedApp.Interfaces
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.IO;
  using System.Threading.Tasks;

#if NETFX_CORE
  using Windows.Foundation;
#else
  using System.Windows;
#endif

  interface IRenderBackground
  {
    Task<byte[]> RenderBackgroundToByteArrayAsync(Rect offset);
  }
}
