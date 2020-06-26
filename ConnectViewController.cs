using Foundation;
using MobileCoreServices;
using Newtonsoft.Json;
using Opc.Ua;
using System;
using System.IO;
using UIKit;
using CoreGraphics;

namespace NewTestApp
{
    public partial class ConnectViewController : UIViewController
    {
        public string cvcFileName;
        public OpcConnection OpcUa;
        public bool fileIsNew;
        public bool fileIsLoaded;

        UIViewPropertyAnimator ConnectAnimator, DisconnectAnimator;

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
            

            
            DisconnectAnimator = new UIViewPropertyAnimator(0.75, UIViewAnimationCurve.EaseInOut, () =>
            {
                BrowseNodesButton.Alpha = 0;
                MonitorSubButton.Alpha = 0;
                DisconnectButton.Alpha = 0;
                ConnectButton.Alpha = 1;
            });

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            TitleFileName.Text = cvcFileName;
            NavigationItem.Title = cvcFileName;
            NavigationController.Title = cvcFileName;
            NavigationController.NavigationBarHidden = true;
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
            origConnectFrame = ConnectAddress.Frame;
            ConnectAddress.SizeToFit();

            
 
            UIViewPropertyAnimator.CreateRunningPropertyAnimator(0.75,0, UIViewAnimationOptions.CurveEaseInOut, () =>
            {
                BrowseNodesButton.Alpha = 1;
                MonitorSubButton.Alpha = 1;
                DisconnectButton.Alpha = 1;
                ConnectButton.Alpha = 0;
                ConnectAddress.TextAlignment = UITextAlignment.Center;
                ConnectAddress.BorderStyle = UITextBorderStyle.None;
                ConnectAddress.Center = new CoreGraphics.CGPoint(ParentViewController.View.Center.X, ConnectAddress.Center.Y); 
                
            }, null);

            
            ConnectAddress.UserInteractionEnabled = false;
            
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
        private CGRect origConnectFrame;

        private void Picker_WasCancelled(object sender, EventArgs e)
        {
            Console.WriteLine("Picker was Cancelled");
        }


        partial void ReturnToFileBrowserButton(UIButton sender)
        {
            if(!(OpcUa is null))
            {
                OpcUa.ResetOpc();
            }

            NavigationController.PopToRootViewController(true);
        }

        partial void DisconnectButtonUp(UIButton sender)
        {
            OpcUa.ResetOpc();
            UIViewPropertyAnimator.CreateRunningPropertyAnimator(0.75, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
            {
                BrowseNodesButton.Alpha = 0;
                MonitorSubButton.Alpha = 0;
                DisconnectButton.Alpha = 0;
                ConnectButton.Alpha = 1;
                ConnectAddress.TextAlignment = UITextAlignment.Left;
                ConnectAddress.BorderStyle = UITextBorderStyle.RoundedRect;
                ConnectAddress.Frame = origConnectFrame;
            }, null);



            ConnectAddress.UserInteractionEnabled = true;
            

        }

    }



}