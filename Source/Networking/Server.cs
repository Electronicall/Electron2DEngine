﻿using Riptide;
using Riptide.Transports;

namespace Electron2D.Networking.ClientServer
{
    /// <summary>
    /// A simple host/client implementation using <see cref="Riptide.Server"/>
    /// </summary>
    public class Server
    {
        private class NetworkGameClassData
        {
            public uint UpdateVersion;
            public int RegisterID;
            public string NetworkID;
            public ushort OwnerID;
            public string JsonData;
        }

        public Riptide.Server RiptideServer { get; private set; }
        public long TimeStarted { get; private set; } = -1;
        public bool IsRunning => RiptideServer.IsRunning;
        public bool AllowNonHostOwnership { get; private set; } = true;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private Dictionary<uint, List<NetworkGameClassData>> _syncingClientSnapshots = new();
        private Dictionary<string, ushort> _networkGameClassOwners = new();
        private bool _hostAssigned = false;
        private ushort _hostID = 1;
        private string _serverPassword = null;

        public Server()
        {
            RiptideServer = new Riptide.Server();
            RiptideServer.HandleConnection = ValidateConnection;
            RiptideServer.ClientConnected += HandleClientConnected;
            RiptideServer.ClientDisconnected += HandleClientDisconnected;
            RiptideServer.MessageReceived += HandleMessageReceived;
        }

        /// <summary>
        /// Whether non-host clients can own and/or create network game classes.
        /// </summary>
        /// <param name="allowNonHostOwnership"></param>
        public void SetAllowNonHostOwnership(bool allowNonHostOwnership)
        {
            AllowNonHostOwnership = allowNonHostOwnership;
        }

        /// <summary>
        /// Should be called at a fixed timestep.
        /// </summary>
        public void ServerFixedUpdate()
        {
            RiptideServer.Update();
        }

        public void Send(Message message, ushort client, bool shouldRelease = true)
        {
            RiptideServer.Send(message, client, shouldRelease);
        }
        public void Send(Message message, Connection toClient, bool shouldRelease = true)
        {
            RiptideServer.Send(message, toClient, shouldRelease);
        }
        public void SendToAll(Message message, bool shouldRelease = true)
        {
            RiptideServer.SendToAll(message, shouldRelease);
        }
        public void SendToAll(Message message, ushort exceptToClient, bool shouldRelease = true)
        {
            RiptideServer.SendToAll(message, exceptToClient, shouldRelease);
        }

        public void Start(ushort port, ushort maxClientCount, string password = "")
        {
            if (IsRunning) return;
            _serverPassword = password;
            RiptideServer.Start(port, maxClientCount, useMessageHandlers: false);
            TimeStarted = DateTime.UtcNow.Ticks;
        }
        public void Stop()
        {
            if (!IsRunning) return;
            RiptideServer.Stop();
            _syncingClientSnapshots.Clear();
            _networkGameClassOwners.Clear();
            _serverPassword = "";
            _hostAssigned = false;
            _hostID = 0;
        }

        #region Handlers
        private void HandleMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            if (e.MessageId < 60000 || e.MessageId > 60004)
            {
                MessageReceived?.Invoke(sender, e);
                return;
            }

            ushort client = e.FromConnection.Id;
            switch((NetworkMessageType)e.MessageId)
            {
                case NetworkMessageType.NetworkClassCreated:
                    HandleNetworkClassCreated(client, e.Message);
                    break;
                case NetworkMessageType.NetworkClassUpdated:
                    HandleNetworkClassUpdated(client, e.Message);
                    break;
                case NetworkMessageType.NetworkClassDeleted:
                    HandleNetworkClassDeleted(client, e.Message);
                    break;
                case NetworkMessageType.NetworkClassSync:
                    HandleNetworkClassSync(client, e.Message);
                    break;
                case NetworkMessageType.NetworkClassRequestSyncData:
                    HandleNetworkClassRequestSyncData(client, e.Message);
                    break;
            }
        }
        private void HandleNetworkClassRequestSyncData(ushort client, Message message)
        {
            if(client != _hostID)
            {
                Debug.LogError($"Non-host client [{client}] tried to send client sync data to the server.");
                return;
            }

            Debug.Log("Server: Received requested sync data from host. Asking client to sync...");
            ushort toClient = message.GetUShort();
            _syncingClientSnapshots.Add(toClient, new List<NetworkGameClassData>());
            int classCount = message.GetInt();
            for(int i = 0; i < classCount; i++)
            {
                NetworkGameClassData data = new NetworkGameClassData();
                data.UpdateVersion = message.GetUInt();
                data.RegisterID = message.GetInt();
                data.NetworkID = message.GetString();
                data.OwnerID = message.GetUShort();
                data.JsonData = message.GetString();
                _syncingClientSnapshots[toClient].Add(data);
            }
            Message returnMessage = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageType.NetworkClassSync);
            returnMessage.AddInt(classCount);
            Send(returnMessage, toClient);
        }
        private void HandleNetworkClassCreated(ushort client, Message message)
        {
            if (!AllowNonHostOwnership && client != _hostID)
            {
                Debug.LogWarning($"A non-host client [{client}] tried to create a network game class. Using the " +
                    $"current networking settings, this is not allowed.");
                return;
            }

            uint version = message.GetUInt();
            int registerID = message.GetInt();
            string networkID = message.GetString();
            string json = message.GetString();

            if (_networkGameClassOwners.ContainsKey(networkID))
            {
                Debug.LogError($"Network game class with id [{networkID}] already exists on the server. Cannot spawn.");
                return;
            }

            _networkGameClassOwners.Add(networkID, client);
            Message returnMessage = Message.Create(MessageSendMode.Reliable,
                (ushort)NetworkMessageType.NetworkClassCreated);
            returnMessage.AddUInt(version);
            returnMessage.AddInt(registerID);
            returnMessage.AddString(networkID);
            returnMessage.AddUShort(client);
            returnMessage.AddString(json);
            SendToAll(returnMessage);
        }
        private void HandleNetworkClassUpdated(ushort client, Message message)
        {
            string networkID = message.GetString();
            uint updateVersion = message.GetUInt();
            ushort type = message.GetUShort();
            string json = message.GetString();

            // Checking if client is owner of object
            if (_networkGameClassOwners[networkID] != client)
            {
                Debug.LogWarning($"Client {client} is trying to update a network game class with " +
                    $"id [{networkID}] that doesn't belong to them!");
                return;
            }

            Message returnMessage = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageType.NetworkClassUpdated);
            returnMessage.AddString(networkID);
            returnMessage.AddUInt(updateVersion);
            returnMessage.AddUShort(type);
            returnMessage.AddString(json);
            SendToAll(returnMessage, client);
        }
        private void HandleNetworkClassSync(ushort client, Message message)
        {
            if (!_syncingClientSnapshots.ContainsKey(client))
            {
                Debug.LogError($"Client [{client}] tried to get sync data from server without permission.");
                return;
            }

            Debug.Log($"Server: Received sync confirmation from client {client}. Sending data...");
            List<NetworkGameClassData> dataList = _syncingClientSnapshots[client];
            foreach (var data in dataList)
            {
                Message returnMessage = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageType.NetworkClassSync);
                returnMessage.AddUInt(data.UpdateVersion);
                returnMessage.AddInt(data.RegisterID);
                returnMessage.AddString(data.NetworkID);
                returnMessage.AddUShort(data.OwnerID);
                returnMessage.AddString(data.JsonData);
                Send(returnMessage, client);
            }
            _syncingClientSnapshots.Remove(client);
        }
        private void HandleNetworkClassDeleted(ushort client, Message message)
        {
            string networkID = message.GetString();

            if (_networkGameClassOwners[networkID] != client)
            {
                Debug.LogWarning($"Client {client} tried to delete network game class id [{networkID}] which doesn't belong " +
                    "to them!");
                return;
            }

            _networkGameClassOwners.Remove(networkID);
            Message returnMessage = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageType.NetworkClassDeleted);
            returnMessage.AddString(networkID);
            SendToAll(returnMessage);
        }
        private void HandleClientConnected(object? sender, ServerConnectedEventArgs e)
        {
            if (e.Client.Id == _hostID) return;

            Debug.Log("Server: Client joined, sending sync signal.");
            Message toHostMessage = Message.Create(MessageSendMode.Reliable,
                (ushort)NetworkMessageType.NetworkClassRequestSyncData);
            toHostMessage.AddUShort(e.Client.Id);
            Send(toHostMessage, _hostID);
        }
        private void HandleClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
        {
            if(e.Client.Id == _hostID)
            {
                Debug.Log("Until host transferring is implemented, the server will stop when the host leaves.");
                Stop();
                return;
            }

            foreach (var pair in _networkGameClassOwners)
            {
                if(pair.Value == e.Client.Id)
                {
                    Message message = Message.Create();
                    message.AddString(pair.Key);
                    HandleNetworkClassDeleted(e.Client.Id, message);
                    message.Release();
                }
            }
        }
        #endregion

        private void ValidateConnection(Connection pendingConnection, Message connectMessage)
        {
            string password = connectMessage.GetString();
            if (_serverPassword != "")
            {
                if (_serverPassword.Equals(password))
                {
                    RiptideServer.Accept(pendingConnection);
                }
                else
                {
                    RiptideServer.Reject(pendingConnection, Message.Create().AddString("Incorrect password."));
                }
            }

            RiptideServer.Accept(pendingConnection);
        }
    }
}
