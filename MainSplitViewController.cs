using System;
using UIKit;
namespace NewTestApp
{

    public class SplitViewControllerDelegate : UISplitViewControllerDelegate
    {
        public override bool CollapseSecondViewController(UISplitViewController splitViewController, UIViewController secondaryViewController, UIViewController primaryViewController)
        {
            return true;
        }
    }

    public partial class MainSplitViewController : UISplitViewController
    {
        

        public OpcConnection OpcUa;
        public int test_cnt=6;

        public MainSplitViewController (IntPtr handle) : base (handle)
        {
            
            
            this.Delegate = new SplitViewControllerDelegate();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.SetNeedsFocusUpdate();
        }
    }
}