using Foundation;
using System;
using UIKit;

namespace NewTestApp
{
    public partial class ConnectViewController : UIViewController
    {
        public OpcConnection OpcUa;
        public ConnectViewController (IntPtr handle) : base (handle)
        {
            
        }
        public override bool ShouldPerformSegue(string segueIdentifier, NSObject sender)
        {

            if (segueIdentifier == "showDetail")
            {
                return true;

            }
            else
            {
                return false;
            }

        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            OpcUa = new OpcConnection();
            OpcUa.Connect(ConnectAddress.Text.ToString());
            var nextVc = segue.DestinationViewController
                                          as MainSplitViewController;

            if (nextVc != null)
            {
                nextVc.test_cnt = 15;
                nextVc.OpcUa = OpcUa;
            }
        }



    }



}