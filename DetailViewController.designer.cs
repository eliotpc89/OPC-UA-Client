// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace NewTestApp
{
    [Register ("DetailViewController")]
    partial class DetailViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField DataChangeBox { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel detailDescriptionLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch SubscribeSwitch { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton WriteDataButton { get; set; }

        [Action ("WriteDataButtonUp:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void WriteDataButtonUp (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (DataChangeBox != null) {
                DataChangeBox.Dispose ();
                DataChangeBox = null;
            }

            if (detailDescriptionLabel != null) {
                detailDescriptionLabel.Dispose ();
                detailDescriptionLabel = null;
            }

            if (SubscribeSwitch != null) {
                SubscribeSwitch.Dispose ();
                SubscribeSwitch = null;
            }

            if (WriteDataButton != null) {
                WriteDataButton.Dispose ();
                WriteDataButton = null;
            }
        }
    }
}