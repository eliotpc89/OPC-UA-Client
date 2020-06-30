using Opc.Ua;   // Install-Package OPCFoundation.NetStandard.Opc.Ua
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;

using System.IO;
using Newtonsoft.Json;
using Foundation;
using UIKit;

namespace NewTestApp
{

    public class MonitorValue
    {
        public MonitoredItem monItem;
        public DataValue value;
        public string dataName;
        public MonitorValue(MonitoredItem iMonItem, DataValue iValue)
        {

            monItem = iMonItem;
            value = iValue;

        }
    }
    public class SavedObject
    {
        public IEnumerable<MonitoredItem> fileSubMon;
        public string fileSavedAddress;
        public SavedObject() { }
        public SavedObject(OpcConnection opcCon)
        {
            fileSavedAddress = opcCon.savedAddress;
            fileSubMon = opcCon.m_subscription.MonitoredItems;
        }
    }
    public class OpcConnection
    {
        public EndpointDescription selectedEndpoint { get; private set; }
        private bool connected { get; set; }
        public Session m_session { get; private set; }
        public SessionReconnectHandler reconnectHandler;

        public TreeNode<ReferenceDescription> NodeTreeRoot { get; set; }
        public TreeNode<ReferenceDescription> NodeTreeLoc { get; set; }
        public Dictionary<NodeId, TreeNode<ReferenceDescription>> NodeTreeDict { get; set; }
        public ApplicationInstance m_application { get; set; }
        public string savedAddress { get; set; }
        public List<MonitoredItem> SubList { get; set; }
        public Subscription m_subscription { get; set; }
        public string m_sub_val { get; set; }
        public string filePath { get; set; }
        public string appDataPath { get; set; }
        public string fileName { get; set; }
        public string fullFileName { get; set; }
        public string fileContents { get; set; }
        public MonitoredItem monitoredItem { get; set; }
        public Dictionary<NodeId, MonitorValue> subDict { get; set; }
        public MonitoredItemNotificationEventHandler m_MonitoredItem_Notification;
        const int ReconnectPeriod = 3;


        public OpcConnection()
        {
            filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        public OpcConnection(NSUrl fname)
        {
            fileName = fname.LastPathComponent;
            string tempName = fname.AbsoluteString;
            tempName = tempName.Remove(0, "file://".Length);
            fullFileName = tempName;
            filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var savedObject = new SavedObject();
            string fileArr = filePath + "/" + fileName;
            NSData data = new NSData();
            data = NSData.FromFile(tempName);
           
            Console.WriteLine(tempName);
            Console.WriteLine(data);

            if (data != null)
            {
                if (!(subDict is null))
                {
                    ResetOpc();
                }
                Console.WriteLine("found file");
                savedObject = JsonConvert.DeserializeObject<SavedObject>(data.ToString());
                savedAddress = savedObject.fileSavedAddress;
                Connect(savedAddress);
                CreateMonitoredItems(savedObject.fileSubMon);


            }



        }
        public void ResetOpc()
        {
            if (connected)
            {
                connected = false;
                if (subDict != null)
                {
                    List<NodeId> nodeList = new List<NodeId>(subDict.Keys);

                    foreach (NodeId ii in nodeList)
                    {
                        RemoveMonitoredItem(ii, false);
                        Console.WriteLine("removing item");
                    }

                    m_session.CloseSession(null, true);
                }
            }


        }
        public void Connect(string OpcAddress)

        {

            savedAddress = OpcAddress;
            Console.WriteLine("Step 1 - Create application configuration and certificate.");
            var config = new ApplicationConfiguration()
            {
                ApplicationName = "MyHomework",
                ApplicationUri = Utils.Format(@"urn:{0}:MyHomework", System.Net.Dns.GetHostName()),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = appDataPath + @"\OPC Foundation\CertificateStores\MachineDefault", SubjectName = "MyHomework" },

                    AutoAcceptUntrustedCertificates = true,


                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 5000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 3000 },
                TraceConfiguration = new TraceConfiguration()
            };

            config.Validate(ApplicationType.Client).GetAwaiter().GetResult();
            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
            }

            m_application = new ApplicationInstance
            {
                ApplicationName = "MyHomework",
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = config
            };
            //application.CheckApplicationInstanceCertificate(false, 2048).GetAwaiter().GetResult();

            var selectedEndpoint = CoreClientUtils.SelectEndpoint(OpcAddress, useSecurity: false, operationTimeout: 9000);

            m_MonitoredItem_Notification = new MonitoredItemNotificationEventHandler(MonitoredItem_Notification);
            Console.WriteLine($"Step 2 - Create a session with your server: {selectedEndpoint.EndpointUrl} ");
            m_session = Session.Create(config, new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(config)), false, "", 60000, null, null).GetAwaiter().GetResult();
            m_session.KeepAlive += Client_KeepAlive;
            m_subscription = new Subscription(m_session.DefaultSubscription) { PublishingInterval = 100 };
            m_session.AddSubscription(m_subscription);
            m_subscription.Create();
            connected = true;
            Console.WriteLine("Step 3 - Browse the server namespace.");

            ReferenceDescription rootRef = new ReferenceDescription();
            rootRef.DisplayName = "Root";
            NodeTreeRoot = new TreeNode<ReferenceDescription>(rootRef);

            NodeTreeLoc = new TreeNode<ReferenceDescription>(null);
            NodeTreeLoc = NodeTreeRoot;
            NodeId rootNodeId = ObjectIds.ObjectsFolder;
            NodeTreeDict = new Dictionary<NodeId, TreeNode<ReferenceDescription>>();
            NodeTreeDict[rootNodeId] = NodeTreeRoot;
            NodeTreeRoot.Parent = NodeTreeRoot;
            NodeTreeRoot.Data.NodeId = rootNodeId;
            subDict = new Dictionary<NodeId, MonitorValue>();
            BrowseNextTree(NodeTreeRoot);



        }

        private void Client_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            if (connected && e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                Console.WriteLine("{0} {1}/{2}", e.Status, sender.OutstandingRequestCount, sender.DefunctRequestCount);

                if (reconnectHandler == null)
                {
                    Console.WriteLine("--- RECONNECTING ---");
                    reconnectHandler = new SessionReconnectHandler();
                    reconnectHandler.BeginReconnect(sender, ReconnectPeriod * 1000, Client_ReconnectComplete);
                }
            }
        }
        private void Client_ReconnectComplete(object sender, EventArgs e)
        {
            // ignore callbacks from discarded objects.
            if (!Object.ReferenceEquals(sender, reconnectHandler))
            {
                return;
            }

            m_session = reconnectHandler.Session;
            reconnectHandler.Dispose();
            reconnectHandler = null;
            var tempSubList = (List<Subscription>)m_session.Subscriptions;
            m_subscription = tempSubList[0];
            Console.WriteLine("--- RECONNECTED ---");
        }

        public bool BrowseNextTree(TreeNode<ReferenceDescription> treeNode)
        {
            ReferenceDescriptionCollection nextRefs;
            Byte[] cp;

            NodeId lNodeId = ExpandedNodeId.ToNodeId(treeNode.Data.NodeId, m_session.NamespaceUris);
            NodeTreeLoc = treeNode;

            m_session.Browse(null, null, lNodeId, 10000u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out cp, out nextRefs);


            if (nextRefs.Count > 0)
            {

                if (NodeTreeDict[lNodeId].Children.Count == 0)
                {
                    foreach (var nextRd in nextRefs)
                    {
                        var iNodeId = ExpandedNodeId.ToNodeId(nextRd.NodeId, m_session.NamespaceUris);
                        NodeTreeDict[iNodeId] = NodeTreeLoc.AddChild(nextRd);
                        Console.WriteLine("Next: {0}", NodeTreeDict[iNodeId].Data.DisplayName);
                    }


                }

                return true;
            }
            else
            {

                return false;
            }
        }

        public string BrowsePrevTree()
        {

            NodeTreeLoc = NodeTreeLoc.Parent;
            return NodeTreeLoc.Parent.Data.DisplayName.ToString();
        }



        public void CreateMonitoredItems(IEnumerable<MonitoredItem> fSubMon)
        {
            m_subscription.RemoveItems(m_subscription.MonitoredItems);

            foreach (MonitoredItem ii in fSubMon)
            {
                CreateMonitoredItem(ii.ResolvedNodeId, ii.DisplayName, false);
            }
            UpdateDict();
            m_subscription.ApplyChanges();
            SaveFile();

        }
        public void CreateMonitoredItem(NodeId nodeId, string displayName, bool saveFile)
        {

            // add the new monitored item.

            monitoredItem = new MonitoredItem(m_subscription.DefaultItem);
            monitoredItem.StartNodeId = nodeId;
            monitoredItem.AttributeId = Attributes.Value;
            monitoredItem.DisplayName = displayName;
            monitoredItem.MonitoringMode = MonitoringMode.Reporting;
            monitoredItem.SamplingInterval = 100;
            monitoredItem.QueueSize = 0;
            monitoredItem.DiscardOldest = false;
            monitoredItem.Notification += m_MonitoredItem_Notification;

            m_subscription.AddItem(monitoredItem);

            if (saveFile)
            {

                subDict[nodeId] = new MonitorValue(monitoredItem, new DataValue(0));
                m_subscription.ApplyChanges();
                SaveFile();

            }

        }

        public void UpdateDict()
        {
            subDict.Clear();
            foreach (MonitoredItem ii in m_subscription.MonitoredItems)
            {
                subDict[ii.ResolvedNodeId] = new MonitorValue(ii, new DataValue(0));
            }
        }
        public void SaveFile()
        {

            var json = new SavedObject(this);
            //var fname = Path.Combine(filePath, fileName);
            Console.WriteLine(fullFileName);
            File.WriteAllText(fullFileName, JsonConvert.SerializeObject(json));


        }



        public void RemoveMonitoredItem(NodeId rNodeId, bool saveFile)
        {

            foreach (MonitoredItem monitorItem in m_subscription.MonitoredItems)
            {
                if (monitorItem.ResolvedNodeId == rNodeId)
                {
                    m_subscription.RemoveItem(monitorItem);
                    if (subDict.ContainsKey(rNodeId))
                    {
                        subDict.Remove(rNodeId);
                    }

                    break;
                }
            }
            if (saveFile)
            {
                SaveFile();
            }

        }
        public void MonitoredItem_Notification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {



            MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;

            subDict[monitoredItem.ResolvedNodeId] = new MonitorValue(monitoredItem, notification.Value);
            foreach (var value in monitoredItem.DequeueValues())
            {
                Console.WriteLine("{0}: {1}, {2}, {3}", monitoredItem.DisplayName, value.Value, value.SourceTimestamp, value.StatusCode);
                subDict[monitoredItem.ResolvedNodeId].value = value;

            }
            m_sub_val = notification.Value.ToString();

            //Console.WriteLine("MonitoringNotification");
        }

        public static object ChangeType(string valueText, BuiltInType bitype)
        {
            object value;

            switch (bitype)
            {
                case BuiltInType.Boolean:
                    {
                        value = Convert.ToBoolean(valueText);
                        break;
                    }

                case BuiltInType.SByte:
                    {
                        value = Convert.ToSByte(valueText);
                        break;
                    }

                case BuiltInType.Byte:
                    {
                        value = Convert.ToByte(valueText);
                        break;
                    }

                case BuiltInType.Int16:
                    {
                        value = Convert.ToInt16(valueText);
                        break;
                    }

                case BuiltInType.UInt16:
                    {
                        value = Convert.ToUInt16(valueText);
                        break;
                    }

                case BuiltInType.Int32:
                    {
                        value = Convert.ToInt32(valueText);
                        break;
                    }

                case BuiltInType.UInt32:
                    {
                        value = Convert.ToUInt32(valueText);
                        break;
                    }

                case BuiltInType.Int64:
                    {
                        value = Convert.ToInt64(valueText);
                        break;
                    }

                case BuiltInType.UInt64:
                    {
                        value = Convert.ToUInt64(valueText);
                        break;
                    }

                case BuiltInType.Float:
                    {
                        value = Convert.ToSingle(valueText);
                        break;
                    }

                case BuiltInType.Double:
                    {
                        value = Convert.ToDouble(valueText);
                        break;
                    }

                default:
                    {
                        value = valueText;
                        break;
                    }
            }

            return value;
        }
        public void ConnectError(UIViewController controller, bool popToRoot, string title, string prompt)
        {
            UIAlertController alert = UIAlertController.Create(title, prompt, UIAlertControllerStyle.Alert);
            var ctrlr = controller as BrowserViewController;
            alert.AddAction(UIAlertAction.Create("Dismiss", UIAlertActionStyle.Default, action =>
            {
                alert.DismissViewController(true, () =>
                {
                    if (popToRoot)
                    {
                        controller.NavigationController.PopToRootViewController(true);
                    }
                    
                });

            }));
            controller.PresentViewController(alert, animated: true, null);
            NodeTreeLoc = NodeTreeRoot;
            ResetOpc();

        }

    }

}
