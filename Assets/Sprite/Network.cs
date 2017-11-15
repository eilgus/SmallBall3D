using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Network : MonoBehaviour {



    //private Player mPlayer;
    private bool _connected = false;
    private bool _isServer = false;
    private string _ip = "192.168.1.100";
    private int _port = 18000;
    public int _name;
    private int _packageSize;
    List<Package> _packages = new List<Package>();

    TcpListener _listener;


    GlobalSingleton globalSigton;
    




    [Serializable]
    public struct Package
    {
        public int name;
        public Vector3Serializer pos;
        public Vector3Serializer velocity;
        public Vector3Serializer rot;
        public int hp;
    }

    [Serializable]
    public struct Vector3Serializer
    {
        public float x;
        public float y;
        public float z;

        public void Fill(Vector3 v3)
        {
            x = v3.x;
            y = v3.y;
            z = v3.z;
        }

        public Vector3 V3
        { get { return new Vector3(x, y, z); } }
    }

    //获取包的大小
    private int PackageSize()
    {
        Package p = new Package();
        byte[] b;
        Serialize(p, out b);
        return b.Length;
    }

    //序列化数据包
    public bool Serialize(object obj, out byte[] result)
    {
        bool ret = false;
        result = null;

        try
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, obj);
            result = ms.ToArray();

            ret = true;
        }
        catch (Exception e)
        {
            ret = false;
            Debug.Log(e.Message);
        }

        return ret;
    }

    // 反序列化数据包
    public bool Deserialize(byte[] data, out object result)
    {
        bool ret = false;
        result = new object();

        try
        {
            MemoryStream ms = new MemoryStream(data);
            BinaryFormatter bf = new BinaryFormatter();
            result = bf.Deserialize(ms);

            ret = true;
        }
        catch (Exception e)
        {
            ret = false;
            Debug.Log(e.Message);
        }

        return ret;
    }



    // Use this for initialization
    void Start()
    {
        //mPlayer = gameObject.GetComponent<Player>();
        globalSigton = GlobalSingleton.GetInstance();
        _packageSize = PackageSize();
    }

    // Update is called once per frame
    void Update()
    {


    }



    private struct Client
    {
        public TcpClient client;
        public byte[] buffer;
        public List<byte> pendingDate;
    }

    List<Client> _clients = new List<Client>();

    



    void OnGUI()
    {
        if (globalSigton.mode == GlobalSingleton.Mode.Alone)
            return;


        GUI.Box(new Rect(0, 0, 800, 60), "网络设置");

        if (_connected)
        {
            GUI.Label(new Rect(10, 25, 100, 25), _isServer ? "已建立服务端" : "已连接服务端");
        }
        else if (!_connected)
        {
            GUI.Label(new Rect(10, 25, 100, 25), "未连接");
        }

        GUI.Label(new Rect(130, 25, 20, 25), "IP:");
        GUI.Label(new Rect(270, 25, 40, 25), "端口:");
        GUI.Label(new Rect(380, 25, 40, 25), "Name:");

        if (!_connected && !_isServer)
        {
            _ip = GUI.TextField(new Rect(150, 25, 100, 25), _ip, 100);
            _port = System.Convert.ToInt32(GUI.TextField(new Rect(310, 25, 50, 25), _port.ToString(), 100));
            _name = System.Convert.ToInt32(GUI.TextField(new Rect(420, 25, 100, 25), _name.ToString(), 100));
        }
        else
        {
            GUI.TextField(new Rect(150, 25, 100, 25), _ip, 100);
            GUI.TextField(new Rect(310, 25, 50, 25), _port.ToString(), 100);
            GUI.TextField(new Rect(420, 25, 100, 25), _name.ToString(), 100);
        }

        if (!_connected && !_isServer)
        {
            if (GUI.Button(new Rect(540, 25, 100, 25), "开启服务端"))
            {
                StartServer();
                ConnectServer();
            }

            if (GUI.Button(new Rect(660, 25, 100, 25), "连接至服务端"))
            {
                ConnectServer();
            }
        }
        else
        {
            if (_isServer)
            {
                if (GUI.Button(new Rect(540, 25, 100, 25), "关闭服务端"))
                {
                    StopServer();
                }
            }
            else if (_connected)
            {
                if (GUI.Button(new Rect(540, 25, 100, 25), "取消连接"))
                {
                    DisconnectServer();
                }
            }
        }
    }

    //开启服务端
    private void StartServer()
    {
        try{
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            
            //开始监听
            _listener.BeginAcceptSocket(HandleAccepted, _listener);

            _isServer = true;
        }catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    //停止服务端
    private void StopServer()
    {
        try
        {
            _listener.Stop();

            lock (_clients)
            {
                foreach (Client c in _clients)
                {
                    RemoveClient(c);
                }
                _clients.Clear();
            }

            lock(_packages)
            {
                _packages.Clear();
            }

            _isServer = false;
        }
        catch (Exception)
        {

            throw;
        }
    }



    //处理客户端连接的回调函数
    private void HandleAccepted(IAsyncResult iar)
    {
        if(_isServer)
        {
            TcpClient tcpClient = _listener.EndAcceptTcpClient(iar);
            Client client = new Client();
            client.client = tcpClient;
            client.buffer = new byte[tcpClient.ReceiveBufferSize];
            client.pendingDate = new List<byte>();

            lock(_clients)
            {
                AddClient(client);
            }

            tcpClient.GetStream().BeginRead(
                client.buffer,0, 
                client.buffer.Length, 
                HandleClientDataReceived, 
                client);

            _listener.BeginAcceptSocket(HandleAccepted, _listener);
        }
    }

    private void HandleClientDataReceived(IAsyncResult iar)
    {
        try
        {
            if(_isServer)
            {
                //用于接受数据的client  绑定到对应的客户端
                Client client = (Client)iar.AsyncState; 
                NetworkStream ns = client.client.GetStream();
                int bytesRead = ns.EndRead(iar);


                if (bytesRead == 0)//即为客户端发送了结束通信的包
                {
                    lock (_clients)
                    {
                        _clients.Remove(client);
                    }
                    return;
                }

                //保存数据0-0  并且为了下一步把包再分成一个个Package
                for(int i = 0;i<bytesRead;++i)
                {
                    client.pendingDate.Add(client.buffer[i]);
                }

                while(client.pendingDate.Count>=_packageSize)
                {
                    //可复用 用于将List按照一定距离分段
                    byte[] bp = client.pendingDate.GetRange(0, _packageSize).ToArray();
                    client.pendingDate.RemoveRange(0, _packageSize);

                    lock(_clients)
                    {
                        //分发0-0？
                        foreach (Client c in _clients)
                        {
                            c.client.GetStream().Write(bp, 0, _packageSize);
                            c.client.GetStream().Flush();
                        }
                    }

                    client.client.GetStream().BeginRead(
                        client.buffer,
                        0,
                        client.buffer.Length,
                        HandleClientDataReceived,
                        client);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            
        }
    }

    private void RemoveClient(Client c)
    {
        c.client.Client.Disconnect(false);
    }

    private void AddClient(Client c)
    {
        _clients.Add(c);
    }

    Client _client;

    private void ConnectServer()
    {
        try
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(_ip, _port);
            _client = new Client();
            _client.client = tcpClient;
            _client.buffer = new byte[tcpClient.ReceiveBufferSize];
            _client.pendingDate = new List<byte>();

            tcpClient.GetStream().BeginRead(
                _client.buffer,
                0,
                tcpClient.ReceiveBufferSize,
                HandleServerDataReceived,
                _client);

            _connected = true;
        }catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void HandleServerDataReceived(IAsyncResult iar)
    {
        if(_connected)
        {
            Client server = (Client)iar.AsyncState;
            NetworkStream ns = server.client.GetStream();
            int bytesRead = ns.EndRead(iar);

            if(bytesRead == 0)
            {
                DisconnectServer();
                return;
            }

            for(int i=0; i<bytesRead;++i)
            {
                server.pendingDate.Add(server.buffer[i]);
            }
            
            while(server.pendingDate.Count >= _packageSize)
            {
                byte[] bp = server.pendingDate.GetRange(0, _packageSize).ToArray();
                server.pendingDate.RemoveRange(0, _packageSize);

                object obj;
                Deserialize(bp, out obj);

                lock(_packages)
                {
                    _packages.Add((Package)obj);
                }
            }

            server.client.GetStream().BeginRead(
                server.buffer,
                0,
                server.client.ReceiveBufferSize,
                HandleServerDataReceived,
                server);
        }
    }

    public void SendStatus(Vector3 pos,Vector3 rot, Vector3 velocity,
        int hp)
    {
        try
        {
            if(_connected)
            {
                Package p = new Package();
                p.name = _name;
                p.pos.Fill(pos);
                p.rot.Fill(rot);
                p.velocity.Fill(velocity);
                p.hp = hp;

                byte[] bp;
                Serialize(p, out bp);

                lock (_client.client)
                {
                    _client.client.GetStream().Write(bp, 0, _packageSize);
                    _client.client.GetStream().Flush();

                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);

            DisconnectServer();
        }
    }

    private void DisconnectServer()
    {
        try
        {
            lock (_client.client)
            {
                _client.client.Client.Close();
            }

            // 清空数据包
            lock (_packages)
            {
                _packages.Clear();
            }

            // 删除所有客户人物模型
            //mPlayer.RemoveAllEnemyCharacter();

            _connected = false;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public bool NextPackage(out Package p)
    {
        lock (_packages)
        {
            if (_packages.Count == 0)
            {
                p = new Package();
                return false;
            }

            p = _packages[0];
            _packages.RemoveAt(0);
        }

        return true;
    }
}
