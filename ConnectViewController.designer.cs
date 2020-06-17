// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace NewTestApp
{
    [Register ("ConnectViewController")]
    partial class ConnectViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField ConnectAddress { get; set; }

        [Action ("FileBrowserButton:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void FileBrowserButton (UIKit.UIButton sender);

        [Action ("OpcUaConnectUp:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void OpcUaConnectUp (UIKit.UIButton sender);

        [Action ("UIButton10755_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UIButton10755_TouchUpInside (UIKit.UIButton sender);

        [Action ("UIButton26799_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UIButton26799_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (ConnectAddress != null) {
                ConnectAddress.Dispose ();
                ConnectAddress = null;
            }
        }
    }
}