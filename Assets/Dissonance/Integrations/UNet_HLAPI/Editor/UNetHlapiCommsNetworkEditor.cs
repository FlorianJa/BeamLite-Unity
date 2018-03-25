using Dissonance.Editor;
using UnityEditor;
using UnityEngine;

namespace Dissonance.Integrations.UNet_HLAPI.Editor
{
    [CustomEditor(typeof(HlapiCommsNetwork))]
    public class UNetCommsNetworkEditor
        : BaseDissonnanceCommsNetworkEditor<HlapiCommsNetwork, HlapiServer, HlapiClient, HlapiConn, Unit, Unit>
    {
        private bool _advanced;

        private int _typeCode;
        private int _reliableSequencedChannel;
        private int _unreliableChannel;

        protected void OnEnable()
        {
            if (target != null)
            {
                var network = (HlapiCommsNetwork)target;
                _typeCode = network.TypeCode;
                _reliableSequencedChannel = network.ReliableSequencedChannel;
                _unreliableChannel = network.UnreliableChannel;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var network = (HlapiCommsNetwork)target;

            if (GUILayout.Button("Open Documentation"))
                Help.BrowseURL("https://placeholder-software.co.uk/dissonance/docs/Basics/Quick-Start-UNet-HLAPI/");

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                //Set the two QoS channels
                EditorGUILayout.HelpBox("Dissonance requires 2 HLAPI QoS channels.", MessageType.Info);
                _reliableSequencedChannel = EditorGUILayout.DelayedIntField("Reliable Channel", _reliableSequencedChannel);
                _unreliableChannel = EditorGUILayout.DelayedIntField("Unreliable Channel", _unreliableChannel);
                if (_unreliableChannel < 0 || _unreliableChannel >= byte.MaxValue || _reliableSequencedChannel < 0 || _reliableSequencedChannel >= byte.MaxValue)
                    EditorGUILayout.HelpBox("Channel IDs must be between 0 and 255", MessageType.Error);
                else if (_unreliableChannel == _reliableSequencedChannel)
                    EditorGUILayout.HelpBox("Channel IDs must be unique", MessageType.Error);
                else
                {
                    network.ReliableSequencedChannel = (byte)_reliableSequencedChannel;
                    network.UnreliableChannel = (byte)_unreliableChannel;
                }

                _advanced = EditorGUILayout.Foldout(_advanced, "Advanced Configuration");
                if (_advanced)
                {
                    //Set type code
                    EditorGUILayout.HelpBox("Dissonance requires a HLAPI type code. If you are not sending raw HLAPI network packets you should use the default value.", MessageType.Info);
                    _typeCode = EditorGUILayout.DelayedIntField("Type Code", _typeCode);
                    if (_typeCode >= ushort.MaxValue || _typeCode < 1000)
                        EditorGUILayout.HelpBox("Event code must be between 1000 and 65535", MessageType.Error);
                    else
                        network.TypeCode = (short)_typeCode;
                }
            }
        }
    }
}