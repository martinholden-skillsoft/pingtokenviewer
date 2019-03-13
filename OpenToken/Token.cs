using OpenToken.zlib;
using System;
using System.IO;
using System.Security.Cryptography;

namespace OpenToken
{
    public class Token
    {
        private static byte[] V1_HEADER = new byte[4]
        {
      (byte) 79,
      (byte) 84,
      (byte) 75,
      (byte) 1
        };
        private static byte[] V1_MAC = new byte[20];
        private static int V1_MAC_POS = 5;
        private static int V1_CS_POS = 4;
        private static int V1_IVLEN_POS = 25;
        private static int V1_IV_POS = 26;

        private Token()
        {
        }

        public static MultiStringDictionary decode(string token, byte[] key, bool useVerboseErrorMessages)
        {
            Token.KeyInfoCallback cb = (Token.KeyInfoCallback)(keyinfo => key);
            return Token.decode(token, cb, useVerboseErrorMessages);
        }

        public static MultiStringDictionary decode(string token, Token.KeyInfoCallback cb, bool useVerboseErrorMssages)
        {
            MultiStringDictionary stringDictionary;
            try
            {
                byte[] buffer = Token.b64decode(token);
                if (buffer == null)
                    throw new TokenException("Base64 decoding of token failed.");
                for (int index = 0; index < Token.V1_HEADER.Length; ++index)
                {
                    if ((int)buffer[index] != (int)Token.V1_HEADER[index])
                        throw new TokenException("Invalid token header.");
                }
                Token.CipherSuite cs = (Token.CipherSuite)buffer[Token.V1_CS_POS];
                if (cs < Token.CipherSuite.NULL || cs > Token.CipherSuite.DES3_168_CBC)
                    throw new TokenException("Unknown cipher suite used in token.");
                byte[] numArray1 = new byte[Token.V1_MAC.Length];
                Array.Copy((Array)buffer, Token.V1_MAC_POS, (Array)numArray1, 0, Token.V1_MAC.Length);
                int length = (int)buffer[Token.V1_IVLEN_POS];
                if (!Token.validateIV(length, cs))
                    throw new TokenException("Decode failed; IV length does not work with selected cipher suite.");
                byte[] numArray2 = (byte[])null;
                if (length > 0)
                {
                    numArray2 = new byte[length];
                    Array.Copy((Array)buffer, Token.V1_IV_POS, (Array)numArray2, 0, length);
                }
                int index1 = Token.V1_IV_POS + length;
                int num = (int)buffer[index1];
                byte[] numArray3 = (byte[])null;
                if (num > 0)
                    throw new TokenException("Decode failed; key info matadata value not supported.");
                byte[] key = cb(numArray3);
                if (!Token.validateKey(cs, key))
                    throw new TokenException("Decode failed; cipher suite in token does not work with provided key.");
                SymmetricAlgorithm symmetricAlgorithm = Token.setupCipher(cs, key);
                if (numArray2 != null)
                    symmetricAlgorithm.IV = numArray2;
                TokenMAC tokenMac = new TokenMAC(key);
                tokenMac.Update((byte)1);
                tokenMac.Update((byte)cs);
                if (numArray2 != null)
                    tokenMac.Update(symmetricAlgorithm.IV);
                if (numArray3 != null)
                    tokenMac.Update(numArray3);
                int offset = index1 + num + 1;
                int count = (int)Token.networkToShort(buffer, offset);
                int index2 = offset + 2;
                Stream stream = (Stream)new MemoryStream(buffer, index2, count, false);
                if (symmetricAlgorithm != null)
                    stream = (Stream)new CryptoStream(stream, symmetricAlgorithm.CreateDecryptor(), CryptoStreamMode.Read);



                StreamReader streamReader = new StreamReader((Stream)new CryptoStream((Stream)new ZlibStream(stream, ZlibStream.Mode.Decompress), (ICryptoTransform)tokenMac, CryptoStreamMode.Read));
                try
                {
                    stringDictionary = KeyValueSerializer.deserialize((TextReader)streamReader);
                }
                catch (Exception ex)
                {
                    throw new TokenException("Stream error occurred while decoding the token.", ex);
                }
                for (int index3 = 0; index3 < Token.V1_MAC.Length; ++index3)
                {
                    if ((int)tokenMac.Hash[index3] != (int)numArray1[index3])
                        throw new TokenException("MAC verification failed.");
                }
            }
            catch (Exception ex)
            {
                if (useVerboseErrorMssages)
                    throw new TokenException(ex.Message, ex);
                throw new TokenException("Error");
            }
            return stringDictionary;
        }

        private static bool validateKey(Token.CipherSuite cs, byte[] key)
        {
            switch (cs)
            {
                case Token.CipherSuite.NULL:
                    return key == null;
                case Token.CipherSuite.AES_256_CBC:
                    return key.Length == 32;
                case Token.CipherSuite.AES_128_CBC:
                    return key.Length == 16;
                case Token.CipherSuite.DES3_168_CBC:
                    return key.Length == 24;
                default:
                    return false;
            }
        }

        private static bool validateIV(int ivlen, Token.CipherSuite cs)
        {
            switch (cs)
            {
                case Token.CipherSuite.NULL:
                    return ivlen == 0;
                case Token.CipherSuite.AES_256_CBC:
                case Token.CipherSuite.AES_128_CBC:
                    return ivlen == 16;
                case Token.CipherSuite.DES3_168_CBC:
                    return ivlen == 8;
                default:
                    return false;
            }
        }

        private static SymmetricAlgorithm setupCipher(Token.CipherSuite cs, byte[] key)
        {
            SymmetricAlgorithm symmetricAlgorithm = (SymmetricAlgorithm)null;
            switch (cs)
            {
                case Token.CipherSuite.AES_256_CBC:
                case Token.CipherSuite.AES_128_CBC:
                    symmetricAlgorithm = (SymmetricAlgorithm)new RijndaelManaged();
                    break;
                case Token.CipherSuite.DES3_168_CBC:
                    symmetricAlgorithm = (SymmetricAlgorithm)new TripleDESCryptoServiceProvider();
                    break;
            }
            if (symmetricAlgorithm != null)
            {
                symmetricAlgorithm.Mode = CipherMode.CBC;
                symmetricAlgorithm.Padding = PaddingMode.PKCS7;
                symmetricAlgorithm.Key = key;
            }
            return symmetricAlgorithm;
        }

        private static byte[] b64decode(string data)
        {
            char[] inArray = new char[data.Length];
            for (int index = 0; index < data.Length; ++index)
            {
                char ch = data[index];
                switch (ch)
                {
                    case '*':
                        inArray[index] = '=';
                        break;
                    case '-':
                        inArray[index] = '+';
                        break;
                    case '_':
                        inArray[index] = '/';
                        break;
                    default:
                        inArray[index] = ch;
                        break;
                }
            }
            return Convert.FromBase64CharArray(inArray, 0, inArray.Length);
        }

        private static string b64encode(byte[] data)
        {
            string base64String = Convert.ToBase64String(data, Base64FormattingOptions.None);
            char[] charArray = base64String.ToCharArray();
            for (int index = 0; index < base64String.Length; ++index)
            {
                switch (charArray[index])
                {
                    case '+':
                        charArray[index] = '-';
                        break;
                    case '/':
                        charArray[index] = '_';
                        break;
                    case '=':
                        charArray[index] = '*';
                        break;
                }
            }
            return new string(charArray, 0, charArray.Length);
        }

        private static byte[] shortToNetwork(ushort value)
        {
            return new byte[2]
            {
        (byte) ((int) value >> 8 & (int) byte.MaxValue),
        (byte) ((uint) value & (uint) byte.MaxValue)
            };
        }

        private static ushort networkToShort(byte[] value, int offset)
        {
            return (ushort)((uint)(ushort)(0U + (uint)(ushort)(((int)value[offset] & (int)byte.MaxValue) << 8)) + (uint)(ushort)((uint)value[offset + 1] & (uint)byte.MaxValue));
        }

        public delegate byte[] KeyInfoCallback(byte[] keyinfo);

        public delegate Token.KeyInfo EncryptionKeyCallback();

        public struct KeyInfo
        {
            public byte[] key;
            public byte[] keyinfo;
            public Token.CipherSuite cs;

            public KeyInfo(byte[] key, byte[] keyinfo, Token.CipherSuite cs)
            {
                this.key = key;
                this.keyinfo = keyinfo;
                this.cs = cs;
            }
        }

        public enum CipherSuite : byte
        {
            NULL,
            AES_256_CBC,
            AES_128_CBC,
            DES3_168_CBC,
        }
    }
}
