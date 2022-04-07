using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class NetPlayer : NetworkBehaviour
{
    public Transform HMD, LeftHand, RightHand, Pelvis, LeftFoot, RightFoot;

    private AvatarGo ClientAvatarVR;
    private bool CalibrationFinished = false;

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer && IsClient)
        {
            AvatarGo[] avatars = FindObjectsOfType<AvatarGo>();
            ClientAvatarVR = null;
            foreach (AvatarGo avatar in avatars)
            {
                NetPlayer netPlayer = avatar.GetComponentInParent<NetPlayer>();
                if (netPlayer == null)
                {
                    if (ClientAvatarVR != null) Debug.LogWarning("Multiple avatars found. Only one avatar per client should be in the scene.");
                    ClientAvatarVR = avatar;
                }
            }
            if (ClientAvatarVR != null)
            {
                if (ClientAvatarVR.IsCalibrationFinished) OnCalibrationFinished();
                else ClientAvatarVR.OnCalibrationFinished.AddListener(OnCalibrationFinished);
            }
        }

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsLocalPlayer && IsClient)
        {
            if (ClientAvatarVR != null)
            {
                ClientAvatarVR.OnCalibrationFinished.RemoveListener(OnCalibrationFinished);
            }
        }
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Invoked by the Server when a client connects
        SendAvatarsClientRpc();
    }

    private void OnCalibrationFinished()
    {
        ClientAvatarVR.OnCalibrationFinished.RemoveListener(OnCalibrationFinished);
        InitAvatarServerRpc(ClientAvatarVR.body.bodyMeasures, ClientAvatarVR.AvatarPrefab.name, ClientAvatarVR.controllersStyle);
    }

    private void Update()
    {
        if (IsLocalPlayer && IsClient && CalibrationFinished)
        {
            TrackersData data = new TrackersData()
            {
                HMDPosition = ClientAvatarVR.driver.head.transform.position,
                HMDOrientation = ClientAvatarVR.driver.head.transform.rotation,
                LeftHandPosition = ClientAvatarVR.driver.handLeft.transform.position,
                LeftHandOrientation = ClientAvatarVR.driver.handLeft.transform.rotation,
                RightHandPosition = ClientAvatarVR.driver.handRight.transform.position,
                RightFootOrientation = ClientAvatarVR.driver.footRight.transform.rotation,
                PelvisPosition = ClientAvatarVR.driver.pelvis.transform.position,
                PelvisOrientation = ClientAvatarVR.driver.pelvis.transform.rotation,
                LeftFootPosition = ClientAvatarVR.driver.footLeft.transform.position,
                LeftFootOrientation = ClientAvatarVR.driver.footLeft.transform.rotation,
                RightFootPosition = ClientAvatarVR.driver.footRight.transform.position,
                RightHandOrientation = ClientAvatarVR.driver.handRight.transform.rotation
            };
            UpdateTrackersServerRpc(data);
        }
    }

    [ClientRpc]
    private void SendAvatarsClientRpc()
    {
        if (IsLocalPlayer && CalibrationFinished)
        {
            InitAvatarServerRpc(ClientAvatarVR.body.bodyMeasures, ClientAvatarVR.AvatarPrefab.name, ClientAvatarVR.controllersStyle);
        }
    }

    [ServerRpc]
    private void InitAvatarServerRpc(AvatarBody.BodyMeasures bodyMeasures, string avatarPrefab, AvatarGo.ControllersStyle controllersStyle)
    {
        InitAvatarClientRpc(bodyMeasures, avatarPrefab, controllersStyle);
    }

    [ClientRpc]
    private void InitAvatarClientRpc(AvatarBody.BodyMeasures bodyMeasures, string avatarPrefab, AvatarGo.ControllersStyle controllersStyle)
    {
        if (CalibrationFinished) return;

        if (!IsLocalPlayer)
        {
            // Force Root Position
            HMD.transform.position = bodyMeasures.headRootStep;
            HMD.transform.rotation = bodyMeasures.headRotRootStep;
            LeftHand.transform.position = bodyMeasures.leftHandRootStep;
            LeftHand.transform.rotation = bodyMeasures.leftHandRotRootStep;
            RightHand.transform.position = bodyMeasures.rightHandRootStep;
            RightHand.transform.rotation = bodyMeasures.rightHandRotRootStep;
            Pelvis.transform.position = bodyMeasures.pelvisRootStep;
            Pelvis.transform.rotation = bodyMeasures.pelvisRotRootStep;
            LeftFoot.transform.position = bodyMeasures.leftFootRootStep;
            LeftFoot.transform.rotation = bodyMeasures.leftFootRotRootStep;
            RightFoot.transform.position = bodyMeasures.rightFootRootStep;
            RightFoot.transform.rotation = bodyMeasures.rightFootRotRootStep;

            // Avatar
            AvatarGo avatar = GetComponentInChildren<AvatarGo>();
            avatar.InitBody(bodyMeasures);
            avatar.AvatarPrefab = Resources.Load<GameObject>("Avatars/" + avatarPrefab);
            if (avatar.AvatarPrefab == null)
            {
                Debug.LogError("Avatar prefab not found: " + avatarPrefab);
            }
            avatar.controllersStyle = controllersStyle;
            avatar.enabled = true;

            StartCoroutine(WaitForCalibration());
        }
        else
        {
            CalibrationFinished = true;
        }
    }

    [ServerRpc]
    private void UpdateTrackersServerRpc(TrackersData data)
    {
        UpdateTrackersClientRpc(data);
    }

    [ClientRpc]
    private void UpdateTrackersClientRpc(TrackersData data)
    {
        if (CalibrationFinished && !IsLocalPlayer)
        {
            HMD.transform.position = data.HMDPosition;
            HMD.transform.rotation = data.HMDOrientation;
            LeftHand.transform.position = data.LeftHandPosition;
            LeftHand.transform.rotation = data.LeftHandOrientation;
            RightHand.transform.position = data.RightHandPosition;
            RightHand.transform.rotation = data.RightHandOrientation;
            Pelvis.transform.position = data.PelvisPosition;
            Pelvis.transform.rotation = data.PelvisOrientation;
            LeftFoot.transform.position = data.LeftFootPosition;
            LeftFoot.transform.rotation = data.LeftFootOrientation;
            RightFoot.transform.position = data.RightFootPosition;
            RightFoot.transform.rotation = data.RightFootOrientation;
        }
    }

    [System.Serializable]
    private struct TrackersData
    {
        public Vector3 HMDPosition, LeftHandPosition, RightHandPosition, PelvisPosition, LeftFootPosition, RightFootPosition;
        public Quaternion HMDOrientation, LeftHandOrientation, RightHandOrientation, PelvisOrientation, LeftFootOrientation, RightFootOrientation;
    }

    private IEnumerator WaitForCalibration()
    {
        yield return new WaitUntil(() => GetComponentInChildren<AvatarController_UnityIK>() != null);
        AvatarController_UnityIK controller = GetComponentInChildren<AvatarController_UnityIK>();
        yield return new WaitUntil(() => controller.CalibrationFinished);
        CalibrationFinished = true;
    }
}
