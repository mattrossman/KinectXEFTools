using KinectXEFTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XEFExtract
{
    class XEFFloorWriter : IXEFDataWriter, IDisposable
    {
        //
        //  Members
        //

        private StreamWriter _writer;

        private bool _seenEvent = false;

        //
        //  Properties
        //

        public string BodyFilePath { get; private set; }

        public string FilePath { get; private set; }

        public long EventCount { get; private set; }

        public TimeSpan StartTime { get; private set; }

        public TimeSpan EndTime { get; private set; }

        public TimeSpan Duration { get { return EndTime - StartTime; } }

        //
        //  Constructor
        //

        public XEFFloorWriter(string path)
        {
            FilePath = path;
            EventCount = 0;
            StartTime = TimeSpan.Zero;
            EndTime = TimeSpan.Zero;

            _writer = new StreamWriter(path);

            WriteHeaders();
        }

        ~XEFFloorWriter()
        {
            Dispose(false);
        }

        //
        //	IDisposable
        //

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _writer.Dispose();
                }

                disposed = true;
            }
        }

        //
        //  Methods
        //

        private void WriteHeaders()
        {
            _writer.WriteLine("EventIndex,Time,X,Y,Z,W");

        }

        public void Close()
        {
            Dispose(true);
        }

        public void ProcessEvent(XEFEvent ev)
        {
            if (ev.EventStreamDataTypeId != StreamDataTypeIds.Body)
            {
                return;
            }

            // Update start/end time
            if (!_seenEvent)
            {
                StartTime = ev.RelativeTime;
                _seenEvent = true;
            }
            EndTime = ev.RelativeTime;

            // Get raw body data
            XEFBodyFrame bodyFrame = XEFBodyFrame.FromByteArray(ev.EventData);

            // Write floor plane data
            XEFVector floorClipPlane = bodyFrame.FloorClipPlane;
            _writer.Write("{0},{1}",
                ev.EventIndex,
                ev.RelativeTime.Ticks);
            _writer.Write(",{0},{1},{2},{3}",
                floorClipPlane.x,
                floorClipPlane.y,
                floorClipPlane.z,
                floorClipPlane.w);
            _writer.WriteLine();

            EventCount++;
        }
    }
}
