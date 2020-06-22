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
        public bool fileIsNew;
        public string fileName;
        public string fileData;
        public BrowserViewController(IntPtr handle) : base(handle)
        {

            var test = new UIDocumentBrowserViewControllerDelegate();
            


        }
        public BrowserViewController(string[] allowedUTIs) : base(allowedUTIs)
        {

            

        }
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBarHidden = true;
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
                nextVc.fileIsNew = fileIsNew;
                Console.WriteLine("Performing Segue to CVC: {0}", fileName);

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
                ctrlr.fileIsNew = true;

                Console.WriteLine(ctrlr.fileName);
                

                alert.DismissViewController(true, () =>
                {
                    controller.PerformSegue("PageVcSegue", null);
                });
                
            }));

            alert.AddTextField((field) => {
                //field.Text = true;
            });
            
            controller.PresentViewController(alert, animated: false, null);
            

        }
        public override void DidRequestDocumentCreation(UIDocumentBrowserViewController controller, Action<NSUrl, UIDocumentBrowserImportMode> importHandler)
        {

            importHandler(null, UIDocumentBrowserImportMode.Copy);
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
            ctrlr.fileIsNew = false;

            Console.WriteLine(filename);

            controller.PerformSegue("PageVcSegue", null);
        }



    }
}
