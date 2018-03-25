namespace VRTK.Examples
{
    using UnityEngine;
    using UnityEventHelper;

    public class ButtonReactor : MonoBehaviour
    {
        public Whiteboard Whiteboard;

        private VRTK_Button_UnityEvents buttonEvents;

        private void Start()
        {
            buttonEvents = GetComponent<VRTK_Button_UnityEvents>();
            if (buttonEvents == null)
            {
                buttonEvents = gameObject.AddComponent<VRTK_Button_UnityEvents>();
            }
            buttonEvents.OnPushed.AddListener(handlePush);
        }

        private void handlePush(object sender, Control3DEventArgs e)
        {
            VRTK_Logger.Info("Pushed");
            Whiteboard.Clear();
        }
    }
}