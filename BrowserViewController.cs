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

        public bool fileIsNew;
        public string fileName;
        public string fileData;
        public BrowserViewController(IntPtr handle) : base(handle)
        {

            var test = new UIDocumentBrowserViewControllerDelegate();
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
            dbvcDelegate dvbcdDel = new dbvcDelegate();
            this.Delegate = dvbcdDel;
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);
            var nextVc = segue.DestinationViewController
                         as ConnectViewController;
            if (segue.Identifier == "PageVcSegue")
            {

                nextVc.cvcFileName = fileName;
                
                nextVc.fileIsNew = fileIsNew;
                Console.WriteLine("Performing Segue to CVC: {0}", fileName);

            }


        }

        public virtual void DidImportDocument(UIDocumentBrowserViewController controller, NSUrl sourceUrl, NSUrl destinationUrl)
        {



        }



    }
    public class dbvcDelegate : UIDocumentBrowserViewControllerDelegate
    {
        private void ShowAlert(UIViewController controller, string title, string prompt)
        {

            UIAlertController alert = UIAlertController.Create(title, prompt, UIAlertControllerStyle.Alert);
            var ctrlr = controller as BrowserViewController;
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action =>
            {
                // This code is invoked when the user taps on login, and this shows how to access the field values
                if (alert.TextFields[0].Text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    alert.DismissViewController(true, () =>
                    {
                        this.ShowAlert(controller, "Invalid File Name", "Please enter a valid file name");
                    });
                }
                else
                {
                    Console.WriteLine("User: {0}", alert.TextFields[0].Text);
                    ctrlr.fileName = alert.TextFields[0].Text + ".json";
                    ctrlr.fileIsNew = true;
                    Console.WriteLine(ctrlr.fileName);
                    alert.DismissViewController(true, () =>
                    {
                        controller.PerformSegue("PageVcSegue", null);
                    });
                }


            }));
            alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, action =>
            {
                alert.DismissViewController(true, null);

            }));

            alert.AddTextField((field) =>
            {
                //field.Text = true;
            });

            controller.PresentViewController(alert, animated: true, null);


        }
        public override void DidRequestDocumentCreation(UIDocumentBrowserViewController controller, Action<NSUrl, UIDocumentBrowserImportMode> importHandler)
        {

            importHandler(null, UIDocumentBrowserImportMode.Copy);
            ShowAlert(controller, "Create New File", "Enter a file name");


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
