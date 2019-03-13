using System;
using Fiddler;

namespace FiddlerPingTokenViewer
{
    public class PingTokenRequestView : PingTokenViewBase, IRequestInspector2
    {
       
        public HTTPRequestHeaders headers { get; set; }

        protected override HTTPHeaders GetHeaders()
        {
            return headers;
        }
    }

}
