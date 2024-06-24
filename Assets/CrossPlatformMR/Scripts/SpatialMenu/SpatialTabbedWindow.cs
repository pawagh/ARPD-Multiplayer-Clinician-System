using Fusion;
using Fusion.Addons.VisionOsHelpers;
using Fusion.Addons.XRHandsSync;
using Fusion.XR.Shared;
using Fusion.XR.Shared.Rig;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpatialTabbedWindow : MonoBehaviour, IMenuManager
{
    [SerializeField]
    GameObject menuGameObject = null;

    [SerializeField]
    GameObject tabsRoot = null;

    [SerializeField]
    SpatialButton initialTabButton;

    [SerializeField]
    List<GameObject> menuTabs = new List<GameObject>();

    [SerializeField]
    GameObject initialTab = null;

    [SerializeField]
    NetworkRunner runner;

    [SerializeField]
    float forwardDistanceForSpawn = 0.7f;

    [SerializeField]
    float downDistanceForSpawn = 0.4f;

    [HideInInspector]
    public HardwareHand handDrivingMenu = null;

    bool openedForLeftHand = false;
    bool openedForRightHand = false;
    bool positionedAfterAppeared = false;

    [Header("Controller input")]
    public InputActionProperty leftMenuAction;
    public InputActionProperty rightMenuAction;
    HardwareRig rig;

    public Transform MenuObjectTransform => menuGameObject.transform;

    public bool ShouldDisplayActivationSpinner => menuGameObject.activeSelf == false;

    #region IMenuManager
    public void RequiredByHand(HardwareHand hand, bool isRequired)
    {
        bool wasOpened = openedForLeftHand || openedForRightHand;

        if (menuGameObject == null) return;
        if (hand.side == RigPart.LeftController)
            openedForLeftHand = isRequired;
        else
            openedForRightHand = isRequired;
        bool isOpened = openedForLeftHand || openedForRightHand;
        bool wasMenuActive = menuGameObject.activeSelf;

        if (isOpened == false)
            handDrivingMenu = null;
        else if (wasMenuActive == false)
            handDrivingMenu = hand;
        else if (handDrivingMenu && openedForLeftHand == false && handDrivingMenu.side == RigPart.LeftController)
            handDrivingMenu = hand;
        else if (handDrivingMenu && openedForRightHand == false && handDrivingMenu.side == RigPart.RightController)
            handDrivingMenu = hand;

        // We only accept appear command from hand
        if (isRequired == false)
        {
            return;
        }
        else if (menuGameObject.activeSelf)
        {
            // We do not open it again (in case it was closed by the user on purpose)
            return;
        }
        positionedAfterAppeared = false;


        menuGameObject.SetActive(openedForLeftHand || openedForRightHand);
    }


    public void PoseChangeRequestedByHand(HardwareRig rig, HardwareHand hand, Pose pose, bool forceMove)
    {
        
        if (positionedAfterAppeared == false)
        {
            positionedAfterAppeared = true;
            menuGameObject.transform.position = pose.position;
            // as we rotate just once, we directly update the required pose position
            var rotation = Quaternion.LookRotation(rig.headset.transform.position - MenuObjectTransform.position);
            menuGameObject.transform.rotation = rotation;
        }

    }
    #endregion


    private void Start()
    {
        leftMenuAction.EnableWithDefaultXRBindings(RigPart.LeftController, new List<string> { "thumbstickClicked", "primaryButton", "secondaryButton" });
        rightMenuAction.EnableWithDefaultXRBindings(RigPart.RightController, new List<string> { "thumbstickClicked", "primaryButton", "secondaryButton" });

        if (menuGameObject == null) menuGameObject = gameObject;
        if (tabsRoot == null) tabsRoot = menuGameObject;
        if (menuTabs.Count == 0)
        {
            // Detecting tabs
            foreach (Transform child in tabsRoot.transform)
            {
                if (child.GetComponentInChildren<SpatialTouchable>() != null)
                {
                    menuTabs.Add(child.gameObject);
                }
            }
        }

        if(initialTabButton)
        {
            initialTabButton.ChangeButtonStatus(true);
        }
        else
        { 
            if (initialTab == null && menuTabs.Count > 0) initialTab = menuTabs[0];

            if (initialTab)
            {
                ActivateMenuTab(initialTab);
            }
        }

        menuGameObject.SetActive(false);
    }

    void CheckController(RigPart side)
    {
        var action = side == RigPart.LeftController ? leftMenuAction : rightMenuAction;
        if (action.action.ReadValue<float>() == 1)
        {
            if (rig == null) rig = FindObjectOfType<HardwareRig>();
            if (rig)
            {
                HardwareHand hand;
                if (side == RigPart.LeftController)
                {
                    openedForRightHand = false;
                    hand = rig.leftHand;
                }
                else
                {
                    openedForLeftHand = false;
                    hand = rig.rightHand;
                }
                handDrivingMenu = hand;
                menuGameObject.SetActive(true);
                RequiredByHand(hand, true);
                PoseChangeRequestedByHand(rig, hand, new Pose { position = hand.transform.position, rotation = rig.headset.transform.rotation }, true);
            }
        }
    }
    private void Update()
    {
        CheckController(RigPart.LeftController);
        CheckController(RigPart.RightController);
    }

    public void ActivateMenuTab(GameObject tab)
    {
        foreach (var menuTab in menuTabs)
        {
            bool isSelectedTab = menuTab == tab;
            menuTab.SetActive(isSelectedTab);

            SpatialButton defaultSpatialButton = null;
            bool shouldActivateDefaultButton = true;
            foreach (var button in menuTab.GetComponentsInChildren<SpatialButton>())
            {
                if (button.IsRadioButton == false) 
                {
                    continue; 
                }

                if (defaultSpatialButton == null)
                    defaultSpatialButton = button;

                if (button.toggleStatus)
                {   // no need to select default button if a button is activated
                    shouldActivateDefaultButton = false;
                    break;
                }
            }

            if (shouldActivateDefaultButton && defaultSpatialButton)
            {
                defaultSpatialButton.ChangeButtonStatus(true);
            }
        }
    }


    public void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SpawnPrefab(GameObject prefab)
    {
        if (runner == null)
        {
            runner = FindObjectOfType<NetworkRunner>();
        }
        if (runner == null)
        {
            Debug.LogError("Unable to spawn without a runner");
            return;
        }
        var position = menuGameObject.transform.position - forwardDistanceForSpawn * menuGameObject.transform.forward - menuGameObject.transform.up * downDistanceForSpawn;
        var rotation = Quaternion.Euler(0, menuGameObject.transform.rotation.eulerAngles.y, 0);
        if (handDrivingMenu)
        {
            var rig = handDrivingMenu.GetComponentInParent<HardwareRig>();
            if (rig)
            {
                // Loot at the player
                rotation = Quaternion.Euler(0, rig.headset.transform.rotation.eulerAngles.y + 180, 0);
                var forward = rig.headset.transform.forward;
                forward.y = 0;
                position = rig.headset.transform.position + forward * forwardDistanceForSpawn - rig.transform.up * downDistanceForSpawn;
            }
        }
        Debug.LogError("Spawing");
        runner.Spawn(prefab, position, rotation);
    }

}
