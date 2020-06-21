using Foundation;
using MobileCoreServices;
using Newtonsoft.Json;
using Opc.Ua;
using System;
using System.IO;
using UIKit;

namespace NewTestApp
{
    public partial class ConnectViewController : UIViewController
    {
        public string cvcFileName;
        public OpcConnection OpcUa;
        public bool fileIsNew;
        public bool fileIsLoaded;
        public ConnectViewController(IntPtr handle) : base(handle)
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

            NavigationController.Title = cvcFileName;
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            fileIsLoaded = false;


        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            TitleFileName.Text = cvcFileName;
            NavigationController.Title = cvcFileName;
            if (!fileIsNew && !fileIsLoaded)
            {

                OpcUa = new OpcConnection(cvcFileName);
                fileIsLoaded = true;

            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);



            if (segue.Identifier == "showNodes")
            {
                var nextVc = segue.DestinationViewController
                                         as NodeViewController;


                nextVc.OpcUa = OpcUa;

                foreach (var ii in OpcUa.subDict)
                {
                    Console.WriteLine(ii.Value);
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
            if (fileIsNew)
            {
                OpcUa = new OpcConnection();
                OpcUa.fileName = cvcFileName;
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


        partial void ReturnToFileBrowserButton(UIButton sender)
        {
            OpcUa.ResetOpc();
            NavigationController.PopToRootViewController(true);
        }
    }



}