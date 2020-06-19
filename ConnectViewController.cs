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
        public string cvcFileName;
        public OpcConnection OpcUa;
        
        public ConnectViewController (IntPtr handle) : base (handle)
        {
            
        }
        public void SetConnectAddress(string nAddress)
        {
            ConnectAddress.Text = nAddress;
        }
        public ConnectViewController() : base()
        {

        }
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);


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
            if (OpcUa == null)
            {
                OpcUa = new OpcConnection();
            }
            
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



    }



}