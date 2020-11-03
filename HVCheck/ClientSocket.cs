using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;

namespace TT_ClientSocketLibrary
{
    public enum Enum_ConnectionEventClient
    {
        RECEIVEDATA,
        STARTCLIENT,
        STOPCLIENT
    }
    public class ClientSocket
    {
        public bool Started = false;
        byte[] m_DataBuffer = new byte[512];
        IAsyncResult m_asynResult;
        public AsyncCallback pfnCallBack;
        public Socket client;
        public string ip = "";
        public Int32 port = 0;
        public string ConnectString = "";
        public string Subfix = "";
        public string ReceiveString = "";
        public delegate void EventHandler(Enum_ConnectionEventClient e, object obj);
        public event EventHandler ConnectionEventCallBack;
        public void StartClient()
        {
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(ip);
                if (reply.Status == IPStatus.Success)
                {

                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress Ip = IPAddress.Parse(ip);
                    int iPortNo = System.Convert.ToInt32(port);
                    IPEndPoint ipEnd = new IPEndPoint(Ip, iPortNo);
                    client.Connect(ipEnd);
                    Send(ConnectString);
                    Started = true;
                    WaitForData();
                    if (ConnectionEventCallBack != null)
                    {
                        ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.STARTCLIENT, client.Connected);
                    }
                }

            }
            catch (Exception)
            {
                Started = false;
            }

        }
        public void WaitForData()
        {
            try
            {
                if (pfnCallBack == null)
                {
                    pfnCallBack = new AsyncCallback(OnDataReceived);
                }
                CSocketPacket theSocPkt = new CSocketPacket();
                theSocPkt.thisSocket = client;
                m_asynResult = client.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, pfnCallBack, theSocPkt);
            }
            catch (Exception)
            {
            }

        }
        public class CSocketPacket
        {
            public System.Net.Sockets.Socket thisSocket;
            public byte[] dataBuffer = new byte[512];
        }
        public void StopClient()
        {
            if (client != null)
            {
                client.Disconnect(true);
                client.Dispose();
                //Started = false;
                if (ConnectionEventCallBack != null)
                {
                    ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.STOPCLIENT, true);
                }
            }
        }
        public void Send(string data)
        {
            if (client != null && data != null)
            {
                try
                {
                    Object objData = data;
                    byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString() + Subfix);
                    client.Send(byData);
                }
                catch (SocketException)
                {
                    client = null;
                }
            }
        }
        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                CSocketPacket theSockId = (CSocketPacket)asyn.AsyncState;
                int iRx = 0;
                iRx = theSockId.thisSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(theSockId.dataBuffer, 0, iRx, chars, 0);
                System.String szData = new System.String(chars);
                ReceiveString = szData;
                if (ConnectionEventCallBack != null)
                {
                    ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.RECEIVEDATA, szData);
                }
                Started = true;
                WaitForData();
            }
            catch (ObjectDisposedException)
            {
                //Started = false;
            }
            catch (SocketException)
            {
                //Started = false;
            }
        }
    }
}
