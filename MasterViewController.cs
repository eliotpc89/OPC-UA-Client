﻿using System;
using System.Collections.Generic;
using UIKit;
using Foundation;
using Opc.Ua;   // Install-Package OPCFoundation.NetStandard.Opc.Ua

namespace NewTestApp
{
    public partial class MasterViewController : UITableViewController
    {
        DataSource dataSource;
        UIBarButtonItem backButton; 
        protected MasterViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

       

            Title = NSBundle.MainBundle.GetLocalizedString("Master", "Master");
            SplitViewController.PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;
            MainSplitViewController rootvc = (MainSplitViewController)SplitViewController;
            

            // Perform any additional setup after loading the view, typically from a nib.
            
            backButton = new UIBarButtonItem("",UIBarButtonItemStyle.Plain, AddNewItem);
            backButton.AccessibilityLabel = "addButton";
            if (rootvc.OpcUa.NodeTreeLoc.Data == rootvc.OpcUa.NodeTreeRoot.Data)
            {
                backButton.Title = "";
                
            }
            else
            {
                backButton.Title = "<" + " " + rootvc.OpcUa.NodeTreeLoc.Data.DisplayName;
                
            }
            NavigationItem.LeftBarButtonItem = backButton;
  
            TableView.Source = dataSource = new DataSource(this);
            foreach(var ii in rootvc.OpcUa.NodeTreeLoc.Children)
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
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        void AddNewItem(object sender, EventArgs args)
        {
            MainSplitViewController rootvc = (MainSplitViewController)SplitViewController;
            rootvc.OpcUa.BrowsePrevTree();
            if (rootvc.OpcUa.NodeTreeLoc.Data == rootvc.OpcUa.NodeTreeRoot.Data)
            {
                backButton.Title = "";
               
            }
            else
            {
                backButton.Title = "<" + " " + rootvc.OpcUa.NodeTreeLoc.Data.DisplayName;
                this.NavigationItem.LeftBarButtonItem = backButton;
            }
            
            
            dataSource.Objects.Clear();
            foreach (var rd in rootvc.OpcUa.NodeTreeLoc.Children)
            {
                dataSource.Objects.Add(rd);

            }
            TableView.ReloadData();

        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
           
            if (segue.Identifier == "showDetail")
            {
                MainSplitViewController rootvc = (MainSplitViewController)SplitViewController;
                var indexPath = TableView.IndexPathForSelectedRow;
                var indexTreeNode = dataSource.Objects[indexPath.Row] as TreeNode<ReferenceDescription>;
                var item = indexTreeNode.Data;



                var controller = (DetailViewController)((UINavigationController)segue.DestinationViewController).TopViewController;
                controller.OpcUa = rootvc.OpcUa;
                
                controller.SetDetailItem(item) ;
                controller.NavigationItem.LeftBarButtonItem = SplitViewController.DisplayModeButtonItem;
                controller.NavigationItem.LeftItemsSupplementBackButton = true;
                
            }
        }

        public override bool ShouldPerformSegue(string   segueIdentifier, NSObject sender)
        {
            
            if (segueIdentifier == "showDetail")
            {
                MainSplitViewController rootvc = (MainSplitViewController)SplitViewController;

                var indexPath = TableView.IndexPathForSelectedRow;
                var indexTreeNode = dataSource.Objects[indexPath.Row] as TreeNode<ReferenceDescription>;
                var item = indexTreeNode.Data;

                if (rootvc.OpcUa.BrowseNextTree(indexTreeNode))
                {
                    backButton.Title = "<"+" " + item.DisplayName.ToString();
                    dataSource.Objects.Clear();
                    
                    
                    foreach (var rd in rootvc.OpcUa.NodeTreeLoc.Children)
                    {
                        dataSource.Objects.Add(rd);
                        Console.WriteLine(rd.Data.DisplayName);
                        Console.WriteLine(rd.Data.TypeDefinition.ToString());

                        
                    }

                    TableView.ReloadData();
                    return false;
                }
                rootvc.OpcUa.NodeTreeLoc = rootvc.OpcUa.NodeTreeLoc.Parent;
            }
            
            return base.ShouldPerformSegue(segueIdentifier, sender);
        }

        class DataSource : UITableViewSource
        {
            static readonly NSString CellIdentifier = new NSString("Cell");
            readonly List<object> objects = new List<object>();
            readonly MasterViewController controller;

            public DataSource(MasterViewController controller)
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
