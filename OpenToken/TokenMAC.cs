// Decompiled with JetBrains decompiler
// Type: opentoken.TokenMAC
// Assembly: opentoken-agent, Version=2.5.0.0, Culture=neutral, PublicKeyToken=51e867d115bd07ce
// MVID: DE6925CD-88FC-4A0D-8693-C20A8A4B9660
// Assembly location: C:\Users\mnholden\Downloads\opentoken.net\pf-dotnet-integration-kit\dist\opentoken-agent.dll

using System;
using System.IO;
using System.Security.Cryptography;

namespace OpenToken
{
    public class TokenMAC : ICryptoTransform, IDisposable
    {
        private MemoryStream ms = new MemoryStream();
        private HashAlgorithm transform;

        public byte[] Hash
        {
            get
            {
                return this.transform.Hash;
            }
        }

        public byte[] PreviewHash
        {
            get
            {
                byte[] array = this.ms.ToArray();
                TokenMAC tokenMac = new TokenMAC((byte[])null);
                tokenMac.Update(array);
                tokenMac.Finish();
                return tokenMac.Hash;
            }
        }

        public byte[] PreviewData
        {
            get
            {
                return this.ms.ToArray();
            }
        }

        public bool CanReuseTransform
        {
            get
            {
                return this.transform.CanReuseTransform;
            }
        }

        public bool CanTransformMultipleBlocks
        {
            get
            {
                return this.transform.CanTransformMultipleBlocks;
            }
        }

        public int InputBlockSize
        {
            get
            {
                return this.transform.InputBlockSize;
            }
        }

        public int OutputBlockSize
        {
            get
            {
                return this.transform.OutputBlockSize;
            }
        }

        public TokenMAC(byte[] key)
        {
            if (key == null)
                this.transform = (HashAlgorithm)new SHA1CryptoServiceProvider();
            else
                this.transform = (HashAlgorithm)new HMACSHA1(key);
        }

        public void Update(byte b)
        {
            byte[] data = new byte[1] { b };
            this.Update(data, 0, data.Length);
        }

        public void Update(byte[] data)
        {
            this.Update(data, 0, data.Length);
        }

        public void Update(byte[] data, int offset, int len)
        {
            this.ms.Write(data, offset, len);
            this.transform.TransformBlock(data, offset, len, data, offset);
        }

        public void Finish()
        {
            this.transform.TransformFinalBlock(new byte[0], 0, 0);
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            int count = this.transform.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            this.ms.Write(inputBuffer, inputOffset, count);
            return count;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            this.ms.Write(inputBuffer, inputOffset, inputCount);
            return this.transform.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
        }

        public void Dispose()
        {
            this.transform.Clear();
        }
    }
}
