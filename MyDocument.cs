using System;
using System.IO;
using Foundation;
using UIKit;

namespace NewTestApp
{
    public class MyDocument : UIDocument
    {

		public string LocalFilePath { get; private set; }
		public event EventHandler Loading = delegate { };

		public MyDocument(string localFilePath)
			: base(NSUrl.FromFilename(localFilePath))
		{
			LocalFilePath = localFilePath;
		}

		public MyDocument(NSUrl url)
			: base(url)
		{
			LocalFilePath = url.AbsoluteString;
		}

		public bool IsOpen { get { return !DocumentState.HasFlag(UIDocumentState.Closed); } }



		string textData = "";
		public virtual string TextData
		{
			get { return textData; }
			set { textData = value ?? ""; }
		}



		public void WriteData(string content)
        {

			string tempName = LocalFilePath.Remove(0, "file://".Length);
			Console.WriteLine("DocWrite: "+tempName);
			Console.WriteLine("DocContent: " + content);
			File.WriteAllText(tempName, content);

		}
		public override bool LoadFromContents(NSObject contents, string typeName, out NSError outError)
		{
			try
			{
				outError = null;
				var data = contents as NSData;
				if (data != null)
				{
					TextData = data.ToString(NSStringEncoding.UTF8);
				}
				else
				{
					TextData = "";
				}

				Loading(this, EventArgs.Empty);

				return true;
			}
			catch (Exception ex)
			{

				outError = new NSError(new NSString(""), 335);
				return false;
			}
		}




	}


}
