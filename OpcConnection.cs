using Opc.Ua;   // Install-Package OPCFoundation.NetStandard.Opc.Ua
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;
using UIKit;
namespace NewTestApp
{

    public class MonitorValue
    {
        public MonitoredItem monItem;
        public DataValue value;
        public MonitorValue(MonitoredItem iMonItem, DataValue iValue)
        {

            monItem = iMonItem;
            value = iValue;

        }
    }
    public class OpcConnection
    {
        public EndpointDescription selectedEndpoint { get; private set; }

        public Session m_session { get; private set; }

        public ReferenceDescriptionCollection NodeList { get; set; }
        public TreeNode<ReferenceDescription> NodeTreeRoot { get; set; }
        public TreeNode<ReferenceDescription> NodeTreeLoc { get; set; }
        public Dictionary<NodeId, TreeNode<ReferenceDescription>> NodeTreeDict { get; set; }
        public Stack<String> NodePath { get; set; }
        public Stack<ReferenceDescriptionCollection> NodeHistory { get; set; }
        public List<MonitoredItem> SubList { get; set; }
        public Subscription m_subscription { get; set; }
        public string m_sub_val { get; set; }
        public MonitoredItem lastMonitoredItem { get; set; }
        public Dictionary<NodeId, MonitorValue> subDict {get; set;}
        private MonitoredItemNotificationEventHandler m_MonitoredItem_Notification;


        public void addMonitorValue(NodeId iNodeId, MonitoredItem iMonItem, DataValue iValue)
        {
            MonitorValue newMonVal = new MonitorValue(iMonItem, iValue);
            subDict[iNodeId] = newMonVal;
        }
        public OpcConnection() { }


        public void Connect(string OpcAddress)

        {

            Console.WriteLine("Step 1 - Create application configuration and certificate.");
            var config = new ApplicationConfiguration()
            {
                ApplicationName = "MyHomework",
                ApplicationUri = Utils.Format(@"urn:{0}:MyHomework", System.Net.Dns.GetHostName()),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault", SubjectName = "MyHomework" },
                    TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" },
                    TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" },
                    RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                    AutoAcceptUntrustedCertificates = true
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                TraceConfiguration = new TraceConfiguration()
            };
            config.Validate(ApplicationType.Client).GetAwaiter().GetResult();
            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
            }

            var application = new ApplicationInstance
            {
                ApplicationName = "MyHomework",
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = config
            };
            //application.CheckApplicationInstanceCertificate(false, 2048).GetAwaiter().GetResult();

            var selectedEndpoint = CoreClientUtils.SelectEndpoint(OpcAddress, useSecurity: false, operationTimeout: 15000);
            m_MonitoredItem_Notification = new MonitoredItemNotificationEventHandler(MonitoredItem_Notification);
            Console.WriteLine($"Step 2 - Create a session with your server: {selectedEndpoint.EndpointUrl} ");
            m_session = Session.Create(config, new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(config)), false, "", 60000, null, null).GetAwaiter().GetResult();
            m_subscription = new Subscription(m_session.DefaultSubscription) { PublishingInterval = 100 };
            m_session.AddSubscription(m_subscription);
            m_subscription.Create();
            Console.WriteLine("Step 3 - Browse the server namespace.");
            ReferenceDescriptionCollection refs;
            Byte[] cp;
            NodeList = new ReferenceDescriptionCollection();
            NodeHistory = new Stack<ReferenceDescriptionCollection>();
            NodePath = new Stack<String>();
            ReferenceDescription rootRef = new ReferenceDescription();
            rootRef.DisplayName = "Root >";
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
            //m_session.Browse(null, null, ObjectIds.ObjectsFolder, 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out cp, out refs);
            //DisplayName: BrowseName, NodeClass"
            NodePath.Push("");
            //foreach (var rd in refs)
            //{
            //    NodeList.Add(rd);
            //    NodeTreeDict[ExpandedNodeId.ToNodeId(rd.NodeId,m_session.NamespaceUris)] = NodeTreeRoot.AddChild(rd); ;
            //}


  


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

                if (NodeTreeDict[lNodeId].Children.Count==0)
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



        public void CreateMonitoredItem(NodeId nodeId, string displayName, MonitoredItem monitoredItem)
        {

            // add the new monitored item.
            
            lastMonitoredItem = new MonitoredItem(m_subscription.DefaultItem);
            monitoredItem.StartNodeId = nodeId;
            monitoredItem.AttributeId = Attributes.Value;
            monitoredItem.DisplayName = displayName;
            monitoredItem.MonitoringMode = MonitoringMode.Reporting;
            monitoredItem.SamplingInterval = 100;
            monitoredItem.QueueSize = 0;
            monitoredItem.DiscardOldest = false;
          
            monitoredItem.Notification += m_MonitoredItem_Notification;
            lastMonitoredItem = monitoredItem;
            m_subscription.AddItem(monitoredItem);
            m_subscription.ApplyChanges();

            
        }
        public void RemoveMonitoredItem(NodeId rNodeId)
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
        }
        public void MonitoredItem_Notification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {



            MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;

            subDict[monitoredItem.ResolvedNodeId] = new MonitorValue( monitoredItem, notification.Value);
            foreach (var value in monitoredItem.DequeueValues())
            {
                Console.WriteLine("{0}: {1}, {2}, {3}", monitoredItem.DisplayName, value.Value, value.SourceTimestamp, value.StatusCode);
                //subDict[monitoredItem.ResolvedNodeId].value=value;
            }
            m_sub_val = notification.Value.ToString();
            
            //Console.WriteLine("MonitoringNotification");
        }

        public static object ChangeType( string valueText, BuiltInType bitype)
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

    }

}
