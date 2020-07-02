using System;
using System.Collections.Generic;
using UIKit;
using Foundation;
using Opc.Ua;   // Install-Package OPCFoundation.NetStandard.Opc.Ua
using CoreGraphics;

namespace NewTestApp
{
    public partial class NodeViewController : UITableViewController
    {
        DataSource dataSource;
        UIBarButtonItem upButton;
        UIScreenEdgePanGestureRecognizer PanRightGR;
        public OpcConnection OpcUa;

        protected NodeViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            PanRightGR = new UIScreenEdgePanGestureRecognizer();
            PanRightGR.AddTarget(() => PanImage(PanRightGR));
            PanRightGR.Edges = UIRectEdge.Left;
            PanRightGR.Delegate = new GestureDelegate(this);
            this.View.AddGestureRecognizer(PanRightGR);
            // Perform any additional setup after loading the view, typically from a nib.
            NavigationController.InteractivePopGestureRecognizer.CanBePreventedByGestureRecognizer(PanRightGR);
            var prevented = PanRightGR.CanPreventGestureRecognizer(NavigationController.InteractivePopGestureRecognizer);

            TableView.Source = dataSource = new DataSource(this);
            foreach (var ii in OpcUa.NodeTreeLoc.Children)
            {
                dataSource.Objects.Add(ii);
                using (var indexPath = NSIndexPath.FromRowSection(0, 0))
                {
                    TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);

                }
            }
            TableView.ReloadSections(new NSIndexSet(0), UITableViewRowAnimation.Left);



        }

        public override void ViewWillAppear(bool animated)
        {

            base.ViewWillAppear(animated);
            NavigationController.NavigationBarHidden = false;
            this.Title = OpcUa.NodeTreeLoc.Data.DisplayName.ToString();
            var upButtonImage = UIImage.GetSystemImage("chevron.left");
            var upButton = new UIBarButtonItem(upButtonImage, UIBarButtonItemStyle.Plain, BackButtonAct);
            var DoneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, DoneButtonAct);
            NavigationItem.LeftBarButtonItem = upButton;
            NavigationItem.RightBarButtonItem = DoneButton;




        }
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            NavigationController.InteractivePopGestureRecognizer.Enabled = false;
        }
        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.InteractivePopGestureRecognizer.Enabled = true;
        }
        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
        void BackAct()
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
            var midX = this.View.Frame.Width / 2.0;
            if (TableView.Center.X != midX)
            {


                TableView.ReloadData();
                TableView.Center = new CGPoint(midX, TableView.Center.Y);


            }

            foreach (var rd in OpcUa.NodeTreeLoc.Children)
            {
                dataSource.Objects.Add(rd);

            }
            TableView.ReloadSections(new NSIndexSet(0), UITableViewRowAnimation.Right);
        }

        void BackButtonAct(object sender, EventArgs args)
        {

            BackAct();
        }
        void DoneButtonAct(object sender, EventArgs args)
        {
            this.NavigationController.PopViewController(true);
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
            bool hasChildren = false;
            try
            {
                hasChildren = OpcUa.BrowseNextTree(indexTreeNode);
            }
            catch
            {
                OpcUa.ConnectError(this, true, "Connection Failed", "Failed to Connect to OPC UA Server");
                return false;
            }

            if (hasChildren)
            {
                this.Title = item.DisplayName.ToString();
                dataSource.Objects.Clear();


                foreach (var rd in OpcUa.NodeTreeLoc.Children)
                {
                    dataSource.Objects.Add(rd);
                    Console.WriteLine(rd.Data.DisplayName);
                    Console.WriteLine(rd.Data.TypeDefinition.ToString());

                }

                TableView.ReloadSections(new NSIndexSet(0), UITableViewRowAnimation.Left);
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
                var nodeId = ExpandedNodeId.ToNodeId(node.NodeId, controller.OpcUa.m_session.NamespaceUris);
                cell.TextLabel.Text = node.DisplayName.ToString();
                Console.WriteLine(node.NodeClass.ToString());
                if (node.NodeClass.ToString() == "Object")
                {
                    cell.ImageView.Image = UIImage.GetSystemImage("square.grid.2x2");
                }
                else
                {
                    if (controller.OpcUa.subDict.ContainsKey(nodeId))
                    {
                        cell.ImageView.Image = UIImage.GetSystemImage("square.fill");
                    }
                    else
                    {
                        cell.ImageView.Image = UIImage.GetSystemImage("square");
                    }

                }

                return cell;
            }


            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                // Return false if you do not want the specified item to be editable.
                var cellTreeNode = objects[indexPath.Row] as TreeNode<ReferenceDescription>;
                var node = cellTreeNode.Data;
                var nodeId = ExpandedNodeId.ToNodeId(node.NodeId, controller.OpcUa.m_session.NamespaceUris);

                return (node.NodeClass.ToString() != "Object");

            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                if (editingStyle == UITableViewCellEditingStyle.Delete)
                {
                    var cellTreeNode = objects[indexPath.Row] as TreeNode<ReferenceDescription>;
                    var node = cellTreeNode.Data;
                    var nodeId = ExpandedNodeId.ToNodeId(node.NodeId, controller.OpcUa.m_session.NamespaceUris);

                    controller.OpcUa.RemoveMonitoredItem(nodeId, true);
                    NSIndexPath[] indexPathArr = { indexPath };
                    controller.TableView.ReloadRows(indexPathArr, UITableViewRowAnimation.Fade);


                }
                else if (editingStyle == UITableViewCellEditingStyle.Insert)
                {
                    // Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view.
                }
            }
            public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
            {   // Optional - default text is 'Delete'
                return "Unsubscribe";
            }
            public override UISwipeActionsConfiguration GetLeadingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
            {
                var flagAction = ContextualFlagAction(indexPath.Row);
                var leadingSwipe = UISwipeActionsConfiguration.FromActions(new UIContextualAction[] { flagAction });

                leadingSwipe.PerformsFirstActionWithFullSwipe = true;
                return leadingSwipe;
            }


            public UIContextualAction ContextualFlagAction(int row)
            {
                var action = UIContextualAction.FromContextualActionStyle(UIContextualActionStyle.Normal,
                    "Subscribe",
                    (FlagAction, view, success) =>
                    {
                        var cellTreeNode = objects[row] as TreeNode<ReferenceDescription>;
                        var node = cellTreeNode.Data;
                        var nodeId = ExpandedNodeId.ToNodeId(node.NodeId, controller.OpcUa.m_session.NamespaceUris);
                        string dispNamePfx = controller.OpcUa.NodeTreeLoc.Data.DisplayName.ToString();
                        string dispName = dispNamePfx + "." + node.DisplayName.ToString();
                        try
                        {
                            if (node.NodeClass.ToString() != "Object")
                            {
                                controller.OpcUa.CreateMonitoredItem(nodeId, dispName, true);
                            }

                        }
                        catch
                        {

                        }
                        NSIndexPath[] indexPath = { NSIndexPath.FromRowSection(row, 0) };
                        controller.TableView.ReloadRows(indexPath, UITableViewRowAnimation.Fade);

                        success(true);
                    });

                action.Image = UIImage.FromFile("feedback.png");
                action.BackgroundColor = UIColor.Blue;

                return action;
            }


        }




        void PanImage(UIScreenEdgePanGestureRecognizer gestureRecognizer)
        {
            //AdjustAnchorPointForGestureRecognizer(gestureRecognizer);
            var view = gestureRecognizer.View;
            var vel = gestureRecognizer.VelocityInView(view);

            Console.WriteLine("Vel: {0}", vel.X);
            if (gestureRecognizer.State == UIGestureRecognizerState.Began || gestureRecognizer.State == UIGestureRecognizerState.Changed)
            {
                var translation = gestureRecognizer.TranslationInView(View);

                view.Center = new CGPoint(view.Center.X + translation.X, view.Center.Y);
                // Reset the gesture recognizer's translation to {0, 0} - the next callback will get a delta from the current position.
                gestureRecognizer.SetTranslation(CGPoint.Empty, view);
            }
            if (gestureRecognizer.State == UIGestureRecognizerState.Ended)
            {
                var retX = 0.0;
                var edgeX = view.Center.X - view.Frame.Width / 2.0;
                bool goBack = false;
                if (edgeX > this.View.Frame.Width / 2 || vel.X > 500.0)
                {
                    retX = this.View.Frame.Width * 2.0;
                    goBack = true;
                }
                else
                {
                    retX = this.View.Frame.Width / 2.0;
                    goBack = false;
                }
                UIViewPropertyAnimator.CreateRunningPropertyAnimator(0.25, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        view.Center = new CGPoint(retX, view.Center.Y);

                    },
                    completion =>
                    {
                        if (goBack)
                        {
                            BackAct();

                        }


                    });

                Console.WriteLine("ended");


            }


        }


    }


    class GestureDelegate : UIGestureRecognizerDelegate
    {
        NodeViewController controller;

        public GestureDelegate(NodeViewController controller)
        {
            this.controller = controller;
        }


        public override bool ShouldReceiveTouch(UIGestureRecognizer aRecogniser, UITouch aTouch)
        {
            return true;
        }

        // Ensure that the pinch, pan and rotate gestures are all recognized simultaneously
        public override bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {

            return false;
        }

        public override bool ShouldBegin(UIGestureRecognizer recognizer)
        {
            return true;

        }
    }
}
