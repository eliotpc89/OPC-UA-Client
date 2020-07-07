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
                nextVc.fullFileName = fullFilename;
                nextVc.cvcFileName = fileName;
                nextVc.fileIsNew = fileIsNew;
                nextVc.myDoc = myDoc;
                nextVc.bm = bm;
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

                        var fileRef = url.FileReferenceUrl;
                        ctrlr.fileName = url.LastPathComponent;
                        url.StartAccessingSecurityScopedResource();
                        ctrlr.myDoc = new MyDocument(url);
                        ctrlr.myDoc.Save(url, UIDocumentSaveOperation.ForCreating, saveSuccess =>
                        {
                            if (!saveSuccess)
                            {
                                importHandler(null, UIDocumentBrowserImportMode.None);
                                return;
                            }
                            ctrlr.myDoc.DocumentString = "HELLO WORLD LOTS OF TEXT AND WORDS";
                            ctrlr.myDoc.UpdateChangeCount(UIDocumentChangeKind.Done);
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




                        ctrlr.fullFilename = fileRef.AbsoluteUrl;

                        Console.WriteLine(ctrlr.fullFilename);
                        Console.WriteLine("Success SavingFile");
                        var fileUrl = url.ToString().Remove(0, "file://".Length);

                        controller.PerformSegue("PageVcSegue", null);
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
            //bool success = MoveFileToApp(didPickDocArgs.Url);
            var ctrlr = controller as BrowserViewController;
            var success = true;
            ctrlr.fullFilename = documentUrls[0];
            string filename = documentUrls[0].LastPathComponent;
            string msg = success ? string.Format("Successfully imported file '{0}'", filename) : string.Format("Failed to import file '{0}'", filename);

            ctrlr.fileName = filename;

            //ctrlr.myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //if (!ctrlr.fullFilename.ToString().Contains(ctrlr.myDocs))
            //{
            //    string tempFile = ctrlr.myDocs + "/" + "__";
            //    NSUrl neighborUrl = new NSUrl("file://" + tempFile);

            //    File.Create(tempFile);
            //    ctrlr.ImportDocument(documentUrls[0], neighborUrl, UIDocumentBrowserImportMode.Copy, (url, error) =>
            //    {
            //        Console.WriteLine("Import Successful");
            //        File.Delete(tempFile);
            //    });
            //}




            NSData data = new NSData();
            data = NSData.FromFile(ctrlr.fullFilename.Path);
            Console.WriteLine(ctrlr.fullFilename.AbsoluteString);
            Console.WriteLine(data);
            Console.WriteLine(filename);
            if (!(data is null))
            {
                ctrlr.fileIsNew = !(data.Length > 0);

            }


            controller.PerformSegue("PageVcSegue", null);
        }



    }
}
