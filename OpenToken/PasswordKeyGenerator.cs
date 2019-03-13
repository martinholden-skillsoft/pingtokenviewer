using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenToken
{
    public class PasswordKeyGenerator
    {
        private PasswordKeyGenerator()
        {
        }

        public static byte[] Generate(string password, Token.CipherSuite cs, byte[] salt, int iterations)
        {
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations);
            switch (cs)
            {
                case Token.CipherSuite.AES_256_CBC:
                    return rfc2898DeriveBytes.GetBytes(32);
                case Token.CipherSuite.AES_128_CBC:
                    return rfc2898DeriveBytes.GetBytes(16);
                case Token.CipherSuite.DES3_168_CBC:
                    return rfc2898DeriveBytes.GetBytes(24);
                default:
                    return (byte[])null;
            }
        }

        public static byte[] Generate(string password, Token.CipherSuite cs)
        {
            return PasswordKeyGenerator.Generate(password, cs, new byte[8], 1000);
        }
    }
}
