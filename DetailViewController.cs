﻿using System;
using System.Threading.Tasks;
using UIKit;
using Opc.Ua;   // Install-Package OPCFoundation.NetStandard.Opc.Ua
using Opc.Ua.Client;

namespace NewTestApp
{
    public partial class DetailViewController : UIViewController
    {
        public object DetailItem { get; set; }
        public DataValue valData { get; set; } 
        private bool editLock { get; set; }
        MainSplitViewController rootvc { get; set; }
        ReferenceDescription litem { get; set; }
        NodeId localNodeid { get; set; }

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
            rootvc = (MainSplitViewController)SplitViewController;
            litem = DetailItem as ReferenceDescription;
            localNodeid = ExpandedNodeId.ToNodeId(litem.NodeId, rootvc.OpcUa.m_session.NamespaceUris);
            valData = rootvc.OpcUa.m_session.ReadValue(localNodeid);
            rootvc.OpcUa.subDict[localNodeid] = valData;
            DataChangeBox.Text = "";
            bool subscribed = false;
            foreach (MonitoredItem monitorItem in rootvc.OpcUa.m_subscription.MonitoredItems)
            {
                if (monitorItem.ResolvedNodeId == localNodeid)
                {
                    subscribed = true;
                    break;
                    
                }
            }
            SubscribeSwitch.SetState(subscribed, false);

            rootvc.OpcUa.CreateMonitoredItem(localNodeid, litem.DisplayName.ToString());

           

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
                foreach (MonitoredItem monitorItem in rootvc.OpcUa.m_subscription.MonitoredItems)
                {
                    if (monitorItem.ResolvedNodeId == localNodeid)
                    {
                        rootvc.OpcUa.m_subscription.RemoveItem(monitorItem);
                        rootvc.OpcUa.subDict.Remove(localNodeid);
                    }
                }
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
                        //fix section to include something better than m_sub_val
                        if (rootvc.OpcUa.subDict.ContainsKey(localNodeid))
                        {
                            detailDescriptionLabel.Text = rootvc.OpcUa.subDict[localNodeid].ToString();
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

            ResponseHeader responseHeader = rootvc.OpcUa.m_session.Write(
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


