using System;
using System.Threading.Tasks;
using UIKit;
using Opc.Ua;   // Install-Package OPCFoundation.NetStandard.Opc.Ua
using Opc.Ua.Client;

namespace NewTestApp
{
    public partial class DetailViewController : UITableViewController
    {
        public object DetailItem { get; set; }
        public DataValue valData { get; set; }
        public MonitoredItem monitoredItem { get; set; }
        private bool editLock { get; set; }

        ReferenceDescription litem { get; set; }
        NodeId localNodeid { get; set; }
        public OpcConnection OpcUa { get; set; }
        public DetailViewController(IntPtr handle) : base(handle)
        {
        }
        
        public void SetDetailItem(object newDetailItem)
        {
            if (DetailItem != newDetailItem)
            {
                DetailItem = newDetailItem;

                // Update the view
                ConfigureView();
            }
        }
        
        void ConfigureView()
        {
            
            // Update the user interface for the detail item
            if (IsViewLoaded && DetailItem != null)
            {


                
                detailDescriptionLabel.Text = DetailItem.ToString();
                
            }
        }

        public override void ViewDidLoad()
        {
            
            base.ViewDidLoad();

            litem = DetailItem as ReferenceDescription;
            localNodeid = ExpandedNodeId.ToNodeId(litem.NodeId, OpcUa.m_session.NamespaceUris);
            valData = OpcUa.m_session.ReadValue(localNodeid);
            DetailTitleBar.Title = litem.DisplayName.ToString();

            DataChangeBox.Text = "";
            bool subscribed = false;
            foreach (MonitoredItem iMonitorItem in OpcUa.m_subscription.MonitoredItems)
            {
                if (iMonitorItem.ResolvedNodeId == localNodeid)
                {
                    Console.WriteLine(localNodeid.ToString());
                    subscribed = true;
                    monitoredItem = iMonitorItem;
                    break;
                    
                }
            }
            if (!subscribed)
            {
                    
                monitoredItem = new MonitoredItem(OpcUa.m_subscription.DefaultItem);

                
            }
            string dispNamePfx = OpcUa.NodeTreeDict[localNodeid].Parent.Data.DisplayName.ToString();
            var dispName = dispNamePfx + "." + litem.DisplayName.ToString();
            OpcUa.CreateMonitoredItem(localNodeid, dispName, monitoredItem);
            OpcUa.subDict[localNodeid] = new MonitorValue(monitoredItem, valData);
            SubscribeSwitch.SetState(subscribed, false);
            TypeLabelVar.Text = valData.WrappedValue.TypeInfo.ToString();
            var ipath = DetailViewTable.IndexPathsForVisibleRows;


            DataChangeBox.KeyboardType = UIKeyboardType.NumbersAndPunctuation;
            // Perform any additional setup after loading the view, typically from a nib.
            PollDataAsync();
            ConfigureView();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (!SubscribeSwitch.On)
            {
                OpcUa.RemoveMonitoredItem(localNodeid);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        async void PollDataAsync()
        {

            //TestLabel.ResignFirstResponder();
            while (true)
            {
                await Task.Run(async () =>
                {
                   InvokeOnMainThread(() =>
                    {
                        if (OpcUa.subDict.ContainsKey(localNodeid))
                        {
                            detailDescriptionLabel.Text = OpcUa.subDict[localNodeid].value.WrappedValue.ToString();//lmonlist[0].Value.ToString();

                        }


                    });
                    
                    await Task.Delay(100); // example purpose only
                });
    
            }
        }
 

        partial void WriteDataButtonUp(UIButton sender)
        {

            WriteValue valueToWrite = new WriteValue();


            valueToWrite.Value = valData;
            valueToWrite.NodeId = localNodeid;
            try
            {
                valueToWrite.Value.Value = OpcConnection.ChangeType(DataChangeBox.Text, valData.WrappedValue.TypeInfo.BuiltInType);
            }
            catch
            {
                var BadValAlert = UIAlertController.Create("Write Error", "Invalid Value", UIAlertControllerStyle.Alert);
                BadValAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(BadValAlert, true, null);
            }
            valueToWrite.AttributeId = Attributes.Value;
            valueToWrite.Value.StatusCode = StatusCodes.Good;
            valueToWrite.Value.ServerTimestamp = DateTime.MinValue;
            valueToWrite.Value.SourceTimestamp = DateTime.MinValue;

            WriteValueCollection valuesToWrite = new WriteValueCollection();
            valuesToWrite.Add(valueToWrite);

            // write current value.
            StatusCodeCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            ResponseHeader responseHeader = OpcUa.m_session.Write(
                null,
                valuesToWrite,
                out results,
                out diagnosticInfos);

            ClientBase.ValidateResponse(results, valuesToWrite);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, valuesToWrite);

            // check for error.
            if (StatusCode.IsBad(results[0]))
            {
                throw ServiceResultException.Create(results[0], 0, diagnosticInfos, responseHeader.StringTable);
            }
        }

        
  
    }
    
}


