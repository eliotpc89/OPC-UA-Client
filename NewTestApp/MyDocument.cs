using System;
using Foundation;
using UIKit;

namespace NewTestApp
{
    public class MyDocument :UIDocument
    {
		// the 'model', just a chunk of text in this case; must easily convert to NSData
		NSString dataModel;

		// model is wrapped in a nice .NET-friendly property
		public string DocumentString
		{
			get
			{
				return dataModel.ToString();
			}
			set
			{
				dataModel = new NSString(value);

			}
		}

		public MyDocument(NSUrl url) : base(url)
		{
			DocumentString = "";
		}
		// contents supplied by iCloud to display, update local model and display (via notification)
		public override bool LoadFromContents(NSObject contents, string typeName, out NSError outError)
		{
			outError = null;

			Console.WriteLine("LoadFromContents({0})", typeName);

			if (contents != null)
				dataModel = NSString.FromData((NSData)contents, NSStringEncoding.UTF8);

			// LoadFromContents called when an update occurs
			NSNotificationCenter.DefaultCenter.PostNotificationName("monkeyDocumentModified", this);
			return true;
		}


		// return contents for iCloud to save (from the local model)
		public override NSObject ContentsForType(string typeName, out NSError outError)
		{
			outError = null;

			Console.WriteLine("ContentsForType({0})", typeName);
			Console.WriteLine("DocumentText:{0}", dataModel);

			NSData docData = dataModel.Encode(NSStringEncoding.UTF8);
			return docData;
		}





	}
}
