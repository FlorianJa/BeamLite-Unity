using System;
using System.Collections.Generic;
using Dissonance.Datastructures;
using Dissonance.Networking;
using UnityEngine.Networking;
using Dissonance.Extensions;

namespace Dissonance.Integrations.UNet_HLAPI
{
    public class HlapiCommsNetwork
        : BaseCommsNetwork<HlapiServer, HlapiClient, HlapiConn, Unit, Unit>
    {
        // ReSharper disable UnassignedField.Global, FieldCanBeMadeReadOnly.Global, ConvertToConstant.Global (Justification: Changed by the editor in a way code inspector can't understand)
        public byte UnreliableChannel = 1;
        public byte ReliableSequencedChannel = 0;

        public short TypeCode = 18385;
        // ReSharper restore UnassignedField.Global, FieldCanBeMadeReadOnly.Global, ConvertToConstant.Global

        private readonly ConcurrentPool<byte[]> _loopbackBuffers = new ConcurrentPool<byte[]>(8, () => new byte[1024]);
        private readonly List<ArraySegment<byte>> _loopbackQueue = new List<ArraySegment<byte>>();

        protected override HlapiServer CreateServer(Unit details)
        {
            return new HlapiServer(this);
        }

        protected override HlapiClient CreateClient(Unit details)
        {
            return new HlapiClient(this);
        }

        protected override void Update()
        {
            if (IsInitialized)
            {
                // Network is considered active if all of:
                // - Network explicitly claims it is active
                // - Server or client explicitly claim they are active
                // - Also if the client is active only say we're active once the client is non-null and has a non-null connection
                var networkActive = NetworkManager.singleton != null
                                 && NetworkManager.singleton.isNetworkActive
                                 && (NetworkServer.active || NetworkClient.active)
                                 && (!NetworkClient.active || (NetworkManager.singleton.client != null && NetworkManager.singleton.client.connection != null));

                if (networkActive)
                {
                    // switch to the appropriate mode if we have not already
                    var server = NetworkServer.active;
                    var client = NetworkClient.active;

                    if (Mode.IsServerEnabled() != server || Mode.IsClientEnabled() != client)
                    {
                        if (server && client)
                            RunAsHost(Unit.None, Unit.None);
                        else if (server)
                            RunAsDedicatedServer(Unit.None);
                        else if (client)
                            RunAsClient(Unit.None);
                    }
                }
                else if (Mode != NetworkMode.None)
                {
                    // stop the network if unet has shut down
                    Stop();

                    //Discard looped back packets which haven't been delivered yet
                    _loopbackQueue.Clear();
                }

                //Send looped back packets
                for (var i = 0; i < _loopbackQueue.Count; i++)
                {
                    if (Client != null)
                        Client.NetworkReceivedPacket(_loopbackQueue[i]);

                    // Recycle the packet into the pool of byte buffers
                    // ReSharper disable once AssignNullToNotNullAttribute (Justification: ArraySegment array is not null)
                    _loopbackBuffers.Put(_loopbackQueue[i].Array);
                }
                _loopbackQueue.Clear();
            }

            base.Update();
        }

        protected override void Initialize()
        {
            //Sanity check the channels
            if (UnreliableChannel >= NetworkManager.singleton.channels.Count)
            {
                throw Log.CreateUserErrorException(
                    "configured 'unreliable' channel is out of range",
                    "set the wrong channel number in the HLAPI Comms Network component",
                    "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-UNet-HLAPI/",
                    "B19B4916-8709-490B-8152-A646CCAD788E"
                );
            }

            var unreliable = NetworkManager.singleton.channels[UnreliableChannel];
            if (unreliable != QosType.Unreliable)
            {
                throw Log.CreateUserErrorException(
                    string.Format("configured 'unreliable' channel has QoS type '{0}'", unreliable),
                    "not creating the channel with the correct QoS type",
                    "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-UNet-HLAPI/",
                    "24ee53b1-7517-4672-8a4a-64a3e3c87ef6"
                );
            }

            if (ReliableSequencedChannel >= NetworkManager.singleton.channels.Count)
            {
                throw Log.CreateUserErrorException(
                    "configured 'reliable' channel is out of range",
                    "set the wrong channel number in the HLAPI Comms Network component",
                    "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-UNet-HLAPI/",
                    "5F5F2875-ECC8-433D-B0CB-97C151B8094D"
                );
            }

            var reliable = NetworkManager.singleton.channels[ReliableSequencedChannel];
            if (reliable != QosType.ReliableSequenced)
            {
                throw Log.CreateUserErrorException(
                    string.Format("configured 'reliable sequenced' channel has QoS type '{0}'", reliable),
                    "not creating the channel with the correct QoS type",
                    "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-UNet-HLAPI/",
                    "035773ec-aef3-477a-8eeb-c234d416171c"
                );
            }

            NetworkServer.RegisterHandler(TypeCode, NullMessageReceivedHandler);

            base.Initialize();
        }

        internal bool PreprocessPacketToClient(ArraySegment<byte> packet, HlapiConn destination)
        {
            //I have no idea if the HLAPI handles loopback. Whether it does or does not isn't important though - it's more
            //efficient to handle the loopback special case directly instead of passing through the entire network system!

            //This should never even be called if this peer is not the host!
            if (Server == null)
                throw Log.CreatePossibleBugException("server packet preprocessing running, but this peer is not a server", "8f9dc0a0-1b48-4a7f-9bb6-f767b2542ab1");

            //If there is no local client (e.g. this is a dedicated server) then there can't possibly be loopback
            if (Client == null)
                return false;

            //Is this loopback?
            if (NetworkManager.singleton.client.connection != destination.Connection)
                return false;

            //This is loopback!

            // check that we have a valid local client (in cases of startup or in-progress shutdowns)
            if (Client != null)
            {
                // Don't immediately deliver the packet, add it to a queue and deliver it next frame. This prevents the local client from executing "within" ...
                // ...the local server which can cause confusing stack traces.
                _loopbackQueue.Add(packet.CopyTo(_loopbackBuffers.Get()));
            }

            return true;
        }

        internal bool PreprocessPacketToServer(ArraySegment<byte> packet)
        {
            //I have no idea if the HLAPI handles loopback. Whether it does or does not isn't important though - it's more
            //efficient to handle the loopback special case directly instead of passing through the entire network system!

            //This should never even be called if this peer is not a client!
            if (Client == null)
                throw Log.CreatePossibleBugException("client packet processing running, but this peer is not a client", "dd75dce4-e85c-4bb3-96ec-3a3636cc4fbe");

            //Is this loopback?
            if (Server == null)
                return false;

            //This is loopback!

            //Since this is loopback destination == source (by definition)
            Server.NetworkReceivedPacket(new HlapiConn(NetworkManager.singleton.client.connection), packet);

            return true;
        }

        internal static void NullMessageReceivedHandler([NotNull] NetworkMessage netmsg)
        {
            if (netmsg == null)
                throw new ArgumentNullException("netmsg");

            if (Logs.GetLogLevel(LogCategory.Network) <= LogLevel.Trace)
                UnityEngine.Debug.Log("Discarding Dissonance network message");

            var length = (int)netmsg.reader.ReadPackedUInt32();
            for (var i = 0; i < length; i++)
                netmsg.reader.ReadByte();
        }

        internal ArraySegment<byte> CopyToArraySegment([NotNull] NetworkReader msg, ArraySegment<byte> segment)
        {
            if (msg == null) throw new ArgumentNullException("msg");

            var arr = segment.Array;
            if (arr == null) throw new ArgumentNullException("segment");

            var length = (int)msg.ReadPackedUInt32();
            if (length > segment.Count)
                throw Log.CreatePossibleBugException("receive buffer is too small", "A7387195-BF3D-4796-A362-6C64BB546445");

            for (var i = 0; i < length; i++)
                arr[segment.Offset + i] = msg.ReadByte();

            return new ArraySegment<byte>(arr, segment.Offset, length);
        }

        internal int CopyPacketToNetworkWriter(ArraySegment<byte> packet, [NotNull]  NetworkWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");

            var arr = packet.Array;
            if (arr == null) throw new ArgumentNullException("packet");

            writer.SeekZero();
            writer.StartMessage(TypeCode);
            {
                //Length prefix the packet
                writer.WritePackedUInt32((uint)packet.Count);

                //Copy out the bytes.
                //You might think we could use `Write(buffer, offset, count)` here. You would be wrong! In that method the 'offset' is the
                //offset to write to in the packet! This is probably a bug in unity!
                for (var i = 0; i < packet.Count; i++) 
                    writer.Write(arr[packet.Offset + i]); //
            }
            writer.FinishMessage();

            return writer.Position;
        }
    }

    public struct HlapiConn
        : IEquatable<HlapiConn>
    {
        public readonly NetworkConnection Connection;

        public HlapiConn(NetworkConnection connection)
            : this()
        {
            Connection = connection;
        }

        public override int GetHashCode()
        {
            return Connection.GetHashCode();
        }

        public override string ToString()
        {
            return Connection.ToString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is HlapiConn && Equals((HlapiConn)obj);
        }

        public bool Equals(HlapiConn other)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable HeuristicUnreachableCode
            if (Connection == null)
            {
                if (other.Connection == null)
                    return true;
                return false;
            }
            // ReSharper restore HeuristicUnreachableCode

            return Connection.Equals(other.Connection);
        }
    }
}
