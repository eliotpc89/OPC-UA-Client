using Foundation;
using System;
using System.IO;
using UIKit;
using MobileCoreServices;
using Newtonsoft.Json;
using ObjCRuntime;
using System.Threading.Tasks;
using System.Text;

namespace NewTestApp
{


    public partial class BrowserViewController : UIDocumentBrowserViewController
    {

        public bool fileIsNew;
        public string fileName;
        public string fileData;
        public NSUrl fullFilename;
        public string myDocs;
        public MyDocument Doc;
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
                nextVc.fullFileName = fullFilename;
                nextVc.cvcFileName = fileName;
                nextVc.fileIsNew = fileIsNew;
                nextVc.Doc = Doc;

                Console.WriteLine("Performing Segue to CVC: {0}", fileName);

            }


        }

        public virtual void DidImportDocument(UIDocumentBrowserViewController controller, NSUrl sourceUrl, NSUrl destinationUrl)
        {

            Console.WriteLine("Destination{destinationUrl}");

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

                    //ctrlr.fileName = alert.TextFields[0].Text + ".json";
                    Console.WriteLine(Path.GetTempPath());
                    //ctrlr.fullFilename = new NSUrl("file://" + Path.Combine(Path.GetTempPath(), ctrlr.fileName));

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
        public override void FailedToImportDocument(UIDocumentBrowserViewController controller, NSUrl documentUrl, NSError error)
        {
            base.FailedToImportDocument(controller, documentUrl, error);
            Console.WriteLine("Fail: {0}", documentUrl.AbsoluteString);
        }


        public override void DidRequestDocumentCreation(UIDocumentBrowserViewController controller, Action<NSUrl, UIDocumentBrowserImportMode> importHandler)
        {

            var docsDir = Path.GetTempPath();
            var ctrlr = controller as BrowserViewController;
            string urlPath = Path.Combine(docsDir, "EPC1456789.json");
            try
            {
                if (File.Exists(urlPath))
                    File.Delete(urlPath);
            }
            catch
            {
            }

            File.Create(urlPath);
            NSUrl nsu = new NSUrl(urlPath);
            

            var url = NSUrl.FromFilename(urlPath);
            ctrlr.Doc = new MyDocument(url);

            ctrlr.fullFilename = ctrlr.Doc.FileUrl.FileReferenceUrl;

            
            ctrlr.fileName = url.LastPathComponent;



            importHandler(url, UIDocumentBrowserImportMode.Move);



            Console.WriteLine("Success SavingFile");
            var fileUrl = url.ToString().Remove(0, "file://".Length);


            ShowAlert(controller, "Create New File", "Enter a file name");



        }

        public override void DidPickDocumentsAtUrls(UIDocumentBrowserViewController controller, NSUrl[] documentUrls)
        {

            Console.WriteLine("url = {0}", documentUrls[0].FilePathUrl);
            //bool success = MoveFileToApp(didPickDocArgs.Url);
            var ctrlr = controller as BrowserViewController;
            var success = true;
            ctrlr.fullFilename = documentUrls[0];
            string filename = documentUrls[0].LastPathComponent;
            string msg = success ? string.Format("Successfully imported file '{0}'", filename) : string.Format("Failed to import file '{0}'", filename);

            ctrlr.fileName = filename;

            ctrlr.myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!ctrlr.fullFilename.ToString().Contains(ctrlr.myDocs))
            {
                string tempFile = ctrlr.myDocs + "/" + "__";
                NSUrl neighborUrl = new NSUrl("file://" + tempFile);

                File.Create(tempFile);
                ctrlr.ImportDocument(documentUrls[0], neighborUrl, UIDocumentBrowserImportMode.Copy, (url, error) =>
                {
                    Console.WriteLine("Import Successful");
                    File.Delete(tempFile);
                });
            }



            ctrlr.fileIsNew = false;
            NSData data = new NSData();
            data = NSData.FromFile(ctrlr.fullFilename.AbsoluteString);
            Console.WriteLine(ctrlr.fullFilename.AbsoluteString);
            Console.WriteLine(data);
            Console.WriteLine(filename);

            controller.PerformSegue("PageVcSegue", null);
        }



    }
}
