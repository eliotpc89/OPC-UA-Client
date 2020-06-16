using Foundation;
using System;
using System.IO;
using UIKit;
using MobileCoreServices;
using Newtonsoft.Json;


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
        public virtual void DidImportDocument (UIDocumentBrowserViewController controller, NSUrl sourceUrl, NSUrl destinationUrl)
        {
            


        }



        public void DidPickDocumentAtUrls (UIDocumentBrowserViewController controller, NSUrl[] documentUrls)
        {
                Console.WriteLine("url = {0}", documentUrls[0].AbsoluteString);
                //bool success = await MoveFileToApp(didPickDocArgs.Url);  
                var success = true;
                string filename = documentUrls[0].LastPathComponent;
                string msg = success ? string.Format("Successfully imported file '{0}'", filename) : string.Format("Failed to import file '{0}'", filename);
                // Some invaild file url returns null  
                NSData data = NSData.FromUrl(documentUrls[0]);
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
                PresentViewController(alertController, true, null);
            }

    }
    public class dbvcDelegate : UIDocumentBrowserViewControllerDelegate
    {

        public override void DidPickDocumentsAtUrls(UIDocumentBrowserViewController controller, NSUrl[] documentUrls)
        {
            //base.DidPickDocumentsAtUrls(controller, documentUrls);
            Console.WriteLine("url = {0}", documentUrls[0].AbsoluteString);
            //bool success = await MoveFileToApp(didPickDocArgs.Url);  
            var success = true;
            string filename = documentUrls[0].LastPathComponent;
            string msg = success ? string.Format("Successfully imported file '{0}'", filename) : string.Format("Failed to import file '{0}'", filename);
            // Some invaild file url returns null  
            NSData data = NSData.FromUrl(documentUrls[0]);
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
            controller.PresentViewController(alertController, true, null);
        }
    }
}
