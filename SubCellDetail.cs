using CoreGraphics;
using Foundation;
using System;
using UIKit;

namespace NewTestApp
{
    public partial class SubCellDetail : UITableViewCell
    {
        
        public SubCellDetail (IntPtr handle) : base (handle)
        {
        }
        public UILabel tagName, tagValue;
        
        public SubCellDetail (NSString cellId) : base (UITableViewCellStyle.Default, cellId)
        {

            tagName = new UILabel()
            {

                TextColor = UIColor.LinkColor,
                TextAlignment = UITextAlignment.Left,
                
                BackgroundColor = UIColor.Clear
            };
            tagValue = new UILabel() {
                TextColor = UIColor.LabelColor,
                TextAlignment = UITextAlignment.Right,
                BackgroundColor = UIColor.Clear
            };
            ContentView.AddSubviews(new UIView[] {tagName, tagValue});

        }
        public void UpdateCell(string name, string value)
        {

            tagName.Text = name;
            tagValue.Text = value;
        }
        
        public override void LayoutSubviews ()
        {
            base.LayoutSubviews ();
            var padding = 15;
            tagName.Frame = new CGRect (padding, 0, ContentView.Bounds.Width / 2.0-padding, ContentView.Bounds.Height);
            tagValue.Frame = new CGRect (ContentView.Bounds.Width / 2.0+padding, 0, ContentView.Bounds.Width / 2.0-2*padding, ContentView.Bounds.Height);
        }

    }
}