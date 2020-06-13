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
            //ReloadTable();


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
            async void PollDataAsync(SubCellDetail cell, MonitorValue node, UITableView table)
            {

                //TestLabel.ResignFirstResponder();
                while (true)
                {
                    await Task.Run(async () =>
                    {
                        InvokeOnMainThread(() =>
                        {

                            var cellName = node.monItem.DisplayName;
                            var cellValue = "";
                            if (controller.OpcUa.subDict.ContainsKey(node.monItem.ResolvedNodeId))
                            {
                                cellValue= controller.OpcUa.subDict[node.monItem.ResolvedNodeId].value.WrappedValue.ToString();

                            }
                            if (cell != null)
                            {
                                cell.UpdateCell(node.monItem.DisplayName, cellValue);

                            }

                            //table.ReloadData();

                        });

                        await Task.Delay(100); // example purpose only
                    });

                }
            }
            // Customize the appearance of table view cells.
            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                NSString cellId = (NSString) "CellDetail" ;
                var cell = tableView.DequeueReusableCell(cellId) as SubCellDetail;
                var node = objects[indexPath.Row] as MonitorValue;
                
                //cell = new UITableViewCell(UITableViewCellStyle.Value2, "cell");
                
                cell.ResignFirstResponder();
                // cell.TextLabel.Text = node.monItem.DisplayName;
                // cell.DetailTextLabel.Text = controller.OpcUa.subDict[node.monItem.ResolvedNodeId].value.WrappedValue.ToString();



                cell= new SubCellDetail(cellId);


                PollDataAsync(cell, node, tableView);

                

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
                    var node = objects[indexPath.Row] as MonitorValue;
                    objects.RemoveAt(indexPath.Row);
                    controller.OpcUa.subDict.Remove(node.monItem.ResolvedNodeId);
                    controller.OpcUa.m_subscription.RemoveItem(node.monItem);
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