using System.Windows.Forms;
using Fiddler;

namespace FiddlerPingTokenViewer
{
    public abstract class PingTokenViewBase : Inspector2
    {
        private readonly PingTokenViewer _viewer = new PingTokenViewer();
        private byte[] _decodedBody;
        private byte[] _body;

        public override void AddToTab(TabPage o)
        {
           o.Text = "PingToken";
           o.Controls.Add(_viewer);
           _viewer.Dock = DockStyle.Fill;
        }

        public override int GetOrder()
        {
            return 1000;
        }

        public void Clear()
        {
            _body = null;
            _decodedBody = null;
            _viewer.Clear();
        }

        public byte[] body
        {
            get { return _body; }
            set
            {
                _body = value;
                DisplayData();
            }
        }

        private void DisplayData()
        {
            _decodedBody = _body;
            Utilities.utilDecodeHTTPBody(GetHeaders(), ref _decodedBody);
            _viewer.DisplayData(GetHeaders(), _decodedBody);
        }

        protected abstract HTTPHeaders GetHeaders();

        public bool bDirty
        {
            get { return false; }
        }

        public bool bReadOnly
        {
            get { return true; }
            set { }
        }
    }

}
