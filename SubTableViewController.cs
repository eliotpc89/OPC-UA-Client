using System;
using System.Collections.Generic;
using UIKit;
using Foundation;
using Opc.Ua;
using Opc.Ua.Client;
using System.Threading.Tasks;

namespace NewTestApp
{
    public partial class SubTableViewController : UITableViewController
    {
        public OpcConnection OpcUa;
        DataSource dataSource;
        public SubTableViewController (IntPtr handle) : base (handle)
        {
        }
        async void PollDataAsync(UITableViewCell cell, NodeId node, UITableView table)
        {

            //TestLabel.ResignFirstResponder();
            while (true)
            {
                await Task.Run(async () =>
                {
                    InvokeOnMainThread(() =>
                    {

                        //var DataText= OpcUa.subDict[node].value.WrappedValue.Value.ToString();
                        //Console.WriteLine("async_text_update{0}",DateTime.Now.ToString());
                        table.ReloadData();

                    });

                    await Task.Delay(100); // example purpose only
                });

            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();


            TableView.Source = dataSource = new DataSource(this);
            Console.WriteLine(OpcUa.subDict.Count);
            foreach(var ii in OpcUa.subDict)
            {
                var iinode = ii.Value;
                dataSource.Objects.Add(iinode);
            
                using (var indexPath = NSIndexPath.FromRowSection(0, 0))
                {
                    TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                    
                }
            }
            TableView.ReloadData();
            ReloadTable();


        }
        async void ReloadTable()
        {
            while (true)
            {
                await Task.Run(async () =>
                {
                    InvokeOnMainThread(() =>
                    {

                        TableView.ReloadData();

                    });

                    await Task.Delay(100); // example purpose only
                });

            }
        }

        class DataSource : UITableViewSource
        {
            static readonly NSString CellIdentifier = new NSString("Cell");
            readonly List<object> objects = new List<object>();
            public SubTableViewController controller;

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
                var cell = tableView.DequeueReusableCell("CellDetail");
                var node = objects[indexPath.Row] as MonitorValue;
                //cell.ImageView.Image = UIImage.GetSystemImage("square.grid.2x2");
                cell = new UITableViewCell(UITableViewCellStyle.Value2, "cell");
                cell.ResignFirstResponder();
                cell.TextLabel.Text = node.monItem.DisplayName;
                
                cell.DetailTextLabel.Text =
                    controller.OpcUa.subDict[node.monItem.ResolvedNodeId].value.WrappedValue.ToString()+
                    DateTime.Now.ToString();//node.value.WrappedValue.Value.ToString();
                Console.WriteLine("hi {0}",DateTime.Now.ToString());


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