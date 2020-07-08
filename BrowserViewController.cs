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
        public string connectAddress;
        public NSUrl fullFilename;
        public NSData bm;
        public string myDocs;
        public MyDocument myDoc;
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
                nextVc.myDoc = myDoc;
                nextVc.fullFileName = fullFilename;

                Console.WriteLine("Performing Segue to CVC: {0}", fullFilename);

            }


        }





    }
    public class dbvcDelegate : UIDocumentBrowserViewControllerDelegate
    {
        private bool alertCancelled;
        private string alertInput;

        public override void DidImportDocument(UIDocumentBrowserViewController controller, NSUrl sourceUrl, NSUrl destinationUrl)
        {

            Console.WriteLine("Destination{0}", destinationUrl.AbsoluteString);
            var ctrlr = controller as BrowserViewController;
            ctrlr.fullFilename =  destinationUrl;
            ctrlr.myDoc = new MyDocument(destinationUrl);
            ctrlr.PerformSegue("PageVcSegue", null);

        }
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
                    Console.WriteLine(Path.GetTempPath());
                    ctrlr.fileIsNew = true;
                    alert.DismissViewController(true, () =>
                    {
                        alertInput = alert.TextFields[0].Text;
                        alertCancelled = false;

                    });
                }


            }));
            alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, action =>
            {
                alert.DismissViewController(true, () =>
                {
                    alertCancelled = true;
                });


            }));

            alert.AddTextField((field) =>
            {
                //field.Text = true;
            });

            controller.PresentViewController(alert, animated: true, null);


        }
        public override void FailedToImportDocument(UIDocumentBrowserViewController controller, NSUrl documentUrl, NSError error)
        {
            Console.WriteLine("Fail: {0}", documentUrl.AbsoluteString);
            Console.WriteLine("Fail Error {0}", error.LocalizedDescription);
        }


        public override void DidRequestDocumentCreation(UIDocumentBrowserViewController controller, Action<NSUrl, UIDocumentBrowserImportMode> importHandler)
        {
            var title = "Create New File";
            var prompt = "Enter a file name";

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
                    Console.WriteLine(Path.GetTempPath());
                    ctrlr.fileIsNew = true;
                    alert.DismissViewController(true, () =>
                    {
                        alertInput = alert.TextFields[0].Text;
                        alertCancelled = false;
                        var docsDir = Path.GetTempPath();
                        string urlPath = Path.Combine(docsDir, alertInput + ".json");

                        File.Create(urlPath);
                        NSUrl nsu = NSUrl.FromString(urlPath);
                        var url = NSUrl.FromFilename(urlPath);

                        ctrlr.fileName = url.LastPathComponent;
                        ctrlr.myDoc = new MyDocument(url);
                        ctrlr.myDoc.Save(url, UIDocumentSaveOperation.ForCreating, saveSuccess =>
                        {
                            if (!saveSuccess)
                            {
                                importHandler(null, UIDocumentBrowserImportMode.None);
                                return;
                            } 
                            ctrlr.myDoc.Close(closeSuccess =>
                            {
                                if (!closeSuccess)
                                {
                                    importHandler(null, UIDocumentBrowserImportMode.None);
                                    return;
                                }
                                importHandler(url, UIDocumentBrowserImportMode.Move);
                                Console.WriteLine("Close Success");
                            });
                        }
                        );


                        
                    });
                }


            }));
            alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, action =>
            {
                alert.DismissViewController(true, () =>
                {
                    alertCancelled = true;
                    importHandler(null, UIDocumentBrowserImportMode.Move);
                });


            }));

            alert.AddTextField((field) =>
            {
                //field.Text = true;
            });

            controller.PresentViewController(alert, animated: true, null);



        }

        public override void DidPickDocumentsAtUrls(UIDocumentBrowserViewController controller, NSUrl[] documentUrls)
        {

            Console.WriteLine("url = {0}", documentUrls[0].FilePathUrl);
            NSUrl url = documentUrls[0].FilePathUrl;
            var ctrlr = controller as BrowserViewController;
            var success = true;
            ctrlr.fullFilename = documentUrls[0];
            
            ctrlr.fileName = documentUrls[0].LastPathComponent;
            string msg = success ? string.Format("Successfully imported file '{0}'", ctrlr.fileName) : string.Format("Failed to import file '{0}'", ctrlr.fileName);
            
            ctrlr.myDoc = new MyDocument(url);
            ctrlr.myDoc.Open((success) =>
            {
                Console.WriteLine("CompletionText: {0}", url.AbsoluteString);
                ctrlr.fileIsNew = !(ctrlr.myDoc.DocumentString.Length > 0);
                Console.WriteLine("Picked Document is New: {0}", ctrlr.fileIsNew.ToString());
                ctrlr.fullFilename = url;
                controller.PerformSegue("PageVcSegue", null);
            });

        }



    }
}
