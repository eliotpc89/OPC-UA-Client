using Foundation;
using MobileCoreServices;
using Newtonsoft.Json;
using System;
using System.IO;
using UIKit;

namespace NewTestApp
{
    public partial class ConnectViewController : UIViewController
    {
        public OpcConnection OpcUa;
        public ConnectViewController (IntPtr handle) : base (handle)
        {
            
        }

        public ConnectViewController() : base()
        {

        }
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            var controller = new BrowserViewController(allowedUTIs);
            controller.AllowsPickingMultipleItems = false;
            PresentViewController(controller, true, null);

        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

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
        private string[] allowedUTIs =  {
                    UTType.UTF8PlainText,
                    UTType.PlainText,
                    UTType.RTF,
                    UTType.PNG,
                    UTType.Text,
                    UTType.PDF,
                    UTType.Image,
                    UTType.JSON,
                    UTType.XML
                    
                };
        private string[] allowedUrls =
        {

        };

        private void Picker_WasCancelled(object sender, EventArgs e)
        {
            Console.WriteLine("Picker was Cancelled");
        }


        partial void FileBrowserButton(UIButton sender)
        {
            var controller = new BrowserViewController(allowedUTIs);
            PresentViewController(controller, false, null);
        }
    }



}