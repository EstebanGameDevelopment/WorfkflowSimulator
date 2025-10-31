using UnityEngine;
using yourvrexperience.Utils;
using UnityEngine.EventSystems;

namespace yourvrexperience.WorkDay
{
	public class EditionSubStateBase : IBasicState
	{
		public const string EventSubStateBaseEnableMovement = "EventSubStateBaseEnableMovement";

		protected int _maskFloor;
		protected int _maskCell;
		protected int _maskItem;
		protected int _maskArea;

		protected bool _isPressed = false;
		protected bool _isMoving = false;
		protected Vector3 _anchorCamera;
		protected Vector3 _anchorMouse;

		private bool _enableMovement = true;
		private bool _enableZoom = true;
		private Vector3 _lastPositionInside = Vector3.zero;

		public virtual void Initialize()
		{
			_maskFloor = LayerMask.GetMask(WorkDayData.LayerFloor);
			_maskCell = LayerMask.GetMask(WorkDayData.LayerCell);
			_maskItem = LayerMask.GetMask(WorkDayData.LayerItem) | LayerMask.GetMask(WorkDayData.LayerHuman) | LayerMask.GetMask(WorkDayData.LayerChair);
			_maskArea = LayerMask.GetMask(WorkDayData.LayerArea);

			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;

			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionUpdateTitle, "Idle");
		}

		public virtual void Destroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
		}

        private void OnUIEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(ScreenFinalRequestAIView.EventScreenFinalRequestAIViewOpened))
			{
				_enableZoom = false;
            }
            if (nameEvent.Equals(ScreenFinalRequestAIView.EventScreenFinalRequestAIViewClosed))
            {
                _enableZoom = true;
            }
        }

        protected virtual void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventSubStateBaseEnableMovement))
            {				
				_enableMovement = (bool)parameters[0];
			}
		}

		protected Vector3 GetMouseFloorWorldPosition()
        {
			bool worldIntection = true;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
			if (Input.touchCount > 0)
			{
				Touch touch = Input.GetTouch(0);
				if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
				{
					worldIntection = false;
				}
			}
#else
			if (EventSystem.current.IsPointerOverGameObject())
            {
				worldIntection = false;
			}
#endif
			if (worldIntection)
            {
				RaycastHit collisionPoint = new RaycastHit();
				Vector3 position = RaycastingTools.GetMouseCollisionPoint(Camera.main, ref collisionPoint, _maskFloor);
				return position;
			}
			else
            {
				return Vector3.zero;
            }
		}

		protected GameObject GetMouseCellWorld()
		{
			bool worldIntection = true;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
			if (Input.touchCount > 0)
			{
				Touch touch = Input.GetTouch(0);
				if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
				{
					worldIntection = false;
				}
			}
#else
			if (EventSystem.current.IsPointerOverGameObject())
			{
				worldIntection = false;
			}
#endif
			if (worldIntection)
			{
				RaycastHit collisionPoint = new RaycastHit();
				GameObject cellCollided = RaycastingTools.GetMouseCollisionObject(Camera.main, ref collisionPoint, _maskCell);
				return cellCollided;
			}
			else
			{
				return null;
			}
		}

		protected GameObject GetMouseItemWorld()
		{
			bool worldIntection = true;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
			if (Input.touchCount > 0)
			{
				Touch touch = Input.GetTouch(0);
				if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
				{
					worldIntection = false;
				}
			}
#else
			if (EventSystem.current.IsPointerOverGameObject())
			{
				worldIntection = false;
			}
#endif
			if (worldIntection)
			{
				RaycastHit collisionPoint = new RaycastHit();
				GameObject cellCollided = RaycastingTools.GetMouseCollisionObject(Camera.main, ref collisionPoint, _maskItem);
				return cellCollided;
			}
			else
			{
				return null;
			}
		}

		protected GameObject GetMouseAreaWorld()
		{
			bool worldIntection = true;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
			if (Input.touchCount > 0)
			{
				Touch touch = Input.GetTouch(0);
				if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
				{
					worldIntection = false;
				}
			}
#else
			if (EventSystem.current.IsPointerOverGameObject())
			{
				worldIntection = false;
			}
#endif
			if (worldIntection)
			{
				RaycastHit collisionPoint = new RaycastHit();
				GameObject areaCollided = RaycastingTools.GetMouseCollisionObject(Camera.main, ref collisionPoint, _maskArea);
				return areaCollided;
			}
			else
			{
				return null;
			}
		}

		protected void MoveCameraPosition(Vector3 positionMouse)
		{
			Vector3 anchor = new Vector3();
			Vector3 pos = new Vector3();

			switch (ApplicationController.Instance.PlayerView.CurrentRotation)
			{
				case 0:
					anchor = new Vector3(_anchorMouse.x, 0, _anchorMouse.z);
					pos = new Vector3(positionMouse.x, 0, positionMouse.z);
					break;
				case 90:
					anchor = new Vector3(_anchorMouse.z, 0, -_anchorMouse.x);
					pos = new Vector3(positionMouse.z, 0, -positionMouse.x);
					break;
				case 180:
					anchor = new Vector3(-_anchorMouse.x, 0, -_anchorMouse.z);
					pos = new Vector3(-positionMouse.x, 0, -positionMouse.z);
					break;
				case 270:
					anchor = new Vector3(-_anchorMouse.z, 0, _anchorMouse.x);
					pos = new Vector3(-positionMouse.z, 0, positionMouse.x);
					break;
			}

			_lastPositionInside = ApplicationController.Instance.PlayerView.transform.position;
			Vector3 newPos = _anchorCamera + ((anchor - pos) / 100);
			ApplicationController.Instance.PlayerView.transform.position = newPos;

			RaycastHit collisionPoint = new RaycastHit();
			Vector3 posFloor = RaycastingTools.GetRaycastOriginForward(ApplicationController.Instance.PlayerView.transform.position, ApplicationController.Instance.PlayerView.transform.forward, ref collisionPoint, Mathf.Infinity, _maskFloor);
			if (!ApplicationController.Instance.LevelView.CheckPositionInside(posFloor))
            {
				ApplicationController.Instance.PlayerView.transform.position = _lastPositionInside;
			}
		}

		protected bool RunCameraMovement()
		{
			if (!_enableMovement) return true;

			if (!_isMoving)
			{
				if (_enableZoom)
				{
                    if (Input.mouseScrollDelta.y != 0)
                    {
                        float zoom = (Input.mouseScrollDelta.y > 0 ? 1 : -1);
                        Vector3 posCam = ApplicationController.Instance.PlayerView.transform.position;
                        ApplicationController.Instance.PlayerView.transform.position += zoom * ApplicationController.Instance.PlayerView.transform.forward;
                        if (ApplicationController.Instance.PlayerView.transform.position.y < 2f)
                        {
                            ApplicationController.Instance.PlayerView.transform.position = new Vector3(posCam.x, 2f, posCam.z);
                        }
                        else
                        if (ApplicationController.Instance.PlayerView.transform.position.y > 13f)
                        {
                            ApplicationController.Instance.PlayerView.transform.position = new Vector3(posCam.x, 13, posCam.z);
                        }
                    }
                }

                if (Input.GetMouseButtonDown(0))
				{
					Vector3 position = GetMouseFloorWorldPosition();
					if (position != Vector3.zero)
					{
						_anchorCamera = ApplicationController.Instance.PlayerView.transform.position;
						_anchorMouse = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y);
						_isPressed = true;
					}
				}
				if (_isPressed)
                {
					if (Input.GetMouseButton(0))
					{
						Vector3 positionMouse = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y);
						if (Vector3.Distance(_anchorMouse, positionMouse) > 2)
						{
							_isMoving = true;
						}
					}
					if (Input.GetMouseButtonUp(0))
                    {
						_isPressed = false;
						return true;
					}
				}
			}
			else
			{
				if (Input.GetMouseButtonUp(0))
				{
					_isMoving = false;
					_isPressed = false;

					return true;
				}
				else
				{
					Vector3 position = GetMouseFloorWorldPosition();					
					if (position != Vector3.zero)
					{
						Vector3 positionMouse = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y);
						MoveCameraPosition(positionMouse);
					}
				}
			}
			return false;
		}

		public virtual void Run()
		{
		}
	}
}