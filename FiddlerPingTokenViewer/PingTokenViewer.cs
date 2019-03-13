using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using Fiddler;
using OpenToken;
using System.IO;
using FiddlerPingTokenViewer.Encoding;

namespace FiddlerPingTokenViewer
{
    public partial class PingTokenViewer : UserControl
    {
        CustomPropertyList myProperties = new CustomPropertyList();

        string tokenName = "";
        string pingConfig = "";

        public PingTokenViewer()
        {
            InitializeComponent();

            this.tokenName = FiddlerApplication.Prefs.GetStringPref(PreferenceNames.PING_TOKEN, "");
            this.pingConfig = System.Text.Encoding.UTF8.DecodeBase64(FiddlerApplication.Prefs.GetStringPref(PreferenceNames.PING_CONFIG, ""));

            //Fiddler.FiddlerApplication.Log.LogString("TokenName: "+this.tokenName);
            //Fiddler.FiddlerApplication.Log.LogString("Config: " + this.pingConfig);
            propertyGrid1.SelectedObject = myProperties;
        }

        public void Clear()
        {
            myProperties.Clear();
        }

        public static Stream StringToStream(string src)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(src);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private Dictionary<string, string> DecodeToken(string token, Stream config)
        {
            var agentConfig = new AgentConfiguration(config);
            Agent agent = new Agent(agentConfig);
            return agent.ReadToken(token, false);
        }



        public void DisplayData(HTTPHeaders headers, byte[] data)
        {
            //Let us examine the data
            // From byte array to string
            string s = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
            string token = null;
            string config = this.pingConfig;

            if (s.StartsWith(this.tokenName, true, CultureInfo.InvariantCulture))
            {
                if (!String.IsNullOrEmpty(config))
                {
                    token = s.Substring(this.tokenName.Length);
                    //Fiddler.FiddlerApplication.Log.LogString("Token: " + token);
                    try
                    {
                        Dictionary<string, string> results = DecodeToken(token, StringToStream(config));
                        foreach (KeyValuePair<string, string> kvp in results)
                        {
                            myProperties.Add(new CustomProperty(kvp.Key, kvp.Value, true, true));
                        }
                    }
                    catch (Exception ex)
                    {
                        myProperties.Add(new CustomProperty("Error", ex.Message, true, true));
                    }
                }
            }
            else
            {

            }


            propertyGrid1.Refresh();

            Clear();
        }

        private void toolStripBtnSettings_Click(object sender, EventArgs e)
        {
            Form f = new SettingsForm();
            f.ShowDialog(this);
        }
    }
}