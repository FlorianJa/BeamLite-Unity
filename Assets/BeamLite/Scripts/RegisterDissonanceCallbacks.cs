using Dissonance;
using Dissonance.Integrations.UNet_HLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegisterDissonanceCallbacks : MonoBehaviour {

    public DissonanceComms DissonanceComms;
    // Use this for initialization
    void Start ()
    {
        if(DissonanceComms != null)
        {
            DissonanceComms.OnPlayerJoinedSession += DissonanceComms_OnPlayerJoinedSession;
            DissonanceComms.OnPlayerLeftSession += DissonanceComms_OnPlayerLeftSession;
        }
    }

    private void DissonanceComms_OnPlayerLeftSession(VoicePlayerState player)
    {
        Debug.Log("Player " + player.Name + " Left session");
    }

    private void DissonanceComms_OnPlayerJoinedSession(VoicePlayerState player)
    {
        Debug.Log("Player " + player.Name + " Joined session");


        //get all networkplayer
        NetworkPlayer[] networkPlayers = FindObjectsOfType(typeof(NetworkPlayer)) as NetworkPlayer[];
        foreach (var networkplayer in networkPlayers)
        {
            var playerId = networkplayer.gameObject.GetComponent<BeamLiteHlapiPlayer>().PlayerId;
            if (player.Name == playerId)
            {
                player.OnStartedSpeaking += networkplayer.Avatar.GetComponent<AvatarController>().Player_OnStartedSpeaking;
                player.OnStoppedSpeaking += networkplayer.Avatar.GetComponent<AvatarController>().Player_OnStoppedSpeaking;
                break;
            }
        }
        if (Utils.CurrentPlayerType == Utils.PlayerType.HoloLens)
        {
            if (player.Name != DissonanceComms.LocalPlayerName)
            {
                foreach (var networkplayer in networkPlayers)
                {
                    var playerId = networkplayer.gameObject.GetComponent<BeamLiteHlapiPlayer>().PlayerId;
                    if (player.Name == playerId && networkplayer.PlayerType == Utils.PlayerType.HoloLens)
                    {
                        player.IsLocallyMuted = true;
                    }
                }
            }
        }
    }

    public void OnDestroy()
    {
        if (DissonanceComms != null)
        {
            DissonanceComms.OnPlayerJoinedSession -= DissonanceComms_OnPlayerJoinedSession;
            DissonanceComms.OnPlayerLeftSession -= DissonanceComms_OnPlayerLeftSession;
        }
    }

}
