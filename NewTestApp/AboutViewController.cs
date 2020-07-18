using Foundation;
using System;
using UIKit;

namespace NewTestApp
{
    public partial class AboutViewController : UIViewController
    {
        public AboutViewController (IntPtr handle) : base (handle)
        {
        }

        partial void DoneUpInside(UIButton sender)
        {
            DismissModalViewController(true);
        }


        partial void ContributeLink(UIButton sender)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl("https://github.com/eliotpc89/OPC-UA-Client"));
        }
    }
}