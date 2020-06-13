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
    [Register ("SubCellDetail")]
    partial class SubCellDetail
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel LeftLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel RightLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (LeftLabel != null) {
                LeftLabel.Dispose ();
                LeftLabel = null;
            }

            if (RightLabel != null) {
                RightLabel.Dispose ();
                RightLabel = null;
            }
        }
    }
}