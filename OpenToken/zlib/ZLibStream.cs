using ComponentAce.Compression.Libs.zlib;
using System;
using System.IO;

namespace OpenToken.zlib
{
    public class ZlibStream : Stream
    {
        private ZInputStream zis;
        private ZOutputStream zos;
        private ZlibStream.Mode mode;

        public override bool CanRead
        {
            get
            {
                return this.mode == ZlibStream.Mode.Decompress;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.mode == ZlibStream.Mode.Compress;
            }
        }

        public override long Length
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public override long Position
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public ZlibStream(Stream s, ZlibStream.Mode m)
        {
            this.mode = m;
            if (m == ZlibStream.Mode.Compress)
                this.zos = new ZOutputStream(s);
            else
                this.zis = new ZInputStream(s);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Math.Max(this.zis.read(buffer, offset, count), 0);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.zos.Write(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SetLength(long value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Close()
        {
            if (this.mode == ZlibStream.Mode.Compress)
                this.zos.Close();
            else
                this.zis.Close();
        }

        public enum Mode
        {
            Compress,
            Decompress,
        }
    }
}
