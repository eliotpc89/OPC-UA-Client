using System;
using System.Collections.Generic;
using UIKit;
using Foundation;
using Opc.Ua;   // Install-Package OPCFoundation.NetStandard.Opc.Ua

namespace NewTestApp
{
    public partial class NodeViewController : UITableViewController
    {
        DataSource dataSource;
        UIBarButtonItem upButton;
        public OpcConnection OpcUa;
        protected NodeViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.

            var upButtonImage = UIImage.GetSystemImage("chevron.up");

            upButton = new UIBarButtonItem(upButtonImage, UIBarButtonItemStyle.Plain, BackButtonAct);
            NavigationItem.RightBarButtonItem = upButton;

            TableView.Source = dataSource = new DataSource(this);
            foreach (var ii in OpcUa.NodeTreeLoc.Children)
            {
                dataSource.Objects.Add(ii);
                using (var indexPath = NSIndexPath.FromRowSection(0, 0))
                {
                    TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);

                }
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBarHidden = false;
            this.Title = OpcUa.NodeTreeLoc.Data.DisplayName.ToString();

        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        void BackButtonAct(object sender, EventArgs args)
        {
            if (OpcUa.NodeTreeLoc.Data == OpcUa.NodeTreeRoot.Data)
            {
                NavigationController.PopViewController(true);
            }
            OpcUa.BrowsePrevTree();
            if (OpcUa.NodeTreeLoc.Data == OpcUa.NodeTreeRoot.Data)
            {
                this.Title = "Root";


            }
            else
            {
                this.Title = OpcUa.NodeTreeLoc.Data.DisplayName.ToString();
            }


            dataSource.Objects.Clear();
            foreach (var rd in OpcUa.NodeTreeLoc.Children)
            {
                dataSource.Objects.Add(rd);

            }
            TableView.ReloadData();

        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {

            if (segue.Identifier == "showDetail")
            {

                var indexPath = TableView.IndexPathForSelectedRow;
                var indexTreeNode = dataSource.Objects[indexPath.Row] as TreeNode<ReferenceDescription>;
                var item = indexTreeNode.Data;



                var nextVc = segue.DestinationViewController
                         as DetailViewController;
                nextVc.OpcUa = OpcUa;
                nextVc.SetDetailItem(item);
                NavigationController.NavigationItem.LeftItemsSupplementBackButton = true;

            }
        }

        public bool ShouldPerformProgramSegue()
        {

            var indexPath = TableView.IndexPathForSelectedRow;
            var indexTreeNode = dataSource.Objects[indexPath.Row] as TreeNode<ReferenceDescription>;
            var item = indexTreeNode.Data;
            Console.WriteLine("shouldperform?");
            if (OpcUa.BrowseNextTree(indexTreeNode))
            {
                this.Title = item.DisplayName.ToString();
                dataSource.Objects.Clear();


                foreach (var rd in OpcUa.NodeTreeLoc.Children)
                {
                    dataSource.Objects.Add(rd);
                    Console.WriteLine(rd.Data.DisplayName);
                    Console.WriteLine(rd.Data.TypeDefinition.ToString());

                }

                TableView.ReloadData();
                return false;
            }
            OpcUa.NodeTreeLoc = OpcUa.NodeTreeLoc.Parent;


            return true;
        }

        class DataSource : UITableViewSource
        {
            static readonly NSString CellIdentifier = new NSString("Cell");
            readonly List<object> objects = new List<object>();
            readonly NodeViewController controller;

            public DataSource(NodeViewController controller)
            {
                this.controller = controller;
            }

            public IList<object> Objects
            {
                get { return objects; }
            }

            // Customize the number of sections in the table view.
            public override nint NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return objects.Count;
            }
            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                //use this method we can push to a new viewController
                //the first parameter is the identifier we set above

                Console.WriteLine("Table Button Pressed{0}", indexPath.ToString());
                if (controller.ShouldPerformProgramSegue())
                {
                    controller.PerformSegue("showDetail", indexPath);
                }



            }
            // Customize the appearance of table view cells.
            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(CellIdentifier, indexPath);
                var cellTreeNode = objects[indexPath.Row] as TreeNode<ReferenceDescription>;
                var node = cellTreeNode.Data;
                cell.TextLabel.Text = node.DisplayName.ToString();
                Console.WriteLine(node.NodeClass.ToString());
                if (node.NodeClass.ToString() == "Object")
                {
                    cell.ImageView.Image = UIImage.GetSystemImage("square.grid.2x2");
                }
                else
                {
                    cell.ImageView.Image = UIImage.GetSystemImage("square");
                }

                return cell;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                // Return false if you do not want the specified item to be editable.
                return true;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                if (editingStyle == UITableViewCellEditingStyle.Delete)
                {
                    // Delete the row from the data source.
                    objects.RemoveAt(indexPath.Row);
                    controller.TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Fade);
                }
                else if (editingStyle == UITableViewCellEditingStyle.Insert)
                {
                    // Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view.
                }
            }
        }







    }
}
