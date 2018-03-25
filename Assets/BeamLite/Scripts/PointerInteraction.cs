namespace VRTK.Examples
{
    using HoloToolkit.Examples.InteractiveElements;
    using UnityEngine;

    public class PointerInteraction : VRTK_InteractableObject
    {
        InteractiveToggle it;
        public override void StartUsing(VRTK_InteractUse usingObject)
        {
            base.StartUsing(usingObject);
            it.ToggleLogic();
        }

        public override void StopUsing(VRTK_InteractUse usingObject)
        {
            base.StopUsing(usingObject);
            it.ToggleLogic();
        }

        protected void Start()
        {
            it = GetComponent<InteractiveToggle>();
        }

    }
}