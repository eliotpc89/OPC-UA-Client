using Foundation;
using MobileCoreServices;
using Newtonsoft.Json;
using Opc.Ua;
using System;
using System.IO;
using UIKit;
using CoreGraphics;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace NewTestApp
{
    public partial class ConnectViewController : UIViewController
    {
        public string cvcFileName;
        public string cvcAddress;
        public NSUrl fullFileName;
        public NSData bm;
        public MyDocument myDoc;
        public OpcConnection OpcUa;
        public bool fileIsNew;
        public bool fileIsLoaded;
        public bool connected;
        UIViewPropertyAnimator ConnectAnimator, DisconnectAnimator;

        public ConnectViewController(IntPtr handle) : base(handle)
        {

        }
        public ConnectViewController() : base()
        {

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            fileIsLoaded = false;


            DisconnectAnimator = new UIViewPropertyAnimator(0.75, UIViewAnimationCurve.EaseInOut, () =>
            {
                BrowseNodesButton.Alpha = 0;
                MonitorSubButton.Alpha = 0;

                ConnectButton.Alpha = 1;
            });

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            Console.WriteLine(myDoc.DocumentString);

            TitleFileName.Text = cvcFileName.Substring(0, cvcFileName.Length - ".json".Length);
            NavigationItem.Title = TitleFileName.Text;
            NavigationController.Title = TitleFileName.Text;
            NavigationController.NavigationBarHidden = true;
            if (!connected)
            {
                BrowseNodesButton.Alpha = 0;
                MonitorSubButton.Alpha = 0;
                ConnectButton.Alpha = 1;

            }
            if (!fileIsNew && !fileIsLoaded)
            {
                ConnectButton.Alpha = 0;
                ConnectAddress.BorderStyle = UITextBorderStyle.None;
                ConnectAddress.Text = "Connecting to Server...";
            }

        }
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (!fileIsNew && !fileIsLoaded)
            {


                activitySpinner.StartAnimating();

                try
                {
                    OpcUa = new OpcConnection(myDoc);

                }
                catch
                {
                    OpcUa = new OpcConnection();
                    OpcUa.ConnectError(this, false, "Connection Failed", "Failed to Connect to OPC UA Server");
                    return;
                }

                fileIsLoaded = true;
                ConnectAddress.Text = OpcUa.savedAddress;

                AnimateConnection();
            }
            activitySpinner.StopAnimating();
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
                List<NodeId> nodeList = new List<NodeId>(OpcUa.subDict.Keys);
                foreach (var ii in nodeList)
                {
                    Console.WriteLine(ii);
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
            activitySpinner.Hidden = false;
            activitySpinner.StartAnimating();



            if (fileIsNew)
            {
                OpcUa = new OpcConnection(myDoc);
                OpcUa.fileName = cvcFileName;

            }
            else
            {
                OpcUa = new OpcConnection(myDoc);
            }
            try
            {


                OpcUa.Connect(ConnectAddress.Text.ToString());

            }
            catch
            {
                OpcUa.ConnectError(this, false, "Connection Failed", "Failed to Connect to OPC UA Server");
                activitySpinner.StopAnimating();
                return;
            }
            activitySpinner.StopAnimating();
            AnimateConnection();


        }

        private CGRect origConnectFrame;

        private void Picker_WasCancelled(object sender, EventArgs e)
        {
            Console.WriteLine("Picker was Cancelled");
        }

        private void AnimateConnection()
        {
            origConnectFrame = ConnectAddress.Frame;
            ConnectAddress.SizeToFit();
            UIViewPropertyAnimator.CreateRunningPropertyAnimator(0.75, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
            {
                BrowseNodesButton.Alpha = 1;
                MonitorSubButton.Alpha = 1;
                ConnectButton.Alpha = 0;
                ConnectAddress.TextAlignment = UITextAlignment.Center;
                ConnectAddress.BorderStyle = UITextBorderStyle.None;
                ConnectAddress.Center = new CoreGraphics.CGPoint(ParentViewController.View.Center.X, ConnectAddress.Center.Y);

            }, null);
            ConnectAddress.UserInteractionEnabled = false;
            connected = true;
        }
        partial void ReturnToFileBrowserButton(UIButton sender)
        {
            if (!(OpcUa is null))
            {
                OpcUa.ResetOpc();
            }
            myDoc.Close(null);
            connected = false;
            NavigationController.PopToRootViewController(true);
        }

    }



}