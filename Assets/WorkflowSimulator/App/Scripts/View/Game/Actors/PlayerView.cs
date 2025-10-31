using yourvrexperience.Utils;
using yourvrexperience.VR;
using UnityEngine;
using yourvrexperience.Networking;

namespace yourvrexperience.WorkDay
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
	public class PlayerView : MonoBehaviour, ICameraPlayer, INetworkObject
	{
		public const string EventPlayerAppHasStarted = "EventPlayerAppHasStarted";
		public const string EventPlayerAppEnableMovement = "EventPlayerAppEnableMovement";
		public const string EventPlayerViewPositionUpdated = "EventPlayerViewPositionUpdated";
		public const string EventPlayerViewMovePlayerForward = "EventPlayerViewMovePlayerForward";
		public const string EventPlayerViewInitBody = "EventPlayerViewInitBody";
		public const string EventPlayerViewRequestBody = "EventPlayerViewRequestBody";
		public const string EventPlayerViewReleaseGameResources = "EventPlayerViewReleaseGameResources";
		public const string EventPlayerDisconnectParent = "EventPlayerDisconnectParent";
		public const string EventPlayerViewEnableBody = "EventPlayerViewEnableBody";
		public const string EventPlayerViewDisableBody = "EventPlayerViewDisableBody";
		public const string EventPlayerViewEnableMovement = "EventPlayerViewEnableMovement";

		[SerializeField] private GameObject Body;
		[SerializeField] private string NameAssetBody;
		[SerializeField] private float ScaleBody = 1;

		private GameObject _bodyAsset = null;
		private GameObject _bodyContainerNetwork = null;
		private GameObject _bodyNetwork = null;
		private float _rotationY = 0F;
		private Vector3 _forwardCamera = Vector3.zero;
		private bool _enableMovement = true;
		private Camera _camera;
		private Collider _collider;
		private Rigidbody _rigidBody;
		private bool _isOnFloor = true;
		private int _layerFloor = -1;
		private bool _hasBeenInited = false;
		private bool _isVRClient = false;
		private AnimatorSystem _animatorSystem;

		private bool _freeMode = true;

		private int _currentRotation = 0;

		private NetworkObjectID _networkGameID;
		public NetworkObjectID NetworkGameIDView
		{
			get
			{
				if (_networkGameID == null)
				{
					if (this != null)
					{
						_networkGameID = GetComponent<NetworkObjectID>();
					}
				}
				return _networkGameID;
			}
		}

		public string NameNetworkPrefab
		{
			get { return null; }
		}
		public string NameNetworkPath
		{
			get { return null; }
		}
		public bool LinkedToCurrentLevel
		{
			get { return false; }
		}

		public void SetInitData(string initializationData)
		{
		}

		public void OnInitDataEvent(string initializationData)
		{
		}

		public GameObject GetGameObject()
		{
			return this.gameObject;
		}

		public Vector3 PositionCamera
		{
			get { return _camera.transform.position; }
			set { _camera.transform.position = value; }
		}
		public Vector3 ForwardCamera
		{
			get { return _camera.transform.forward; }
			set { _camera.transform.forward = value; }
		}
		public Vector3 PositionBase
		{
			get { return this.transform.position + new Vector3(0, transform.localScale.y, 0); }
		}
		public int CurrentRotation
        {
			get { return _currentRotation; }
			set { _currentRotation = value; }
        }

		public bool IsOwner()
		{
			return true;
		}

		void Awake()
		{
			_collider = this.GetComponent<Collider>();
			_rigidBody = this.GetComponent<Rigidbody>();

			_collider.isTrigger = true;
			_rigidBody.useGravity = false;
			_rigidBody.isKinematic = true;
		}

		void Start()
		{
			SystemEventController.Instance.DispatchSystemEvent(EventPlayerAppHasStarted, this);
		}

		public void Initialize(bool freeMode)
		{
			_freeMode = freeMode;
			_camera = Camera.main;
			_layerFloor = LayerMask.NameToLayer(WorkDayData.LayerFloor);

			SystemEventController.Instance.Event += OnSystemEvent;
			NetworkController.Instance.NetworkEvent += OnNetworkEvent;

			bool shouldRun = true;
			if (ApplicationController.Instance.IsMultiplayer)
			{
				NetworkGameIDView.InitedEvent += OnInitDataEvent;
#if ENABLE_MIRROR
				NetworkGameIDView.RefreshAuthority();
#endif
			}

			if (!ApplicationController.Instance.IsMultiplayer)
			{
				bool isVRMode = false;
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
				isVRMode = true;
#endif

#if UNITY_EDITOR
				isVRMode = true;
#endif
				Body.SetActive(false);
				SystemEventController.Instance.DispatchSystemEvent(CameraXRController.EventCameraPlayerReadyForCamera, this);

				_bodyAsset = new GameObject();
			}
			else
			{
				if (NetworkGameIDView.AmOwner())
				{
					bool isVRMode = false;
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
					isVRMode = true;
#endif

#if UNITY_EDITOR
					isVRMode = true;
#endif
					Body.SetActive(false);
					SystemEventController.Instance.DispatchSystemEvent(CameraXRController.EventCameraPlayerReadyForCamera, this);
					NetworkController.Instance.DelayNetworkEvent(EventPlayerViewInitBody, 0.1f, -1, -1, NetworkController.Instance.UniqueNetworkID, NetworkGameIDView.GetViewID(), NameAssetBody, isVRMode);
				}
				else
				{
					yourvrexperience.Utils.Utilities.EnableRenderers(Body.transform, false);
					NetworkController.Instance.DelayNetworkEvent(EventPlayerViewRequestBody, 1f, -1, -1, NetworkController.Instance.UniqueNetworkID, NetworkGameIDView.GetViewID());
					shouldRun = false;
				}
			}
		}

		void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (NetworkController.Instance != null) NetworkController.Instance.NetworkEvent -= OnNetworkEvent;
			NetworkGameIDView.InitedEvent -= OnInitDataEvent;
		}

		public void ActivatePhysics(bool activation, bool force = false)
		{
			if (_freeMode)
            {
				_collider.isTrigger = true;
				_rigidBody.useGravity = false;
				_rigidBody.isKinematic = true;
			}
			else
            {
				_collider.isTrigger = !activation;
				_rigidBody.useGravity = activation;
				_rigidBody.isKinematic = !activation;
			}
		}

		private void Move()
		{
			float axisVertical = Input.GetAxis("Vertical");
			float axisHorizontal = Input.GetAxis("Horizontal");
			float finalSpeed = WorkDayData.Instance.PlayersDesktopSpeed;
#if (UNITY_ANDROID && !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR || ENABLE_NREAL)) && !UNITY_EDITOR
			finalSpeed = 10;
#endif
			Vector3 forward = axisVertical * _camera.transform.forward * finalSpeed * Time.deltaTime;
			if (_freeMode)
			{
				forward.y = 0;
			}
			Vector3 lateral = axisHorizontal * _camera.transform.right * finalSpeed * Time.deltaTime;
			Vector3 increment = forward + lateral;
			if (!_freeMode)
            {
				increment.y = 0;
			}			
			transform.GetComponent<Rigidbody>().MovePosition(transform.position + increment);
#if (UNITY_ANDROID && !(ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR || ENABLE_NREAL)) && !UNITY_EDITOR
			if (_cameraContainer != null) _cameraContainer.transform.position = this.transform.position;
#else
			_camera.transform.position = this.transform.position;
#endif
		}

		public void RotateCameraWithMouse()
		{
			float rotationX = _camera.transform.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * WorkDayData.Instance.SensitivityCamera;
			_rotationY = _rotationY + Input.GetAxis("Mouse Y") * WorkDayData.Instance.SensitivityCamera;
			_rotationY = Mathf.Clamp(_rotationY, -60, 60);
			Quaternion rotation = Quaternion.Euler(-_rotationY, rotationX, 0);
			_forwardCamera = rotation * Vector3.forward;
			this.transform.forward = new Vector3(_forwardCamera.x, 0, _forwardCamera.z);
			_camera.transform.forward = _forwardCamera;
		}

		public void Jump()
		{
			if (_isOnFloor)
			{
				_isOnFloor = false;
				transform.GetComponent<Rigidbody>().AddForce(Vector3.up * 20, ForceMode.Impulse);
			}
		}

		void OnCollisionEnter(Collision collision)
		{
			if (!_isOnFloor)
			{
				if (collision.gameObject.layer == _layerFloor)
				{
					_isOnFloor = true;
				}
			}
		}

		private void OnNetworkEvent(string nameEvent, int originNetworkID, int targetNetworkID, object[] parameters)
		{
			if (nameEvent.Equals(EventPlayerViewRequestBody))
			{
				int netID = (int)parameters[0];
				int playerNetID = (int)parameters[1];
				if (NetworkGameIDView.GetViewID() == playerNetID)
				{
					if (NetworkGameIDView.AmOwner())
					{
						bool isVRMode = false;
#if (ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR)
						isVRMode = true;
#endif

#if UNITY_EDITOR
						isVRMode = true;
#endif
						NetworkController.Instance.DelayNetworkEvent(EventPlayerViewInitBody, 0.1f, -1, -1, NetworkController.Instance.UniqueNetworkID, NetworkGameIDView.GetViewID(), NameAssetBody, isVRMode);
					}
				}
			}
			if (nameEvent.Equals(EventPlayerViewInitBody))
			{
				int netID = (int)parameters[0];
				int playerNetID = (int)parameters[1];
				string bodyPrefab = (string)parameters[2];
				if (NetworkGameIDView.GetViewID() == playerNetID)
				{
					_isVRClient = (bool)parameters[3];
					if (!NetworkGameIDView.AmOwner())
					{
						if (_bodyAsset == null)
						{
							_bodyAsset = AssetBundleController.Instance.CreateGameObject(bodyPrefab) as GameObject;
							_bodyAsset.transform.localScale = new Vector3(ScaleBody, ScaleBody, ScaleBody);
#if UNITY_EDITOR
							yourvrexperience.Utils.Utilities.ResetMaterials(_bodyAsset);
#endif
							_bodyAsset.transform.parent = Body.transform;
							_bodyAsset.transform.localPosition = Vector3.zero;
							_bodyAsset.transform.rotation = Body.transform.rotation;							
						}
					}
				}
			}
		}

		public Vector3 RayCastFloor()
        {
			RaycastHit collision = new RaycastHit();
			Vector3 posCollision = RaycastingTools.GetRaycastOriginForward(this.transform.position, this.transform.forward, ref collision, 1000, LayerMask.GetMask(WorkDayData.LayerFloor));
			return posCollision;
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventPlayerViewEnableBody))
			{
				if (NetworkGameIDView.AmOwner())
				{
					bool activate = (bool)parameters[0];
					Body.SetActive(activate);
				}
			}
			if (nameEvent.Equals(EventPlayerViewDisableBody))
			{
				if (NetworkGameIDView.GetViewID() == (int)parameters[0])
				{
					Body.SetActive(false);
					if (_bodyAsset != null) GameObject.Destroy(_bodyAsset);
				}
			}
			if (nameEvent.Equals(LevelView.EventLevelViewDestroy))
			{
				ActivatePhysics(false);
			}
			if (nameEvent.Equals(CameraXRController.EventCameraResponseToPlayer))
			{
				_camera = (Camera)parameters[0];
			}
			if (nameEvent.Equals(EventPlayerAppEnableMovement))
			{				
				_enableMovement = (bool)parameters[0];
			}
			if (nameEvent.Equals(SystemEventController.EventSystemEventControllerDontDestroyOnLoad))
			{
				DontDestroyOnLoad(this.gameObject);
			}
			if (nameEvent.Equals(EventPlayerViewReleaseGameResources))
			{
				GameObject.Destroy(this.gameObject);
			}
			if (nameEvent.Equals(LevelView.EventLevelViewStarted))
			{
				if (!_hasBeenInited)
				{
					_hasBeenInited = true;

					LevelView levelView = (LevelView)parameters[0];
					transform.position = levelView.InitialPosition.transform.position;
					transform.rotation = levelView.InitialPosition.transform.rotation;
					SystemEventController.Instance.DispatchSystemEvent(EventPlayerViewPositionUpdated);

					if (!ApplicationController.Instance.IsMultiplayer)
					{
						ActivatePhysics(true);
					}
				}
				else
				{
					ActivatePhysics(true);
				}
			}
			if (nameEvent.Equals(EventPlayerDisconnectParent))
			{
				_hasBeenInited = true;
				this.transform.parent = null;
			}
		}

		public void Run()
		{
			if (!_enableMovement) return;

			bool runLogic = true;
			if (ApplicationController.Instance.IsMultiplayer)
			{
				runLogic = NetworkGameIDView.AmOwner();
			}

			if (runLogic)
			{
				UpdatePosition();
			}
		}

		public void UpdatePosition()
        {
			if (_freeMode)
			{
				_camera.transform.position = this.transform.position;
				_camera.transform.forward = this.transform.forward;
			}
			else
			{
				this.transform.forward = new Vector3(_camera.transform.position.x, 0, _camera.transform.position.z);
				_camera.transform.position = this.transform.position;
				RotateCameraWithMouse();
			}
		}

		void Update()
		{
			if ((NetworkGameIDView != null) && ApplicationController.Instance.IsMultiplayer)
			{
				if (!NetworkGameIDView.AmOwner())
				{
					Quaternion rotateFaceRigth = Quaternion.LookRotation(new Vector3(this.transform.forward.x, 0, this.transform.forward.z));
					Body.transform.localRotation = Quaternion.Inverse(this.transform.rotation);
					if (_bodyAsset != null) _bodyAsset.transform.localRotation = rotateFaceRigth;
				}
			}
		}
	}
}
