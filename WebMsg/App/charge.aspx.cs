using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Sockets;
using WolfInv.com.DbAccessLib;
using System.Data;
using System.Xml;
using XmlProcess;
using WolfInv.Com.JsLib;
using System.Net;
using System.Text;
using WolfInv.com.WinInterComminuteLib;
using WolfInv.com.ChargeLib;
namespace WebMsg
{
    [Serializable]
    public partial class charge : System.Web.UI.Page
    {
        DbClass db;
        WinComminuteClass comm;
        ChargeRemoteClass crc = null;
        private ChargeOperator cotor = null;
        //private readonly string ip = "192.168.172.142";
        //private readonly int port = 8090;
        private readonly int buffSize = 1024 * 1024 * 2;//2M
        bool isDebug = false;
        Page page;
        //bool responsed = false;
        bool IpcSuc = false;
        string reqId = null;
        string strAmt = null;
        string strWxId = null;
        string strWxName = null;
        string strChargeAccount = null;
        public charge()
        {
            if (!isDebug)
                db = new DbClass("www.wolfinv.com", "sa", "bolts", "pk10db");
            comm = new WinComminuteClass();
            //cotor = new ChargeOperator();

            try
            {
                
            }
            catch (Exception ce)
            {

            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            string strJson = "<root><wxId>{0}</wxId><wxName>{1}</wxName><reqId>{2}</reqId><errcode>{3}</errcode><msg>{4}</msg><orderId>{5}</orderId><chargeAmt>{6}</chargeAmt><imgData>{7}</imgData><respTime>{8}</respTime></root>";
            string retJsonModel = "";
            string ret = null;
            //responsed = false;
            if (this.IsPostBack)
            {
                return;
            }
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(strJson);
            retJsonModel = XML_JSON.XML2Json(xmldoc, "root", true);
            retJsonModel = retJsonModel.Substring(1, retJsonModel.Length - 2);
            XmlNode rootnode = xmldoc.SelectSingleNode("root");
            reqId = null;
            strAmt = Request["chargeAmt"];
            strWxId = Request["wxId"];
            strWxName = Request["wxName"];
            strChargeAccount = Request["chargeAccount"];
            if (string.IsNullOrEmpty(strWxId))
            {
                return;
            }
            if(string.IsNullOrEmpty(strChargeAccount))
            {
                return;
            }
            ////if (!IpcSuc || cotor == null)
            ////{
            ////    ret = string.Format(retJsonModel, strWxId, strWxName, reqId??"未指定", "2001", "充值服务器未工作",null, strAmt, null);
            ////    Response.Write("{{0}}".Replace("{0}",ret));
            ////    return;
            ////}
            try
            {
                if (crc == null)
                {
                    string strclassname = typeof(ChargeRemoteClass).Name.Split('\'')[0];
                    string url = string.Format("ipc://IPC_{0}_{1}/{2}", "CM", strChargeAccount, strclassname);
                    //LogableClass.ToLog("监控终端", "刷新数据", url);
                    //_UseSetting = wc.GetServerObject<ServiceSetting<TimeSerialData>>(url);
                    crc = comm.GetServerObject<ChargeRemoteClass>(url,false);
                    if(crc == null)
                    {
                        ret = string.Format(retJsonModel, strWxId, strWxName, reqId ?? "未指定", "2001", "充值服务器未工作", null, strAmt, null,DateTime.Now.ToLongTimeString());
                        Response.Write("{{0}}".Replace("{0}",ret));
                        return;
                    }
                    cotor = crc.Operate;
                    IpcSuc = true;
                }
                reqId = Guid.NewGuid().ToString();
                ////if(cotor.OperateChargeForm == null)
                ////{
                ////    ret = string.Format(retJsonModel, strWxId, strWxName, reqId ?? "未指定", "2002", "充值服务器未指定功能", null, strAmt, null);
                ////    Response.Write("{{0}}".Replace("{0}", ret));
                ////    return;
                ////}
                //ChargeOperator cot = cotor;
                //Optclass oc = new Optclass();
                //cotor.db = db;
                cotor.strWxId = strWxId;
                cotor.strWxName = strWxName;
                cotor.reqId = reqId;
                cotor.strAmt = strAmt;
                cotor.strChargeAccount = strChargeAccount;
                //oc.Response = ResponseResult;
                //cotor.Charge(reqId, strAmt);//调用,最好放后面
                //cotor.ResponseCompleted += oc.receivedData;
                //cotor.ResponseCompleted = oc.receivedData;
                //if (cotor.OperateChargeForm != null)
                //{
                //  cotor.OperateChargeForm?.Invoke(reqId, strAmt);
                //}
                cotor.responsed = false;
                cotor.ResponseString = null;
                cotor.Charge(strWxId,strWxName, reqId, strAmt);
                //tcpClientSocket.SendMessage(postdata);
                DateTime currtime = DateTime.Now;
                while(!cotor.responsed)
                {
                    System.Threading.Thread.Sleep(100);//没有相应就不回复
                    if(DateTime.Now.Subtract(currtime).TotalSeconds>2*60)
                    {
                        ret = string.Format(retJsonModel, strWxId, strWxName, reqId ?? "未指定", "2009", "超时"+ DateTime.Now.ToLongTimeString(), null, strAmt, null, DateTime.Now.ToLongTimeString());
                        Response.Write("{{0}".Replace("{0}", ret));
                        return;
                    }
                }
                Response.Write(cotor.ResponseString);

            }
            catch(Exception ce)
            {
                ret = string.Format(retJsonModel, strWxId, strWxName, reqId ?? "未指定", "3009", ce.Message, null, strAmt, null, DateTime.Now.ToLongTimeString());
                Response.Write("{{0}}".Replace("{0}", ret));
            }
            
           
            //skt.Connect()
        }

        void ResponseResult(string a)
        {
            Response.Write(a);
        }

        
        /// <summary>
        /// Server accept by async
        /// </summary>
        
        /// <summary>
        /// </summary>
        /// Accept call back method
        /// <param name="ir">Async result info</param>
       

    }
    
}