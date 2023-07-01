using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace HaremShortcuts
{
    [BepInPlugin(GUID, PluginName, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "frostedashe.hm.haremshortcuts";
        public const string PluginName = "HaremShortcuts";
        public const string Version = "1.0.0";

        private const string SECTION_GENERAL = "General";
        private const string SECTION_HOTKEYS = "Keyboard shortcuts";
        
        private ConfigEntry<float> MovementModifierPrecision {get; set; }
        private static ConfigEntry<bool> RemoveWatermark {get; set; }

        private ConfigEntry<KeyboardShortcut> HideUIHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> MoveCharactersForwardHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> MoveCharactersBackwardHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> MoveCharactersLeftHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> MoveCharactersRightHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> MoveCharactersUpwardHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> MoveCharactersDownwardHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> MoveCharactersRotateLeftHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> MoveCharactersRotateRightHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> ScaleCharactersUpHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> ScaleCharactersDownHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> SlowModifierHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> ResetModifierHotkey { get; set; }
        private ConfigEntry<KeyboardShortcut> MoveCharactersResetAllHotkey {get; set;}

        private static HScene hScene = null;
        private static GameObject mainHPosition = null;
        private static HScene.MapPosData mapPosData = null;
        private static GameObject uiCamTop = null;
        private static CosCtrlPanel cosCtrlPanel = null;

        private float hPositionMoveSpeed = 2;
        private float hPositionRotationSpeed = 100;
        private float hPositionScaleSpeed = 1;
        private float hScale = 1;

        static bool isUIHidden = false;
        float animeSpeed = 1.0f;

        private Harmony harmony;

        private void Awake()
        {
            MovementModifierPrecision = Config.Bind(SECTION_GENERAL, "Movement Precision", 0.85f, new ConfigDescription("The movement of the characters in the map using hotkeys is slowed by this amount when the \"Modifier Slow\" key is held down.", new AcceptableValueRange<float>(0.85f, 0.98f)));
            RemoveWatermark = Config.Bind(SECTION_GENERAL, "Remove Watermark", true, new ConfigDescription("Whether to remove Illusion's watermark from screenshots."));

            HideUIHotkey = Config.Bind(SECTION_HOTKEYS, "Toggle UI", new KeyboardShortcut(KeyCode.Space), new ConfigDescription("Hide or show all User Interface."));
            MoveCharactersForwardHotkey = Config.Bind(SECTION_HOTKEYS, "Move Characters Forward", new KeyboardShortcut(KeyCode.I), new ConfigDescription("Move the characters forward relative to the camera in an H Scene."));
            MoveCharactersBackwardHotkey = Config.Bind(SECTION_HOTKEYS, "Move Characters Backward", new KeyboardShortcut(KeyCode.K), new ConfigDescription("Move the characters backward relative to the camera in an H Scene."));
            MoveCharactersLeftHotkey = Config.Bind(SECTION_HOTKEYS, "Move Characters Left", new KeyboardShortcut(KeyCode.J), new ConfigDescription("Move the characters left relative to the camera in an H Scene."));
            MoveCharactersRightHotkey = Config.Bind(SECTION_HOTKEYS, "Move Characters Right", new KeyboardShortcut(KeyCode.L), new ConfigDescription("Move the characters right relative to the camera in an H Scene."));
            MoveCharactersUpwardHotkey = Config.Bind(SECTION_HOTKEYS, "Move Characters Upward", new KeyboardShortcut(KeyCode.Y), new ConfigDescription("Move the characters upward in an H Scene."));
            MoveCharactersDownwardHotkey = Config.Bind(SECTION_HOTKEYS, "Move Characters Downward", new KeyboardShortcut(KeyCode.H), new ConfigDescription("Move the characters downward in an H Scene."));
            MoveCharactersRotateLeftHotkey = Config.Bind(SECTION_HOTKEYS, "Move Characters Rotate Left", new KeyboardShortcut(KeyCode.U), new ConfigDescription("Rotate the characters left in an H Scene."));
            MoveCharactersRotateRightHotkey = Config.Bind(SECTION_HOTKEYS, "Move Characters Rotate Right", new KeyboardShortcut(KeyCode.O), new ConfigDescription("Rotate the characters right in an H Scene."));
            ScaleCharactersUpHotkey = Config.Bind(SECTION_HOTKEYS, "Scale Characters Increase", new KeyboardShortcut(KeyCode.KeypadPlus), new ConfigDescription("Increase the scale of characters in an H Scene."));
            ScaleCharactersDownHotkey = Config.Bind(SECTION_HOTKEYS, "Scale Characters Decrease", new KeyboardShortcut(KeyCode.KeypadMinus), new ConfigDescription("Decrease the scale of characters in an H Scene."));
            SlowModifierHotkey = Config.Bind(SECTION_HOTKEYS, "Modifier Slow Key", new KeyboardShortcut(KeyCode.LeftShift), new ConfigDescription("When held down, slows character location movement."));
            ResetModifierHotkey = Config.Bind(SECTION_HOTKEYS, "Modifier Reset Key", new KeyboardShortcut(KeyCode.LeftAlt), new ConfigDescription("When held down, shortcut keys reset the location, rotation, height or scale to map default instead of changing them."));
            MoveCharactersResetAllHotkey = Config.Bind(SECTION_HOTKEYS, "Move Characters Reset All", new KeyboardShortcut(KeyCode.Backspace), new ConfigDescription("Reset the characters' location, rotation, height and scale in the H Scene to map default."));
            
            OnHSceneStart();
            harmony = Harmony.CreateAndPatchAll(GetType());
        }

        private void OnDestroy()
        {
            harmony.UnpatchSelf();
        }

        private void Update()
        {
            if(hScene == null)
            {
                return;
            }

            if(Input.GetKeyDown(KeyCode.F7))
            {
                animeSpeed -= 0.5f;
                HScene.HInfo hInfo = Traverse.Create(hScene).Field("info").GetValue<HScene.HInfo>();
                Traverse.Create(hInfo.state).Field("animeSpeedRate").SetValue(animeSpeed);
            }

            if(Input.GetKeyDown(KeyCode.F8))
            {
                animeSpeed += 0.5f;
                HScene.HInfo hInfo = Traverse.Create(hScene).Field("info").GetValue<HScene.HInfo>();
                Traverse.Create(hInfo.state).Field("animeSpeedRate").SetValue(animeSpeed);
            }

            if(HideUIHotkey.Value.IsDown())
            {
                if(uiCamTop != null)
                {
                    if(isUIHidden)
                    {
                        uiCamTop.SetActive(true);
                    }
                    else
                    {
                        Traverse.Create(hScene.hSceneMenu).Field("oIdx").SetValue(-1);
                        uiCamTop.SetActive(false);
                    }
                    isUIHidden = !isUIHidden;
                }
            }

            cosCtrlPanel.SetVisibleAll(!isUIHidden);

            Vector3 hMovement = new Vector3();
            float hVerticalMovement = 0;
            float hRotation = 0;
            float hScaleDelta = 0;
            float movementPrecision = 1.0f;

            if(Input.GetKey(SlowModifierHotkey.Value.MainKey))
            {
                movementPrecision = 1.0f - MovementModifierPrecision.Value;
            }

            if(Input.GetKey(MoveCharactersForwardHotkey.Value.MainKey))
            {
                hMovement.z = (hPositionMoveSpeed * movementPrecision) * Time.deltaTime;
            }
            if(Input.GetKey(MoveCharactersBackwardHotkey.Value.MainKey))
            {
                hMovement.z = -(hPositionMoveSpeed * movementPrecision) * Time.deltaTime;
            }
            if(Input.GetKey(MoveCharactersLeftHotkey.Value.MainKey))
            {
                hMovement.x = -(hPositionMoveSpeed * movementPrecision) * Time.deltaTime;
            }
            if(Input.GetKey(MoveCharactersRightHotkey.Value.MainKey))
            {
                hMovement.x = (hPositionMoveSpeed * movementPrecision) * Time.deltaTime;
            }
            if(Input.GetKey(MoveCharactersUpwardHotkey.Value.MainKey))
            {
                hVerticalMovement = (hPositionMoveSpeed * movementPrecision) * Time.deltaTime;
            }
            if(Input.GetKey(MoveCharactersDownwardHotkey.Value.MainKey))
            {
                hVerticalMovement = -(hPositionMoveSpeed * movementPrecision) * Time.deltaTime;
            }
            if(Input.GetKey(MoveCharactersRotateLeftHotkey.Value.MainKey))
            {
                hRotation = -(hPositionRotationSpeed * movementPrecision) * Time.deltaTime;
            }
            if(Input.GetKey(MoveCharactersRotateRightHotkey.Value.MainKey))
            {
                hRotation = (hPositionRotationSpeed * movementPrecision) * Time.deltaTime;
            }

            if(Input.GetKey(ScaleCharactersUpHotkey.Value.MainKey))
            {
                hScaleDelta += (hPositionScaleSpeed * movementPrecision) * Time.deltaTime;
            }

            if(Input.GetKey(ScaleCharactersDownHotkey.Value.MainKey))
            {
                hScaleDelta -= (hPositionScaleSpeed * movementPrecision) * Time.deltaTime;
            }


            bool shouldResetPos = Input.GetKey(ResetModifierHotkey.Value.MainKey) && (Input.GetKey(MoveCharactersForwardHotkey.Value.MainKey) || Input.GetKey(MoveCharactersBackwardHotkey.Value.MainKey) || Input.GetKey(MoveCharactersLeftHotkey.Value.MainKey) || Input.GetKey(MoveCharactersRightHotkey.Value.MainKey));
            bool shouldResetRot = Input.GetKey(ResetModifierHotkey.Value.MainKey) && ((Input.GetKey(MoveCharactersRotateLeftHotkey.Value.MainKey)) || (Input.GetKey(MoveCharactersRotateRightHotkey.Value.MainKey)));
            bool shouldResetHeight = Input.GetKey(ResetModifierHotkey.Value.MainKey) && ((Input.GetKey(MoveCharactersUpwardHotkey.Value.MainKey)) || (Input.GetKey(MoveCharactersDownwardHotkey.Value.MainKey)));
            bool shouldResetScale = Input.GetKey(ResetModifierHotkey.Value.MainKey) && ((Input.GetKey(ScaleCharactersUpHotkey.Value.MainKey)) || (Input.GetKey(ScaleCharactersDownHotkey.Value.MainKey)));

            if(MoveCharactersResetAllHotkey.Value.IsDown())
            {
                shouldResetPos = true;
                shouldResetRot = true;
                shouldResetHeight = true;
                shouldResetScale = true;
            }

            if(shouldResetPos)
            {
                Vector3 newPos = mapPosData.putList[mapPosData.index].position;
                newPos.y = mainHPosition.transform.position.y;
                mainHPosition.transform.position = newPos;
                hMovement = new Vector3();
            }

            if(shouldResetRot)
            {
                mainHPosition.transform.rotation = mapPosData.putList[mapPosData.index].rotation;
                hRotation = 0;
            }

            if(shouldResetHeight)
            {
                Vector3 newPos = mapPosData.putList[mapPosData.index].position;
                newPos.x = mainHPosition.transform.position.x;
                newPos.z = mainHPosition.transform.position.z;
                mainHPosition.transform.position = newPos;
                hVerticalMovement = 0;
            }

            if(shouldResetScale)
            {
                hScaleDelta = 0.1f;
                hScale = 0.9f;
            }

            bool shouldMovePosition = (hMovement.sqrMagnitude != 0) || (hVerticalMovement != 0) || (hRotation != 0) || (hScaleDelta != 0);
            if(shouldMovePosition)
            {
                if(mainHPosition != null)
                {
                    hMovement = Camera.main.transform.rotation * hMovement;
                    hMovement.y = hVerticalMovement;
                    mainHPosition.transform.Translate(hMovement, Space.World);
                    hMovement.x = 0;
                    hMovement.y = hRotation;
                    hMovement.z = 0;
                    mainHPosition.transform.Rotate(hMovement, Space.World);

                    hScale += hScaleDelta;
                    if(hScale < 0.1f)
                    {
                        hScale = 0.1f;
                    }
                    mainHPosition.transform.localScale = new Vector3(hScale, hScale, hScale);
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HScene), nameof(HScene.Start))]
        private static void OnHSceneStart()
        {
            hScene = FindObjectOfType<HScene>();
            if(hScene)
            {
                mainHPosition = Traverse.Create(hScene).Field("mainPosition").GetValue<GameObject>();
                mapPosData = Traverse.Create(hScene).Field("mapPosData").GetValue<HScene.MapPosData>();
                uiCamTop = GameObject.Find("SpTop");
                cosCtrlPanel = Traverse.Create(hScene).Field("cosCtrlPanel").GetValue<CosCtrlPanel>();
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(GameScreenShot), nameof(GameScreenShot.CaptureProc))]
        private static void OnScreenShot(GameScreenShot __instance)
        {
            GameObject objCapSprite = Traverse.Create(__instance).Field("objCapSprite").GetValue<GameObject>();
            Camera cam = objCapSprite.GetComponent<Camera>();
            cam.enabled = !RemoveWatermark.Value;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneMenu), "CollisionProc")]
        private static bool OnHSceneMenuCollisionProc(ref bool __result)
        {
            if(isUIHidden)
            {
                __result = false;
                return false;
            }
            
            return true;
        }
    }
}
