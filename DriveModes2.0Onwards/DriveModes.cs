using GTA; // This is a reference that is needed! do not edit this
using GTA.Native; // This is a reference that is needed! do not edit this
using GTA.Math;
using System; // This is a reference that is needed! do not edit this
using System.Windows.Forms; // This is a reference that is needed! do not edit this
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using CheatEngine;
using SimpleUI;
using ScriptCommunicatorHelper;
using Control = GTA.Control;

namespace DrivingModes
{
    public delegate void DriveModeChangeEvent(DriveModes sender, Vehicle selectedVehicle);

    public class DriveModes : Script // declare Modname as a script
    {
        static string modInfo = "Drive Modes 2.0 by stillhere";

        List<SpecialVehicles> SpecialVehicles = new List<SpecialVehicles> { };

        List<VehicleMod> modTypes = new List<VehicleMod>
            {
                VehicleMod.Aerials, VehicleMod.AirFilter, VehicleMod.ArchCover, VehicleMod.Armor, VehicleMod.BackWheels, VehicleMod.Brakes,
                VehicleMod.ColumnShifterLevers, VehicleMod.Dashboard, VehicleMod.DialDesign, VehicleMod.DoorSpeakers, VehicleMod.Engine,
                VehicleMod.EngineBlock, VehicleMod.Exhaust, VehicleMod.Fender, VehicleMod.Frame, VehicleMod.FrontBumper, VehicleMod.FrontWheels,
                VehicleMod.Grille, VehicleMod.Hood, VehicleMod.Horns, VehicleMod.Hydraulics, VehicleMod.Livery, VehicleMod.Ornaments, VehicleMod.Plaques,
                VehicleMod.PlateHolder, VehicleMod.RearBumper, VehicleMod.RightFender, VehicleMod.Roof, VehicleMod.Seats, VehicleMod.SideSkirt, VehicleMod.Speakers,
                VehicleMod.Spoilers, VehicleMod.SteeringWheels, VehicleMod.Struts, VehicleMod.Suspension, VehicleMod.Tank, VehicleMod.Transmission, VehicleMod.Trim, VehicleMod.TrimDesign,
                VehicleMod.Trunk, VehicleMod.VanityPlates, VehicleMod.Windows
            };

        List<int> modIndexes = new List<int> { };

        string LastUsedVehicleHash;
        int LastUsedSpecialVehicleIndex;
        string LastUsedVehicleName;

        Camera CustomVehicleCam;
        int CamCurrentMode;
        bool CamOn;
        int lastVehicleViewMode;
        int lastPedViewMode;
        float LeftRightSum = 0f;
        float UpDownSum = 0f;
        int CameraAdjustTimer;
        int AimWaitTimer;
        bool LookBackAim;
        bool QuickLoadCamSettings;
        CameraSettings FirstPersonSettings = new CameraSettings();
        CameraSettings FixedCameraSettings = new CameraSettings();
        CameraSettings ThirdPersonSettings = new CameraSettings();
        List<CameraSettings> CameraList = new List<CameraSettings>() { new CameraSettings() }; //add a dummy first camera setting so that they line up with the CameraTypes enum

        CultureInfo culture;

        int InputTimer;

        //IEnumerator<bool> EditorLoop;

        MenuPool _menuPool;
        UIMenu MainMenu;
        UIMenuItem ItemAddVehicleToList;
        UIMenuItem ItemRefreshAllSettings;

        UIMenu VehicleEditorMenu;
        UIMenuItem ItemConfigSelect;
        UIMenuItem ItemCentreOfMassX;
        UIMenuItem ItemCentreOfMassY;
        UIMenuItem ItemCentreOfMassZ;
        UIMenuItem ItemInertiaMultiplierX;
        UIMenuItem ItemInertiaMultiplierY;
        UIMenuItem ItemInertiaMultiplierZ;
        UIMenuItem ItemDriveBiasFront;
        UIMenuItem ItemnInitialDriveGears;
        UIMenuItem ItemDriveInertia;
        UIMenuItem ItemClutchChangeRateScaleUpShift;
        UIMenuItem ItemClutchChangeRateScaleDownShift;
        UIMenuItem ItemInitialDriveForce;
        UIMenuItem ItemInitialDriveMaxFlatVel;
        UIMenuItem ItemBrakeForce;
        UIMenuItem ItemBrakeBiasFront;
        UIMenuItem ItemHandBrakeForce;
        UIMenuItem ItemSteeringLock;
        UIMenuItem ItemTractionCurveMax;
        UIMenuItem ItemTractionCurveMin;
        UIMenuItem ItemTractionCurveLateral;
        UIMenuItem ItemTractionSpringDeltaMax;
        UIMenuItem ItemLowSpeedTractionLossMult;
        UIMenuItem ItemCamberStiffness;
        UIMenuItem ItemTractionBiasFront;
        UIMenuItem ItemTractionLossMult;
        UIMenuItem ItemSuspensionForce;
        UIMenuItem ItemSuspensionCompDamp;
        UIMenuItem ItemSuspensionReboundDamp;
        UIMenuItem ItemSuspensionUpperLimit;
        UIMenuItem ItemSuspensionLowerLimit;
        UIMenuItem ItemSuspensionBiasFront;
        UIMenuItem ItemAntiRollBarForce;
        UIMenuItem ItemAntiRollBarBiasFront;
        UIMenuItem ItemRollCentreHeightFront;
        UIMenuItem ItemRollCentreHeightRear;
        UIMenuItem ItemDeleteConfig;
        UIMenuItem ItemSaveConfigNew;
        UIMenuItem ItemSaveConfig;
        UIMenuItem ItemReloadConfig;

        UIMenu CameraSettingsMenu;
        UIMenuItem ItemCameraSwitch;
        UIMenuItem ItemPositionOffsetX;
        UIMenuItem ItemPositionOffsetY;
        UIMenuItem ItemPositionOffsetZ;
        UIMenuItem ItemRotationX;
        UIMenuItem ItemRotationY;
        UIMenuItem ItemRotationZ;
        UIMenuItem ItemPointPositionX;
        UIMenuItem ItemPointPositionY;
        UIMenuItem ItemPointPositionZ;
        UIMenuItem ItemLeftRightAdditionalCameraOffset;
        UIMenuItem ItemMouseSensitivity;
        UIMenuItem ItemGamepadLookSensitivity;
        UIMenuItem ItemGamepadAimSensitivity;
        UIMenuItem ItemFieldOfView;
        UIMenuItem ItemAutoCenter;
        UIMenuItem ItemCameraEnabled;
        UIMenuItem ItemSaveCameraSettings;
        UIMenuItem ItemReloadCameraSettings;

        MenuPool _dislayPool;

        public DriveModes() // main function
        {
            if (!File.Exists(@"scripts\DrivingModes.scmod"))
            {
                using (StreamWriter sw = new StreamWriter(@"scripts\DrivingModes.scmod"))
                {
                    sw.WriteLine("Drive Modes");
                    sw.WriteLine("Version 2.0.1");
                }
            }

            SetupKeyboardCulture();
            ReadVehicles();
            SetupMenu();
            //EditorLoop = EditConfig();
            HandleEvents();
            LoadINISettings();
            LoadCameraSettings(@"scripts\DrivingModes\GlobalCameraSettings.ini");

            CameraList.Add(FirstPersonSettings);
            CameraList.Add(FixedCameraSettings);
            CameraList.Add(ThirdPersonSettings);

            Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            Interval = 0;
        }

        void SetupKeyboardCulture()
        {
            culture = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.Name, true);
            culture.NumberFormat.NumberDecimalSeparator = ".";
            forceDecimal();
        }

        void forceDecimal()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        }

        void HandleEvents()
        {
            OnDriveModeChange += (sender, veh) =>
            {
                if (isVehicleSpecial(veh))
                {
                    ApplyAllInConfig(SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).Configs.ElementAt(SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).LastUsedIndex));
                    forceRespawnVeh(veh);
                }
            };
        }

        void SetupMenu()
        {
            _menuPool = new MenuPool();
            MainMenu = new UIMenu("Drive Modes");
            _menuPool.AddMenu(MainMenu);
            MainMenu.TitleBackgroundColor = Color.FromArgb(160, 3, 16, 87);
            MainMenu.TitleColor = Color.FromArgb(255, 255, 255, 255);
            MainMenu.DescriptionBoxColor = Color.FromArgb(230, 222, 222, 222);
            MainMenu.DescriptionTextColor = Color.FromArgb(255, 0, 0, 0);
            MainMenu.HighlightedBoxColor = Color.FromArgb(230, 255, 255, 255);
            MainMenu.HighlightedItemTextColor = Color.FromArgb(255, 0, 0, 0);
            MainMenu.DefaultBoxColor = Color.FromArgb(230, 0, 0, 0);
            MainMenu.DefaultTextColor = Color.FromArgb(255, 255, 255, 255);
            MainMenu.TitleUnderlineColor = Color.FromArgb(80, 150, 255, 255);

            ItemAddVehicleToList = new UIMenuItem("Create Initial Config", null, "If \"1st - Default.cfg\" already exists, it will be overwritten by the vehicle's current handling data. This will also give the current vehicle its own camera settings.");
            MainMenu.AddMenuItem(ItemAddVehicleToList);

            VehicleEditorMenu = new UIMenu("Vehicle Editor");
            _menuPool.AddSubMenu(VehicleEditorMenu, MainMenu, VehicleEditorMenu.Title);

            ItemConfigSelect = new UIMenuItem("Current Drive Mode", null, "Select your config.");
            VehicleEditorMenu.AddMenuItem(ItemConfigSelect);

            ItemCentreOfMassX = new UIMenuItem("vecCentreOfMassOffsetX", null, "This value shifts the center of gravity in meters from side to side (when in vehicle looking forward). 0.0 means that the center of gravity will be in the center of the vehicle. Positive values move the centre of gravity right.");
            VehicleEditorMenu.AddMenuItem(ItemCentreOfMassX);

            ItemCentreOfMassY = new UIMenuItem("vecCentreOfMassOffsetY", null, "This value shifts the center of gravity in meters from side to side (when in vehicle looking forward). 0.0 means that the center of gravity will be in the center of the vehicle. Positive values move the centre of gravity forwards.");
            VehicleEditorMenu.AddMenuItem(ItemCentreOfMassY);

            ItemCentreOfMassZ = new UIMenuItem("vecCentreOfMassOffsetZ", null, "This value shifts the center of gravity in meters from side to side (when in vehicle looking forward). 0.0 means that the center of gravity will be in the center of the vehicle. Positive values move the centre of gravity up. Changing this value to great negative quantities (E.G. -10 or greater) will cause the vehicle to behave erratically, moving many feet per frame.");
            VehicleEditorMenu.AddMenuItem(ItemCentreOfMassZ);

            ItemInertiaMultiplierX = new UIMenuItem("vecInertiaMultiplierX", null, "Not sure what this does!");
            VehicleEditorMenu.AddMenuItem(ItemInertiaMultiplierX);

            ItemInertiaMultiplierY = new UIMenuItem("vecInertiaMultiplierY", null, "Not sure what this does!");
            VehicleEditorMenu.AddMenuItem(ItemInertiaMultiplierY);

            ItemInertiaMultiplierZ = new UIMenuItem("vecInertiaMultiplierZ", null, "Not sure what this does!");
            VehicleEditorMenu.AddMenuItem(ItemInertiaMultiplierZ);

            ItemDriveBiasFront = new UIMenuItem("fDriveBiasFront", null, "0.0 is rear wheel drive, 1.0 is front wheel drive, and any value between 0.01 and 0.99 is four wheel drive (0.5 give both front and rear axles equal force, being perfect 4WD.)");
            VehicleEditorMenu.AddMenuItem(ItemDriveBiasFront);

            ItemnInitialDriveGears = new UIMenuItem("nInitialDriveGears", null, "How many forward speeds a transmission contains. 1 to 16 or more.");
            VehicleEditorMenu.AddMenuItem(ItemnInitialDriveGears);

            ItemDriveInertia = new UIMenuItem("fDriveInertia", null, "Describes how fast an engine will rev. For example an engine with a long stroke crank and heavy flywheel will take longer to redline than an engine with a short stroke and light flywheel. Range: 0.01 - 2.0. Default value is 1.0, or no modification of drive intertia. Bigger value = quicker redline");
            VehicleEditorMenu.AddMenuItem(ItemDriveInertia);

            ItemClutchChangeRateScaleUpShift = new UIMenuItem("fClutchChangeRateScaleUpShift", null, "Clutch speed multiplier on up shifts, bigger number = faster shifts");
            VehicleEditorMenu.AddMenuItem(ItemClutchChangeRateScaleUpShift);
       
            ItemClutchChangeRateScaleDownShift = new UIMenuItem("fClutchChangeRateScaleDownShift", null, "Clutch speed multiplier on down shifts, bigger number = faster shifts");
            VehicleEditorMenu.AddMenuItem(ItemClutchChangeRateScaleDownShift);

            ItemInitialDriveForce = new UIMenuItem("fInitialDriveForce", null, "This multiplier modifies the game's calculation of drive force (from the output of the transmission). Range: 0.01 - 2.0 and above. 1.0 uses drive force calculation unmodified. Values less than 1.0 will in effect give the vehicle less drive force and values greater than 1.0 will produce more drive force. Most cars have between 0.10 and 0.25.");
            VehicleEditorMenu.AddMenuItem(ItemInitialDriveForce);

            ItemInitialDriveMaxFlatVel = new UIMenuItem("fInitialDriveMaxFlatVel", null, "Determines the speed at redline in top gear. Setting this value does not guarantee the vehicle will reach this speed. Multiply by 0.82 to get the speed in mph or multiply by 1.32 to get kph");
            VehicleEditorMenu.AddMenuItem(ItemInitialDriveMaxFlatVel);

            ItemBrakeForce = new UIMenuItem("fBrakeForce", null, "Multiplies the game's calculation of deceleration. Bigger number = harder braking. Range: 0.01 - 2.0 and above. 1.0 uses brake force calculation unmodified");
            VehicleEditorMenu.AddMenuItem(ItemBrakeForce);

            ItemBrakeBiasFront = new UIMenuItem("fBrakeBiasFront", null, "0.0 means the rear axle only receives brake force, 1.0 means the front axle only receives brake force. 0.5 gives both axles equal brake force.");
            VehicleEditorMenu.AddMenuItem(ItemBrakeBiasFront);

            ItemHandBrakeForce = new UIMenuItem("fHandBrakeForce", null, "Braking power for handbrake. Bigger number = harder braking");
            VehicleEditorMenu.AddMenuItem(ItemHandBrakeForce);

            ItemSteeringLock = new UIMenuItem("fSteeringLock", null, "This value is a multiplier of the game's calculation of the angle a steer wheel will turn while at full turn. Steering lock is directly related to over or understeer / turning radius.");
            VehicleEditorMenu.AddMenuItem(ItemSteeringLock);

            ItemTractionCurveMax = new UIMenuItem("fTractionCurveMax", null, "Cornering grip of the vehicle as a multiplier of the tire surface friction.");
            VehicleEditorMenu.AddMenuItem(ItemTractionCurveMax);

            ItemTractionCurveMin = new UIMenuItem("fTractionCurveMin", null, "Accelerating/braking grip of the vehicle as a multiplier of the tire surface friction.");
            VehicleEditorMenu.AddMenuItem(ItemTractionCurveMin);

            ItemTractionCurveLateral = new UIMenuItem("fTractionCurveLateral", null, "Shape of lateral traction curve (peak traction position in degrees).");
            VehicleEditorMenu.AddMenuItem(ItemTractionCurveLateral);

            ItemTractionSpringDeltaMax = new UIMenuItem("fTractionSpringDeltaMax", null, "This value denotes at what distance above the ground the car will lose traction.");
            VehicleEditorMenu.AddMenuItem(ItemTractionSpringDeltaMax);

            ItemLowSpeedTractionLossMult = new UIMenuItem("fLowSpeedTractionLossMult", null, "How much traction is reduced at low speed, 0.0 means normal traction. It affects mainly car burnout (spinning wheels when car doesn't move) when pressing gas. Decreasing value will cause less burnout, less sliding at start. However, the higher value, the more burnout car gets. Optimal is 1.0.");
            VehicleEditorMenu.AddMenuItem(ItemLowSpeedTractionLossMult);

            ItemCamberStiffness = new UIMenuItem("fCamberStiffness", null, "This value modify the grip of the car when you're drifting and you release the gas. In general, it makes your car slide more on sideways movement. More than 0 make the car sliding on the same angle you're drifting and less than 0 make your car oversteer (not recommend to use more than 0.1 / -0.1 if you don't know what you're doing). This value depend of the others, and is better to modify when your handling is finished (or quasi finished). Not recommended to modify it for grip.");
            VehicleEditorMenu.AddMenuItem(ItemCamberStiffness);

            ItemTractionBiasFront = new UIMenuItem("fTractionBiasFront", null, "Determines the distribution of traction from front to rear. Range: 0.01 - 0.99. 0.01 = only rear axle has traction. 0.99 = only front axle has traction. 0.5 = both axles have equal traction.");
            VehicleEditorMenu.AddMenuItem(ItemTractionBiasFront);

            ItemTractionLossMult = new UIMenuItem("fTractionLossMult", null, "How much is traction affected by material grip differences from 1.0. Basically it affects how much grip is changed when driving on asphalt and mud (the higher, the more grip you loose, making car less responsive and prone to sliding).");
            VehicleEditorMenu.AddMenuItem(ItemTractionLossMult);

            ItemSuspensionForce = new UIMenuItem("fSuspensionForce", null, "Affects how strong suspension is. Can help if car is easily flipped over when turning. 1 / (Force * NumWheels) = Lower limit for zero force at full extension.");
            VehicleEditorMenu.AddMenuItem(ItemSuspensionForce);

            ItemSuspensionCompDamp = new UIMenuItem("fSuspensionCompDamp", null, "Damping during strut compression. Bigger = stiffer.");
            VehicleEditorMenu.AddMenuItem(ItemSuspensionCompDamp);

            ItemSuspensionReboundDamp = new UIMenuItem("fSuspensionReboundDamp", null, "Damping during strut rebound. Bigger = stiffer.");
            VehicleEditorMenu.AddMenuItem(ItemSuspensionReboundDamp);

            ItemSuspensionUpperLimit = new UIMenuItem("fSuspensionUpperLimit", null, "How far can wheels move up from original position.");
            VehicleEditorMenu.AddMenuItem(ItemSuspensionUpperLimit);

            ItemSuspensionLowerLimit = new UIMenuItem("fSuspensionLowerLimit", null, "How far can wheels move down from original position.");
            VehicleEditorMenu.AddMenuItem(ItemSuspensionLowerLimit);

            ItemSuspensionBiasFront = new UIMenuItem("fSuspensionBiasFront", null, "This value determines which suspension is stronger, front or rear. If value is above 0.50 then front is stiffer; when below, rear.");
            VehicleEditorMenu.AddMenuItem(ItemSuspensionBiasFront);

            ItemAntiRollBarForce = new UIMenuItem("fAntiRollBarForce", null, "The spring constant that is transmitted to the opposite wheel when under compression larger numbers are a larger force. Larger Numbers = less body roll.");
            VehicleEditorMenu.AddMenuItem(ItemAntiRollBarForce);

            ItemAntiRollBarBiasFront = new UIMenuItem("fAntiRollBarBiasFront", null, "The bias between front and rear for the antiroll bar.");
            VehicleEditorMenu.AddMenuItem(ItemAntiRollBarBiasFront);

            ItemRollCentreHeightFront = new UIMenuItem("fRollCentreHeightFront", null, "This value modifies the weight transmission during an acceleration between the left and right. A negative value will make the weight more important at the opposite direction when you accelerate, and a positive will make your weight more important on the same direction you're going. A positive value is visually unrealistic. Best to do is to only go between 0.15/-0.15.");
            VehicleEditorMenu.AddMenuItem(ItemRollCentreHeightFront);

            ItemRollCentreHeightRear = new UIMenuItem("fRollCentreHeightRear", null, "This value modifies the weight transmission during an acceleration between the front and rear (and can affect the acceleration speed). A negative value will make the weight more important at the rear when you accelerate, and a positive will make your weight more important on the front. A positive value is visually unrealistic. Best to do is to only go between 0.15/-0.15.");
            VehicleEditorMenu.AddMenuItem(ItemRollCentreHeightRear);

            ItemDeleteConfig = new UIMenuItem("Delete current config", null, "There is no confirmation warning so be careful!");
            VehicleEditorMenu.AddMenuItem(ItemDeleteConfig);

            ItemSaveConfigNew = new UIMenuItem("Save as new config", null, "Let's you type in name for a new config");
            VehicleEditorMenu.AddMenuItem(ItemSaveConfigNew);

            ItemSaveConfig = new UIMenuItem("Save settings to current config");
            VehicleEditorMenu.AddMenuItem(ItemSaveConfig);

            ItemReloadConfig = new UIMenuItem("Reload settings from cfg");
            VehicleEditorMenu.AddMenuItem(ItemReloadConfig);

            ItemRefreshAllSettings = new UIMenuItem("Reload all settings, configs, etc.", null, "Use this if you added a new config to the DrivingModes folder or edited anything in said folder with a text editor.");
            MainMenu.AddMenuItem(ItemRefreshAllSettings);

            CameraSettingsMenu = new UIMenu("Camera Editor");
            _menuPool.AddSubMenu(CameraSettingsMenu, MainMenu, CameraSettingsMenu.Title);

            ItemCameraSwitch = new UIMenuItem("Current Camera", "DefaultGameplayCam");
            CameraSettingsMenu.AddMenuItem(ItemCameraSwitch);

            ItemPositionOffsetX = new UIMenuItem("Position Offset X");
            CameraSettingsMenu.AddMenuItem(ItemPositionOffsetX);

            ItemPositionOffsetY = new UIMenuItem("Position Offset Y");
            CameraSettingsMenu.AddMenuItem(ItemPositionOffsetY);

            ItemPositionOffsetZ = new UIMenuItem("Position Offset Z");
            CameraSettingsMenu.AddMenuItem(ItemPositionOffsetZ);

            ItemRotationX = new UIMenuItem("Rotation X");
            CameraSettingsMenu.AddMenuItem(ItemRotationX);

            ItemRotationY = new UIMenuItem("Rotation Y");
            CameraSettingsMenu.AddMenuItem(ItemRotationY);

            ItemRotationZ = new UIMenuItem("Rotation Z");
            CameraSettingsMenu.AddMenuItem(ItemRotationZ);

            ItemPointPositionX = new UIMenuItem("Point Position X");
            CameraSettingsMenu.AddMenuItem(ItemPointPositionX);

            ItemPointPositionY = new UIMenuItem("Point Position Y");
            CameraSettingsMenu.AddMenuItem(ItemPointPositionY);

            ItemPointPositionZ = new UIMenuItem("Point Position Z");
            CameraSettingsMenu.AddMenuItem(ItemPointPositionZ);

            ItemLeftRightAdditionalCameraOffset = new UIMenuItem("Left/Right Additional Offset", null, "Moves the first-person camera to the left or right of the vehicle depending on the direction the camera is facing and whether you are holding the Additional Offset control. This allows you to have the illusion of, for example, sticking your head out of the window when looking backwards.");
            CameraSettingsMenu.AddMenuItem(ItemLeftRightAdditionalCameraOffset);

            ItemFieldOfView = new UIMenuItem("Field of view");
            CameraSettingsMenu.AddMenuItem(ItemFieldOfView);

            ItemAutoCenter = new UIMenuItem("Auto center");
            CameraSettingsMenu.AddMenuItem(ItemAutoCenter);

            ItemCameraEnabled = new UIMenuItem("Camera Enabled");
            CameraSettingsMenu.AddMenuItem(ItemCameraEnabled);

            ItemMouseSensitivity = new UIMenuItem("Mouse Sensitivity", null, "Affects all 1st person and fixed custom cameras for all vehicles.");
            CameraSettingsMenu.AddMenuItem(ItemMouseSensitivity);

            ItemGamepadLookSensitivity = new UIMenuItem("Gamepad Look Sensitivity", null, "Affects all 1st person and fixed custom cameras for all vehicles.");
            CameraSettingsMenu.AddMenuItem(ItemGamepadLookSensitivity);

            ItemGamepadAimSensitivity = new UIMenuItem("Gamepad Aim Sensitivity", null, "Affects all 1st person and fixed custom cameras for all vehicles.");
            CameraSettingsMenu.AddMenuItem(ItemGamepadAimSensitivity);

            ItemSaveCameraSettings = new UIMenuItem("Save all camera settings");
            CameraSettingsMenu.AddMenuItem(ItemSaveCameraSettings);

            ItemReloadCameraSettings = new UIMenuItem("Reload all camera settings");
            CameraSettingsMenu.AddMenuItem(ItemReloadCameraSettings);

            SetupDisplayMenu();
        }

        void SetupDisplayMenu()
        {
            _dislayPool = new MenuPool();

            foreach (SpecialVehicles Spec in SpecialVehicles)
            {
                _dislayPool.AddMenu(Spec.ConfigDisplay);
                Spec.ConfigDisplay.TitleBackgroundColor = MainMenu.TitleBackgroundColor;
                Spec.ConfigDisplay.TitleColor = MainMenu.TitleColor;
                Spec.ConfigDisplay.DescriptionBoxColor = MainMenu.DescriptionBoxColor;
                Spec.ConfigDisplay.DescriptionTextColor = MainMenu.DescriptionTextColor;
                Spec.ConfigDisplay.HighlightedBoxColor = MainMenu.HighlightedBoxColor;
                Spec.ConfigDisplay.HighlightedItemTextColor = MainMenu.HighlightedItemTextColor;
                Spec.ConfigDisplay.DefaultBoxColor = MainMenu.DefaultBoxColor;
                Spec.ConfigDisplay.DefaultTextColor = MainMenu.DefaultTextColor;
                Spec.ConfigDisplay.TitleUnderlineColor = MainMenu.TitleUnderlineColor;

                foreach (Configs conf in Spec.Configs)
                {
                    Spec.ConfigDisplay.AddMenuItem(new UIMenuItem(conf.ConfigName));
                }
            }
        }

        Keys MenuKey;
        Keys modeSwitchKey;
        Keys CamSwitchKey;
        Control gamepadModifierButton;
        Control gamepadSwitchButton;
        Control gamepadCamSwitchButton;
        float MouseSensitivity;
        float GamepadLookSensitivity;
        float GamepadAimSensitivity;
        float LeftRightAdditionalCameraOffset;
        float LeftRightOffsetLerp = 0.5f;
        Control AdditionalOffsetControl;
        void LoadINISettings()
        {
            ScriptSettings config = ScriptSettings.Load(@"scripts\DrivingModes\Settings.ini");

            MenuKey = config.GetValue<Keys>("Keyboard Controls", "Menu Key", Keys.F10);
            modeSwitchKey = config.GetValue<Keys>("Keyboard Controls", "Mode Switch Keyboard Key", Keys.D0);
            CamSwitchKey = config.GetValue<Keys>("Keyboard Controls", "Custom Camera Key", Keys.D9);

            gamepadModifierButton = config.GetValue<GTA.Control>("Gamepad Controls", "Gamepad Hold Button", GTA.Control.VehicleSelectNextWeapon);
            gamepadSwitchButton = config.GetValue<GTA.Control>("Gamepad Controls", "Mode Switch Tap Button", GTA.Control.PhoneDown);
            gamepadCamSwitchButton = config.GetValue<GTA.Control>("Gamepad Controls", "Camera Switch Tap Button", GTA.Control.VehicleHorn);

            AdditionalOffsetControl = config.GetValue<Control>("Camera Movement Settings", "Additional Offset Control", Control.VehicleAim);
            MouseSensitivity = config.GetValue<float>("Camera Movement Settings", "Mouse Sensitivity", 700f);
            GamepadLookSensitivity = config.GetValue<float>("Camera Movement Settings", "Gamepad Look Sensitivity", 150f);
            GamepadAimSensitivity = config.GetValue<float>("Camera Movement Settings", "Gamepad Aim Sensitivity", 50f);

            if (!File.Exists(@"scripts\DrivingModes\Settings.ini"))
            {
                SaveINISettings();
            }
        }

        void SaveINISettings()
        {
            ScriptSettings config = ScriptSettings.Load(@"scripts\DrivingModes\Settings.ini");

            config.SetValue<Keys>("Keyboard Controls", "Menu Key", MenuKey);
            config.SetValue<string>("Keyboard Controls", "Concerning the menu key", "The Menu key only applies if you do not have the Script Communicator Menu mod.");
            config.SetValue<Keys>("Keyboard Controls", "Mode Switch Keyboard Key", modeSwitchKey);
            config.SetValue<Keys>("Keyboard Controls", "Custom Camera Key", CamSwitchKey);
            config.SetValue<string>("Keyboard Controls", "List of keys", "https://msdn.microsoft.com/en-us/library/system.windows.forms.keys(v=vs.110).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-1");
            config.SetValue<string>("Gamepad Controls", "How to use this mod with gamepads", "Hold your chosen HOLD button, then tap your chosen TAP button(s) to switch drive modes or toggle the custom camera, respectively");
            config.SetValue<GTA.Control>("Gamepad Controls", "Gamepad Hold Button", gamepadModifierButton);
            config.SetValue<GTA.Control>("Gamepad Controls", "Mode Switch Tap Button", gamepadSwitchButton);
            config.SetValue<GTA.Control>("Gamepad Controls", "Camera Switch Tap Button", gamepadCamSwitchButton);
            config.SetValue<string>("Gamepad Controls", "List of gamepad buttons", "https://github.com/crosire/scripthookvdotnet/blob/157ac57f9530a1cf55afa61d81a066849b52f8ba/source/scripting/Controls.hpp#L236");
            config.SetValue<Control>("Camera Movement Settings", "Additional Offset Control", AdditionalOffsetControl);
            config.SetValue<float>("Camera Movement Settings", "Mouse Sensitivity", MouseSensitivity);
            config.SetValue<float>("Camera Movement Settings", "Gamepad Look Sensitivity", GamepadLookSensitivity);
            config.SetValue<float>("Camera Movement Settings", "Gamepad Aim Sensitivity", GamepadAimSensitivity);
            config.Save();
        }

        void ReadVehicles()
        {
            List<string> VehicleList = Directory.GetDirectories(@"scripts\DrivingModes\Configs").ToList();
            foreach (string path in VehicleList)
            {
                SpecialVehicles.Add(new SpecialVehicles(path.Remove(0, path.LastIndexOf('\\') + 1), path, new List<Configs>()));
            }

            foreach(SpecialVehicles Spec in SpecialVehicles)
            {
                string[] files = Directory.GetFiles(@Spec.FullPath, "*.cfg");

                foreach (string configs in files)
                {
                    Configs temp = new Configs(Path.GetFileNameWithoutExtension(configs), configs);
                    //LoadHandlingConfig(temp);
                    Spec.Configs.Add(temp);
                }
            }

            //UI.ShowSubtitle(SpecialVehicles[0].VehicleHash.ToString() + ", " + SpecialVehicles[0].Configs[0].ConfigName, 5000);
        }

        void CreateNewConfig(string configName = "1st - Default.cfg")
        {
            string ModelHash = ((VehicleHash)Game.Player.Character.CurrentVehicle.Model.Hash).ToString();

            if (ModelHash == ((VehicleHash)GetHashKey(LastUsedVehicleName)).ToString())
            {
                ModelHash = LastUsedVehicleName;
            }

            Directory.CreateDirectory(@"scripts\DrivingModes\Configs\" + ModelHash);

            if (!File.Exists(@"scripts\DrivingModes\Configs\" + ModelHash + "\\CameraSettings.ini"))
            {
                SaveCamSettings(@"scripts\DrivingModes\Configs\" + ModelHash + "\\CameraSettings.ini");
            }

            SaveCurrentHandlingFromMemory(@"scripts\DrivingModes\Configs\" + ModelHash + "\\" + configName);
            

            SpecialVehicles.Clear();
            LastUsedSpecialVehicleIndex = 0;
            LastUsedVehicleName = "";
            LastUsedVehicleHash = "";
            ReadVehicles();
            _dislayPool.RemoveAllMenus();
            SetupDisplayMenu();
            Wait(500);
        }

        void DeleteConfig(Configs config)
        {
            if (File.Exists(config.FullPath))
            {
                File.Delete(config.FullPath);
                UI.ShowSubtitle(config.ConfigName + " Config Deleted");

                SpecialVehicles.Clear();
                LastUsedSpecialVehicleIndex = 0;
                LastUsedVehicleName = "";
                LastUsedVehicleHash = "";
                ReadVehicles();
                _dislayPool.RemoveAllMenus();
                SetupDisplayMenu();
                Wait(500);
            }
        }

        bool isVehicleSpecial(Vehicle v)
        {
            if (Game.Player.Character.IsInVehicle())
            {
                if (LastUsedVehicleHash == ((VehicleHash)Game.Player.Character.CurrentVehicle.Model.Hash).ToString())
                {
                    return true;
                }
                for (int index = 0; index < SpecialVehicles.Count; index++)
                {
                    if (Game.Player.Character.IsInVehicle())
                    {
                        if (((VehicleHash)v.Model.Hash).ToString() == SpecialVehicles[index].VehicleHash || (VehicleHash)v.Model.Hash == (VehicleHash)GetHashKey(SpecialVehicles[index].VehicleHash))
                        {
                            foreach (Configs conf in SpecialVehicles[index].Configs)
                            {
                                LoadHandlingConfig(conf);
                            }

                            LastUsedSpecialVehicleIndex = index;
                            LastUsedVehicleName = SpecialVehicles[index].VehicleHash;
                            LastUsedVehicleHash = ((VehicleHash)Game.Player.Character.CurrentVehicle.Model.Hash).ToString();
                            
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        void LoadHandlingConfig(Configs config)
        {
            forceDecimal();

            ScriptSettings setting = ScriptSettings.Load(config.FullPath);

            //The default values are taken from the Adder

            config.HandlingData._vecCentreOfMassX = setting.GetValue<float>("Handling", "vecCentreOfMassOffsetX", HandlingUtils.GetCentreOfMassX());
            config.HandlingData._vecCentreOfMassY = setting.GetValue<float>("Handling", "vecCentreOfMassOffsetY", HandlingUtils.GetCentreOfMassY());
            config.HandlingData._vecCentreOfMassZ = setting.GetValue<float>("Handling", "vecCentreOfMassOffsetZ", HandlingUtils.GetCentreOfMassZ());
            config.HandlingData._vecInertiaMultiplierX = setting.GetValue<float>("Handling", "vecInertiaMultiplierX", HandlingUtils.GetInertiaMultiplierX());
            config.HandlingData._vecInertiaMultiplierY = setting.GetValue<float>("Handling", "vecInertiaMultiplierY", HandlingUtils.GetInertiaMultiplierY());
            config.HandlingData._vecInertiaMultiplierZ = setting.GetValue<float>("Handling", "vecInertiaMultiplierZ", HandlingUtils.GetInertiaMultiplierZ());
            config.HandlingData._fDriveBiasFront = setting.GetValue<float>("Handling", "fDriveBiasFront", HandlingUtils.GetDriveBiasFront());
            config.HandlingData._nInitialGears = setting.GetValue<sbyte>("Handling", "nInitialDriveGears", (sbyte)HandlingUtils.GetnInitialDriveGears());
            config.HandlingData._fDriveInertia = setting.GetValue<float>("Handling", "fDriveInertia", HandlingUtils.GetDriveInertia());
            config.HandlingData._fClutchChangeRateScaleUpShift = setting.GetValue<float>("Handling", "fClutchChangeRateScaleUpShift", HandlingUtils.GetClutchChangeRateScaleUpShift());
            config.HandlingData._fClutchChangeRateScaleDownShift = setting.GetValue<float>("Handling", "fClutchChangeRateScaleDownShift", HandlingUtils.GetClutchChangeRateScaleDownShift());
            config.HandlingData._fInitialDriveForce = setting.GetValue<float>("Handling", "fInitialDriveForce", HandlingUtils.GetInitialDriveForce());
            config.HandlingData._fInitialDriveMaxFlatVel = setting.GetValue<float>("Handling", "fInitialDriveMaxFlatVel", HandlingUtils.GetInitialDriveMaxFlatVel());
            config.HandlingData._fBrakeForce = setting.GetValue<float>("Handling", "fBrakeForce", HandlingUtils.GetBrakeForce());
            config.HandlingData._fBrakeBiasFront = setting.GetValue<float>("Handling", "fBrakeBiasFront", HandlingUtils.GetBrakeBiasFront());
            config.HandlingData._fHandBrakeForce = setting.GetValue<float>("Handling", "fHandBrakeForce", HandlingUtils.GetHandBrakeForce());
            config.HandlingData._fSteeringLock = setting.GetValue<float>("Handling", "fSteeringLock", HandlingUtils.GetSteeringLock());
            config.HandlingData._fTractionCurveMax = setting.GetValue<float>("Handling", "fTractionCurveMax", HandlingUtils.GetTractionCurveMax());
            config.HandlingData._fTractionCurveMin = setting.GetValue<float>("Handling", "fTractionCurveMin", HandlingUtils.GetTractionCurveMin());
            config.HandlingData._fTractionCurveLateral = setting.GetValue<float>("Handling", "fTractionCurveLateral", HandlingUtils.GetTractionCurveLateral());
            config.HandlingData._fTractionSprintDeltaMax = setting.GetValue<float>("Handling", "fTractionSprintDeltaMax", HandlingUtils.GetTractionSpringDeltaMax());
            config.HandlingData._fLowSpeedTractionLossMult = setting.GetValue<float>("Handling", "fLowSpeedTractionLossMult", HandlingUtils.GetLowSpeedTractionLossMult());
            config.HandlingData._fCamberStiffness = setting.GetValue<float>("Handling", "fCamberStiffness", HandlingUtils.GetCamberStiffness());
            config.HandlingData._fTractionBiasFront = setting.GetValue<float>("Handling", "fTractionBiasFront", HandlingUtils.GetTractionBiasFront());
            config.HandlingData._fTractionLossMult = setting.GetValue<float>("Handling", "fTractionLossMult", HandlingUtils.GetTractionLossMult());
            config.HandlingData._fSuspensionForce = setting.GetValue<float>("Handling", "fSuspensionForce", HandlingUtils.GetSuspensionForce());
            config.HandlingData._fSuspensionCompDamp = setting.GetValue<float>("Handling", "fSuspensionCompDamp", HandlingUtils.GetSuspensionCompDamp());
            config.HandlingData._fSuspensionReboundDamp = setting.GetValue<float>("Handling", "fSuspensionReboundDamp", HandlingUtils.GetSuspensionReboundDamp());
            config.HandlingData._fSuspensionUpperLimit = setting.GetValue<float>("Handling", "fSuspensionUpperLimit", HandlingUtils.GetSuspensionUpperLimit());
            config.HandlingData._fSuspensionLowerLimit = setting.GetValue<float>("Handling", "fSuspensionLowerLimit", HandlingUtils.GetSuspensionLowerLimit());
            config.HandlingData._fSuspensionBiasFront = setting.GetValue<float>("Handling", "fSuspensionBiasFront", HandlingUtils.GetSuspensionBiasFront());
            config.HandlingData._fAntiRollBarForce = setting.GetValue<float>("Handling", "fAntiRollBarForce", HandlingUtils.GetAntiRollBarForce());
            config.HandlingData._fAntiRollBarBiasFront = setting.GetValue<float>("Handling", "fAntiRollBarBiasFront", HandlingUtils.GetAntiRollBarBiasFront());
            config.HandlingData._fRollCentreHeightFront = setting.GetValue<float>("Handling", "fRollCentreHeightFront", HandlingUtils.GetRollCentreHeightFront());
            config.HandlingData._fRollCentreHeightRear = setting.GetValue<float>("Handling", "fRollCentreHeightRear", HandlingUtils.GetRollCentreHeightRear());
        }

        void SaveCurrentHandlingToConfig(Configs config)
        {
            forceDecimal();
            //while (!File.Exists(path)) { Wait(500); }

            ScriptSettings setting = ScriptSettings.Load(config.FullPath);

            setting.SetValue<float>("Handling", "vecCentreOfMassOffsetX", config.HandlingData._vecCentreOfMassX);
            setting.SetValue<float>("Handling", "vecCentreOfMassOffsetY", config.HandlingData._vecCentreOfMassY);
            setting.SetValue<float>("Handling", "vecCentreOfMassOffsetZ", config.HandlingData._vecCentreOfMassZ);
            setting.SetValue<float>("Handling", "vecInertiaMultiplierX", config.HandlingData._vecInertiaMultiplierX);
            setting.SetValue<float>("Handling", "vecInertiaMultiplierY", config.HandlingData._vecInertiaMultiplierY);
            setting.SetValue<float>("Handling", "vecInertiaMultiplierZ", config.HandlingData._vecInertiaMultiplierZ);
            setting.SetValue<float>("Handling", "fDriveBiasFront", config.HandlingData._fDriveBiasFront);
            setting.SetValue<int>("Handling", "nInitialDriveGears", config.HandlingData._nInitialGears);
            setting.SetValue<float>("Handling", "fDriveInertia", config.HandlingData._fDriveInertia);
            setting.SetValue<float>("Handling", "fClutchChangeRateScaleUpShift", config.HandlingData._fClutchChangeRateScaleUpShift);
            setting.SetValue<float>("Handling", "fClutchChangeRateScaleDownShift", config.HandlingData._fClutchChangeRateScaleDownShift);
            setting.SetValue<float>("Handling", "fInitialDriveForce", config.HandlingData._fInitialDriveForce);
            setting.SetValue<float>("Handling", "fInitialDriveMaxFlatVel", config.HandlingData._fInitialDriveMaxFlatVel);
            setting.SetValue<float>("Handling", "fBrakeForce", config.HandlingData._fBrakeForce);
            setting.SetValue<float>("Handling", "fBrakeBiasFront", config.HandlingData._fBrakeBiasFront);
            setting.SetValue<float>("Handling", "fHandBrakeForce", config.HandlingData._fHandBrakeForce);
            setting.SetValue<float>("Handling", "fSteeringLock", config.HandlingData._fSteeringLock);
            setting.SetValue<float>("Handling", "fTractionCurveMax", config.HandlingData._fTractionCurveMax);
            setting.SetValue<float>("Handling", "fTractionCurveMin", config.HandlingData._fTractionCurveMin);
            setting.SetValue<float>("Handling", "fTractionCurveLateral", config.HandlingData._fTractionCurveLateral);
            setting.SetValue<float>("Handling", "fTractionSprintDeltaMax", config.HandlingData._fTractionSprintDeltaMax);
            setting.SetValue<float>("Handling", "fLowSpeedTractionLossMult", config.HandlingData._fLowSpeedTractionLossMult);
            setting.SetValue<float>("Handling", "fCamberStiffness", config.HandlingData._fCamberStiffness);
            setting.SetValue<float>("Handling", "fTractionBiasFront", config.HandlingData._fTractionBiasFront);
            setting.SetValue<float>("Handling", "fTractionLossMult", config.HandlingData._fTractionLossMult);
            setting.SetValue<float>("Handling", "fSuspensionForce", config.HandlingData._fSuspensionForce);
            setting.SetValue<float>("Handling", "fSuspensionCompDamp", config.HandlingData._fSuspensionCompDamp);
            setting.SetValue<float>("Handling", "fSuspensionReboundDamp", config.HandlingData._fSuspensionReboundDamp);
            setting.SetValue<float>("Handling", "fSuspensionUpperLimit", config.HandlingData._fSuspensionUpperLimit);
            setting.SetValue<float>("Handling", "fSuspensionLowerLimit", config.HandlingData._fSuspensionLowerLimit);
            setting.SetValue<float>("Handling", "fSuspensionBiasFront", config.HandlingData._fSuspensionBiasFront);
            setting.SetValue<float>("Handling", "fAntiRollBarForce", config.HandlingData._fAntiRollBarForce);
            setting.SetValue<float>("Handling", "fAntiRollBarBiasFront", config.HandlingData._fAntiRollBarBiasFront);
            setting.SetValue<float>("Handling", "fRollCentreHeightFront", config.HandlingData._fRollCentreHeightFront);
            setting.SetValue<float>("Handling", "fRollCentreHeightRear", config.HandlingData._fRollCentreHeightRear);

            setting.Save();
            UI.Notify("~g~ Settings saved!");
        }

        void SaveCurrentHandlingFromMemory(string path)
        {
            forceDecimal();
            //while (!File.Exists(path)) { Wait(500); }

            ScriptSettings setting = ScriptSettings.Load(path);

            setting.SetValue<float>("Handling", "vecCentreOfMassOffsetX", HandlingUtils.GetCentreOfMassX());
            setting.SetValue<float>("Handling", "vecCentreOfMassOffsetY", HandlingUtils.GetCentreOfMassY());
            setting.SetValue<float>("Handling", "vecCentreOfMassOffsetZ", HandlingUtils.GetCentreOfMassZ());
            setting.SetValue<float>("Handling", "vecInertiaMultiplierX", HandlingUtils.GetInertiaMultiplierX());
            setting.SetValue<float>("Handling", "vecInertiaMultiplierY", HandlingUtils.GetInertiaMultiplierY());
            setting.SetValue<float>("Handling", "vecInertiaMultiplierZ", HandlingUtils.GetInertiaMultiplierZ());
            setting.SetValue<float>("Handling", "fDriveBiasFront", HandlingUtils.GetDriveBiasFront());
            setting.SetValue<int>("Handling", "nInitialDriveGears", HandlingUtils.GetnInitialDriveGears());
            setting.SetValue<float>("Handling", "fDriveInertia", HandlingUtils.GetDriveInertia());
            setting.SetValue<float>("Handling", "fClutchChangeRateScaleUpShift", HandlingUtils.GetClutchChangeRateScaleUpShift());
            setting.SetValue<float>("Handling", "fClutchChangeRateScaleDownShift", HandlingUtils.GetClutchChangeRateScaleDownShift());
            setting.SetValue<float>("Handling", "fInitialDriveForce", HandlingUtils.GetInitialDriveForce());
            setting.SetValue<float>("Handling", "fInitialDriveMaxFlatVel", HandlingUtils.GetInitialDriveMaxFlatVel());
            setting.SetValue<float>("Handling", "fBrakeForce", HandlingUtils.GetBrakeForce());
            setting.SetValue<float>("Handling", "fBrakeBiasFront", HandlingUtils.GetBrakeBiasFront());
            setting.SetValue<float>("Handling", "fHandBrakeForce", HandlingUtils.GetHandBrakeForce());
            setting.SetValue<float>("Handling", "fSteeringLock", HandlingUtils.GetSteeringLock());
            setting.SetValue<float>("Handling", "fTractionCurveMax", HandlingUtils.GetTractionCurveMax());
            setting.SetValue<float>("Handling", "fTractionCurveMin", HandlingUtils.GetTractionCurveMin());
            setting.SetValue<float>("Handling", "fTractionCurveLateral", HandlingUtils.GetTractionCurveLateral());
            setting.SetValue<float>("Handling", "fTractionSprintDeltaMax", HandlingUtils.GetTractionSpringDeltaMax());
            setting.SetValue<float>("Handling", "fLowSpeedTractionLossMult", HandlingUtils.GetLowSpeedTractionLossMult());
            setting.SetValue<float>("Handling", "fCamberStiffness", HandlingUtils.GetCamberStiffness());
            setting.SetValue<float>("Handling", "fTractionBiasFront", HandlingUtils.GetTractionBiasFront());
            setting.SetValue<float>("Handling", "fTractionLossMult", HandlingUtils.GetTractionLossMult());
            setting.SetValue<float>("Handling", "fSuspensionForce", HandlingUtils.GetSuspensionForce());
            setting.SetValue<float>("Handling", "fSuspensionCompDamp", HandlingUtils.GetSuspensionCompDamp());
            setting.SetValue<float>("Handling", "fSuspensionReboundDamp", HandlingUtils.GetSuspensionReboundDamp());
            setting.SetValue<float>("Handling", "fSuspensionUpperLimit", HandlingUtils.GetSuspensionUpperLimit());
            setting.SetValue<float>("Handling", "fSuspensionLowerLimit", HandlingUtils.GetSuspensionLowerLimit());
            setting.SetValue<float>("Handling", "fSuspensionBiasFront", HandlingUtils.GetSuspensionBiasFront());
            setting.SetValue<float>("Handling", "fAntiRollBarForce", HandlingUtils.GetAntiRollBarForce());
            setting.SetValue<float>("Handling", "fAntiRollBarBiasFront", HandlingUtils.GetAntiRollBarBiasFront());
            setting.SetValue<float>("Handling", "fRollCentreHeightFront", HandlingUtils.GetRollCentreHeightFront());
            setting.SetValue<float>("Handling", "fRollCentreHeightRear", HandlingUtils.GetRollCentreHeightRear());

            setting.Save();
            UI.Notify("~g~ Settings saved!");
        }

        void LoadCameraSettings(string path)
        {
            forceDecimal();

            ScriptSettings config = ScriptSettings.Load(path);

            FirstPersonSettings.PositionOffset = new Vector3
                (
                config.GetValue<float>("First-Person Camera", "X Position", 0f),
                config.GetValue<float>("First-Person Camera", "Y Position", 0.03f),
                config.GetValue<float>("First-Person Camera", "Z Position", 0.12f)
                );

            FirstPersonSettings.Rotation = new Vector3
                (
                config.GetValue<float>("First-Person Camera", "X Rotation", -1.52f),
                config.GetValue<float>("First-Person Camera", "Y Rotation", 0f),
                config.GetValue<float>("First-Person Camera", "Z Rotation", 0f)
                );

            FirstPersonSettings.FieldOfView = config.GetValue<float>("First-Person Camera", "FOV", 50f);
            FirstPersonSettings.AutoCenterCamera = config.GetValue<bool>("First-Person Camera", "Auto-Center", true);
            FirstPersonSettings.Enabled = config.GetValue<bool>("First-Person Camera", "Enabled", true);
            LeftRightAdditionalCameraOffset = config.GetValue<float>("First-Person Camera", "Additional camera offset when looking left or right", 0.40f);

            FixedCameraSettings.PositionOffset = new Vector3
                (
                config.GetValue<float>("Bonnet Camera", "X Position", 0f),
                config.GetValue<float>("Bonnet Camera", "Y Position", 0f),
                config.GetValue<float>("Bonnet Camera", "Z Position", 0.86f)
                );

            FixedCameraSettings.Rotation = new Vector3
                (
                config.GetValue<float>("Bonnet Camera", "X Rotation", 0f),
                config.GetValue<float>("Bonnet Camera", "Y Rotation", 0f),
                config.GetValue<float>("Bonnet Camera", "Z Rotation", 0f)
                );

            FixedCameraSettings.FieldOfView = config.GetValue<float>("Bonnet Camera", "FOV", 50f);
            FixedCameraSettings.AutoCenterCamera = config.GetValue<bool>("Bonnet Camera", "Auto-Center", true);
            FixedCameraSettings.Enabled = config.GetValue<bool>("Bonnet Camera", "Enabled", true);

            ThirdPersonSettings.PositionOffset = new Vector3
                (
                config.GetValue<float>("Third-Person Camera", "X Position", 0f),
                config.GetValue<float>("Third-Person Camera", "Y Position", 0f),
                config.GetValue<float>("Third-Person Camera", "Z Position", 0f)
                );

            ThirdPersonSettings.PointPositionOffset = new Vector3
                (
                config.GetValue<float>("Third-Person Camera", "X Point", 0f),
                config.GetValue<float>("Third-Person Camera", "Y Point", 1.1f),
                config.GetValue<float>("Third-Person Camera", "Z Point", 0f)
                );

            ThirdPersonSettings.FieldOfView = config.GetValue<float>("Third-Person Camera", "FOV", 50f);
            ThirdPersonSettings.Enabled = config.GetValue<bool>("Third-Person Camera", "Enabled", true);

            if (!File.Exists(path))
            {
                SaveCamSettings(path);
            }
        }

        void SaveCamSettings(string path)
        {
            forceDecimal();

            ScriptSettings config = ScriptSettings.Load(path);

            config.SetValue<float>("First-Person Camera", "X Position", FirstPersonSettings.PositionOffset.X);
            config.SetValue<float>("First-Person Camera", "Y Position", FirstPersonSettings.PositionOffset.Y);
            config.SetValue<float>("First-Person Camera", "Z Position", FirstPersonSettings.PositionOffset.Z);

            config.SetValue<float>("First-Person Camera", "X Rotation", FirstPersonSettings.Rotation.X);
            config.SetValue<float>("First-Person Camera", "Y Rotation", FirstPersonSettings.Rotation.Y);
            config.SetValue<float>("First-Person Camera", "Z Rotation", FirstPersonSettings.Rotation.Z);

            config.SetValue<float>("First-Person Camera", "FOV", FirstPersonSettings.FieldOfView);
            config.SetValue<bool>("First-Person Camera", "Auto-Center", FirstPersonSettings.AutoCenterCamera);
            config.SetValue<bool>("First-Person Camera", "Enabled", FirstPersonSettings.Enabled);
            config.SetValue<float>("First-Person Camera", "Additional camera offset when looking left or right", LeftRightAdditionalCameraOffset);

            config.SetValue<float>("Bonnet Camera", "X Position", FixedCameraSettings.PositionOffset.X);
            config.SetValue<float>("Bonnet Camera", "Y Position", FixedCameraSettings.PositionOffset.Y);
            config.SetValue<float>("Bonnet Camera", "Z Position", FixedCameraSettings.PositionOffset.Z);

            config.SetValue<float>("Bonnet Camera", "X Rotation", FixedCameraSettings.Rotation.X);
            config.SetValue<float>("Bonnet Camera", "Y Rotation", FixedCameraSettings.Rotation.Y);
            config.SetValue<float>("Bonnet Camera", "Z Rotation", FixedCameraSettings.Rotation.Z);

            config.SetValue<float>("Bonnet Camera", "FOV", FixedCameraSettings.FieldOfView);
            config.SetValue<bool>("Bonnet Camera", "Auto-Center", FixedCameraSettings.AutoCenterCamera);
            config.SetValue<bool>("Bonnet Camera", "Enabled", FixedCameraSettings.Enabled);

            config.SetValue<float>("Third-Person Camera", "X Position", ThirdPersonSettings.PositionOffset.X);
            config.SetValue<float>("Third-Person Camera", "Y Position", ThirdPersonSettings.PositionOffset.Y);
            config.SetValue<float>("Third-Person Camera", "Z Position", ThirdPersonSettings.PositionOffset.Z);

            config.SetValue<float>("Third-Person Camera", "X Point", ThirdPersonSettings.PointPositionOffset.X);
            config.SetValue<float>("Third-Person Camera", "Y Point", ThirdPersonSettings.PointPositionOffset.Y);
            config.SetValue<float>("Third-Person Camera", "Z Point", ThirdPersonSettings.PointPositionOffset.Z);

            config.SetValue<float>("Third-Person Camera", "FOV", ThirdPersonSettings.FieldOfView);
            config.SetValue<bool>("Third-Person Camera", "Enabled", ThirdPersonSettings.Enabled);

            config.Save();
        }

        void OnTick(object sender, EventArgs e) // This is where most of your script goes
        {

            //1.0f - (GetHandlingValue(fDriveBiasRear) / 2.0f)
            /*if (Game.IsControlJustPressed(2, Control.VehicleHandbrake))
            {
                string temp = HandlingUtils.GetDriveBiasFrontExact().ToString();
                string temp2 = HandlingUtils.GetDriveBiasRearExact().ToString();
                UI.ShowSubtitle("front: " + temp + ", rear" + temp2 + ", actual: " + HandlingUtils.GetDriveBiasFront().ToString());
            }*/

            if (isVehicleSpecial(Game.Player.Character.CurrentVehicle))
            {
                if (!IsKeyboard())
                {
                    if (Game.IsControlPressed(2, gamepadModifierButton))
                    {
                        SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).ConfigDisplay.IsVisible = true;
                        if (Game.IsControlJustPressed(2, gamepadSwitchButton))
                        {
                            GoToNextConfig(SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex));
                            SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).IndexWasChanged = true;
                        }
                    }

                    if (Game.IsControlJustReleased(2, gamepadModifierButton))
                    {
                        SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).ConfigDisplay.IsVisible = false;
                        if (SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).IndexWasChanged == true)
                        {
                            SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).IndexWasChanged = false;
                            DriveModeChange(Game.Player.Character.CurrentVehicle);
                        }
                    }
                }
            }

            ManageCameras();
            
            ProcessMenus();
        }

        ScriptCommunicator DMComm = new ScriptCommunicator("DrivingModes");
        void ProcessMenus()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                _menuPool.ProcessMenus();

                _dislayPool.ProcessMenus();

                if (DMComm.IsEventTriggered())
                {
                    Wait(250);
                    if (!_menuPool.IsAnyMenuOpen())
                    {
                        _menuPool.LastUsedMenu.IsVisible = !_menuPool.LastUsedMenu.IsVisible;
                    }
                    else
                    {
                        _menuPool.CloseAllMenus();
                    }
                    DMComm.ResetEvent();
                }

                if (_menuPool.IsAnyMenuOpen())
                {
                    DMComm.BlockScriptCommunicatorModMenu();

                    if (MainMenu.IsVisible)
                    {
                        if (MainMenu.JustPressedAccept())
                        {
                            if (MainMenu.SelectedItem == ItemAddVehicleToList)
                            {
                                CreateNewConfig();
                            }

                            if (MainMenu.SelectedItem == ItemRefreshAllSettings)
                            {
                                SpecialVehicles.Clear();
                                LastUsedSpecialVehicleIndex = 0;
                                LastUsedVehicleName = "";
                                LastUsedVehicleHash = "";
                                _dislayPool.RemoveAllMenus();
                                ReadVehicles();
                                SetupDisplayMenu();
                                LoadINISettings();
                            }

                            MainMenu.SetInputWait();
                        }
                    }

                    if (VehicleEditorMenu.IsVisible)
                    {
                        if (!isVehicleSpecial(Game.Player.Character.CurrentVehicle))
                        {
                            UI.ShowSubtitle("Please add vehicle to be editable", 1);
                            return;
                        }

                        if (isVehicleSpecial(Game.Player.Character.CurrentVehicle))
                        {
                            int configCount = SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).Configs.Count();

                            UI.ShowSubtitle(configCount + " configs found, Current Folder: " + SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).VehicleHash, 1);
                            if (configCount > 0)
                            {
                                for (int i = 0; i < configCount; i++)
                                {
                                    if (SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).LastUsedIndex == i)
                                    {
                                        var conf = SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).Configs.ElementAt(i);

                                        ItemConfigSelect.Value = conf.ConfigName;

                                        conf.HandlingData.CentreOfMassX = VehicleEditorMenu.ControlFloatValue(ItemCentreOfMassX, conf.HandlingData._vecCentreOfMassX, 0.01f, 1f, 6);
                                        conf.HandlingData.CentreOfMassY = VehicleEditorMenu.ControlFloatValue(ItemCentreOfMassY, conf.HandlingData._vecCentreOfMassY, 0.01f, 1f, 6);
                                        conf.HandlingData.CentreOfMassZ = VehicleEditorMenu.ControlFloatValue(ItemCentreOfMassZ, conf.HandlingData._vecCentreOfMassZ, 0.01f, 1f, 6);
                                        conf.HandlingData.InertiaMultiplierX = VehicleEditorMenu.ControlFloatValue(ItemInertiaMultiplierX, conf.HandlingData._vecInertiaMultiplierX, 0.01f, 1f, 6);
                                        conf.HandlingData.InertiaMultiplierY = VehicleEditorMenu.ControlFloatValue(ItemInertiaMultiplierY, conf.HandlingData._vecInertiaMultiplierY, 0.01f, 1f, 6);
                                        conf.HandlingData.InertiaMultiplierZ = VehicleEditorMenu.ControlFloatValue(ItemInertiaMultiplierZ, conf.HandlingData._vecInertiaMultiplierZ, 0.01f, 1f, 6);
                                        conf.HandlingData.DriveBiasFront = VehicleEditorMenu.ControlFloatValue(ItemDriveBiasFront, conf.HandlingData._fDriveBiasFront, 0.01f, 1f, 6);
                                        conf.HandlingData.nInitialDriveGears = (sbyte)VehicleEditorMenu.ControlIntValue(ItemnInitialDriveGears, conf.HandlingData._nInitialGears, 1, 2);
                                        conf.HandlingData.DriveInertia = VehicleEditorMenu.ControlFloatValue(ItemDriveInertia, conf.HandlingData._fDriveInertia, 0.01f, 1f, 6);
                                        conf.HandlingData.ClutchChangeRateScaleUpShift = VehicleEditorMenu.ControlFloatValue(ItemClutchChangeRateScaleUpShift, conf.HandlingData._fClutchChangeRateScaleUpShift, 0.01f, 1f, 6);
                                        conf.HandlingData.ClutchChangeRateScaleDownShift = VehicleEditorMenu.ControlFloatValue(ItemClutchChangeRateScaleDownShift, conf.HandlingData._fClutchChangeRateScaleDownShift, 0.01f, 1f, 6);
                                        conf.HandlingData.InitialDriveForce = VehicleEditorMenu.ControlFloatValue(ItemInitialDriveForce, conf.HandlingData._fInitialDriveForce, 0.001f, 1f, 6);
                                        conf.HandlingData.InitialDriveMaxFlatVel = VehicleEditorMenu.ControlFloatValue(ItemInitialDriveMaxFlatVel, conf.HandlingData._fInitialDriveMaxFlatVel, 0.01f, 1f, 6);
                                        conf.HandlingData.BrakeForce = VehicleEditorMenu.ControlFloatValue(ItemBrakeForce, conf.HandlingData._fBrakeForce, 0.01f, 1f, 6);
                                        conf.HandlingData.BrakeBiasFront = VehicleEditorMenu.ControlFloatValue(ItemBrakeBiasFront, conf.HandlingData._fBrakeBiasFront, 0.01f, 1f, 6);
                                        conf.HandlingData.HandBrakeForce = VehicleEditorMenu.ControlFloatValue(ItemHandBrakeForce, conf.HandlingData._fHandBrakeForce, 0.01f, 1f, 6);
                                        conf.HandlingData.SteeringLock = VehicleEditorMenu.ControlFloatValue(ItemSteeringLock, conf.HandlingData._fSteeringLock, 0.1f, 1f, 6);
                                        conf.HandlingData.TractionCurveMax = VehicleEditorMenu.ControlFloatValue(ItemTractionCurveMax, conf.HandlingData._fTractionCurveMax, 0.01f, 1f, 6);
                                        conf.HandlingData.TractionCurveMin = VehicleEditorMenu.ControlFloatValue(ItemTractionCurveMin, conf.HandlingData._fTractionCurveMin, 0.01f, 1f, 6);
                                        conf.HandlingData.TractionCurveLateral = VehicleEditorMenu.ControlFloatValue(ItemTractionCurveLateral, conf.HandlingData._fTractionCurveLateral, 0.01f, 1f, 6);
                                        conf.HandlingData.TractionSpringDeltaMax = VehicleEditorMenu.ControlFloatValue(ItemTractionSpringDeltaMax, conf.HandlingData._fTractionSprintDeltaMax, 0.01f, 1f, 6);
                                        conf.HandlingData.LowSpeedTractionLossMult = VehicleEditorMenu.ControlFloatValue(ItemLowSpeedTractionLossMult, conf.HandlingData._fLowSpeedTractionLossMult, 0.01f, 1f, 6);
                                        conf.HandlingData.CamberStiffness = VehicleEditorMenu.ControlFloatValue(ItemCamberStiffness, conf.HandlingData._fCamberStiffness, 0.01f, 1f, 6);
                                        conf.HandlingData.TractionBiasFront = VehicleEditorMenu.ControlFloatValue(ItemTractionBiasFront, conf.HandlingData._fTractionBiasFront, 0.001f, 0.01f, 6);
                                        conf.HandlingData.TractionLossMult = VehicleEditorMenu.ControlFloatValue(ItemTractionLossMult, conf.HandlingData._fTractionLossMult, 0.01f, 1f, 6);
                                        conf.HandlingData.SuspensionForce = VehicleEditorMenu.ControlFloatValue(ItemSuspensionForce, conf.HandlingData._fSuspensionForce, 0.01f, 1f, 6);
                                        conf.HandlingData.SuspensionCompDamp = VehicleEditorMenu.ControlFloatValue(ItemSuspensionCompDamp, conf.HandlingData._fSuspensionCompDamp, 0.01f, 1f, 6);
                                        conf.HandlingData.SuspensionReboundDamp = VehicleEditorMenu.ControlFloatValue(ItemSuspensionReboundDamp, conf.HandlingData._fSuspensionReboundDamp, 0.01f, 1f, 6);
                                        conf.HandlingData.SuspensionUpperLimit = VehicleEditorMenu.ControlFloatValue(ItemSuspensionUpperLimit, conf.HandlingData._fSuspensionUpperLimit, 0.01f, 1f, 6);
                                        conf.HandlingData.SuspensionLowerLimit = VehicleEditorMenu.ControlFloatValue(ItemSuspensionLowerLimit, conf.HandlingData._fSuspensionLowerLimit, 0.01f, 1f, 6);
                                        conf.HandlingData.SuspensionBiasFront = VehicleEditorMenu.ControlFloatValue(ItemSuspensionBiasFront, conf.HandlingData._fSuspensionBiasFront, 0.01f, 1f, 6);
                                        conf.HandlingData.AntiRollBarForce = VehicleEditorMenu.ControlFloatValue(ItemAntiRollBarForce, conf.HandlingData._fAntiRollBarForce, 0.01f, 1f, 6);
                                        conf.HandlingData.AntiRollBarBiasFront = VehicleEditorMenu.ControlFloatValue(ItemAntiRollBarBiasFront, conf.HandlingData._fAntiRollBarBiasFront, 0.01f, 1f, 6);
                                        conf.HandlingData.RollCentreHeightFront = VehicleEditorMenu.ControlFloatValue(ItemRollCentreHeightFront, conf.HandlingData._fRollCentreHeightFront, 0.01f, 1f, 6);
                                        conf.HandlingData.RollCentreHeightRear = VehicleEditorMenu.ControlFloatValue(ItemRollCentreHeightRear, conf.HandlingData._fRollCentreHeightRear, 0.01f, 1f, 6);

                                        if (VehicleEditorMenu.JustPressedAccept())
                                        {
                                            if (VehicleEditorMenu.SelectedItem == ItemDeleteConfig)
                                            {
                                                DeleteConfig(conf);
                                            }
                                            if (VehicleEditorMenu.SelectedItem == ItemSaveConfigNew)
                                            {
                                                string confName = Game.GetUserInput(30);
                                                if (confName == null || confName == "") { return; }
                                                CreateNewConfig(confName + ".cfg");
                                                DriveModeChange(Game.Player.Character.CurrentVehicle);
                                            }
                                            if (VehicleEditorMenu.SelectedItem == ItemSaveConfig)
                                            {
                                                SaveCurrentHandlingToConfig(conf);
                                                DriveModeChange(Game.Player.Character.CurrentVehicle);
                                            }
                                            if (VehicleEditorMenu.SelectedItem == ItemReloadConfig)
                                            {
                                                LoadHandlingConfig(conf);
                                                DriveModeChange(Game.Player.Character.CurrentVehicle);
                                            }
                                            VehicleEditorMenu.SetInputWait();
                                        }
                                    }
                                    if (LastUsedVehicleHash != ((VehicleHash)Game.Player.Character.CurrentVehicle.Model.Hash).ToString())
                                    {
                                        return;
                                    }
                                }

                                if (VehicleEditorMenu.JustPressedAccept())
                                {
                                    if (VehicleEditorMenu.SelectedItem == ItemConfigSelect)
                                    {
                                        //Go to next config in list
                                        GoToNextConfig(SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex));
                                        DriveModeChange(Game.Player.Character.CurrentVehicle);
                                    }
                                    VehicleEditorMenu.SetInputWait();
                                }

                                if (VehicleEditorMenu.JustPressedRight())
                                {
                                    if (VehicleEditorMenu.SelectedItem == ItemConfigSelect)
                                    {
                                        //Go to next config in list
                                        GoToNextConfig(SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex));
                                        DriveModeChange(Game.Player.Character.CurrentVehicle);
                                    }
                                    VehicleEditorMenu.SetInputWait();
                                }

                                if (VehicleEditorMenu.JustPressedLeft())
                                {
                                    if (VehicleEditorMenu.SelectedItem == ItemConfigSelect)
                                    {
                                        //Go to previous config in list
                                        GoToPreviousConfig(SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex));
                                        DriveModeChange(Game.Player.Character.CurrentVehicle);
                                    }
                                    VehicleEditorMenu.SetInputWait();
                                }

                                /*if (EditorLoop.Current == false)
                                {
                                    EditorLoop.MoveNext();
                                }*/
                            }
                        }
                    }

                    if (CameraSettingsMenu.IsVisible)
                    {
                        if (CamCurrentMode == (int)CameraTypes.DefaultGameplayCam)
                        {
                            foreach (UIMenuItem item in CameraSettingsMenu.UIMenuItemList)
                            {
                                if (item != ItemCameraSwitch)
                                    item.Value = null;
                            }
                        }
                        else if (CamCurrentMode == (int)CameraTypes.FirstPersonCam)
                        {
                            FirstPersonSettings.PositionOffset = new Vector3
                                (
                                CameraSettingsMenu.ControlFloatValue(ItemPositionOffsetX, FirstPersonSettings.PositionOffset.X, 0.01f, 0.1f),
                                CameraSettingsMenu.ControlFloatValue(ItemPositionOffsetY, FirstPersonSettings.PositionOffset.Y, 0.01f, 0.1f),
                                CameraSettingsMenu.ControlFloatValue(ItemPositionOffsetZ, FirstPersonSettings.PositionOffset.Z, 0.01f, 0.1f)
                                );

                            FirstPersonSettings.Rotation = new Vector3
                                (
                                CameraSettingsMenu.ControlFloatValue(ItemRotationX, FirstPersonSettings.Rotation.X, 0.5f, 1f),
                                CameraSettingsMenu.ControlFloatValue(ItemRotationY, FirstPersonSettings.Rotation.Y, 0.5f, 1f),
                                CameraSettingsMenu.ControlFloatValue(ItemRotationZ, FirstPersonSettings.Rotation.Z, 0.5f, 1f)
                                );

                            ItemPointPositionX.Value = "Third-Person mode ONLY";
                            ItemPointPositionY.Value = "Third-Person mode ONLY";
                            ItemPointPositionZ.Value = "Third-Person mode ONLY";

                            LeftRightAdditionalCameraOffset = CameraSettingsMenu.ControlFloatValue(ItemLeftRightAdditionalCameraOffset, LeftRightAdditionalCameraOffset, 0.01f, 0.05f);
                            FirstPersonSettings.FieldOfView = CameraSettingsMenu.ControlFloatValue(ItemFieldOfView, FirstPersonSettings.FieldOfView, 1f, 5f);
                            FirstPersonSettings.AutoCenterCamera = CameraSettingsMenu.ControlBoolValue(ItemAutoCenter, FirstPersonSettings.AutoCenterCamera);
                            FirstPersonSettings.Enabled = CameraSettingsMenu.ControlBoolValue(ItemCameraEnabled, FirstPersonSettings.Enabled);
                        }
                        else if (CamCurrentMode == (int)CameraTypes.FixedCam)
                        {
                            FixedCameraSettings.PositionOffset = new Vector3
                                (
                                CameraSettingsMenu.ControlFloatValue(ItemPositionOffsetX, FixedCameraSettings.PositionOffset.X, 0.01f, 0.1f),
                                CameraSettingsMenu.ControlFloatValue(ItemPositionOffsetY, FixedCameraSettings.PositionOffset.Y, 0.01f, 0.1f),
                                CameraSettingsMenu.ControlFloatValue(ItemPositionOffsetZ, FixedCameraSettings.PositionOffset.Z, 0.01f, 0.1f)
                                );

                            FixedCameraSettings.Rotation = new Vector3
                                (
                                CameraSettingsMenu.ControlFloatValue(ItemRotationX, FixedCameraSettings.Rotation.X, 0.5f, 1f),
                                CameraSettingsMenu.ControlFloatValue(ItemRotationY, FixedCameraSettings.Rotation.Y, 0.5f, 1f),
                                CameraSettingsMenu.ControlFloatValue(ItemRotationZ, FixedCameraSettings.Rotation.Z, 0.5f, 1f)
                                );

                            ItemPointPositionX.Value = "Third-Person mode ONLY";
                            ItemPointPositionY.Value = "Third-Person mode ONLY";
                            ItemPointPositionZ.Value = "Third-Person mode ONLY";

                            ItemLeftRightAdditionalCameraOffset.Value = "First-Person mode ONLY";
                            FixedCameraSettings.FieldOfView = CameraSettingsMenu.ControlFloatValue(ItemFieldOfView, FixedCameraSettings.FieldOfView, 1f, 5f);
                            FixedCameraSettings.AutoCenterCamera = CameraSettingsMenu.ControlBoolValue(ItemAutoCenter, FixedCameraSettings.AutoCenterCamera);
                            FixedCameraSettings.Enabled = CameraSettingsMenu.ControlBoolValue(ItemCameraEnabled, FixedCameraSettings.Enabled);
                        }
                        else if (CamCurrentMode == (int)CameraTypes.ThirdPersonCam)
                        {
                            ThirdPersonSettings.PositionOffset = new Vector3
                                (
                                CameraSettingsMenu.ControlFloatValue(ItemPositionOffsetX, ThirdPersonSettings.PositionOffset.X, 0.01f, 0.1f),
                                CameraSettingsMenu.ControlFloatValue(ItemPositionOffsetY, ThirdPersonSettings.PositionOffset.Y, 0.01f, 0.1f),
                                CameraSettingsMenu.ControlFloatValue(ItemPositionOffsetZ, ThirdPersonSettings.PositionOffset.Z, 0.01f, 0.1f)
                                );

                            ItemRotationX.Value = "Not Supported";
                            ItemRotationY.Value = "Not Supported";
                            ItemRotationZ.Value = "Not Supported";

                            ThirdPersonSettings.PointPositionOffset = new Vector3
                                (
                                CameraSettingsMenu.ControlFloatValue(ItemPointPositionX, ThirdPersonSettings.PointPositionOffset.X, 0.01f, 0.1f),
                                CameraSettingsMenu.ControlFloatValue(ItemPointPositionY, ThirdPersonSettings.PointPositionOffset.Y, 0.01f, 0.1f),
                                CameraSettingsMenu.ControlFloatValue(ItemPointPositionZ, ThirdPersonSettings.PointPositionOffset.Z, 0.01f, 0.1f)
                                );

                            ItemLeftRightAdditionalCameraOffset.Value = "First-Person mode ONLY";
                            ThirdPersonSettings.FieldOfView = CameraSettingsMenu.ControlFloatValue(ItemFieldOfView, ThirdPersonSettings.FieldOfView, 1f, 5f);
                            ItemAutoCenter.Value = "Not Supported";
                            ThirdPersonSettings.Enabled = CameraSettingsMenu.ControlBoolValue(ItemCameraEnabled, ThirdPersonSettings.Enabled);
                        }

                        MouseSensitivity = CameraSettingsMenu.ControlFloatValue(ItemMouseSensitivity, MouseSensitivity, 10f, 50f, 0);
                        GamepadLookSensitivity = CameraSettingsMenu.ControlFloatValue(ItemGamepadLookSensitivity, GamepadLookSensitivity, 10f, 50f, 0);
                        GamepadAimSensitivity = CameraSettingsMenu.ControlFloatValue(ItemGamepadAimSensitivity, GamepadAimSensitivity, 10f, 50f, 0);

                        if (CameraSettingsMenu.SelectedItem == ItemCameraSwitch)
                        {
                            if (CameraSettingsMenu.JustPressedAccept())
                            {
                                SwitchCameraNow();
                                CameraSettingsMenu.SetInputWait();
                            }
                        }
                        else if (CameraSettingsMenu.SelectedItem == ItemSaveCameraSettings)
                        {
                            if (isVehicleSpecial(Game.Player.Character.CurrentVehicle))
                            {
                                ItemSaveCameraSettings.Description = "For the current vehicle only";
                                if (CameraSettingsMenu.JustPressedAccept())
                                {
                                    SaveCamSettings(@"scripts\DrivingModes\Configs\" + LastUsedVehicleName + "\\CameraSettings.ini");
                                    SaveINISettings();
                                    UI.Notify("~g~ Camera Settings Saved!");
                                    CameraSettingsMenu.SetInputWait();
                                }
                            }
                            else
                            {
                                ItemSaveCameraSettings.Description = "For all vehicles. To use specific settings for this vehicle, go to the main menu and create an initial config.";
                                if (CameraSettingsMenu.JustPressedAccept())
                                {
                                    SaveCamSettings(@"scripts\DrivingModes\GlobalCameraSettings.ini");
                                    SaveINISettings();
                                    UI.Notify("~g~ Global Camera Settings Saved!");
                                    CameraSettingsMenu.SetInputWait();
                                }
                            }
                        }
                        else if (CameraSettingsMenu.SelectedItem == ItemReloadCameraSettings)
                        {
                            if (isVehicleSpecial(Game.Player.Character.CurrentVehicle))
                            {
                                ItemReloadCameraSettings.Description = "For the current vehicle only";
                                if (CameraSettingsMenu.JustPressedAccept())
                                {
                                    LoadCameraSettings(@"scripts\DrivingModes\Configs\" + LastUsedVehicleName + "\\CameraSettings.ini");
                                    LoadINISettings();
                                    UI.Notify("~g~ Camera Settings Reloaded!");
                                    CameraSettingsMenu.SetInputWait();
                                }
                            }
                            else
                            {
                                ItemReloadCameraSettings.Description = "For all vehicles. To use specific settings for this vehicle, go to the main menu and create an initial config.";
                                if (CameraSettingsMenu.JustPressedAccept())
                                {
                                    LoadCameraSettings(@"scripts\DrivingModes\GlobalCameraSettings.ini");
                                    LoadINISettings();
                                    UI.Notify("~g~ Global Camera Settings Reloaded!");
                                    CameraSettingsMenu.SetInputWait();
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (DMComm.ScriptCommunicatorMenuIsBlocked())
                    {
                        DMComm.UnblockScriptCommunicatorModMenu();
                    }
                }
            }
            else
            {
                if (DMComm.IsEventTriggered())
                {
                    UI.ShowSubtitle("Please enter vehicle first");
                    DMComm.ResetEvent();
                }
                //_menuPool.CloseAllMenus();
            }
        }

        void GoToNextConfig(SpecialVehicles spec)
        {
            int numConfigs = spec.Configs.Count;
            if (spec.LastUsedIndex < numConfigs - 1)
            {
                spec.LastUsedIndex++;
                spec.ConfigDisplay.GoToNextItem();
            }
            else
            {
                spec.LastUsedIndex = 0;
                spec.ConfigDisplay.GoToFirstItem();
            }
        }

        void GoToPreviousConfig(SpecialVehicles spec)
        {
            int numConfigs = spec.Configs.Count;
            if (spec.LastUsedIndex > 0)
            {
                spec.LastUsedIndex--;
                spec.ConfigDisplay.GoToPreviousItem();
            }
            else
            {
                spec.LastUsedIndex = numConfigs - 1;
                spec.ConfigDisplay.GoToLastItem();
            }
        }

        void ApplyAllInConfig(Configs conf)
        {
            HandlingUtils.SetCentreOfMassX(conf.HandlingData._vecCentreOfMassX);
            HandlingUtils.SetCentreOfMassY(conf.HandlingData._vecCentreOfMassY);
            HandlingUtils.SetCentreOfMassZ(conf.HandlingData._vecCentreOfMassZ);
            HandlingUtils.SetInertiaMultiplierX(conf.HandlingData._vecInertiaMultiplierX);
            HandlingUtils.SetInertiaMultiplierY(conf.HandlingData._vecInertiaMultiplierY);
            HandlingUtils.SetInertiaMultiplierZ(conf.HandlingData._vecInertiaMultiplierZ);
            HandlingUtils.SetDriveBiasFront(conf.HandlingData._fDriveBiasFront);
            HandlingUtils.SetnInitialDriveGears(conf.HandlingData._nInitialGears);
            HandlingUtils.SetDriveInertia(conf.HandlingData._fDriveInertia);
            HandlingUtils.SetClutchChangeRateScaleUpShift(conf.HandlingData._fClutchChangeRateScaleUpShift);
            HandlingUtils.SetClutchChangeRateScaleDownShift(conf.HandlingData._fClutchChangeRateScaleDownShift);
            HandlingUtils.SetInitialDriveForce(conf.HandlingData._fInitialDriveForce);
            HandlingUtils.SetInitialDriveMaxFlatVel(conf.HandlingData._fInitialDriveMaxFlatVel);
            HandlingUtils.SetBrakeForce(conf.HandlingData._fBrakeForce);
            HandlingUtils.SetBrakeBiasFront(conf.HandlingData._fBrakeBiasFront);
            HandlingUtils.SetHandBrakeForce(conf.HandlingData._fHandBrakeForce);
            HandlingUtils.SetSteeringLock(conf.HandlingData._fSteeringLock);
            HandlingUtils.SetTractionCurveMax(conf.HandlingData._fTractionCurveMax);
            HandlingUtils.SetTractionCurveMin(conf.HandlingData._fTractionCurveMin);
            HandlingUtils.SetTractionCurveLateral(conf.HandlingData._fTractionCurveLateral);
            HandlingUtils.SetTractionSpringDeltaMax(conf.HandlingData._fTractionSprintDeltaMax);
            HandlingUtils.SetLowSpeedTractionLossMult(conf.HandlingData._fLowSpeedTractionLossMult);
            HandlingUtils.SetCamberStiffness(conf.HandlingData._fCamberStiffness);
            HandlingUtils.SetTractionBiasFront(conf.HandlingData._fTractionBiasFront);
            HandlingUtils.SetTractionLossMult(conf.HandlingData._fTractionLossMult);
            HandlingUtils.SetSuspensionForce(conf.HandlingData._fSuspensionForce);
            HandlingUtils.SetSuspensionCompDamp(conf.HandlingData._fSuspensionCompDamp);
            HandlingUtils.SetSuspensionReboundDamp(conf.HandlingData._fSuspensionReboundDamp);
            HandlingUtils.SetSuspensionUpperLimit(conf.HandlingData._fSuspensionUpperLimit);
            HandlingUtils.SetSuspensionLowerLimit(conf.HandlingData._fSuspensionLowerLimit);
            HandlingUtils.SetSuspensionBiasFront(conf.HandlingData._fSuspensionBiasFront);
            HandlingUtils.SetAntiRollBarForce(conf.HandlingData._fAntiRollBarForce);
            HandlingUtils.SetAntiRollBarBiasFront(conf.HandlingData._fAntiRollBarBiasFront);
            HandlingUtils.SetRollCentreHeightFront(conf.HandlingData._fRollCentreHeightFront);
            HandlingUtils.SetRollCentreHeightRear(conf.HandlingData._fRollCentreHeightRear);
        }

        enum CameraTypes
        {
            DefaultGameplayCam = 0,
            FirstPersonCam = 1,
            FixedCam = 2,
            ThirdPersonCam = 3
        }

        void ManageCameras()
        {
            SetupCameraOnce();
            ManageGameplayCamViewMode();
            EnableBehindAimNow();
            FPSAimingOffsets();
            ShowCam();

            if (CameraSettingsMenu.IsVisible)
            {
                if (QuickLoadCamSettings)
                {
                    if (isVehicleSpecial(Game.Player.Character.CurrentVehicle))
                    {
                        LoadCameraSettings(@"scripts\DrivingModes\Configs\" + LastUsedVehicleName + "\\CameraSettings.ini");
                        QuickLoadCamSettings = false;
                    }
                    else
                    {
                        LoadCameraSettings(@"scripts\DrivingModes\GlobalCameraSettings.ini");
                        QuickLoadCamSettings = false;
                    }
                }
            }
            else
            {
                QuickLoadCamSettings = true;
            }
        }

        void ShowCam()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                if ((CamCurrentMode == (int)CameraTypes.FirstPersonCam && FirstPersonSettings.AutoCenterCamera) || (CamCurrentMode == (int)CameraTypes.FixedCam && FixedCameraSettings.AutoCenterCamera))
                {
                    ResetUpDownLeftRightCamView();
                }

                if (Game.Player.Character.IsInVehicle())
                {
                    if (CameraSwitchPressed())
                    {
                        SwitchCameraNow(true);
                    }

                    if (CamCurrentMode > 0)
                    {
                        CamOn = true;
                    }
                    else
                    {
                        CamOn = false;
                    }

                    if (CamOn)// || CamEditor.Visible)
                    {
                        if (!GameplayCamera.IsLookingBehind)
                        {
                            if (CamCurrentMode == (int)CameraTypes.FirstPersonCam)
                            {
                                RenderFPPCam();
                            }
                            else if (CamCurrentMode == (int)CameraTypes.FixedCam)
                            {
                                RenderBonnCam();
                            }
                            else if (CamCurrentMode == (int)CameraTypes.ThirdPersonCam)
                            {
                                RenderTPPCam();
                            }
                        }
                        else
                        {
                            AimWaitTimer = Game.GameTime + 100; //To allow LookBack aiming but not all other aiming.

                            if (World.RenderingCamera == CustomVehicleCam)
                            {
                                World.RenderingCamera = null;
                            }
                        }
                    }
                    else
                    {
                        if (World.RenderingCamera == CustomVehicleCam)
                        {
                            World.RenderingCamera = null;
                        }
                    }
                }
                else
                {
                    if (World.RenderingCamera == CustomVehicleCam)
                    {
                        World.RenderingCamera = null;
                    }
                }
            }
            else
            {
                if (World.RenderingCamera == CustomVehicleCam)
                {
                    World.RenderingCamera = null;
                }
            }
        }

        void SwitchCameraNow(bool skipDisabled = false)
        {
            if (skipDisabled)
            {
                if (CamCurrentMode < 3)
                {
                    foreach (CameraSettings cam in CameraList)
                    {
                        if (CameraList.IndexOf(cam) > CamCurrentMode && cam.Enabled)
                        {
                            CamCurrentMode = CameraList.IndexOf(cam);
                            break;
                        }
                        if (CameraList.IndexOf(cam) == 3)
                        {
                            CamCurrentMode = 0;
                            break;
                        }
                    }
                }
                else
                {
                    CamCurrentMode = 0;
                }
            }
            else
            {
                if (CamCurrentMode < 3)
                {
                    CamCurrentMode++;
                }
                else
                {
                    CamCurrentMode = 0;
                }
            }

            foreach (CameraTypes mode in Enum.GetValues(typeof(CameraTypes)))
            {
                if (CamCurrentMode == (int)mode)
                {
                    ItemCameraSwitch.Value = mode.ToString();
                }
            }
            
            if (!CameraSettingsMenu.IsVisible)
            {
                if (isVehicleSpecial(Game.Player.Character.CurrentVehicle))
                {
                    LoadCameraSettings(@"scripts\DrivingModes\Configs\" + LastUsedVehicleName + "\\CameraSettings.ini");
                }
                else
                {
                    LoadCameraSettings(@"scripts\DrivingModes\GlobalCameraSettings.ini");
                }
            }

            LeftRightSum = 0f;
            UpDownSum = 0f;

            SetupInputWait();
        }

        void SetupCameraOnce()
        {
            if (CustomVehicleCam != null)
            {
                if (!CustomVehicleCam.Exists())
                {
                    CustomVehicleCam = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, GameplayCamera.FieldOfView);
                }
            }
            else
            {
                CustomVehicleCam = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, GameplayCamera.FieldOfView);
            }
        }

        void ManageGameplayCamViewMode()
        {
            if ((CamOn && !Game.Player.Character.IsInVehicle())/* || CameraSwitchPressed()*/)
            {
                Function.Call<int>(Hash.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE, lastVehicleViewMode);
                Function.Call<int>(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, lastPedViewMode);
                Game.Player.Character.Task.AchieveHeading(Game.Player.Character.LastVehicle.Heading, 500);
                CamOn = false;
            }

            if (CamOn && World.RenderingCamera == CustomVehicleCam)
            {
                int viewMode = Function.Call<int>(Hash.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE);

                if (viewMode != 0)
                {
                    Function.Call<int>(Hash.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE, 0);
                }

                Game.DisableControlThisFrame(2, GTA.Control.NextCamera);
            }
            else
            {
                lastVehicleViewMode = Function.Call<int>(Hash.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE);
                lastPedViewMode = Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE);
            }
        }

        void EnableBehindAimNow()
        {
            if (Game.GameTime < AimWaitTimer)
            {
                if (Game.IsControlPressed(2, GTA.Control.VehicleAim))
                {
                    LookBackAim = true;
                }
                else
                {
                    LookBackAim = false;
                }
            }
            else
            {
                if (Game.IsControlJustReleased(2, GTA.Control.VehicleAim))
                {
                    LookBackAim = false;
                }
            }
        }

        void RenderFPPCam() //First Person Perspective Camera
        {
            if (!LookBackAim)
            {
                World.RenderingCamera = CustomVehicleCam;

                CustomVehicleCam.FieldOfView = FirstPersonSettings.FieldOfView;
                Function.Call(Hash.SET_CAM_AFFECTS_AIMING, CustomVehicleCam, true);

                CustomVehicleCam.StopPointing();

                Vector3 FPCamOffset = Game.Player.Character.GetOffsetFromWorldCoords(Game.Player.Character.GetBoneCoord(Bone.IK_Head));

                CustomVehicleCam.AttachTo(Game.Player.Character, FPCamOffset + FirstPersonSettings.PositionOffset + new Vector3(MathHelper.Lerp(-LeftRightAdditionalCameraOffset, LeftRightAdditionalCameraOffset, LeftRightOffsetLerp), 0f, 0f));

                Vector3 rotation = FirstPersonSettings.Rotation;

                if (vehNoRoll())
                {
                    CustomVehicleCam.Rotation = new Vector3(Game.Player.Character.CurrentVehicle.Rotation.X + rotation.X + FPLookUpDownValue(), rotation.Y, Game.Player.Character.CurrentVehicle.Rotation.Z + rotation.Z + FPLookLeftRightValue());
                }
                else
                {
                    CustomVehicleCam.Rotation = new Vector3(Game.Player.Character.CurrentVehicle.Rotation.X + rotation.X + FPLookUpDownValue(), Game.Player.Character.CurrentVehicle.Rotation.Y + rotation.Y, Game.Player.Character.CurrentVehicle.Rotation.Z + rotation.Z + FPLookLeftRightValue());
                }

                if (Game.IsControlPressed(2, GTA.Control.VehicleAim))
                {
                    Function.Call(Hash.SHOW_HUD_COMPONENT_THIS_FRAME, 14);
                }
            }
            else
            {
                if (World.RenderingCamera == CustomVehicleCam)
                {
                    World.RenderingCamera = null;
                }
            }
        }

        void RenderBonnCam() //Bonnet Perspective Camera
        {
            if (!LookBackAim)
            {
                World.RenderingCamera = CustomVehicleCam;

                CustomVehicleCam.StopPointing();
                CustomVehicleCam.Detach();

                CustomVehicleCam.FieldOfView = FixedCameraSettings.FieldOfView;
                Function.Call(Hash.SET_CAM_AFFECTS_AIMING, CustomVehicleCam, true);

                Vector3 position = FixedCameraSettings.PositionOffset;
                Vector3 rotation = FixedCameraSettings.Rotation;
                Vehicle vehicle = Game.Player.Character.CurrentVehicle;

                CustomVehicleCam.Position = vehicle.GetOffsetInWorldCoords(new Vector3(0, vehicle.Model.GetDimensions().Y / 2, vehicle.Model.GetDimensions().Z * -0.3f)) + vehicle.ForwardVector * position.X + vehicle.RightVector * position.Y + vehicle.UpVector * position.Z;
                CustomVehicleCam.Rotation = new Vector3(vehicle.Rotation.X + rotation.X + FPLookUpDownValue(), vehicle.Rotation.Y + rotation.Y, vehicle.Rotation.Z + rotation.Z + FPLookLeftRightValue());

                if (Game.IsControlPressed(2, GTA.Control.VehicleAim))
                {
                    Function.Call(Hash.SHOW_HUD_COMPONENT_THIS_FRAME, 14);
                }
            }
            else
            {
                if (World.RenderingCamera == CustomVehicleCam)
                {
                    World.RenderingCamera = null;
                }
            }
        }

        void RenderTPPCam() //Third Person Perspective Camera //Completely by LeeC2202! Thanks!!
        {
            if (!Game.IsControlPressed(2, GTA.Control.VehicleAim))
            {
                World.RenderingCamera = CustomVehicleCam;

                CustomVehicleCam.FieldOfView = ThirdPersonSettings.FieldOfView;

                Vector3 TPCamOffset = Game.Player.Character.GetOffsetFromWorldCoords(GameplayCamera.Position);

                CustomVehicleCam.AttachTo(Game.Player.Character, TPCamOffset + ThirdPersonSettings.PositionOffset);

                CustomVehicleCam.PointAt(Game.Player.Character.CurrentVehicle, ThirdPersonSettings.PointPositionOffset);
            }
            else
            {
                if (World.RenderingCamera == CustomVehicleCam)
                {
                    World.RenderingCamera = null;
                }
            }
        }

        float FPLookUpDownValue()
        {
            float UpDown = Game.GetControlNormal(2, GTA.Control.LookUpDown);

            if (UpDown > 0)
            {
                //if (UpDownSum > -100f)
                //{
                    if (IsKeyboard())
                    {
                        UpDownSum -= (UpDown * (MouseSensitivity * Game.LastFrameTime));
                    }
                    else
                    {
                        if (Game.IsControlPressed(2, GTA.Control.VehicleAim))
                        {
                            UpDownSum -= (UpDown * GamepadAimSensitivity * Game.LastFrameTime);
                        }
                        else
                        {
                            UpDownSum -= (UpDown * GamepadLookSensitivity * Game.LastFrameTime);
                        }
                    }

                if (UpDown <= -360f) { UpDown = 0; }
                //}
                return UpDownSum;
            }
            else
            {
                //if (UpDownSum < 100f)
                //{
                    if (IsKeyboard())
                    {
                        UpDownSum -= (UpDown * (MouseSensitivity * Game.LastFrameTime));
                    }
                    else
                    {
                        if (Game.IsControlPressed(2, GTA.Control.VehicleAim))
                        {
                            UpDownSum -= (UpDown * GamepadAimSensitivity * Game.LastFrameTime);
                        }
                        else
                        {
                            UpDownSum -= (UpDown * GamepadLookSensitivity * Game.LastFrameTime);
                        }
                }
                if (UpDown >= 360f) { UpDown = 0; }
                //}
                return UpDownSum;
            }
        }

        float FPLookLeftRightValue()
        {
            float LeftRight = Game.GetControlNormal(2, GTA.Control.LookLeftRight);

            if (LeftRight > 0)
            {
                //if (LeftRightSum > -180f)
                //{
                    if (IsKeyboard())
                    {
                        LeftRightSum -= (LeftRight * MouseSensitivity * Game.LastFrameTime);
                    }
                    else
                    {
                        if (Game.IsControlPressed(2, GTA.Control.VehicleAim))
                        {
                            LeftRightSum -= (LeftRight * GamepadAimSensitivity * Game.LastFrameTime);
                        }
                        else
                        {
                            LeftRightSum -= (LeftRight * GamepadLookSensitivity * Game.LastFrameTime);
                        }
                    }
                    if (LeftRightSum <= -360f) { LeftRightSum = 0; }
                //}
                return LeftRightSum;
            }
            else
            {
                //if (LeftRightSum < 180f)
                //{
                    if (IsKeyboard())
                    {
                        LeftRightSum -= (LeftRight * (MouseSensitivity * Game.LastFrameTime));
                    }
                    else
                    {
                        if (Game.IsControlPressed(2, GTA.Control.VehicleAim))
                        {
                            LeftRightSum -= (LeftRight * GamepadAimSensitivity * Game.LastFrameTime);
                        }
                        else
                        {
                            LeftRightSum -= (LeftRight * GamepadLookSensitivity * Game.LastFrameTime);
                        }
                    }
                if (LeftRightSum >= 360f) { LeftRightSum = 0; }
                //}
                return LeftRightSum;
            }
        }

        void FPSAimingOffsets()
        {
            if (Game.Player.Character.IsInVehicle() && CamCurrentMode == (int)CameraTypes.FirstPersonCam && World.RenderingCamera == CustomVehicleCam)
            {
                //float camHeading = MathHelper.DirectionToHeading(CustomVehicleCam.Direction);
                ////float adjustedCamHeading = (camHeading < 0 ? camHeading + 180.0f : camHeading - 180.0f);
                //float adjustedCamHeading = MathHelper.CalculatePosition((camHeading < 0 ? camHeading + 180.0f : camHeading - 180.0f), -180.0f, 180.0f, 0.0f, 360.0f);
                ////float adjustedVehicleHeading = MathHelper.CalculatePosition(Game.Player.Character.CurrentVehicle.Heading, 0.0f, 360.0f, -180.0f, 180.0f);

                ////UI.ShowSubtitle("Cam Heading: " + adjustedCamHeading + ", Vehicle heading: " + Game.Player.Character.CurrentVehicle.Heading);
                //UI.ShowSubtitle("Difference: " + (Game.Player.Character.CurrentVehicle.Heading - adjustedCamHeading));

                //CameraDirectionRelativeToVehicle();

                /*string LeftRightRotation = LeftRightSum.ToString();
                UI.ShowSubtitle(LeftRightRotation);*/

                if (Game.IsControlPressed(2, AdditionalOffsetControl))
                {
                    if (LeftRightSum >= 90f)
                    {
                        if (LeftRightOffsetLerp > 0f)
                        {
                            LeftRightOffsetLerp -= 1.25f * Game.LastFrameTime;
                        }
                    }
                    else if (LeftRightSum > -90f)
                    {
                        bool SeatedOnLeft = Game.Player.Character.SeatIndex == VehicleSeat.Driver || Game.Player.Character.SeatIndex == VehicleSeat.LeftRear;
                        if (LeftRightSum >= 20f && SeatedOnLeft)
                        {
                            if (LeftRightOffsetLerp > 0f)
                            {
                                LeftRightOffsetLerp -= 1.25f * Game.LastFrameTime;
                            }
                        }
                        else
                        {
                            if (LeftRightOffsetLerp <= 0.52f && LeftRightOffsetLerp >= 0.48f)
                            {
                                LeftRightOffsetLerp = 0.5f;
                            }
                            else if (LeftRightOffsetLerp > 0.5f)
                            {
                                LeftRightOffsetLerp -= 1.25f * Game.LastFrameTime;
                            }
                            else if (LeftRightOffsetLerp < 0.5f)
                            {
                                LeftRightOffsetLerp += 1.25f * Game.LastFrameTime;
                            }
                        }
                    }
                    else if (LeftRightSum <= -90f)
                    {
                        if (LeftRightOffsetLerp < 1f)
                        {
                            LeftRightOffsetLerp += 1.25f * Game.LastFrameTime;
                        }
                    }
                }
                else
                {
                    if (LeftRightOffsetLerp <= 0.52f && LeftRightOffsetLerp >= 0.48f)
                    {
                        LeftRightOffsetLerp = 0.5f;
                    }
                    else if (LeftRightOffsetLerp > 0.5f)
                    {
                        LeftRightOffsetLerp -= 1.25f * Game.LastFrameTime;
                    }
                    else if (LeftRightOffsetLerp < 0.5f)
                    {
                        LeftRightOffsetLerp += 1.25f * Game.LastFrameTime;
                    }
                }
            }
        }

        void ResetUpDownLeftRightCamView()
        {
            if (Game.GetControlNormal(2, GTA.Control.LookUpDown) != 0 || Game.GetControlNormal(2, GTA.Control.LookLeftRight) != 0 || Game.IsControlPressed(2, GTA.Control.VehicleAim))
            {
                CameraAdjustTimer = Game.GameTime + 500;
            }

            if (Game.GameTime > CameraAdjustTimer)
            {
                if (Game.Player.Character.CurrentVehicle.Speed >= 5 * 0.44704f)
                {
                    if (LeftRightSum > 0f)
                    {
                        if (LeftRightSum < 1.5f)
                        {
                            LeftRightSum -= (2.5f * Game.LastFrameTime);
                        }
                        else if (LeftRightSum < 180f)
                        {
                            LeftRightSum -= (100f * Game.LastFrameTime);
                        }
                        else
                        {
                            LeftRightSum -= (300f * Game.LastFrameTime);
                        }
                    }
                    if (LeftRightSum < 0f)
                    {
                        if (LeftRightSum > -1.5f)
                        {
                            LeftRightSum += (2.5f * Game.LastFrameTime);
                        }
                        else if (LeftRightSum > -180f)
                        {
                            LeftRightSum += (100f * Game.LastFrameTime);
                        }
                        else
                        {
                            LeftRightSum += (300f * Game.LastFrameTime);
                        }
                    }
                    if (UpDownSum > 0f)
                    {
                        if (UpDownSum < 0.5f)
                        {
                            UpDownSum -= (2.5f * Game.LastFrameTime);
                        }
                        else
                        {
                            UpDownSum -= (75f * Game.LastFrameTime);
                        }
                    }
                    if (UpDownSum < 0f)
                    {
                        if (UpDownSum > -0.5f)
                        {
                            UpDownSum += (2.5f * Game.LastFrameTime);
                        }
                        else
                        {
                            UpDownSum += (75f * Game.LastFrameTime);
                        }
                    }
                }
            }
        }

        bool vehNoRoll()
        {
            if (Game.Player.Character.CurrentVehicle.Model.IsBicycle) { return true; }
            if (Game.Player.Character.CurrentVehicle.Model.IsBike) { return true; }
            if (Game.Player.Character.CurrentVehicle.Model.IsQuadbike) { return true; }
            return false;
        }

        void forceRespawnVeh(Vehicle recentVeh)
        {
            Vector3 position = recentVeh.Position;
            Vector3 velocity = Function.Call<Vector3>(Hash.GET_ENTITY_VELOCITY, recentVeh);
            Vector3 rotationVelocity = Function.Call<Vector3>(Hash.GET_ENTITY_ROTATION_VELOCITY, recentVeh);
            Vector3 rotation = Function.Call<Vector3>(Hash.GET_ENTITY_ROTATION, 2);
            Vector3 fwdVector = recentVeh.ForwardVector;
            float heading = recentVeh.Heading;
            bool engineState = recentVeh.EngineRunning;
            float speed = recentVeh.Speed;
            float rpm = recentVeh.CurrentRPM;

            float lastBodyHealth = recentVeh.BodyHealth;
            float lastEngHealth = recentVeh.EngineHealth;
            float lastTankHealth = recentVeh.PetrolTankHealth;
            int lastHealth = recentVeh.Health;
            float dirtLevel = recentVeh.DirtLevel;
            float fuelLevel = recentVeh.FuelLevel;

            int radio = Function.Call<int>(Hash.GET_PLAYER_RADIO_STATION_INDEX);

            VehicleColor pColor = recentVeh.PrimaryColor;
            VehicleColor sColor = recentVeh.SecondaryColor;
            VehicleColor dashColor = recentVeh.DashboardColor;
            Color neonColor = recentVeh.NeonLightsColor;
            VehicleColor pearlColor = recentVeh.PearlescentColor;
            VehicleColor rimColor = recentVeh.RimColor;
            Color smokeColor = recentVeh.TireSmokeColor;
            VehicleColor trimColor = recentVeh.TrimColor;
            bool BackNeonOn = recentVeh.IsNeonLightsOn(VehicleNeonLight.Back);
            bool FrontNeonOn = recentVeh.IsNeonLightsOn(VehicleNeonLight.Front);
            bool LeftNeonOn = recentVeh.IsNeonLightsOn(VehicleNeonLight.Left);
            bool RightNeonOn = recentVeh.IsNeonLightsOn(VehicleNeonLight.Right);

            string plate = recentVeh.NumberPlate;
            NumberPlateType plateType = recentVeh.NumberPlateType;
            VehicleWindowTint winTint = recentVeh.WindowTint;
            VehicleWheelType wheelType = recentVeh.WheelType;
            VehicleRoofState roofState = recentVeh.RoofState;
            bool turboState = Function.Call<bool>(Hash.IS_TOGGLE_MOD_ON, recentVeh, 18);
            bool smokeState = Function.Call<bool>(Hash.IS_TOGGLE_MOD_ON, recentVeh, 20);
            bool xenonState = Function.Call<bool>(Hash.IS_TOGGLE_MOD_ON, recentVeh, 22);
            bool bulletproofTiresState = recentVeh.CanTiresBurst;
            bool customTires = Function.Call<bool>(Hash.GET_VEHICLE_MOD_VARIATION, recentVeh, 23);
            int frontWheelIndex = recentVeh.GetMod(VehicleMod.FrontWheels);
            bool customTiresBikes = Function.Call<bool>(Hash.GET_VEHICLE_MOD_VARIATION, recentVeh, 24);
            int bikeWheelIndex = recentVeh.GetMod(VehicleMod.BackWheels);
            bool extra1 = ExtraExist(recentVeh, 1);
            bool extra2 = ExtraExist(recentVeh, 2);
            bool extra3 = ExtraExist(recentVeh, 3);
            bool extra4 = ExtraExist(recentVeh, 4);
            bool extra5 = ExtraExist(recentVeh, 5);
            bool extra6 = ExtraExist(recentVeh, 6);
            bool extra7 = ExtraExist(recentVeh, 7);
            bool extra8 = ExtraExist(recentVeh, 8);
            bool extra9 = ExtraExist(recentVeh, 9);

            VehicleSeat pSeat = Game.Player.Character.SeatIndex;
            Ped Driver = recentVeh.GetPedOnSeat(VehicleSeat.Driver);
            Ped ExtraSeat1 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat1);
            Ped ExtraSeat10 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat10);
            Ped ExtraSeat11 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat11);
            Ped ExtraSeat12 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat12);
            Ped ExtraSeat2 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat2);
            Ped ExtraSeat3 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat3);
            Ped ExtraSeat4 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat4);
            Ped ExtraSeat5 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat5);
            Ped ExtraSeat6 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat6);
            Ped ExtraSeat7 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat7);
            Ped ExtraSeat8 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat8);
            Ped ExtraSeat9 = recentVeh.GetPedOnSeat(VehicleSeat.ExtraSeat9);
            Ped None = recentVeh.GetPedOnSeat(VehicleSeat.None);
            Ped Passenger = recentVeh.GetPedOnSeat(VehicleSeat.Passenger);
            Ped LeftFront = recentVeh.GetPedOnSeat(VehicleSeat.LeftFront);
            Ped LeftRear = recentVeh.GetPedOnSeat(VehicleSeat.LeftRear);
            Ped RightFront = recentVeh.GetPedOnSeat(VehicleSeat.RightFront);
            Ped RightRear = recentVeh.GetPedOnSeat(VehicleSeat.RightRear);

            modIndexes.Clear();

            foreach (VehicleMod modType in modTypes)
            {
                recentVeh.InstallModKit();
                modIndexes.Add(recentVeh.GetMod(modType));
            }

            recentVeh.HasCollision = false;
            Vehicle newVeh = World.CreateVehicle(recentVeh.Model, position, heading);

            newVeh.EngineRunning = engineState;
            //Function.Call(Hash.SET_ENTITY_ROTATION, newVeh, rotation.X, rotation.Y, rotation.Z, 2, true);
            newVeh.Speed = speed;
            Function.Call(Hash.SET_ENTITY_VELOCITY, newVeh, velocity.X, velocity.Y, velocity.Z);
            Function.Call(Hash.APPLY_FORCE_TO_ENTITY, newVeh, 5, rotationVelocity.X * fwdVector.X, rotationVelocity.Y * fwdVector.Y, rotationVelocity.Z * fwdVector.Z, rotation.X * fwdVector.X, rotation.Y * fwdVector.Y, rotation.Z * fwdVector.Z, 2, true, false, true, false, false);

            Game.Player.Character.SetIntoVehicle(newVeh, pSeat);
            if (Driver.Exists() && Driver != Game.Player.Character) { Driver.SetIntoVehicle(newVeh, VehicleSeat.Driver); }
            if (ExtraSeat1.Exists() && ExtraSeat1 != Game.Player.Character) { ExtraSeat1.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat1); }
            if (ExtraSeat10.Exists() && ExtraSeat10 != Game.Player.Character) { ExtraSeat10.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat10); }
            if (ExtraSeat11.Exists() && ExtraSeat11 != Game.Player.Character) { ExtraSeat11.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat11); }
            if (ExtraSeat12.Exists() && ExtraSeat12 != Game.Player.Character) { ExtraSeat12.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat12); }
            if (ExtraSeat2.Exists() && ExtraSeat2 != Game.Player.Character) { ExtraSeat2.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat2); }
            if (ExtraSeat3.Exists() && ExtraSeat3 != Game.Player.Character) { ExtraSeat3.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat3); }
            if (ExtraSeat4.Exists() && ExtraSeat4 != Game.Player.Character) { ExtraSeat4.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat4); }
            if (ExtraSeat5.Exists() && ExtraSeat5 != Game.Player.Character) { ExtraSeat5.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat5); }
            if (ExtraSeat6.Exists() && ExtraSeat6 != Game.Player.Character) { ExtraSeat6.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat6); }
            if (ExtraSeat7.Exists() && ExtraSeat7 != Game.Player.Character) { ExtraSeat7.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat7); }
            if (ExtraSeat8.Exists() && ExtraSeat8 != Game.Player.Character) { ExtraSeat8.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat8); }
            if (ExtraSeat9.Exists() && ExtraSeat9 != Game.Player.Character) { ExtraSeat9.SetIntoVehicle(newVeh, VehicleSeat.ExtraSeat9); }
            if (None.Exists() && None != Game.Player.Character) { None.SetIntoVehicle(newVeh, VehicleSeat.None); }
            if (Passenger.Exists() && Passenger != Game.Player.Character) { Passenger.SetIntoVehicle(newVeh, VehicleSeat.Passenger); }
            if (LeftFront.Exists() && LeftFront != Game.Player.Character) { LeftFront.SetIntoVehicle(newVeh, VehicleSeat.LeftFront); }
            if (LeftRear.Exists() && LeftRear != Game.Player.Character) { LeftRear.SetIntoVehicle(newVeh, VehicleSeat.LeftRear); }
            if (RightFront.Exists() && RightFront != Game.Player.Character) { RightFront.SetIntoVehicle(newVeh, VehicleSeat.RightFront); }
            if (RightRear.Exists() && RightRear != Game.Player.Character) { RightRear.SetIntoVehicle(newVeh, VehicleSeat.RightRear); }

            recentVeh.Delete();

            if (radio != 255) //255 is "Off"
            {
                Function.Call(Hash.SET_RADIO_TO_STATION_INDEX, radio);
            }
            else
            {
                Function.Call(Hash.SET_VEH_RADIO_STATION, newVeh, "OFF");
            }

            newVeh.CurrentRPM = rpm;
            newVeh.NumberPlate = plate;
            newVeh.NumberPlateType = plateType;
            newVeh.WindowTint = winTint;
            newVeh.WheelType = wheelType;
            newVeh.RoofState = roofState;
            Function.Call(Hash.TOGGLE_VEHICLE_MOD, newVeh, 18, turboState);
            Function.Call(Hash.TOGGLE_VEHICLE_MOD, newVeh, 20, smokeState);
            Function.Call(Hash.TOGGLE_VEHICLE_MOD, newVeh, 22, xenonState);
            newVeh.CanTiresBurst = bulletproofTiresState;

            if (modIndexes.Count > 0)
            {
                foreach (VehicleMod mType in modTypes)
                {
                    newVeh.InstallModKit();
                    newVeh.SetMod(mType, modIndexes[0], true);
                    modIndexes.RemoveAt(0);
                    //UI.ShowSubtitle("SETTING MODS");
                }
            }

            Function.Call(Hash.SET_VEHICLE_MOD, newVeh, 23, frontWheelIndex, customTires);
            Function.Call(Hash.SET_VEHICLE_MOD, newVeh, 24, bikeWheelIndex, customTiresBikes);
            newVeh.ToggleExtra(1, extra1);
            newVeh.ToggleExtra(2, extra2);
            newVeh.ToggleExtra(3, extra3);
            newVeh.ToggleExtra(4, extra4);
            newVeh.ToggleExtra(5, extra5);
            newVeh.ToggleExtra(6, extra6);
            newVeh.ToggleExtra(7, extra7);
            newVeh.ToggleExtra(8, extra8);
            newVeh.ToggleExtra(9, extra9);

            newVeh.PrimaryColor = pColor;
            newVeh.SecondaryColor = sColor;
            newVeh.DashboardColor = dashColor;
            newVeh.NeonLightsColor = neonColor;
            newVeh.PearlescentColor = pearlColor;
            newVeh.RimColor = rimColor;
            newVeh.TireSmokeColor = smokeColor;
            newVeh.TrimColor = trimColor;
            newVeh.SetNeonLightsOn(VehicleNeonLight.Back, BackNeonOn);
            newVeh.SetNeonLightsOn(VehicleNeonLight.Front, FrontNeonOn);
            newVeh.SetNeonLightsOn(VehicleNeonLight.Left, LeftNeonOn);
            newVeh.SetNeonLightsOn(VehicleNeonLight.Right, RightNeonOn);

            newVeh.BodyHealth = lastBodyHealth;
            newVeh.EngineHealth = lastEngHealth;
            newVeh.PetrolTankHealth = lastTankHealth;
            newVeh.Health = lastHealth;
            newVeh.DirtLevel = dirtLevel;
            newVeh.FuelLevel = fuelLevel;
        }

        bool ExtraExist(Vehicle v, int i)
        {
            if (v.ExtraExists(i))
            { return v.IsExtraOn(i); }
            else { return false; }
        }

        int GetHashKey(string thing)
        {
            return Function.Call<int>(Hash.GET_HASH_KEY, thing);
        }

        bool CameraSwitchPressed()
        {
            if (IsKeyboard())
            {
                if (Game.IsKeyPressed(CamSwitchKey))
                {
                    if (IsAllowedInput())
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (Game.IsControlPressed(2, gamepadModifierButton))
                {
                    Game.DisableControlThisFrame(2, gamepadCamSwitchButton);
                    if (Game.IsControlJustPressed(2, gamepadCamSwitchButton))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool IsKeyboard()
        {
            return Game.CurrentInputMode == InputMode.MouseAndKeyboard;
        }

        bool IsAllowedInput()
        {
            return Game.GameTime > InputTimer;
        }

        void SetupInputWait(int milliseconds = 100)
        {
            InputTimer = Game.GameTime + milliseconds;
        }

        /*IEnumerator<bool> EditConfig()
        {
            for (int i = 0; i < LastUsedSpecialVehicle.Configs.Count(); ++i)
            {
                if (LastUsedSpecialVehicle.LastUsedIndex == i)
                {
                    var conf = LastUsedSpecialVehicle.Configs.ElementAt(i);

                    conf.HandlingData.DriveBiasFront = VehicleEditorMenu.ControlFloatValue(ItemDriveBiasFront, conf.HandlingData._fDriveBiasFront, 0.01f, 1f, 6);
                    conf.HandlingData.DriveInertia = VehicleEditorMenu.ControlFloatValue(ItemDriveInertia, conf.HandlingData._fDriveInertia, 0.01f, 1f, 6);
                    conf.HandlingData.ClutchChangeRateScaleUpShift = VehicleEditorMenu.ControlFloatValue(ItemClutchChangeRateScaleUpShift, conf.HandlingData._fClutchChangeRateScaleUpShift, 0.01f, 1f, 6);
                    conf.HandlingData.ClutchChangeRateScaleDownShift = VehicleEditorMenu.ControlFloatValue(ItemClutchChangeRateScaleDownShift, conf.HandlingData._fClutchChangeRateScaleDownShift, 0.01f, 1f, 6);
                    conf.HandlingData.InitialDriveForce = VehicleEditorMenu.ControlFloatValue(ItemInitialDriveForce, conf.HandlingData._fInitialDriveForce, 0.01f, 1f, 6);
                    conf.HandlingData.InitialDriveMaxFlatVel = VehicleEditorMenu.ControlFloatValue(ItemInitialDriveMaxFlatVel, conf.HandlingData._fInitialDriveMaxFlatVel, 0.01f, 1f, 6);
                    conf.HandlingData.BrakeForce = VehicleEditorMenu.ControlFloatValue(ItemBrakeForce, conf.HandlingData._fBrakeForce, 0.01f, 1f, 6);
                    conf.HandlingData.BrakeBiasFront = VehicleEditorMenu.ControlFloatValue(ItemBrakeBiasFront, conf.HandlingData._fBrakeBiasFront, 0.01f, 1f, 6);
                    conf.HandlingData.HandBrakeForce = VehicleEditorMenu.ControlFloatValue(ItemHandBrakeForce, conf.HandlingData._fHandBrakeForce, 0.01f, 1f, 6);
                    conf.HandlingData.SteeringLock = VehicleEditorMenu.ControlFloatValue(ItemSteeringLock, conf.HandlingData._fSteeringLock, 0.01f, 1f, 6);
                    conf.HandlingData.TractionCurveMax = VehicleEditorMenu.ControlFloatValue(ItemTractionCurveMax, conf.HandlingData._fTractionCurveMax, 0.01f, 1f, 6);
                    conf.HandlingData.TractionCurveMin = VehicleEditorMenu.ControlFloatValue(ItemTractionCurveMin, conf.HandlingData._fTractionCurveMin, 0.01f, 1f, 6);
                    conf.HandlingData.TractionCurveLateral = VehicleEditorMenu.ControlFloatValue(ItemTractionCurveLateral, conf.HandlingData._fTractionCurveLateral, 0.01f, 1f, 6);
                    conf.HandlingData.SuspensionForce = VehicleEditorMenu.ControlFloatValue(ItemSuspensionForce, conf.HandlingData._fSuspensionForce, 0.01f, 1f, 6);
                    conf.HandlingData.SuspensionCompDamp = VehicleEditorMenu.ControlFloatValue(ItemSuspensionCompDamp, conf.HandlingData._fSuspensionCompDamp, 0.01f, 1f, 6);
                    conf.HandlingData.SuspensionReboundDamp = VehicleEditorMenu.ControlFloatValue(ItemSuspensionReboundDamp, conf.HandlingData._fSuspensionReboundDamp, 0.01f, 1f, 6);
                    conf.HandlingData.SuspensionUpperLimit = VehicleEditorMenu.ControlFloatValue(ItemSuspensionUpperLimit, conf.HandlingData._fSuspensionUpperLimit, 0.01f, 1f, 6);
                    conf.HandlingData.SuspensionLowerLimit = VehicleEditorMenu.ControlFloatValue(ItemSuspensionLowerLimit, conf.HandlingData._fSuspensionLowerLimit, 0.01f, 1f, 6);
                    conf.HandlingData.SuspensionBiasFront = VehicleEditorMenu.ControlFloatValue(ItemSuspensionBiasFront, conf.HandlingData._fSuspensionBiasFront, 0.01f, 1f, 6);
                    conf.HandlingData.AntiRollBarForce = VehicleEditorMenu.ControlFloatValue(ItemAntiRollBarForce, conf.HandlingData._fAntiRollBarForce, 0.01f, 1f, 6);
                    conf.HandlingData.AntiRollBarBiasFront = VehicleEditorMenu.ControlFloatValue(ItemAntiRollBarBiasFront, conf.HandlingData._fAntiRollBarBiasFront, 0.01f, 1f, 6);
                    conf.HandlingData.RollCentreHeightFront = VehicleEditorMenu.ControlFloatValue(ItemRollCentreHeightFront, conf.HandlingData._fRollCentreHeightFront, 0.01f, 1f, 6);
                    conf.HandlingData.RollCentreHeightRear = VehicleEditorMenu.ControlFloatValue(ItemRollCentreHeightRear, conf.HandlingData._fRollCentreHeightRear, 0.01f, 1f, 6);

                    if (VehicleEditorMenu.JustPressedAccept())
                    {
                        if (VehicleEditorMenu.SelectedItem == ItemSaveConfig)
                        {
                            SaveCurrentHandlingToConfig(conf);
                        }
                        if (VehicleEditorMenu.SelectedItem == ItemReloadConfig)
                        {
                            LoadHandlingConfig(conf);
                        }
                        if (VehicleEditorMenu.SelectedItem == ItemConfigSelect)
                        {
                            //add navigation in list here.
                        }
                        VehicleEditorMenu.SetInputWait();
                    }
                }
               yield return true;
            }
            yield return false;
        }*/

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == modeSwitchKey && !e.Alt && !e.Control && !e.Shift && isVehicleSpecial(Game.Player.Character.CurrentVehicle))
            {
                SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).ConfigDisplay.IsVisible = true;
            }
        }

        void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W && !e.Alt && !e.Control && !e.Shift && isVehicleSpecial(Game.Player.Character.CurrentVehicle) && SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).ConfigDisplay.IsVisible)
            {
                GoToPreviousConfig(SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex));
                SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).IndexWasChanged = true;
            }

            if (e.KeyCode == Keys.S && !e.Alt && !e.Control && !e.Shift && isVehicleSpecial(Game.Player.Character.CurrentVehicle) && SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).ConfigDisplay.IsVisible)
            {
                GoToNextConfig(SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex));
                SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).IndexWasChanged = true;
            }

            if (e.KeyCode == modeSwitchKey && !e.Alt && !e.Control && !e.Shift && isVehicleSpecial(Game.Player.Character.CurrentVehicle))
            {
                SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).ConfigDisplay.IsVisible = false;
                if (SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).IndexWasChanged == true)
                {
                    SpecialVehicles.ElementAt(LastUsedSpecialVehicleIndex).IndexWasChanged = false;
                    DriveModeChange(Game.Player.Character.CurrentVehicle);
                }
            }

            if (e.KeyCode == MenuKey && !e.Alt && !e.Control && !e.Shift)
            {
                if (!DMComm.ScriptCommunicatorMenuDllExists() || (DMComm.ScriptCommunicatorMenuIsBlocked() && _menuPool.IsAnyMenuOpen()))
                {
                    if (!_menuPool.IsAnyMenuOpen())
                    {
                        if (Game.Player.Character.IsInVehicle())
                        {
                            _menuPool.LastUsedMenu.IsVisible = !_menuPool.LastUsedMenu.IsVisible;
                        }
                        else
                        {
                            UI.ShowSubtitle("Please enter vehicle first");
                        }
                    }
                    else
                    {
                        _menuPool.CloseAllMenus();
                    }
                }
            }
        }

        public event DriveModeChangeEvent OnDriveModeChange;

        protected virtual void DriveModeChange(Vehicle selectedVehicle)
        {
            OnDriveModeChange?.Invoke(this, selectedVehicle);
        }
    }

    public class SpecialVehicles
    {
        public string VehicleHash; //Holds just the name of the folder
        public string FullPath; //Holds full path
        public List<Configs> Configs;// = new List<Configs>();
        public int LastUsedIndex = 0;
        public UIMenuDisplayOnly ConfigDisplay;
        public bool IndexWasChanged;

        public SpecialVehicles(string vehiclehash, string fullpath, List<Configs> configList)
        {
            VehicleHash = vehiclehash;
            FullPath = fullpath;
            Configs = configList;
            ConfigDisplay = new UIMenuDisplayOnly("Drive Modes");
        }
    }

    public class Configs
    {
        public string ConfigName; //Hold name of cfg, without path or extension
        public string FullPath; //Holds path AND extension
        public CurrentHandling HandlingData;

        public Configs(string configName, string fullpath)
        {
            ConfigName = configName;
            FullPath = fullpath;
            HandlingData = new CurrentHandling();
        }
    }

    public class CurrentHandling
    {
        public float _vecCentreOfMassX;
        public float _vecCentreOfMassY;
        public float _vecCentreOfMassZ;
        public float _vecInertiaMultiplierX;
        public float _vecInertiaMultiplierY;
        public float _vecInertiaMultiplierZ;
        public float _fDriveBiasFront;
        public sbyte _nInitialGears;
        public float _fDriveInertia;
        public float _fClutchChangeRateScaleUpShift;
        public float _fClutchChangeRateScaleDownShift;
        public float _fInitialDriveForce;
        public float _fInitialDriveMaxFlatVel;
        public float _fBrakeForce;
        public float _fBrakeBiasFront;
        public float _fHandBrakeForce;
        public float _fSteeringLock;
        public float _fTractionCurveMax;
        public float _fTractionCurveMin;
        public float _fTractionCurveLateral;
        public float _fTractionSprintDeltaMax;
        public float _fLowSpeedTractionLossMult;
        public float _fCamberStiffness;
        public float _fTractionBiasFront;
        public float _fTractionLossMult;
        public float _fSuspensionForce;
        public float _fSuspensionCompDamp;
        public float _fSuspensionReboundDamp;
        public float _fSuspensionUpperLimit;
        public float _fSuspensionLowerLimit;
        public float _fSuspensionBiasFront;
        public float _fAntiRollBarForce;
        public float _fAntiRollBarBiasFront;
        public float _fRollCentreHeightFront;
        public float _fRollCentreHeightRear;
        public float _TorqueMult;
        public float _EngPowMult;
        public int _SpeedLimit;
        
        public CurrentHandling()
        {
        }

        public float CentreOfMassX
        {
            get { return _vecCentreOfMassX; }
            set
            {
                if (_vecCentreOfMassX != value)
                { HandlingUtils.SetCentreOfMassX(value); }
                _vecCentreOfMassX = value;
            }
        }

        public float CentreOfMassY
        {
            get { return _vecCentreOfMassY; }
            set
            {
                if (_vecCentreOfMassY != value)
                { HandlingUtils.SetCentreOfMassY(value); }
                _vecCentreOfMassY = value;
            }
        }

        public float CentreOfMassZ
        {
            get { return _vecCentreOfMassZ; }
            set
            {
                if (_vecCentreOfMassZ != value)
                { HandlingUtils.SetCentreOfMassZ(value); }
                _vecCentreOfMassZ = value;
            }
        }

        public float InertiaMultiplierX
        {
            get { return _vecInertiaMultiplierX; }
            set
            {
                if (_vecInertiaMultiplierX != value)
                { HandlingUtils.SetInertiaMultiplierX(value); }
                _vecInertiaMultiplierX = value;
            }
        }

        public float InertiaMultiplierY
        {
            get { return _vecInertiaMultiplierY; }
            set
            {
                if (_vecInertiaMultiplierY != value)
                { HandlingUtils.SetInertiaMultiplierY(value); }
                _vecInertiaMultiplierY = value;
            }
        }

        public float InertiaMultiplierZ
        {
            get { return _vecInertiaMultiplierZ; }
            set
            {
                if (_vecInertiaMultiplierZ != value)
                { HandlingUtils.SetInertiaMultiplierZ(value); }
                _vecInertiaMultiplierZ = value;
            }
        }

        public float DriveBiasFront
        {
            get { return _fDriveBiasFront; }
            set
            {
                if (_fDriveBiasFront != value)
                { HandlingUtils.SetDriveBiasFront(value); }
                _fDriveBiasFront = value;
            }
        }

        public sbyte nInitialDriveGears
        {
            get { return _nInitialGears; }
            set
            {
                if (_nInitialGears != value)
                { HandlingUtils.SetnInitialDriveGears(value); }
                _nInitialGears = value;
            }
        }

        public float DriveInertia
        {
            get { return _fDriveInertia; }
            set
            {
                if (_fDriveInertia != value)
                { HandlingUtils.SetDriveInertia(value); }
                _fDriveInertia = value;  }
        }

        public float ClutchChangeRateScaleUpShift
        {
            get { return _fClutchChangeRateScaleUpShift; }
            set
            {
                if (_fClutchChangeRateScaleUpShift != value)
                { HandlingUtils.SetClutchChangeRateScaleUpShift(value); }
                _fClutchChangeRateScaleUpShift = value;  }
        }

        public float ClutchChangeRateScaleDownShift
        {
            get { return _fClutchChangeRateScaleDownShift; }
            set
            {
                if (_fClutchChangeRateScaleDownShift != value)
                { HandlingUtils.SetClutchChangeRateScaleDownShift(value); }
                _fClutchChangeRateScaleDownShift = value;  }
        }

        public float InitialDriveForce
        {
            get { return _fInitialDriveForce; }
            set
            {
                if (_fInitialDriveForce != value)
                { HandlingUtils.SetInitialDriveForce(value); }
                _fInitialDriveForce = value;  }
        }

        public float InitialDriveMaxFlatVel
        {
            get { return _fInitialDriveMaxFlatVel; }
            set
            {
                if (_fInitialDriveMaxFlatVel != value)
                { HandlingUtils.SetInitialDriveMaxFlatVel(value); }
                _fInitialDriveMaxFlatVel = value; }
        }

        public float BrakeForce
        {
            get { return _fBrakeForce; }
            set
            {
                if (_fBrakeForce != value)
                { HandlingUtils.SetBrakeForce(value); }
                _fBrakeForce = value;  }
        }

        public float BrakeBiasFront
        {
            get { return _fBrakeBiasFront; }
            set
            {
                if (_fBrakeBiasFront != value)
                { HandlingUtils.SetBrakeBiasFront(value); }
                _fBrakeBiasFront = value;  }
        }

        public float HandBrakeForce
        {
            get { return _fHandBrakeForce; }
            set
            {
                if (_fHandBrakeForce != value)
                { HandlingUtils.SetHandBrakeForce(value); }
                _fHandBrakeForce = value; }
        }

        public float SteeringLock
        {
            get { return _fSteeringLock; }
            set
            {
                if (_fSteeringLock != value)
                { HandlingUtils.SetSteeringLock(value); }
                _fSteeringLock = value; }
        }

        public float TractionCurveMax
        {
            get { return _fTractionCurveMax; }
            set
            {
                if (_fTractionCurveMax != value)
                { HandlingUtils.SetTractionCurveMax(value); }
                _fTractionCurveMax = value;  }
        }

        public float TractionCurveMin
        {
            get { return _fTractionCurveMin; }
            set
            {
                if (_fTractionCurveMin != value)
                { HandlingUtils.SetTractionCurveMin(value); }
                _fTractionCurveMin = value; }
        }

        public float TractionCurveLateral
        {
            get { return _fTractionCurveLateral; }
            set
            {
                if (_fTractionCurveLateral != value)
                { HandlingUtils.SetTractionCurveLateral(value); }
                _fTractionCurveLateral = value;  }
        }

        public float TractionSpringDeltaMax
        {
            get { return _fTractionSprintDeltaMax; }
            set
            {
                if (_fTractionSprintDeltaMax != value)
                { HandlingUtils.SetTractionSpringDeltaMax(value); }
                _fTractionSprintDeltaMax = value;
            }
        }

        public float LowSpeedTractionLossMult
        {
            get { return _fLowSpeedTractionLossMult; }
            set
            {
                if (_fLowSpeedTractionLossMult != value)
                { HandlingUtils.SetLowSpeedTractionLossMult(value); }
                _fLowSpeedTractionLossMult = value;
            }
        }

        public float CamberStiffness
        {
            get { return _fCamberStiffness; }
            set
            {
                if (_fCamberStiffness != value)
                { HandlingUtils.SetCamberStiffness(value); }
                _fCamberStiffness = value;
            }
        }

        public float TractionBiasFront
        {
            get { return _fTractionBiasFront; }
            set
            {
                if (_fTractionBiasFront != value)
                { HandlingUtils.SetTractionBiasFront(value); }
                _fTractionBiasFront = value;
            }
        }

        public float TractionLossMult
        {
            get { return _fTractionLossMult; }
            set
            {
                if (_fTractionLossMult != value)
                { HandlingUtils.SetTractionLossMult(value); }
                _fTractionLossMult = value;
            }
        }

        public float SuspensionForce
        {
            get { return _fSuspensionForce; }
            set
            {
                if (_fSuspensionForce != value)
                { HandlingUtils.SetSuspensionForce(value); }
                _fSuspensionForce = value; }
        }

        public float SuspensionCompDamp
        {
            get { return _fSuspensionCompDamp; }
            set
            {
                if (_fSuspensionCompDamp != value)
                { HandlingUtils.SetSuspensionCompDamp(value); }
                _fSuspensionCompDamp = value; }
        }

        public float SuspensionReboundDamp
        {
            get { return _fSuspensionReboundDamp; }
            set
            {
                if (_fSuspensionReboundDamp != value)
                { HandlingUtils.SetSuspensionReboundDamp(value); }
                _fSuspensionReboundDamp = value;  }
        }

        public float SuspensionUpperLimit
        {
            get { return _fSuspensionUpperLimit; }
            set
            {
                if (_fSuspensionUpperLimit != value)
                { HandlingUtils.SetSuspensionUpperLimit(value); }
                _fSuspensionUpperLimit = value;  }
        }

        public float SuspensionLowerLimit
        {
            get { return _fSuspensionLowerLimit; }
            set
            {
                if (_fSuspensionLowerLimit != value)
                { HandlingUtils.SetSuspensionLowerLimit(value); }
                _fSuspensionLowerLimit = value; }
        }

        public float SuspensionBiasFront
        {
            get { return _fSuspensionBiasFront; }
            set
            {
                if (_fSuspensionBiasFront != value)
                { HandlingUtils.SetSuspensionBiasFront(value); }
                _fSuspensionBiasFront = value; }
        }

        public float AntiRollBarForce
        {
            get { return _fAntiRollBarForce; }
            set
            {
                if (_fAntiRollBarForce != value)
                { HandlingUtils.SetAntiRollBarForce(value); }
                _fAntiRollBarForce = value; }
        }

        public float AntiRollBarBiasFront
        {
            get { return _fAntiRollBarBiasFront; }
            set
            {
                if (_fAntiRollBarBiasFront != value)
                { HandlingUtils.SetAntiRollBarBiasFront(value); }
                _fAntiRollBarBiasFront = value;  }
        }

        public float RollCentreHeightFront
        {
            get { return _fRollCentreHeightFront; }
            set
            {
                if (_fRollCentreHeightFront != value)
                { HandlingUtils.SetRollCentreHeightFront(value); }
                _fRollCentreHeightFront = value;  }
        }

        public float RollCentreHeightRear
        {
            get { return _fRollCentreHeightRear; }
            set
            {
                if (_fRollCentreHeightRear != value)
                { HandlingUtils.SetRollCentreHeightRear(value); }
                _fRollCentreHeightRear = value;  }
        }

        public int SpeedLimit
        {
            get { return _SpeedLimit; }
            set { _SpeedLimit = value; }
        }
    }

    public static class HandlingUtils
    {
        static int vecCentreOfMassX = 0x0020;
        static int vecCentreOfMassY = 0x0024;
        static int vecCentreOfMassZ = 0x0028;
        static int vecInertiaMultiplierX = 0x0030;
        static int vecInertiaMultiplierY = 0x0034;
        static int vecInertiaMultiplierZ = 0x0038;
        static int fDriveBiasFront = 0x0048;
        static int fDriveBiasRear = 0x004C;
        static byte nInitialDriveGears = 0x0050;
        static int fDriveInertia = 0x0054;
        static int fClutchChangeRateScaleUpShift = 0x0058;
        static int fClutchChangeRateScaleDownShift = 0x005C;
        static int fInitialDriveForce = 0x0060;
        static int fDriveMaxFlatVel = 0x0064;
        static int fInitialDriveMaxFlatVel = 0x0068;
        static int fBrakeForce = 0x006C;
        static int fBrakeBiasFront = 0x0074;
        static int fBrakeBiasRear = 0x0078;
        static int fHandBrakeForce = 0x007C;
        static int fSteeringLock = 0x80;
        static int fSteeringLockRatio = 0x84;
        static int fTractionCurveMax = 0x88;
        static int fTractionCurveMaxRatio = 0x8C;
        static int fTractionCurveMin = 0x90;
        static int fTractionCurveMinRatio = 0x94;
        static int fTractionCurveLateral = 0x98;
        static int fTractionCurveLateralRatio = 0x009C;
        static int fTractionSpringDeltaMax = 0x00A0;
        static int fTractionSpringDeltaMaxRatio = 0x00A4;
        static int fLowSpeedTractionLossMult = 0x00A8;
        static int fCamberStiffness = 0x00AC;
        static int fTractionBiasFront = 0x00B0;
        static int fTractionBiasRear = 0x00B4;
        static int fTractionLossMult = 0x00B8;
        static int fSuspensionForce = 0x00BC;
        static int fSuspensionCompDamp = 0x00C0;
        static int fSuspensionReboundDamp = 0x00C4;
        static int fSuspensionUpperLimit = 0x00C8;
        static int fSuspensionLowerLimit = 0x00CC;
        static int fSuspensionBiasFront = 0x00D4;
        static int fSuspensionBiasRear = 0x00D8;
        static int fAntiRollBarForce = 0x00DC;
        static int fAntiRollBarBiasFront = 0x00E0;
        static int fAntiRollBarBiasRear = 0x00E4;
        static int fRollCentreHeightFront = 0x00E8;
        static int fRollCentreHeightRear = 0x00EC;

        /*Set Values*/

        public static void SetCentreOfMassX(float value)
        {
            SetHandlingValue(vecCentreOfMassX, value);
        }

        public static void SetCentreOfMassY(float value)
        {
            SetHandlingValue(vecCentreOfMassY, value);
        }

        public static void SetCentreOfMassZ(float value)
        {
            SetHandlingValue(vecCentreOfMassZ, value);
        }

        public static void SetInertiaMultiplierX(float value)
        {
            SetHandlingValue(vecInertiaMultiplierX, value);
        }

        public static void SetInertiaMultiplierY(float value)
        {
            SetHandlingValue(vecInertiaMultiplierY, value);
        }

        public static void SetInertiaMultiplierZ(float value)
        {
            SetHandlingValue(vecInertiaMultiplierZ, value);
        }

        public static void SetDriveBiasFront(float value)
        {
            if (value == 1.0f)
            {
                SetHandlingValue(fDriveBiasRear, 0.0f);
                SetHandlingValue(fDriveBiasFront, 1.0f);
            }
            else if (value == 0.0f)
            {
                SetHandlingValue(fDriveBiasRear, 1.0f);
                SetHandlingValue(fDriveBiasFront, 0.0f);
            }
            else
            {
                SetHandlingValue(fDriveBiasRear, 2.0f * (1.0f - (value)));
                SetHandlingValue(fDriveBiasFront, value * 2.0f);
            }
        }

        public static void SetnInitialDriveGears(int value)
        {
            SetHandlingValueInt(nInitialDriveGears, value);
        }

        public static void SetDriveInertia(float value)
        {
            SetHandlingValue(fDriveInertia, value);
        }

        public static void SetClutchChangeRateScaleUpShift(float value)
        {
            SetHandlingValue(fClutchChangeRateScaleUpShift, value);
        }

        public static void SetClutchChangeRateScaleDownShift(float value)
        {
            SetHandlingValue(fClutchChangeRateScaleDownShift, value);
        }

        public static void SetInitialDriveForce(float value)
        {
            SetHandlingValue(fInitialDriveForce, value);
        }

        public static void SetInitialDriveMaxFlatVel(float value)
        {
            SetHandlingValue(fInitialDriveMaxFlatVel, value / 3.6f);
            SetHandlingValue(fDriveMaxFlatVel, value / 3.0f);
        }

        public static void SetBrakeForce(float value)
        {
            SetHandlingValue(fBrakeForce, value);
        }

        public static void SetBrakeBiasFront(float value)
        {
            SetHandlingValue(fBrakeBiasRear, 2.0f * (1.0f - (value)));
            SetHandlingValue(fBrakeBiasFront, value * 2.0f);
        }

        public static void SetHandBrakeForce(float value)
        {
            SetHandlingValue(fHandBrakeForce, value);
        }

        public static void SetSteeringLock(float value)
        {
            SetHandlingValue(fSteeringLock, value * 0.017453292f);
            SetHandlingValue(fSteeringLockRatio, 1.0f / (value * 0.017453292f));
        }

        public static void SetTractionCurveMax(float value)
        {
            SetHandlingValue(fTractionCurveMax, value);
            if (value == 0.0f)
            {
                SetHandlingValue(fTractionCurveMaxRatio, 100000000.000000f);
            }
            else
            {
                SetHandlingValue(fTractionCurveMaxRatio, 1.0f / value);
            }
        }

        public static void SetTractionCurveMin(float value)
        {
            SetHandlingValue(fTractionCurveMin, value);
            float temp_fTractionCurveMax = GetHandlingValue(fTractionCurveMax);
            float temp_ftractionCurveMin = GetHandlingValue(fTractionCurveMin);
            if (temp_fTractionCurveMax <= temp_ftractionCurveMin)
            {
                SetHandlingValue(fTractionCurveMinRatio, 100000000.000000f);
            }
            else
            {
                SetHandlingValue(fTractionCurveMinRatio, 1.0f / (temp_fTractionCurveMax - temp_ftractionCurveMin));
            }
        }

        public static void SetTractionCurveLateral(float value)
        {
            SetHandlingValue(fTractionCurveLateral, value * 0.017453292f);
            SetHandlingValue(fTractionCurveLateralRatio, 1.0f / (value * 0.017453292f));
        }

        public static void SetTractionSpringDeltaMax(float value)
        {
            SetHandlingValue(fTractionSpringDeltaMax, value);
            SetHandlingValue(fTractionSpringDeltaMaxRatio, 1.0f / (value));
        }

        public static void SetLowSpeedTractionLossMult(float value)
        {
            SetHandlingValue(fLowSpeedTractionLossMult, value);
        }

        public static void SetCamberStiffness(float value)
        {
            SetHandlingValue(fCamberStiffness, value);
        }

        public static void SetTractionBiasFront(float value)
        {
            SetHandlingValue(fTractionBiasRear, 2.0f * (1.0f - (value)));
            SetHandlingValue(fTractionBiasFront, value * 2.0f);
        }

        public static void SetTractionLossMult(float value)
        {
            SetHandlingValue(fTractionLossMult, value);
        }

        public static void SetSuspensionForce(float value)
        {
            SetHandlingValue(fSuspensionForce, value);
        }

        public static void SetSuspensionCompDamp(float value)
        {
            SetHandlingValue(fSuspensionCompDamp, value / 10.0f);
        }

        public static void SetSuspensionReboundDamp(float value)
        {
            SetHandlingValue(fSuspensionReboundDamp, value / 10.0f);
        }

        public static void SetSuspensionUpperLimit(float value)
        {
            SetHandlingValue(fSuspensionUpperLimit, value);
        }

        public static void SetSuspensionLowerLimit(float value)
        {
            SetHandlingValue(fSuspensionLowerLimit, value);
        }

        public static void SetSuspensionBiasFront(float value)
        {
            SetHandlingValue(fSuspensionBiasRear, 2.0f * (1.0f - (value)));
            SetHandlingValue(fSuspensionBiasFront, value * 2.0f);
        }

        public static void SetAntiRollBarForce(float value)
        {
            SetHandlingValue(fAntiRollBarForce, value);
        }

        public static void SetAntiRollBarBiasFront(float value)
        {
            SetHandlingValue(fAntiRollBarBiasRear, 2.0f * (1.0f - (value)));
            SetHandlingValue(fAntiRollBarBiasFront, value * 2.0f);
        }

        public static void SetRollCentreHeightFront(float value)
        {
            SetHandlingValue(fRollCentreHeightFront, value);
        }

        public static void SetRollCentreHeightRear(float value)
        {
            SetHandlingValue(fRollCentreHeightRear, value);
        }

        /*End Set Values*/

        /*Start Get Values*/

        public static float GetCentreOfMassX()
        {
            return GetHandlingValue(vecCentreOfMassX);
        }

        public static float GetCentreOfMassY()
        {
            return GetHandlingValue(vecCentreOfMassY);
        }

        public static float GetCentreOfMassZ()
        {
            return GetHandlingValue(vecCentreOfMassZ);
        }

        public static float GetInertiaMultiplierX()
        {
            return GetHandlingValue(vecInertiaMultiplierX);
        }

        public static float GetInertiaMultiplierY()
        {
            return GetHandlingValue(vecInertiaMultiplierY);
        }

        public static float GetInertiaMultiplierZ()
        {
            return GetHandlingValue(vecInertiaMultiplierZ);
        }

        public static float GetDriveBiasFront()
        {
            //return 1.0f - (GetHandlingValue(fDriveBiasRear) / 2.0f);
            float rear = GetHandlingValue(fDriveBiasRear);
            if (rear == 1.0f)
            {
                return 0.0f;
            }
            else if (rear == 0.0f)
            {
                return 1.0f;
            }
            else
            {
                return 1.0f - (rear / 2.0f);
            }
        }

        /*public static float GetDriveBiasRearExact()
        {
            return GetHandlingValue(fDriveBiasRear);
        }

        public static float GetDriveBiasFrontExact()
        {
            return GetHandlingValue(fDriveBiasFront);
        }*/

        public static int GetnInitialDriveGears()
        {
            return GetHandlingValueInt8(nInitialDriveGears);
        }

        public static float GetDriveInertia()
        {
            return GetHandlingValue(fDriveInertia);
        }

        public static float GetClutchChangeRateScaleUpShift()
        {
            return GetHandlingValue(fClutchChangeRateScaleUpShift);
        }

        public static float GetClutchChangeRateScaleDownShift()
        {
            return GetHandlingValue(fClutchChangeRateScaleDownShift);
        }

        public static float GetInitialDriveForce()
        {
            return GetHandlingValue(fInitialDriveForce);
        }

        public static float GetInitialDriveMaxFlatVel()
        {
            return GetHandlingValue(fInitialDriveMaxFlatVel) * 3.6f;
        }

        public static float GetBrakeForce()
        {
            return GetHandlingValue(fBrakeForce);
        }

        public static float GetBrakeBiasFront()
        {
            return 1.0f - GetHandlingValue(fBrakeBiasRear) / 2.0f;
        }

        public static float GetHandBrakeForce()
        {
            return GetHandlingValue(fHandBrakeForce);
        }

        public static float GetSteeringLock()
        {
            return GetHandlingValue(fSteeringLock) * 57.2957795131f;
        }

        public static float GetTractionCurveMax()
        {
            return GetHandlingValue(fTractionCurveMax);
        }

        public static float GetTractionCurveMin()
        {
            return GetHandlingValue(fTractionCurveMin);
        }

        public static float GetTractionCurveLateral()
        {
            return GetHandlingValue(fTractionCurveLateral) * 57.2957795131f;
        }

        public static float GetTractionSpringDeltaMax()
        {
            return GetHandlingValue(fTractionSpringDeltaMax);
        }

        public static float GetLowSpeedTractionLossMult()
        {
            return GetHandlingValue(fLowSpeedTractionLossMult);
        }

        public static float GetCamberStiffness()
        {
            return GetHandlingValue(fCamberStiffness);
        }

        public static float GetTractionBiasFront()
        {
            return 1.0f - GetHandlingValue(fTractionBiasRear) / 2.0f;
        }

        public static float GetTractionLossMult()
        {
            return GetHandlingValue(fTractionLossMult);
        }

        public static float GetSuspensionForce()
        {
            return GetHandlingValue(fSuspensionForce);
        }

        public static float GetSuspensionCompDamp()
        {
            return GetHandlingValue(fSuspensionCompDamp) * 10.0f;
        }

        public static float GetSuspensionReboundDamp()
        {
            return GetHandlingValue(fSuspensionReboundDamp) * 10.0f;
        }

        public static float GetSuspensionUpperLimit()
        {
            return GetHandlingValue(fSuspensionUpperLimit);
        }

        public static float GetSuspensionLowerLimit()
        {
            return GetHandlingValue(fSuspensionLowerLimit);
        }

        public static float GetSuspensionBiasFront()
        {
            return 1.0f - GetHandlingValue(fSuspensionBiasRear) / 2.0f;
        }

        public static float GetAntiRollBarForce()
        {
            return GetHandlingValue(fAntiRollBarForce);
        }

        public static float GetAntiRollBarBiasFront()
        {
            return 1.0f - GetHandlingValue(fAntiRollBarBiasRear) / 2.0f;
        }

        public static float GetRollCentreHeightFront()
        {
            return GetHandlingValue(fRollCentreHeightFront);
        }

        public static float GetRollCentreHeightRear()
        {
            return GetHandlingValue(fRollCentreHeightRear);
        }

        /*End Get Values*/

        /*///////////////////////////////////////////////////*/

        unsafe static float GetHandlingValue(int exactOffset)
        {
            int gameVersion = (int)Game.Version;
            int handlingOffset;
            handlingOffset = (gameVersion > 3 ? 0x830 : 0x820);
            handlingOffset = (gameVersion > 25 ? 0x850 : handlingOffset);
            handlingOffset = (gameVersion > 27 ? 0x878 : handlingOffset);
            handlingOffset = (gameVersion > 29 ? 0x888 : handlingOffset);

            ulong vehPtr = (ulong)Game.Player.Character.CurrentVehicle.MemoryAddress; //convert veh.MemoryAddress to ulong
            ulong handlingPtr = *(ulong*)(vehPtr + (uint)handlingOffset); //add handling offset to address to get handling address
            float fValue = *(float*)(handlingPtr + (uint)exactOffset); //get float value of SuspRaise address
            return fValue;
        }

        unsafe static sbyte GetHandlingValueInt8(int exactOffset)
        {
            int gameVersion = (int)Game.Version;
            int handlingOffset;
            handlingOffset = (gameVersion > 3 ? 0x830 : 0x820);
            handlingOffset = (gameVersion > 25 ? 0x850 : handlingOffset);
            handlingOffset = (gameVersion > 27 ? 0x878 : handlingOffset);
            handlingOffset = (gameVersion > 29 ? 0x888 : handlingOffset);

            ulong vehPtr = (ulong)Game.Player.Character.CurrentVehicle.MemoryAddress; //convert veh.MemoryAddress to ulong
            ulong handlingPtr = *(ulong*)(vehPtr + (uint)handlingOffset); //add handling offset to address to get handling address
            sbyte fValue = *(sbyte*)(handlingPtr + (uint)exactOffset); //get float value of SuspRaise address
            return fValue;
        }

        unsafe static void SetHandlingValue(int exactOffset, float currentValue)
        {
            int gameVersion = (int)Game.Version;
            int handlingOffset;
            handlingOffset = (gameVersion > 3 ? 0x830 : 0x820);
            handlingOffset = (gameVersion > 25 ? 0x850 : handlingOffset);
            handlingOffset = (gameVersion > 27 ? 0x878 : handlingOffset);
            handlingOffset = (gameVersion > 29 ? 0x888 : handlingOffset);

            Process[] processes = Process.GetProcessesByName("GTA5");
            if (processes.Length > 0)
            {
                using (CheatEngine.Memory memory = new CheatEngine.Memory(processes[0]))
                {
                    ulong vehPtr = (ulong)Game.Player.Character.CurrentVehicle.MemoryAddress; //convert veh.MemoryAddress to ulong
                    ulong handlingPtr = *(ulong*)(vehPtr + (uint)handlingOffset); //add handling offset to address to get handling address
                    IntPtr exactPointer = (IntPtr)(handlingPtr + (uint)exactOffset); //convert exact handling address to IntPtr

                    memory.WriteFloat(exactPointer, currentValue); //write
                    //memory.WriteFloat(exactPointer, (float)Math.Round(currentValue += amountDifference, 6)); //write

                    //UI.ShowSubtitle(suspRaise.ToString(), 10000);
                    //memory.Dispose();
                }
            }
        }

        unsafe static void SetHandlingValueInt(int exactOffset, int currentValue)
        {
            int gameVersion = (int)Game.Version;
            int handlingOffset;
            handlingOffset = (gameVersion > 3 ? 0x830 : 0x820);
            handlingOffset = (gameVersion > 25 ? 0x850 : handlingOffset);
            handlingOffset = (gameVersion > 27 ? 0x878 : handlingOffset);
            handlingOffset = (gameVersion > 29 ? 0x888 : handlingOffset);

            Process[] processes = Process.GetProcessesByName("GTA5");
            if (processes.Length > 0)
            {
                using (CheatEngine.Memory memory = new CheatEngine.Memory(processes[0]))
                {
                    ulong vehPtr = (ulong)Game.Player.Character.CurrentVehicle.MemoryAddress; //convert veh.MemoryAddress to ulong
                    ulong handlingPtr = *(ulong*)(vehPtr + (uint)handlingOffset); //add handling offset to address to get handling address
                    IntPtr exactPointer = (IntPtr)(handlingPtr + (uint)exactOffset); //convert exact handling address to IntPtr

                    memory.WriteInt32(exactPointer, currentValue); //write
                    //memory.WriteFloat(exactPointer, (float)Math.Round(currentValue += amountDifference, 6)); //write

                    //UI.ShowSubtitle(suspRaise.ToString(), 10000);
                    //memory.Dispose();
                }
            }
        }
    }

    public class CameraSettings
    {
        Vector3 _positionOffset;
        Vector3 _rotation;
        Vector3 _pointPositionOffset;
        float _fieldOfView;
        bool _autoCenter;
        bool _enabled = true;

        public CameraSettings()
        {
            _positionOffset = Vector3.Zero;
            _rotation = Vector3.Zero;
            _pointPositionOffset = Vector3.Zero;
            _fieldOfView = 50f;
            _autoCenter = true;
            _enabled = true;
        }

        public Vector3 PositionOffset
        {
            get { return _positionOffset; }
            set { _positionOffset = value; }
        }

        public Vector3 Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public Vector3 PointPositionOffset
        {
            get { return _pointPositionOffset; }
            set { _pointPositionOffset = value; }
        }

        public float FieldOfView
        {
            get { return _fieldOfView; }
            set { _fieldOfView = value; }
        }

        public bool AutoCenterCamera
        {
            get { return _autoCenter; }
            set { _autoCenter = value; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }
    }

    public static class MathHelper
    {
        static float Pi = (float)Math.PI;

        public static float RadiansToDegrees(float radian)
        {
            return radian * (180.0f / Pi);
        }

        public static float DirectionToHeading(Vector3 dir)
        {
            dir.Z = 0.0f;
            dir.Normalize();
            return RadiansToDegrees(-(float)Math.Atan2(dir.X, dir.Y));
        }

        public static float CalculatePosition(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            //http://stackoverflow.com/questions/22083199/method-for-calculating-a-value-relative-to-min-max-values
            //Making sure bounderies arent broken...
            if (input > inputMax)
            {
                input = inputMax;
            }
            if (input < inputMin)
            {
                input = inputMin;
            }
            //Return value in relation to min og max

            double position = (double)(input - inputMin) / (inputMax - inputMin);

            float relativeValue = (float)(position * (outputMax - outputMin)) + outputMin;

            return relativeValue;
        }

        public static float Lerp(float from, float to, float amount)
        {
            return (1 - amount) * from + amount * to;
        }
    }
}