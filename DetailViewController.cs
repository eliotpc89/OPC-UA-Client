using System;
using System.Threading.Tasks;
using UIKit;
using Opc.Ua;   // Install-Package OPCFoundation.NetStandard.Opc.Ua
using Opc.Ua.Client;
using Foundation;

namespace NewTestApp
{
    public partial class DetailViewController : UITableViewController
    {
        public object DetailItem { get; set; }
        public DataValue valData { get; set; }
        public MonitoredItem monitoredItem { get; set; }
        private bool editLock { get; set; }
        public string dispName { get; set; }
        public string typName { get; set; }
        public NodeId localNodeid { get; set; }
        public OpcConnection OpcUa { get; set; }
        public bool viewBool { get; set; }
        public DetailViewController(IntPtr handle) : base(handle)
        {
        }
        
        public void SetDetailItem(ReferenceDescription newDetailItem)
        {
            if (DetailItem != newDetailItem)
            {
                DetailItem = newDetailItem;
                localNodeid = ExpandedNodeId.ToNodeId( newDetailItem.NodeId, OpcUa.m_session.NamespaceUris);
                string dispNamePfx = OpcUa.NodeTreeDict[localNodeid].Parent.Data.DisplayName.ToString();
                typName = newDetailItem.TypeDefinition.ToString();
                dispName = dispNamePfx + "." + newDetailItem.DisplayName.ToString();
                DetailTitleBar.Title = dispName;
                // Update the view
                ConfigureView();
            }
        }
        public void SetDetailItem(NodeId newDetailItem, string nodeName)
        {
            if (DetailItem != newDetailItem)
            {
                DetailItem = newDetailItem;
                typName = OpcUa.subDict[newDetailItem].value.WrappedValue.TypeInfo.ToString();
                DetailTitleBar.Title = nodeName;
                dispName = nodeName;
                localNodeid = newDetailItem;
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
                DetailTitleBar.Title = dispName;
                TypeLabelVar.Text = typName;
            }
        }

        public override void ViewDidLoad()
        {
            
            base.ViewDidLoad();

            //valData = OpcUa.m_session.ReadValue(localNodeid);
            //Node valNode = OpcUa.m_session.ReadNode(localNodeid);
            //ReferenceDescription valRef = OpcUa.m_session.FindDataDescription(localNodeid);

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
                valData = new DataValue(new Variant((int)0));
                
                monitoredItem = new MonitoredItem(OpcUa.m_subscription.DefaultItem);
                OpcUa.CreateMonitoredItem(localNodeid, dispName, true);
                SubscribeSwitch.SetState(subscribed, false);
                
            }
            DataChangeBox.KeyboardType = UIKeyboardType.NumbersAndPunctuation;
            // Perform any additional setup after loading the view, typically from a nib.
            PollDataAsync();
            ConfigureView();
        }
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBarHidden = false;
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = "Back" };
            NavigationController.InteractivePopGestureRecognizer.Enabled = true;
        }
        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (!SubscribeSwitch.On)
            {
                OpcUa.RemoveMonitoredItem(localNodeid, true);
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
                            valData = OpcUa.subDict[localNodeid].value;
                            if (valData.WrappedValue.TypeInfo != null)
                            {
                                typName = valData.WrappedValue.TypeInfo.ToString();
                            }
                            
                            TypeLabelVar.Text = typName;
                            if (typName == "Boolean")
                            {
                                DataChangeBox.Hidden = true;
                                WriteBooleanSwitch.Hidden = false;
                            }
                            else
                            {
                                DataChangeBox.Hidden = false;
                                WriteBooleanSwitch.Hidden = true;
                            }
                            


                        }


                    });
                    
                    await Task.Delay(100); // example purpose only
                });
    
            }
        }


        partial void WriteDataButton(UIButton sender)
        {

            WriteValue valueToWrite = new WriteValue();


            valueToWrite.Value = valData;
            valueToWrite.NodeId = localNodeid;
            try
            {
                if (typName == "Boolean")
                {
                    valueToWrite.Value.Value = OpcConnection.ChangeType(WriteBooleanSwitch.On.ToString(), valData.WrappedValue.TypeInfo.BuiltInType);
                }
                else
                {
                    valueToWrite.Value.Value = OpcConnection.ChangeType(DataChangeBox.Text, valData.WrappedValue.TypeInfo.BuiltInType);
                }
                
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


