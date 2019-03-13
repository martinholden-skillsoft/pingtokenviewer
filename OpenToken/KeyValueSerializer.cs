using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenToken
{
    public class KeyValueSerializer
    {
        public static void serialize(Dictionary<string, string> values, TextWriter writer)
        {
            foreach (KeyValuePair<string, string> keyValuePair in values)
            {
                string str1 = keyValuePair.Key.ToString().Trim();
                string str2 = keyValuePair.Value.ToString();
                if (str1.Contains("="))
                    throw new TokenException("Serialization failed; key contained invalid characters.");
                writer.Write("{0}={1}\n", (object)str1, (object)KeyValueSerializer.escapeValue(str2));
            }
        }

        public static void serialize(MultiStringDictionary values, TextWriter writer)
        {
            foreach (KeyValuePair<string, List<string>> keyValuePair in (Dictionary<string, List<string>>)values)
            {
                string str1 = keyValuePair.Key.ToString().Trim();
                if (str1.Contains("="))
                    throw new TokenException("Serialization failed; key contained invalid characters.");
                foreach (string str2 in keyValuePair.Value)
                    writer.Write("{0}={1}\n", (object)str1, (object)KeyValueSerializer.escapeValue(str2));
            }
        }

        public static string serialize(Dictionary<string, string> values)
        {
            StringWriter stringWriter = new StringWriter();
            KeyValueSerializer.serialize(values, (TextWriter)stringWriter);
            return stringWriter.ToString();
        }

        public static string serialize(MultiStringDictionary values)
        {
            StringWriter stringWriter = new StringWriter();
            KeyValueSerializer.serialize(values, (TextWriter)stringWriter);
            return stringWriter.ToString();
        }

        private static string escapeValue(string value)
        {
            bool flag = false;
            int num1 = 0;
            int num2 = 0;
            int num3 = 0;
            if (value == null || value.Length < 1)
                return "";
            for (int index = 0; index < value.Length; ++index)
            {
                switch (value[index])
                {
                    case '\'':
                        ++num1;
                        break;
                    case '\\':
                        ++num3;
                        break;
                    case '\t':
                    case '\n':
                    case ' ':
                        flag = true;
                        break;
                    case '"':
                        ++num2;
                        break;
                }
            }
            if (flag || num1 > 0 || (num2 > 0 || num3 > 0))
                return "'" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("'", "\\'") + "'";
            return value;
        }

        private static string unescapeValue(string value)
        {
            return value.Replace("\\\"", "\"").Replace("\\'", "'").Replace("\\\\", "\\");
        }

        public static MultiStringDictionary deserialize(string data)
        {
            return KeyValueSerializer.deserialize((TextReader)new StringReader(data));
        }

        public static MultiStringDictionary deserialize(TextReader reader)
        {
            MultiStringDictionary stringDictionary = new MultiStringDictionary();
            KeyValueSerializer.State state = KeyValueSerializer.State.LINE_START;
            char ch1 = char.MinValue;
            StringBuilder fullToken = new StringBuilder();
            StringBuilder token = new StringBuilder();
            string key = (string)null;
            int num;
            while ((num = reader.Read()) != -1)
            {
                char ch2 = Convert.ToChar(num);
                fullToken.Append(ch2);
                switch (ch2)
                {
                    case '\'':
                    case '"':
                        if (state == KeyValueSerializer.State.IN_QUOTED_VALUE)
                        {
                            if ((int)ch2 == (int)ch1 && !KeyValueSerializer.isEscaped(token))
                            {
                                stringDictionary.Add(key, KeyValueSerializer.unescapeValue(token.ToString()));
                                token.Length = 0;
                                state = KeyValueSerializer.State.LINE_END;
                                continue;
                            }
                            token.Append(ch2);
                            continue;
                        }
                        if (state == KeyValueSerializer.State.VALUE_START)
                        {
                            state = KeyValueSerializer.State.IN_QUOTED_VALUE;
                            ch1 = ch2;
                            continue;
                        }
                        continue;
                    case '=':
                        if (state == KeyValueSerializer.State.IN_KEY)
                        {
                            key = token.ToString();
                            token.Length = 0;
                            state = KeyValueSerializer.State.VALUE_START;
                            continue;
                        }
                        if (state == KeyValueSerializer.State.IN_VALUE)
                        {
                            token.Append(ch2);
                            continue;
                        }
                        if (state == KeyValueSerializer.State.EMPTY_SPACE)
                        {
                            token.Length = 0;
                            state = KeyValueSerializer.State.VALUE_START;
                            continue;
                        }
                        if (state == KeyValueSerializer.State.IN_QUOTED_VALUE)
                        {
                            token.Append(ch2);
                            continue;
                        }
                        continue;
                    case '\t':
                    case ' ':
                        if (state == KeyValueSerializer.State.IN_KEY)
                        {
                            key = token.ToString();
                            token.Length = 0;
                            state = KeyValueSerializer.State.EMPTY_SPACE;
                            continue;
                        }
                        if (state == KeyValueSerializer.State.IN_VALUE)
                        {
                            stringDictionary.Add(key, KeyValueSerializer.unescapeValue(token.ToString()));
                            token.Length = 0;
                            state = KeyValueSerializer.State.LINE_END;
                            continue;
                        }
                        if (state == KeyValueSerializer.State.IN_QUOTED_VALUE)
                        {
                            token.Append(ch2);
                            continue;
                        }
                        continue;
                    case '\n':
                        if (state == KeyValueSerializer.State.IN_VALUE || state == KeyValueSerializer.State.VALUE_START)
                        {
                            stringDictionary.Add(key, KeyValueSerializer.unescapeValue(token.ToString()));
                            token.Length = 0;
                            state = KeyValueSerializer.State.LINE_START;
                            continue;
                        }
                        if (state == KeyValueSerializer.State.LINE_END)
                        {
                            token.Length = 0;
                            state = KeyValueSerializer.State.LINE_START;
                            continue;
                        }
                        if (state == KeyValueSerializer.State.IN_QUOTED_VALUE)
                        {
                            token.Append(ch2);
                            continue;
                        }
                        continue;
                    default:
                        if (state == KeyValueSerializer.State.LINE_START)
                            state = KeyValueSerializer.State.IN_KEY;
                        else if (state == KeyValueSerializer.State.VALUE_START)
                            state = KeyValueSerializer.State.IN_VALUE;
                        token.Append(ch2);
                        continue;
                }
            }
            if (state == KeyValueSerializer.State.IN_QUOTED_VALUE || state == KeyValueSerializer.State.IN_VALUE)
                stringDictionary.Add(key, KeyValueSerializer.unescapeValue(token.ToString()));

            if (fullToken.Length != 0)
            {
                stringDictionary.Add("RAWTOKEN",fullToken.ToString());
                stringDictionary.Add("UNESCAPEDTOKEN", KeyValueSerializer.unescapeValue(fullToken.ToString()));
            }
            return stringDictionary;
        }

        private static bool isEscaped(StringBuilder token)
        {
            int num = 0;
            for (int index = token.Length - 1; index >= 0 && (int)token[index] == 92; --index)
                ++num;
            return (num & 1) == 1;
        }

        private enum State
        {
            LINE_START,
            EMPTY_SPACE,
            VALUE_START,
            LINE_END,
            IN_KEY,
            IN_VALUE,
            IN_QUOTED_VALUE,
        }
    }
}
