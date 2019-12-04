using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WolfInv.com.WinInterComminuteLib;
using WolfInv.com.WXMsgCom;
namespace WebMsg.App
{
    public partial class WXMsgPage : System.Web.UI.Page
    {
        static WinComminuteClass comm;
        static WebInterfaceClass wxobj;
        static Dictionary<string, string> UserMsgs;
        protected void Page_Load(object sender, EventArgs e)
        {
            if(comm == null)
            {
                comm = new WinComminuteClass();
            }
            UserMsgs = Application["LastMsg"] as Dictionary<string, string>;
            if (UserMsgs == null)
            {
                UserMsgs = new Dictionary<string, string>();
            }
            
            string strToUsers = Request["ToUser"];
            string[] arrUsers = strToUsers.Split(',');
            string strMsg = Request["Msg"];
            
            string WriteLog = Request["LogIt"];
            string ret = "";
            for (int i = 0; i < arrUsers.Length; i++)
            {
                string strToUser = arrUsers[i];
                string strInfo = string.Format("{0}|{1}", strToUser, strMsg);
                if (!UserMsgs.ContainsKey(strToUser))
                {
                    UserMsgs.Add(strToUser, strMsg);
                }
                else
                {
                    if (UserMsgs[strToUser] == strMsg)
                    {
                        Response.Write("重复的消息！");
                        return;
                    }
                    else
                    {
                        UserMsgs[strToUser] = strMsg;
                    }
                }

                ////if(Session["LastMsg"]!= null && Session["LastMsg"].ToString() == strInfo)
                ////{
                ////    Response.Write("重复的消息！");
                ////    return;
                ////}
                
                try
                {
                    if (wxobj == null)
                    {
                        string strclassname = typeof(WebInterfaceClass).Name.Split('\'')[0];
                        string url = string.Format("ipc://IPC_{0}/{1}", "wxmsg", strclassname);
                        //LogableClass.ToLog("监控终端", "刷新数据", url);
                        //_UseSetting = wc.GetServerObject<ServiceSetting<TimeSerialData>>(url);


                        wxobj = comm.GetServerObject<WebInterfaceClass>(url, WriteLog == "1");
                    }
                    if (!wxobj.Success)
                    {
                        ret = wxobj.Message;
                    }
                    else
                        ret = wxobj.SendMsg(string.Format("{0} {1}", DateTime.Now.ToString(), strMsg), strToUser);
                }
                catch (Exception ce)
                {
                    ret = string.Format("Asp获取请求错误[{0}]:{1}", ce.Message, ce.StackTrace);
                }
            }
            Application["LastMsg"] = UserMsgs;
            Response.Write(ret);
        }
    }
}