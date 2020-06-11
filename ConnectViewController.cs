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


        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            if (segue.Identifier == "browseNodes")
            {
                var nextVc = segue.DestinationViewController
                                         as MainSplitViewController;
                if (nextVc != null)
                {

                    nextVc.OpcUa = OpcUa;
                }
            }
               
            if (segue.Identifier == "showSubs")
            {
                var nextVc = segue.DestinationViewController
                                         as SubTableViewController;

                nextVc.OpcUa = OpcUa;
                Console.WriteLine("ShowSubs");
            }

            
        }

        partial void OpcUaConnectUp(UIButton sender)
        {
            OpcUa = new OpcConnection();
            OpcUa.Connect(ConnectAddress.Text.ToString());
        

        
        }


    }



}