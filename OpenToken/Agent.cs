using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenToken
{
    /// <summary>
    /// 
    /// </summary>
    public class Agent
    {
        /// <summary>
        /// The token subject
        /// </summary>
        public const string TOKEN_SUBJECT = "subject";
        /// <summary>
        /// The token not before
        /// </summary>
        public const string TOKEN_NOT_BEFORE = "not-before";
        /// <summary>
        /// The token not on or after
        /// </summary>
        public const string TOKEN_NOT_ON_OR_AFTER = "not-on-or-after";
        /// <summary>
        /// The token renew until
        /// </summary>
        public const string TOKEN_RENEW_UNTIL = "renew-until";
        /// <summary>
        /// The last error
        /// </summary>
        private string lastError;
        /// <summary>
        /// The configuration
        /// </summary>
        private AgentConfiguration config;

        private bool useVerboseErrorMessages;

        /// <summary>
        /// Gets the last error.
        /// </summary>
        /// <value>
        /// The last error.
        /// </value>
        public string LastError
        {
            get
            {
                return this.lastError;
            }
        }

        public bool UseVerboseErrorMessages
        {
            get
            {
                return this.useVerboseErrorMessages;
            }
            set
            {
                this.useVerboseErrorMessages = value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        public Agent()
        {
            this.config = new AgentConfiguration();
            this.useVerboseErrorMessages = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        public Agent(Stream input)
        {
            this.config = new AgentConfiguration(input);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Agent(AgentConfiguration configuration)
        {
            this.config = configuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="file">The file.</param>
        public Agent(string file)
        {
            this.config = new AgentConfiguration(file);
        }

        /// <summary>
        /// Reads the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Dictionary<string, string> ReadToken(string token)
        {
            return ReadToken(token, true);
        }


        /// <summary>
        /// Reads the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="validateTimestamp">if set to <c>true</c> [validate timestamp].</param>
        /// <returns></returns>
        /// <exception cref="TokenException">Invalid dictionary type</exception>
        public Dictionary<string, string> ReadToken(string token, bool validateTimestamp)
        {
            IDictionary dictionary = (IDictionary)Token.decode(token, this.config.GetDecryptKey, this.config.UseVerboseErrorMessages);
            if (!(dictionary is MultiStringDictionary))
                throw new TokenException("Invalid dictionary type");
            Dictionary<string, string> returnValue = this.config.FlattenMultiStringDictionary((MultiStringDictionary)dictionary);
            if (validateTimestamp)
            {
                this.ValidateTimestamp(returnValue);
            }
            return returnValue;
        }

        /// <summary>
        /// Reads the token multi string dictionary.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public MultiStringDictionary ReadTokenMultiStringDictionary(string token)
        {
            return ReadTokenMultiStringDictionary(token, true);
        }

        /// <summary>
        /// Reads the token multi string dictionary.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="validateTimestamp">if set to <c>true</c> [validate timestamp].</param>
        /// <returns></returns>
        /// <exception cref="TokenException">Invalid dictionary type</exception>
        public MultiStringDictionary ReadTokenMultiStringDictionary(string token, bool validateTimestamp)
        {
            IDictionary dictionary = (IDictionary)Token.decode(token, this.config.GetDecryptKey, this.config.UseVerboseErrorMessages);
            if (!(dictionary is MultiStringDictionary))
                throw new TokenException("Invalid dictionary type");
            if (validateTimestamp)
            {
                this.ValidateTimestamp(this.config.FlattenMultiStringDictionary((MultiStringDictionary)dictionary));
            }
            return (MultiStringDictionary)dictionary;
        }


        /// <summary>
        /// Formats the zulu.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        protected static string FormatZulu(DateTime dateTime)
        {
            return dateTime.ToString("s", (IFormatProvider)DateTimeFormatInfo.InvariantInfo) + "Z";
        }

        /// <summary>
        /// Validates the timestamp.
        /// </summary>
        /// <param name="returnValue">The return value.</param>
        /// <exception cref="TokenException">
        /// Invalid token; token lifetime fields are improperly formatted.
        /// or
        /// Invalid token; not-on-or-after precedes not-before. ;notOnOrAfter=" + (object)dateTime3 + ";notBefore=" + (object)dateTime2
        /// or
        /// Invalid token; token is not yet valid (not-before > futureNow) ;notBefore=" + (object)dateTime2 + " ;futureNow=" + (object)dateTime1 + " ;not-before-tolerance=" + (object)this.config.NotBeforeTolerance
        /// </exception>
        /// <exception cref="TokenExpiredException">
        /// Invalid token; token has expired (now > not-on-or-after) ;now=" + (object)localTime + ";notOnOrAfter=" + (object)dateTime3
        /// or
        /// Invalid token; token may no longer be renewed (now > renew-until ;now=" + (object)localTime + ";renewUntil=" + (object)dateTime4
        /// </exception>
        protected void ValidateTimestamp(Dictionary<string, string> returnValue)
        {
            DateTime localTime = DateTime.UtcNow.ToLocalTime();
            DateTime dateTime1 = localTime;
            dateTime1 = dateTime1.Add(new TimeSpan(0, 0, this.config.NotBeforeTolerance));
            DateTime dateTime2;
            DateTime dateTime3;
            DateTime dateTime4;
            try
            {
                dateTime2 = DateTime.Parse(returnValue["not-before"]);
                dateTime3 = DateTime.Parse(returnValue["not-on-or-after"]);
                dateTime4 = DateTime.Parse(returnValue["renew-until"]);
            }
            catch (FormatException ex)
            {
                throw new TokenException("Invalid token; token lifetime fields are improperly formatted.", (Exception)ex);
            }
            if (dateTime2.CompareTo(dateTime3) > 0)
                throw new TokenException("Invalid token; not-on-or-after precedes not-before. ;notOnOrAfter=" + (object)dateTime3 + ";notBefore=" + (object)dateTime2);
            if (dateTime2.CompareTo(dateTime1) > 0)
                throw new TokenException("Invalid token; token is not yet valid (not-before > futureNow) ;notBefore=" + (object)dateTime2 + " ;futureNow=" + (object)dateTime1 + " ;not-before-tolerance=" + (object)this.config.NotBeforeTolerance);
            if (localTime.CompareTo(dateTime3) > 0)
                throw new TokenExpiredException("Invalid token; token has expired (now > not-on-or-after) ;now=" + (object)localTime + ";notOnOrAfter=" + (object)dateTime3);
            if (localTime.CompareTo(dateTime4) > 0)
                throw new TokenExpiredException("Invalid token; token may no longer be renewed (now > renew-until ;now=" + (object)localTime + ";renewUntil=" + (object)dateTime4);
        }
    }
}
