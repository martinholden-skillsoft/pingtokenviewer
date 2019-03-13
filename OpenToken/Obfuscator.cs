using System;
using System.Security.Cryptography;


namespace OpenToken
{
    public class Obfuscator
    {
        private static char[] encoded_key_char = new char[32]
        {
      'W',
      's',
      't',
      'l',
      'C',
      'l',
      'i',
      'P',
      'D',
      'q',
      'x',
      'M',
      'j',
      '1',
      'o',
      'O',
      '4',
      '7',
      'q',
      'l',
      'C',
      'm',
      'P',
      'l',
      'I',
      'H',
      'm',
      '4',
      'x',
      '6',
      'y',
      'T'
        };
        private static char[] encoded_iv_char = new char[12]
        {
      'F',
      '1',
      'j',
      'D',
      'N',
      'R',
      's',
      'V',
      'l',
      'h',
      'U',
      '='
        };

        public static byte[] Deobfuscate(string obfuscatedPassword)
        {
            ICryptoTransform decryptor = new TripleDESCryptoServiceProvider().CreateDecryptor(Convert.FromBase64CharArray(Obfuscator.encoded_key_char, 0, 32), Convert.FromBase64CharArray(Obfuscator.encoded_iv_char, 0, 12));
            byte[] inputBuffer = Convert.FromBase64String(obfuscatedPassword);
            return decryptor.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
        }

        public static byte[] Obfuscate(string password)
        {
            ICryptoTransform encryptor = new TripleDESCryptoServiceProvider().CreateEncryptor(Convert.FromBase64CharArray(Obfuscator.encoded_key_char, 0, 32), Convert.FromBase64CharArray(Obfuscator.encoded_iv_char, 0, 12));
            byte[] inputBuffer = Convert.FromBase64String(password);
            return encryptor.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
        }
    }
}
