using System;
using System.Collections.Generic;
using Dissonance.Networking;
using Dissonance.Networking.Server;
using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_HLAPI
{
    public class HlapiServer
        : BaseServer<HlapiServer, HlapiClient, HlapiConn>
    {
        #region fields and properties
        [NotNull] private readonly HlapiCommsNetwork _network;

        private readonly NetworkWriter _sendWriter = new NetworkWriter(new byte[1024]);
        private readonly byte[] _receiveBuffer = new byte[1024];

        private readonly List<NetworkConnection> _addedConnections = new List<NetworkConnection>();
        #endregion

        #region constructors
        public HlapiServer([NotNull] HlapiCommsNetwork network)
        {
            if (network == null) throw new ArgumentNullException("network");

            _network = network;
        }
        #endregion

        public override void Connect()
        {
            NetworkServer.RegisterHandler(_network.TypeCode, OnMessageReceivedHandler);

            base.Connect();
        }

        private void OnMessageReceivedHandler([NotNull] NetworkMessage netmsg)
        {
            NetworkReceivedPacket(new HlapiConn(netmsg.conn), _network.CopyToArraySegment(netmsg.reader, new ArraySegment<byte>(_receiveBuffer)));
        }

        protected override void AddClient([NotNull] ClientInfo<HlapiConn> client)
        {
            base.AddClient(client);

            //Add this player to the list of known connections (do not add the local player)
            if (client.PlayerName != _network.PlayerName)
                _addedConnections.Add(client.Connection.Connection);
        }

        public override void Disconnect()
        {
            base.Disconnect();
            NetworkServer.RegisterHandler(_network.TypeCode, HlapiCommsNetwork.NullMessageReceivedHandler);
        }

        protected override void ReadMessages()
        {
            //Messages are received in an event handler, so we don't need to do any work to read events
        }

        ///// <summary>
        ///// This _should_ be called to inform Dissonance that a client has disconnected
        ///// </summary>
        ///// <param name="connection"></param>
        //public static void OnServerDisconnect(NetworkConnection connection)
        //{
        //    if (_instance != null)
        //        _instance.OnServerDisconnect(new HlapiConn(connection));
        //}

        //private void OnServerDisconnect(HlapiConn conn)
        //{
        //    var index = _addedConnections.IndexOf(conn.Connection);
        //    if (index >= 0)
        //    {
        //        Log.Debug("Force disconnected HLAPI connection '{0}'", conn);

        //        _addedConnections.RemoveAt(index);
        //        ClientDisconnected(conn);
        //    }
        //}

        public override ServerState Update()
        {
            // The only way to get an event regarding disconnections from HLAPI is to be a NetworkManager. We aren't a
            // NetworkManager and don't want to be because it would make setting up the HLAPI integration significantly
            // more complex. Instead we'll have to poll for disconnections.
            for (var i = _addedConnections.Count - 1; i >= 0; i--)
            {
                var conn = _addedConnections[i];
                if (!conn.isConnected || conn.lastError == NetworkError.Timeout || !NetworkServer.connections.Contains(_addedConnections[i]))
                {
                    ClientDisconnected(new HlapiConn(_addedConnections[i]));
                    _addedConnections.RemoveAt(i);
                }
            }

            return base.Update();
        }

        #region send
        protected override void SendReliable(HlapiConn connection, ArraySegment<byte> packet)
        {
            if (!Send(packet, connection, _network.ReliableSequencedChannel))
                FatalError("Failed to send reliable packet (unknown HLAPI error)");
        }

        protected override void SendUnreliable(HlapiConn connection, ArraySegment<byte> packet)
        {
            Send(packet, connection, _network.UnreliableChannel);
        }

        /// <summary>
        /// Send a packet
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="connection"></param>
        /// <param name="channel"></param>
        /// <returns>false if there was an error during sending, otherwise true</returns>
        private bool Send(ArraySegment<byte> packet, HlapiConn connection, byte channel)
        {
            if (_network.PreprocessPacketToClient(packet, connection))
                return true;

            // We don't consider sending to a disconnected connection a failure.
            // It could easily be caused by a race (i.e. they only just disconnected) and we don't really care if packets to non-clients get lost!
            if (!connection.Connection.isConnected || connection.Connection.lastError == NetworkError.Timeout)
                return true;

            var length = _network.CopyPacketToNetworkWriter(packet, _sendWriter);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse (Justification it shouldn't be null, but sanity check anyway)
            // ReSharper disable HeuristicUnreachableCode
            if (connection.Connection == null)
            {
                Log.Error("Cannot send to a null destination");
                return false;
            }
            // ReSharper restore HeuristicUnreachableCode
            else if (!connection.Connection.SendBytes(_sendWriter.AsArray(), length, channel))
            {
                return false;
            }



            return true;
        }
        #endregion
    }
}
