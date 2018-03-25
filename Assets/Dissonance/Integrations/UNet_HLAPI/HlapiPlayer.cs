using UnityEngine;
using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_HLAPI
{
    /// <summary>
    /// When added to the player prefab, allows Dissonance to automatically track
    /// the location of remote players for positional audio for games using the
    /// Unity Networking High-Level API (HLAPI).
    /// </summary>
    [RequireComponent(typeof (NetworkIdentity))]
    public class HlapiPlayer : NetworkBehaviour, IDissonancePlayer
    {
        private static readonly Log Log = Logs.Create(LogCategory.Network, "HLAPI Player Component");

        private DissonanceComms _comms;

        public bool IsTracking { get; private set; }

        /// <summary>
        /// The name of the player
        /// </summary>
        /// <remarks>
        /// This is a syncvar, this means unity will handle setting this value.
        /// This is important for Join-In-Progress because new clients will join and instantly have the player name correctly set without any effort on our part.
        /// https://docs.unity3d.com/Manual/UNetStateSync.html
        /// </remarks>
        [SyncVar]
        private string _playerId;
        public string PlayerId { get { return _playerId; } }

        public Vector3 Position
        {
            get { return transform.position; }
        }

        public Quaternion Rotation
        {
            get { return transform.rotation; }
        }

        public NetworkPlayerType Type
        {
            get { return isLocalPlayer ? NetworkPlayerType.Local : NetworkPlayerType.Remote; }
        }
        
        public void OnDestroy()
        {
            if (_comms != null)
                _comms.LocalPlayerNameChanged -= SetPlayerName;
        }

        public void OnEnable()
        {
            _comms = FindObjectOfType<DissonanceComms>();
        }

        public void OnDisable()
        {
            if (IsTracking)
                StopTracking();
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            var comms = FindObjectOfType<DissonanceComms>();
            if (comms == null)
            {
                throw Log.CreateUserErrorException(
                    "cannot find DissonanceComms component in scene",
                    "not placing a DissonanceComms component on a game object in the scene",
                    "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-UNet-HLAPI/",
                    "9A79FDCB-263E-4124-B54D-67EDA39C09A5"
                );
            }

            Log.Debug("OnStartAuthority Name={0}", comms.LocalPlayerName);

            // This method is called on the client which has control authority over this object. This will be the local client of whichever player we are tracking.
            if (comms.LocalPlayerName != null)
                SetPlayerName(comms.LocalPlayerName);

            //Subscribe to future name changes (this is critical because we may not have run the initial set name yet and this will trigger that initial call)
            comms.LocalPlayerNameChanged += SetPlayerName;
        }

        private void SetPlayerName(string playerName)
        {
            //We need the player name to be set on all the clients and then tracking to be started (on each client).
            //To do this we send a command from this client, informing the server of our name. The server will pass this on to all the clients (with an RPC)
            // Client -> Server -> Client

            //We need to stop and restart tracking to handle the name change
            if (IsTracking)
                StopTracking();

            //Perform the actual work
            _playerId = playerName;
            StartTracking();

            //Inform the server the name has changed
            if (hasAuthority)
                CmdSetPlayerName(playerName);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            //A client is starting. Start tracking if the name has been properly initialised.
            if (!string.IsNullOrEmpty(PlayerId))
                StartTracking();
        }

        /// <summary>
        /// Invoking on client will cause it to run on the server
        /// </summary>
        /// <param name="playerName"></param>
        [Command]
        private void CmdSetPlayerName(string playerName)
        {
            _playerId = playerName;

            //Now call the RPC to inform clients they need to handle this changed value
            RpcSetPlayerName(playerName);
        }

        /// <summary>
        /// Invoking on the server will cause it to run on all the clients
        /// </summary>
        /// <param name="playerName"></param>
        [ClientRpc]
        private void RpcSetPlayerName(string playerName)
        {
            //received a message from server (on all clients). If this is a non authoritative client perform the name change
            if (!hasAuthority)
                SetPlayerName(playerName);
        }

        private void StartTracking()
        {
            if (IsTracking)
                throw Log.CreatePossibleBugException("Attempting to start player tracking, but tracking is already started", "B7D1F25E-72AF-4E93-8CFF-90CEBEAC68CF");

            if (_comms != null)
            {
                _comms.TrackPlayerPosition(this);
                IsTracking = true;
            }
        }

        private void StopTracking()
        {
            if (!IsTracking)
                throw Log.CreatePossibleBugException("Attempting to stop player tracking, but tracking is not started", "EC5C395D-B544-49DC-B33C-7D7533349134");

            if (_comms != null)
            {
                _comms.StopTracking(this);
                IsTracking = false;
            }
        }
    }
}