using CSharpServerFramework.Extension;
using CSharpServerFramework;
using CSharpServerFramework.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSServerJsonProtocol;
using Newtonsoft.Json;

namespace CSharpServerConnector
{
    public class CSharpServerConnectorManager
    {
        internal static CSharpServerConnectorManager _instance;
        internal static CSharpServerConnectorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception("Not Use CSharpServerConnectorExtension");
                }
                return _instance;
            }
        }

        public CSharpServerConnectorManager()
        {
            Connectors = new Dictionary<object, CSharpServerConnector>();
        }

        public IDictionary<object, CSharpServerConnector> Connectors { get; private set; }

    }

    public class CSharpServerConnector : ICSharpServerUser
    {
        public bool IsUserValidate { get { return true; } }

        public ICSharpServerSession Session { get; set; }
    }

    [ValidateExtension]
    [ExtensionInfo("CSSConnectorValidate")]
    public class CSharpServerConnectorValidateExtension : JsonExtensionBase
    {

        public override void Init()
        {
            
        }
    }

    [ExtensionInfo("CSharpServerConnector")]
    public class CSharpServerConnectorExtension : JsonExtensionBase
    {

        public override void Init()
        {
            CSharpServerConnectorManager._instance = new CSharpServerConnectorManager();
        }

        [CommandInfo(1, "AcceptMsg")]
        public void AcceptRemoteRedirectMessage(ICSharpServerSession session, dynamic msg)
        {
            Log("Accept Remote Redirect Message");
            string extName = msg.ExtName;
            string CmdName = msg.CmdName;
            var valueObject = JsonConvert.DeserializeObject(msg.Value);
            if (string.IsNullOrWhiteSpace(CmdName))
            {
                int CmdId = msg.CmdId;
                RedirectMessage(extName, CmdId, session, valueObject);
            }
            else
            {
                RedirectMessage(extName, CmdName, session, valueObject);
            }
        }
    }

    public static class CSharpServerBuilderExtension
    {
        public static CSharpServerConnectorManager UseConnectorExtension(this ICSServerBuilder Builder)
        {
            var extension = new CSharpServerConnectorExtension();
            Builder.UseExtension(extension);
            return CSharpServerConnectorManager.Instance;
        }
    }

    public static class ConnectorRedirectExtension
    {
        public static void RedirectMessageToRemote(this ICSharpServerExtension Extension, ICSharpServerSession UserSession, object RemoteId, dynamic Value, string RemoteExtName, string RemoteCmdName, int RemoteCmdId = -1)
        {
            var aptMsg = new
            {
                Value = JsonConvert.SerializeObject(Value),
                ExtName = RemoteExtName,
                CmdId = RemoteCmdId,
                CmdName = RemoteCmdName
            };
            var connector = CSharpServerConnectorManager.Instance.Connectors[RemoteId];
            Extension.SendJsonResponse(connector.Session, aptMsg, "CSharpServerConnector", "AcceptMsg");
        }
    }
}
