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
    [Register ("AboutViewController")]
    partial class AboutViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView AboutVc { get; set; }

        [Action ("ContributeLink:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ContributeLink (UIKit.UIButton sender);

        [Action ("DoneUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void DoneUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (AboutVc != null) {
                AboutVc.Dispose ();
                AboutVc = null;
            }
        }
    }
}