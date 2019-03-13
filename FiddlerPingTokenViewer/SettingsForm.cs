using Fiddler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FiddlerPingTokenViewer.Encoding;

namespace FiddlerPingTokenViewer
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            this.txtTokenName.Text = FiddlerApplication.Prefs.GetStringPref(PreferenceNames.PING_TOKEN,"");
            this.txtConfiguration.Text = System.Text.Encoding.UTF8.DecodeBase64(FiddlerApplication.Prefs.GetStringPref(PreferenceNames.PING_CONFIG, ""));
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            FiddlerApplication.Prefs.SetStringPref(PreferenceNames.PING_CONFIG, System.Text.Encoding.UTF8.EncodeBase64(this.txtConfiguration.Text));
            FiddlerApplication.Prefs.SetStringPref(PreferenceNames.PING_TOKEN,this.txtTokenName.Text);
        }
    }
}
