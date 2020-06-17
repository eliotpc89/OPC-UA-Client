using Foundation;
using System;
using UIKit;
using MobileCoreServices;

namespace NewTestApp
{
    public partial class DummyVC : UIViewController
    {
        private string[] allowedUTIs =  {

                UTType.JSON,
                UTType.XML

            };
        public DummyVC (IntPtr handle) : base (handle)
        {
        }
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(false);
            var controller = new BrowserViewController(allowedUTIs);
            PresentViewController(controller, false, null);
        }
    }
}