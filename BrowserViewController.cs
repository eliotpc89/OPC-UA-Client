using Foundation;
using System;
using System.IO;
using UIKit;
using MobileCoreServices;
using Newtonsoft.Json;
using ObjCRuntime;
using System.Threading.Tasks;

namespace NewTestApp
{


    public partial class BrowserViewController : UIDocumentBrowserViewController
    {
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

        public string fileName;
        public string fileData;
        public BrowserViewController(IntPtr handle) : base(handle)
        {

            var test = new UIDocumentBrowserViewControllerDelegate();
            


        }
        public BrowserViewController(string[] allowedUTIs) : base(allowedUTIs)
        {

            

        }


        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
           
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var test = new dbvcDelegate();
            this.Delegate = test;
            
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            if (segue.Identifier == "PageVcSegue")
            {
                var nextVc = segue.DestinationViewController
                                         as ConnectViewController;
                nextVc.cvcFileName = fileName;
                NSData data = new NSData();
                data = NSData.FromFile(fileName);
                if (data != null)
                {
                    Console.WriteLine("CVC{0}", fileName);
                    nextVc.SetConnectAddress(fileName);
                }
                if (!(nextVc.OpcUa is null))
                {
                    nextVc.OpcUa.m_session.CloseSession(null, true);
                    
                }
                nextVc.OpcUa = new OpcConnection(fileName);
            }
        }

        public virtual void DidImportDocument (UIDocumentBrowserViewController controller, NSUrl sourceUrl, NSUrl destinationUrl)
        {
            


        }



    }
    public class dbvcDelegate : UIDocumentBrowserViewControllerDelegate
    {
        private void ShowAlert(UIViewController controller)
        {

            UIAlertController alert = UIAlertController.Create("Create File", "Enter file name", UIAlertControllerStyle.Alert);
            var ctrlr = controller as BrowserViewController;
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => {
                // This code is invoked when the user taps on login, and this shows how to access the field values
                Console.WriteLine("User: {0}", alert.TextFields[0].Text);
                ctrlr.fileName = alert.TextFields[0].Text +".json";
                controller.DismissViewController(true, null);
                controller.PerformSegue("PageVcSegue",null);
                
                Console.WriteLine(ctrlr.fileName);
            }));

            alert.AddTextField((field) => {
                //field.Text = true;
            });
            
            controller.PresentViewController(alert, animated: true, null);
        }
        public override void DidRequestDocumentCreation(UIDocumentBrowserViewController controller, Action<NSUrl, UIDocumentBrowserImportMode> importHandler)
        {
            ShowAlert(controller);

 
        }

        public override void DidPickDocumentsAtUrls(UIDocumentBrowserViewController controller, NSUrl[] documentUrls)
        {
            
            Console.WriteLine("url = {0}", documentUrls[0].AbsoluteString);
            //bool success = await MoveFileToApp(didPickDocArgs.Url);  
            var success = true;
            string filename = documentUrls[0].LastPathComponent;
            string msg = success ? string.Format("Successfully imported file '{0}'", filename) : string.Format("Failed to import file '{0}'", filename);
            var ctrlr = controller as BrowserViewController;
            NSData data = NSData.FromUrl(documentUrls[0]);
            ctrlr.fileData = data.ToString();
            ctrlr.fileName = filename;
            if (data != null)
            {
                byte[] dataBytes = new byte[data.Length];

                System.Runtime.InteropServices.Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));

                for (int i = 0; i < dataBytes.Length; i++)
                {
                    Console.WriteLine(dataBytes[i]);
                }
            }

            Console.WriteLine(data + "Completed");

            var alertController = UIAlertController.Create("import", msg, UIAlertControllerStyle.Alert);
            var okButton = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (obj) =>
            {
                alertController.DismissViewController(true, null);
            });
            alertController.AddAction(okButton);
            controller.DismissModalViewController(true);
            controller.PerformSegue("PageVcSegue", null);
        }
        
    }
}
