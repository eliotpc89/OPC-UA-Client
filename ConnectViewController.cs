using Foundation;
using MobileCoreServices;
using Newtonsoft.Json;
using System;
using System.IO;
using UIKit;

namespace NewTestApp
{
    public partial class ConnectViewController : UIViewController
    {
        public OpcConnection OpcUa;
        public ConnectViewController (IntPtr handle) : base (handle)
        {
            
        }


        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            if (segue.Identifier == "browseNodes")
            {
                var nextVc = segue.DestinationViewController
                                         as MainSplitViewController;
                
                if (nextVc != null)
                {
                    
                    nextVc.OpcUa = OpcUa;
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
            OpcUa = new OpcConnection();
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
        partial void UIButton26799_TouchUpInside(UIButton sender)
        {
            string test = "hello world";

            var json = JsonConvert.SerializeObject(test, Newtonsoft.Json.Formatting.Indented);

            // Save to file
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filename =  Path.Combine(documents,"account2.JSON");
            File.WriteAllText(filename, json);
            var picker = new UIDocumentPickerViewController(allowedUTIs, UIDocumentPickerMode.Open);
            picker.DirectoryUrl = new  NSUrl("file://" +documents);
            Console.WriteLine(documents);
            Console.WriteLine(filename);
            picker.WasCancelled += Picker_WasCancelled;
            picker.DidPickDocumentAtUrls += (object s, UIDocumentPickedAtUrlsEventArgs e) =>
            {
                Console.WriteLine("url = {0}", e.Urls[0].AbsoluteString);
                //bool success = await MoveFileToApp(didPickDocArgs.Url);  
                var success = true;
                string filename = e.Urls[0].LastPathComponent;
                string msg = success ? string.Format("Successfully imported file '{0}'", filename) : string.Format("Failed to import file '{0}'", filename);
                // Some invaild file url returns null  
                NSData data = NSData.FromUrl(e.Urls[0]);
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
            };
            PresentViewController(picker, true, null);
        }
    }



}