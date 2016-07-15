namespace BodyFrameReaders
{
#if NETFX_CORE
  using WindowsPreview.Kinect;
#else
  using Microsoft.Kinect;
#endif
  using System.Collections.Generic;
  using System.Linq;
  using System;

  public class TrackedBodyFrameSource
  {
    public TrackedBodyFrameSource(KinectSensor sensor,
      FrameSourceTypes additionalFrameTypes = FrameSourceTypes.None)
    {
      this.sensor = sensor;
      this.additionalFrameTypes = additionalFrameTypes;
      this.trackedBodyFrameReaders = new List<TrackedBodyFrameReader>();
      this.singleBodyFrameReaders = new Dictionary<ulong, List<SingleBodyFrameReader>>();
      this.trackedBodyIds = new List<ulong>();
      this.bodies = new Body[this.sensor.BodyFrameSource.BodyCount];
      this.trackedBodyFrameEventArgs = new TrackedBodyFrameEventArgs()
      {
        Source = this
      };
      this.singleBodyFrameEventArgs = new SingleBodyFrameEventArgs()
      {
        Source = this
      };
    }
    public KinectSensor Sensor
    {
      get
      {
        return (this.sensor);
      }
    }
    public SingleBodyFrameReader OpenSingleBodyReader(ulong trackingId)
    {
      this.OpenIfClosed();
      SingleBodyFrameReader reader = new SingleBodyFrameReader(this);
      reader.TrackingId = trackingId;
      this.AddSingleBodyFrameReader(reader, trackingId);
      return (reader);
    }
    public TrackedBodyFrameReader OpenTrackedBodyReader()
    {
      this.OpenIfClosed();
      TrackedBodyFrameReader reader = new TrackedBodyFrameReader(this);
      this.trackedBodyFrameReaders.Add(reader);
      return (reader);
    }
    internal void RemoveDisposedTrackedBodyFrameReader(TrackedBodyFrameReader reader)
    {
      this.trackedBodyFrameReaders.Remove(reader);
      this.CloseIfIdle();
    }
    internal void RemoveDisposedSingleBodyFrameReader(SingleBodyFrameReader reader)
    {
      this.singleBodyFrameReaders[reader.TrackingId].Remove(reader);

      if (this.singleBodyFrameReaders[reader.TrackingId].Count == 0)
      {
        this.singleBodyFrameReaders.Remove(reader.TrackingId);
      }
      this.CloseIfIdle();
    }
    internal void AddSingleBodyFrameReader(SingleBodyFrameReader reader,
      ulong trackingId)
    {
      List<SingleBodyFrameReader> readers = null;

      if (!this.singleBodyFrameReaders.TryGetValue(trackingId, out readers))
      {
        readers = new List<SingleBodyFrameReader>();
        this.singleBodyFrameReaders[trackingId] = readers;
      }
      readers.Add(reader);
    }
    internal void RemoveSingleBodyFrameReadersForLostBody(ulong trackingId)
    {
      List<SingleBodyFrameReader> readers = null;

      if (this.singleBodyFrameReaders.TryGetValue(trackingId, out readers))
      {
        while (readers.Count > 0)
        {
          readers[0].FireTrackingIdLost();
        }
      }
      this.singleBodyFrameReaders.Remove(trackingId);

      this.CloseIfIdle();
    }
    void OpenIfClosed()
    {
      if (!this.isOpen)
      {
        this.reader = sensor.OpenMultiSourceFrameReader(
          FrameSourceTypes.Body | this.additionalFrameTypes);

        this.reader.MultiSourceFrameArrived += this.OnMultiSourceFrameArrived;

        this.isOpen = true;
      }
    }
    void CloseIfIdle()
    {
      if (this.isOpen && 
        (this.trackedBodyFrameReaders.Count == 0) &&
        (this.singleBodyFrameReaders.Count == 0))
      {
        this.reader.MultiSourceFrameArrived -= this.OnMultiSourceFrameArrived;
        this.reader.Dispose();
        this.reader = null;
        this.trackedBodyIds.Clear();
        this.isOpen = false;
      }
    }
    void OnMultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
    {
      if (e.FrameReference != null)
      {
        var frame = e.FrameReference.AcquireFrame();

        if (frame != null)
        {
          this.trackedBodyFrameEventArgs.ClearBodyFrameData();
          this.trackedBodyFrameEventArgs.PopulateAdditionalFrameData(frame);

          using (var bodyFrame = frame.BodyFrameReference.AcquireFrame())
          {
            if (bodyFrame != null)
            {
              bodyFrame.GetAndRefreshBodyData(this.bodies);

              var idToIndexMap = this.bodies
                .Select(
                  (body, index) => new { Body = body, Index = index })
                .Where(
                  entry => entry.Body.IsTracked)
                .ToDictionary(
                  entry => entry.Body.TrackingId, entry => entry.Index);  

              var trackedBodiesInFrame = this.bodies.Where(b => b.IsTracked).ToList();
              var idToBodyMap = trackedBodiesInFrame.ToDictionary(b => b.TrackingId);
              var trackedIdsInFrame = trackedBodiesInFrame.Select(b => b.TrackingId).ToList();

              var lostBodyIdsFromPreviousFrame =
                this.trackedBodyIds.Where(
                  id => !trackedIdsInFrame.Contains(id)).ToList();

              var newBodyIdsFromPreviousFrame =
                trackedIdsInFrame.Where(
                  id => !this.trackedBodyIds.Contains(id)).ToList();

              this.trackedBodyFrameEventArgs.PopulateBodyFrameData(
                idToBodyMap,
                newBodyIdsFromPreviousFrame,
                lostBodyIdsFromPreviousFrame);
              
              this.trackedBodyIds = trackedIdsInFrame;

              this.ProcessSingleFrameBodyReaders(
                idToIndexMap,
                idToBodyMap, 
                lostBodyIdsFromPreviousFrame);
            }
          }
          // whether we are tracking any bodies or not, we fire this event such that
          // a consumer gets their "additional frame types" like color, etc.
          foreach (var reader in this.trackedBodyFrameReaders)
          {
            reader.FireFrameArrived(this.trackedBodyFrameEventArgs);
          }
        }
      }
    }
    void ProcessSingleFrameBodyReaders(
      IReadOnlyDictionary<ulong,int> idToIndexMap,
      IReadOnlyDictionary<ulong,Body> idToBodyMap,
      IReadOnlyList<ulong> lostBodyIdsFromPreviousFrame)
    {
      foreach (var id in lostBodyIdsFromPreviousFrame)
      {
        this.RemoveSingleBodyFrameReadersForLostBody(id);
      }
      foreach (var id in this.singleBodyFrameReaders.Keys)
      {
        this.singleBodyFrameEventArgs.Body = idToBodyMap[id];
        this.singleBodyFrameEventArgs.BodyIndex = idToIndexMap[id];

        List<SingleBodyFrameReader> readers = this.singleBodyFrameReaders[id];

        foreach (var reader in readers)
        {
          reader.FireFrameArrived(this.singleBodyFrameEventArgs);
        }
      }
    }
    bool isOpen;
    Dictionary<ulong, List<SingleBodyFrameReader>> singleBodyFrameReaders;
    List<TrackedBodyFrameReader> trackedBodyFrameReaders;
    FrameSourceTypes additionalFrameTypes;
    List<ulong> trackedBodyIds;
    Body[] bodies;
    MultiSourceFrameReader reader;
    KinectSensor sensor;
    TrackedBodyFrameEventArgs trackedBodyFrameEventArgs;
    SingleBodyFrameEventArgs singleBodyFrameEventArgs;
  }
}