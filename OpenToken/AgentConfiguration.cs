using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenToken
{
    public class AgentConfiguration
    {
        public static bool USE_VERBOSE_TOKEN_EXCEPTION_MESSAGES_DEFAULT = false;
        public static int TOKEN_LIFETIME_DEFAULT = 300;
        public static int RENEW_UNTIL_LIFETIME_DEFALUT = 43200;
        public static int NOT_BEFORE_TOLERANCE_DEFAULT = 120;
        public static bool USE_SUN_JCE_DEFAULT = false;
        public static bool SESSION_COOKIE_DEFAULT = false;
        public static bool SECURE_COOKIE_DEFAULT = false;
        public static bool OBFUSCATE_PASSWORD_DEFAULT = true;
        public static bool USE_COOKIE_DEFAULT = false;
        public static string COOKIE_PATH_DEFAULT = "/";
        public static string COOKIE_DOMAIN_DEFAULT = "";
        public static string TOKEN_NAME_DEFAULT = "opentoken";
        public static string AGENT_CONFIG_FILE_DEFALUT = "agent-config.txt";
        private bool useVerboseErrorMessages = AgentConfiguration.USE_VERBOSE_TOKEN_EXCEPTION_MESSAGES_DEFAULT;
        private int tokenLifetime = AgentConfiguration.TOKEN_LIFETIME_DEFAULT;
        private int renewUntilLifetime = AgentConfiguration.RENEW_UNTIL_LIFETIME_DEFALUT;
        private int notBeforeTolerance = AgentConfiguration.NOT_BEFORE_TOLERANCE_DEFAULT;
        private bool useSunJCE = AgentConfiguration.USE_SUN_JCE_DEFAULT;
        private bool sessionCookie = AgentConfiguration.SESSION_COOKIE_DEFAULT;
        private bool secureCookie = AgentConfiguration.SECURE_COOKIE_DEFAULT;
        private bool obfuscatePassword = AgentConfiguration.OBFUSCATE_PASSWORD_DEFAULT;
        private bool useCookie = AgentConfiguration.USE_COOKIE_DEFAULT;
        private string cookiePath = AgentConfiguration.COOKIE_PATH_DEFAULT;
        private string cookieDomain = AgentConfiguration.COOKIE_DOMAIN_DEFAULT;
        private string tokenName = AgentConfiguration.TOKEN_NAME_DEFAULT;
        private byte[] passwordKey;
        private Token.KeyInfo passwordKeyInfo;
        public Token.KeyInfoCallback GetDecryptKey;
        public Token.EncryptionKeyCallback GetEncryptKey;
        private string password;

        public Token.KeyInfoCallback DecryptKey
        {
            get
            {
                return this.GetDecryptKey;
            }
            set
            {
                this.GetDecryptKey = value;
            }
        }

        public Token.EncryptionKeyCallback EncryptKey
        {
            get
            {
                return this.GetEncryptKey;
            }
            set
            {
                this.GetEncryptKey = value;
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

        public string CookieDomain
        {
            get
            {
                return this.cookieDomain;
            }
            set
            {
                this.cookieDomain = value;
            }
        }

        public string CookiePath
        {
            get
            {
                return this.cookiePath;
            }
            set
            {
                this.cookiePath = value;
            }
        }

        public string TokenName
        {
            get
            {
                return this.tokenName;
            }
            set
            {
                this.tokenName = value;
            }
        }

        public int TokenLifetime
        {
            get
            {
                return this.tokenLifetime;
            }
            set
            {
                this.tokenLifetime = value;
            }
        }

        public int RenewUntilLifetime
        {
            get
            {
                return this.renewUntilLifetime;
            }
            set
            {
                this.renewUntilLifetime = value;
            }
        }

        public bool UseCookie
        {
            get
            {
                return this.useCookie;
            }
            set
            {
                this.useCookie = value;
            }
        }

        public int NotBeforeTolerance
        {
            get
            {
                return this.notBeforeTolerance;
            }
            set
            {
                this.notBeforeTolerance = value;
            }
        }

        public bool UseSunJCE
        {
            get
            {
                return this.useSunJCE;
            }
            set
            {
                this.useSunJCE = value;
            }
        }

        public bool SessionCookie
        {
            get
            {
                return this.sessionCookie;
            }
            set
            {
                this.sessionCookie = value;
            }
        }

        public bool SecureCookie
        {
            get
            {
                return this.secureCookie;
            }
            set
            {
                this.secureCookie = value;
            }
        }

        public bool ObfuscatePassword
        {
            get
            {
                return this.obfuscatePassword;
            }
            set
            {
                this.obfuscatePassword = value;
            }
        }

        public AgentConfiguration(Stream configStream)
        {
            this.LoadConfiguration(configStream);
        }

        public AgentConfiguration()
        {
        }

        public AgentConfiguration(string file)
        {
            Stream baseStream = new StreamReader(file).BaseStream;
            if (baseStream == null)
                return;
            this.LoadConfiguration(baseStream);
        }

        public void SetPassword(string password, Token.CipherSuite cs)
        {
            this.password = password;
            this.UpdatePasswordBasedKeyConfiguration(cs);
        }

        public void LoadConfiguration(Stream input)
        {
            StreamReader streamReader = new StreamReader(input, Encoding.ASCII);
            Dictionary<string, string> config = this.FlattenMultiStringDictionary(KeyValueSerializer.deserialize((TextReader)streamReader));
            streamReader.Close();
            if (!config.ContainsKey("password"))
                throw new IOException("Required configuration parameter 'password' is not base-64 encoded.");
            AgentConfiguration.CheckString(config, "password");
            AgentConfiguration.CheckString(config, "cookie-path");
            AgentConfiguration.CheckString(config, "token-name");
            this.CheckInt(config, "token-renewuntil");
            this.CheckInt(config, "token-lifetime");
            this.CheckInt(config, "cipher-suite");
            this.CheckBool(config, "use-cookie");
            try
            {
                if (config.ContainsKey("obfuscate-password"))
                    this.ObfuscatePassword = bool.Parse(config["obfuscate-password"]);
                Token.CipherSuite cs = (Token.CipherSuite)byte.Parse(config["cipher-suite"]);
                this.SetPassword(!this.ObfuscatePassword ? Encoding.ASCII.GetString(Convert.FromBase64String(config["password"])) : Encoding.UTF8.GetString(Obfuscator.Deobfuscate(config["password"])), cs);
                if (config.ContainsKey("cookie-domain"))
                    this.CookieDomain = config["cookie-domain"];
                this.CookiePath = config["cookie-path"];
                this.TokenName = config["token-name"];
                this.RenewUntilLifetime = int.Parse(config["token-renewuntil"]);
                this.TokenLifetime = int.Parse(config["token-lifetime"]);
                this.UseCookie = bool.Parse(config["use-cookie"]);
                if (config.ContainsKey("token-notbefore-tolerance"))
                    this.NotBeforeTolerance = int.Parse(config["token-notbefore-tolerance"]);
                if (config.ContainsKey("secure-cookie"))
                    this.SecureCookie = bool.Parse(config["secure-cookie"]);
                if (config.ContainsKey("session-cookie"))
                    this.SessionCookie = bool.Parse(config["session-cookie"]);
                if (!config.ContainsKey("use-verbose-error-messages"))
                    return;
                this.UseVerboseErrorMessages = bool.Parse(config["use-verbose-error-messages"]);
            }
            catch (FormatException ex)
            {
                throw new IOException("Required configuration parameter 'password' is not base-64 encoded.", (Exception)ex);
            }
        }

        public Dictionary<string, string> FlattenMultiStringDictionary(MultiStringDictionary props)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (KeyValuePair<string, List<string>> prop in (Dictionary<string, List<string>>)props)
                dictionary.Add(prop.Key, prop.Value[0]);
            return dictionary;
        }

        public void StoreConfiguration(Stream output)
        {
            StreamWriter streamWriter = new StreamWriter(output);
            Dictionary<string, string> values = new Dictionary<string, string>();
            values["obfuscate-password"] = this.ObfuscatePassword.ToString();
            values["password"] = !this.ObfuscatePassword ? Convert.ToBase64String(Encoding.ASCII.GetBytes(this.password)) : Encoding.UTF8.GetString(Obfuscator.Obfuscate(this.password));
            values["cookie-path"] = this.CookiePath;
            values["token-renewuntil"] = this.RenewUntilLifetime.ToString();
            values["cookie-domain"] = this.CookieDomain;
            values["token-lifetime"] = this.TokenLifetime.ToString();
            values["use-cookie"] = this.UseCookie.ToString();
            values["cipher-suite"] = ((byte)this.passwordKeyInfo.cs).ToString();
            values["token-name"] = this.TokenName;
            values["token-notbefore-tolerance"] = this.NotBeforeTolerance.ToString();
            values["secure-cookie"] = this.SecureCookie.ToString();
            values["session-cookie"] = this.SessionCookie.ToString();
            values["use-verbose-error-messages"] = this.UseVerboseErrorMessages.ToString();
            KeyValueSerializer.serialize(values, (TextWriter)streamWriter);
            streamWriter.Close();
        }

        private Token.KeyInfo PasswordBasedEncryptKey()
        {
            return this.passwordKeyInfo;
        }

        private byte[] PasswordBasedDecryptKey(byte[] keyinfo)
        {
            return this.passwordKey;
        }

        private void CheckBool(Dictionary<string, string> config, string name)
        {
            AgentConfiguration.CheckString(config, name);
            bool result;
            if (!bool.TryParse(config[name], out result))
                throw new IOException("Required configuration parameter '" + name + "' is not a valid bool.");
        }

        private void CheckInt(Dictionary<string, string> config, string name)
        {
            AgentConfiguration.CheckString(config, name);
            int result;
            if (!int.TryParse(config[name], out result))
                throw new IOException("Required configuration parameter '" + name + "' is not a valid integer.");
        }

        private static void CheckString(Dictionary<string, string> config, string name)
        {
            if (!config.ContainsKey(name) || config[name].Trim().Length == 0)
                throw new IOException("Required configuration parameter '" + name + "' is missing or empty.");
        }

        private void UpdatePasswordBasedKeyConfiguration(Token.CipherSuite cs)
        {
            this.passwordKey = PasswordKeyGenerator.Generate(this.password, cs);
            this.passwordKeyInfo = new Token.KeyInfo(this.passwordKey, (byte[])null, cs);
            this.DecryptKey = new Token.KeyInfoCallback(this.PasswordBasedDecryptKey);
            this.EncryptKey = new Token.EncryptionKeyCallback(this.PasswordBasedEncryptKey);
        }
    }
}
