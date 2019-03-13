using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenToken
{
    [Serializable]
    public sealed class TokenExpiredException : Exception
    {
        public TokenExpiredException()
        {
        }

        public TokenExpiredException(string message)
          : base(message)
        {
        }

        public TokenExpiredException(string message, Exception inner)
          : base(message, inner)
        {
        }
    }
}
