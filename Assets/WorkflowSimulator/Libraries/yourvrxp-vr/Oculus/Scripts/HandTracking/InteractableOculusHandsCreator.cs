#if ENABLE_OCULUS
using Oculus.Interaction;
using OculusSampleFramework;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace yourvrexperience.VR
{
	/// <summary>
	/// Spawns all interactable tools that are specified for a scene.
	/// </summary>
	public class InteractableOculusHandsCreator : MonoBehaviour
	{
		public const string EventInteractableOculusHandsCreatorStarted = "EventInteractableOculusHandsCreatorStarted";

		public Transform CameraRig;
		[SerializeField] private Transform[] LeftHandTools = null;
		[SerializeField] private Transform[] RightHandTools = null;

#if ENABLE_OCULUS
		private bool _initedLeft = false;
		private bool _initedRight = false;
		private bool _initedGeneral = false;
		private Vector3 _rotationAcumulated = Vector3.zero;

		private List<Transform> m_toolInstances = new List<Transform>();

		void Start()
		{
			VRInputController.Instance.DispatchVREvent(EventInteractableOculusHandsCreatorStarted, this);
		}

		public void Initialize()
		{
			CameraRig = OculusController.Instance.transform;

			if (!_initedGeneral)
            {
				_initedGeneral = true;
				VRInputController.Instance.Event += OnVREvent;
			}

			if (LeftHandTools != null && LeftHandTools.Length > 0 && !_initedLeft)
			{
				_initedLeft = true;
				StartCoroutine(AttachToolsToHands(LeftHandTools, false));
			}

			if (RightHandTools != null && RightHandTools.Length > 0 && !_initedRight)
			{
				_initedRight = true;
				StartCoroutine(AttachToolsToHands(RightHandTools, true));
			}
		}

		void OnDestroy()
        {
			if (VRInputController.Instance != null) VRInputController.Instance.Event -= OnVREvent;
		}

        private IEnumerator AttachToolsToHands(Transform[] toolObjects, bool isRightHand)
		{
			HandsManager handsManagerObj = null;
			while ((handsManagerObj = HandsManager.Instance) == null || !handsManagerObj.IsInitialized())
			{
				yield return null;
			}

			// create set of tools per hand to be safe
			HashSet<Transform> toolObjectSet = new HashSet<Transform>();
			foreach (Transform toolTransform in toolObjects)
			{
				toolObjectSet.Add(toolTransform.transform);
			}

			foreach (Transform toolObject in toolObjectSet)
			{
				OVRSkeleton handSkeletonToAttachTo =
				  isRightHand ? handsManagerObj.RightHandSkeleton : handsManagerObj.LeftHandSkeleton;
				while (handSkeletonToAttachTo == null || handSkeletonToAttachTo.Bones == null)
				{
					yield return null;
				}

				AttachToolToHandTransform(toolObject, isRightHand);
			}
		}

		private void OnVREvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(OculusHandsManager.EventOculusHandsManagerRotationCameraApplied))
            {
				bool shouldSet = (bool)parameters[0];
                Vector3 rotationApplied = (Vector3)parameters[1];
				if (shouldSet)
				{
					_rotationAcumulated = rotationApplied;
				}
				else
				{
					_rotationAcumulated += rotationApplied;
				}
				foreach (Transform tool in m_toolInstances)
                {
					if (shouldSet)
					{
						tool.rotation = Quaternion.identity;
					}
					tool.Rotate(rotationApplied);
				}
			}
		}

		private void AttachToolToHandTransform(Transform tool, bool isRightHanded)
		{
			var newTool = Instantiate(tool).transform;
			newTool.SetParent(CameraRig, false);
			newTool.localPosition = Vector3.zero;
			PinchInteractionTool toolComp = newTool.GetComponent<PinchInteractionTool>();
			toolComp.IsRightHandedTool = isRightHanded;
			// Initialize only AFTER settings have been applied!
			toolComp.Initialize();
			toolComp.RotationAcumulated = _rotationAcumulated;
			newTool.GetComponentInChildren<FingerInteractionRadius>().Hand = (isRightHanded ? XR_HAND.right : XR_HAND.left);
			m_toolInstances.Add(newTool);
			// Debug.LogError("HANDS FULLY INITIALIZED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
		}
#endif
	}
}