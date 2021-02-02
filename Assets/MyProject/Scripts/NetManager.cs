using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

namespace ItSeez3D.AvatarMaker.Editor
{
    [InitializeOnLoad]
    [ExecuteInEditMode]
    public class NetManager : MonoBehaviour
    {

        public static NetManager Instance;


        /// <summary>
        /// 服务端
        /// </summary>
        private Socket _serverSocket;

        /// <summary>
        /// 客户端
        /// </summary>
        private Socket _clientSocket;

        private Thread _thread;

        private Thread _clienThread;

        private bool _isRun = true;
        public int Port = 6000;

        private string _recStr;

        private byte[] _receiveBytes;
        public event Action<byte[]> ConnectEvent;

        NetManager()
        {
            if (Instance != null) throw new UnityException("单例错误");
            Instance = this;
            Debug.Log("init NetManager");  
            _thread = new Thread((StartServer));
            _thread.Start(); 
            EditorApplication.update += Update;

            

        }

      
        private void Start()
        {
            Debug.Log("start");
             OnClickMinimize();
            // //StartCoroutine(WaitTime());
            //  Thread t = new Thread((() =>
            // {
            //     while (true)
            //     {
            //         Thread.Sleep(100);
                   

            //     }
            // }));
            // t.Start();
        }

        private IEnumerator WaitTime()
        {
            yield return new WaitForSeconds(2f);
            OnClickMinimize();

        }
        /// <summary>
        /// 启动服务器，这是一个简单的，同步的，socket服务器  
        /// </summary>
        private void StartServer()
        {
            try
            {
                string ipName = Dns.GetHostName();

                IPAddress[] ipadrlist = Dns.GetHostAddresses(ipName);
                // Debug.Log("ip is" + ipadrlist[0]);
                IPAddress ipAddress = null;

                foreach (IPAddress address in ipadrlist)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = address;
                        // Debug.LogError("本机的IP地址是=> " + ipAddress.ToString()+" 如果连接不上，请检查该IP跟发送过来的两端地址是否同属于一个内网");
                        break;
                    }
                }

                if (ipAddress != null)
                {
                    Debug.Log(ipAddress.ToString());
                    IPEndPoint ipe = new IPEndPoint(ipAddress, Port);

                    _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _serverSocket.Bind(ipe);
                    _serverSocket.Listen(0);
                    Debug.Log("监听已经打开，请等待");

                    while (_isRun)
                    {
                        //receive message
                        _clientSocket = _serverSocket.Accept();
                        Debug.Log("新的连接已经建立");

                        byte[] recByte = new byte[2 * 1024 * 1024];
                        int bytes = _clientSocket.Receive(recByte, recByte.Length, 0);
                        //_recStr += Encoding.UTF8.GetString(recByte, 0, bytes);

                        _receiveBytes = new byte[bytes];

                        Array.Copy(recByte, _receiveBytes, bytes);
                        // Reveice();
                        // Debug.Log(_recStr); 
                        //serverSocket.Close(); 
                        //SendMessageToSocket("Reveice data over");
                    }

                }
                else throw new Exception("IP 地址设置错误  address is null");
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        void Update()
        {
            if (_receiveBytes != null && _receiveBytes.Length > 0)
            {


                if (ConnectEvent != null) ConnectEvent(_receiveBytes);
                _receiveBytes = null;

            }

        }


        private void Reveice()
        {
            _clienThread = new Thread((() =>
            {
                while (true)
                {
                    if (_clientSocket != null)
                    {
                        string str = null;

                        byte[] recByte = new byte[4096];
                        int bytes = _clientSocket.Receive(recByte, recByte.Length, 0);
                        str += Encoding.UTF8.GetString(recByte, 0, bytes);
                        Debug.Log("_clientSocket Reveice Message is " + str);

                    }
                }
            }));

            _clienThread.Start();
        }

        private void SendMessageToSocket(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            _clientSocket.Send(bytes);
            _clientSocket.Close();
        }

        public void SendMessageToSocket(byte[] message)
        {
            if (_clientSocket != null)
            {
                try
                {
                    Debug.Log("数据发送出去");

                    _clientSocket.Send(message);
                    _clientSocket.Close();
                    _clientSocket = null;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());

                }

            }

        }

        public void SendErrorMessage()
        {
            if (_clientSocket != null)
            {

                byte[] bytes = Encoding.UTF8.GetBytes("生成错误");
                Debug.LogError("错误数据发送出去，长度为：" + bytes.Length);
                _clientSocket.Send(bytes);
                _clientSocket.Close();
            }
        }

        private void OnDestroy()
        {
            Relesease();
        }

        private void Relesease()
        {
            Instance = null;
            Debug.Log("deleted NetManager");
            _isRun = false;
            _serverSocket.Close();
            _serverSocket = null;
            _clientSocket.Close();
            _clientSocket = null;
            _thread.Abort();
            _clienThread.Abort();
            _thread = null;
            _clienThread = null;
        }

        ~NetManager()
        {
            Relesease();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);
        /// <summary>
        /// 得到当前活动的窗口
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern System.IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);

        const uint SWP_SHOWWINDOW = 0x0040;
        const int GWL_STYLE = -16;
        const int WS_BORDER = 1;



        const int SW_SHOWMINIMIZED = 2; //{最小化, 激活}
        const int SW_SHOWMAXIMIZED = 3;//最大化
        const int SW_SHOWRESTORE = 1;//还原
        private IntPtr curIntPtr = IntPtr.Zero;
        public  void OnClickMinimize()
        { //最小化 
            if (curIntPtr == IntPtr.Zero) curIntPtr = GetForegroundWindow();
            ShowWindow(curIntPtr, SW_SHOWMINIMIZED);
        }
    }
}
