using System;
using System.Collections.Generic;
using UIKit;
using Foundation;
using Opc.Ua;

namespace NewTestApp
{
    public partial class SubTableViewController : UITableViewController
    {
        public OpcConnection OpcUa;
        DataSource dataSource;
        public SubTableViewController (IntPtr handle) : base (handle)
        {
        }
        public class NodeValue
        {
            public NodeId NodeId;
            public DataValue Value;
            public NodeValue(NodeId iNodeId, DataValue iValue)
            {
                NodeId = iNodeId;
                Value = iValue;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();


            TableView.Source = dataSource = new DataSource(this);
            Console.WriteLine(OpcUa.subDict.Count);
            foreach(var ii in OpcUa.subDict)
            {

                dataSource.Objects.Add(new NodeValue(ii.Key, ii.Value));
                Console.WriteLine(ii.Key.Identifier.ToString());
                using (var indexPath = NSIndexPath.FromRowSection(0, 0))
                {
                    TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                    
                }
            }
            TableView.ReloadData();



        }

        class DataSource : UITableViewSource
        {
            static readonly NSString CellIdentifier = new NSString("Cell");
            readonly List<object> objects = new List<object>();
            readonly SubTableViewController controller;

            public DataSource(SubTableViewController controller)
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
                var node = objects[indexPath.Row] as NodeValue;
                cell.ImageView.Image = UIImage.GetSystemImage("square.grid.2x2");
                cell.TextLabel.Text = node.NodeId.ToString();

         

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