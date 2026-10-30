using Harmony;

namespace FunctionEnhancement
{
    public class DebugPatch
    {
        [HarmonyPatch(typeof(DebugHandler), "OnKeyDown")]
        public static class DebugHandlerOnKeyDownPatch
        {
            public static bool Prefix(ref DebugHandler __instance, KButtonEvent e)
            {
                if (!DebugHandler.enabled)
                {
                    return false;
                }

                if (!e.TryConsume(Action.DebugToggle))
                {
                    return true;
                }

                if (DebugPaintElementScreen.Instance != null)
                {
                    bool activeSelf = DebugPaintElementScreen.Instance.gameObject.activeSelf;
                    DebugPaintElementScreen.Instance.gameObject.SetActive(!activeSelf);
                    if ((bool) (UnityEngine.Object) DebugElementMenu.Instance &&
                        DebugElementMenu.Instance.root.activeSelf)
                    {
                        DebugElementMenu.Instance.root.SetActive(false);
                    }

                    DebugBaseTemplateButton.Instance.gameObject.SetActive(!activeSelf);
                    if (CameraController.Instance != null)
                    {
                        CameraController.Instance.EnableFreeCamera(!activeSelf);
                    }
                }

                return false;
            }
        }
    }
}