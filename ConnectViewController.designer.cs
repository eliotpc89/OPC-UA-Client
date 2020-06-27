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
        UIKit.UIButton BrowseNodesButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField ConnectAddress { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ConnectButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton MonitorSubButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TitleFileName { get; set; }

        [Action ("OpcUaConnectUp:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void OpcUaConnectUp (UIKit.UIButton sender);

        [Action ("ReturnToFileBrowserButton:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ReturnToFileBrowserButton (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (BrowseNodesButton != null) {
                BrowseNodesButton.Dispose ();
                BrowseNodesButton = null;
            }

            if (ConnectAddress != null) {
                ConnectAddress.Dispose ();
                ConnectAddress = null;
            }

            if (ConnectButton != null) {
                ConnectButton.Dispose ();
                ConnectButton = null;
            }

            if (MonitorSubButton != null) {
                MonitorSubButton.Dispose ();
                MonitorSubButton = null;
            }

            if (TitleFileName != null) {
                TitleFileName.Dispose ();
                TitleFileName = null;
            }
        }
    }
}