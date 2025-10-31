using UnityEngine;
using UnityEngine.EventSystems;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class EditionSubStateIdle : EditionSubStateBase, IBasicState
	{
		public const string EventSubStateIdleCancelCurrentSelection = "EventSubStateIdleCancelCurrentSelection";
		public const string EventSubStateIdleRequestHumanToGo = "EventSubStateIdleRequestHumanToGo";
		public const string EventSubStateIdleResponseToGoPosition = "EventSubStateIdleResponseToGoPosition";
		public const string EventSubStateIdleRotateCamera = "EventSubStateIdleRotateCamera";
		public const string EventSubStateIdleEnableWorldSelection = "EventSubStateIdleEnableWorldSelection";
		
		private WorldItemData _itemWorldSelected = null;

		private Transform _target;
		private Vector3 _position;
		private float _rotationSpeed;		
		private float _duration;
		private float _elapsed;

		private bool _requestHumanToGo = false;
		private bool _enableWorldSelection = false;

		public override void Initialize()
		{
			base.Initialize();

			_isMoving = false;
			_enableWorldSelection = true;

			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionUpdateTitle, "Idle");

			UIEventController.Instance.Event += OnUIEvent;
		}

        public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

			_target = null;
		}

		private float _angleRest = 0;

		private void RotateAroundPoint()
		{
			if (_target != null)
            {
				if (_elapsed < _duration)
				{
					float step = _rotationSpeed * Time.deltaTime;
					_angleRest -= Mathf.Abs(step);
					_target.RotateAround(_position, Vector3.up, step);

					_elapsed += Time.deltaTime;
				}
				else
				{
					_target.RotateAround(_position, Vector3.up, ((_rotationSpeed > 0)?_angleRest:-_angleRest));
					_target = null;
					UIEventController.Instance.DispatchUIEvent(ScreenBlockerView.EventScreenBlockerViewDestroy);
				}
			}
		}

		protected override void OnSystemEvent(string nameEvent, object[] parameters)
		{
			base.OnSystemEvent(nameEvent, parameters);

			if (nameEvent.Equals(TimeHUD.EventTimeHUDCancelSelectionObject))
            {
				_requestHumanToGo = false;
			}
			if (nameEvent.Equals(EventSubStateIdleRequestHumanToGo))
            {
				_requestHumanToGo = true;
            }
			if (nameEvent.Equals(EventSubStateIdleCancelCurrentSelection))
            {
				_itemWorldSelected = null;
			}
			if (nameEvent.Equals(EventSubStateIdleRotateCamera))
			{
				bool direction = (bool)parameters[0];
				RaycastHit collisionPoint = new RaycastHit();
				_position = RaycastingTools.GetRaycastOriginForward(ApplicationController.Instance.PlayerView.transform.position, ApplicationController.Instance.PlayerView.transform.forward, ref collisionPoint, Mathf.Infinity, _maskFloor);
				_target = ApplicationController.Instance.PlayerView.transform;
				if (direction)
                {
					_rotationSpeed = 90;
					ApplicationController.Instance.PlayerView.CurrentRotation = (ApplicationController.Instance.PlayerView.CurrentRotation + (int)_rotationSpeed) % 360;
				}
				else
                {
					_rotationSpeed = -90;
					int finalRot = (ApplicationController.Instance.PlayerView.CurrentRotation + (int)_rotationSpeed);
					if (finalRot < 0)
                    {
						finalRot = 360 + finalRot;
					}
					ApplicationController.Instance.PlayerView.CurrentRotation = finalRot % 360;
				}				
				_duration = 1f;
				_elapsed = 0;
				_angleRest = 90;

				UIEventController.Instance.DispatchUIEvent(ScreenBlockerView.EventScreenBlockerViewDestroy);
				ScreenController.Instance.CreateScreen(ScreenBlockerView.ScreenName, false, false);
			}
			if (nameEvent.Equals(EventSubStateIdleEnableWorldSelection))
            {				
				_enableWorldSelection = (bool)parameters[0];
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
		}
		
		public override void Run()
		{
			base.Run();

			RotateAroundPoint();

			ApplicationController.Instance.PlayerView.Run();

			if (_requestHumanToGo)
            {
				if (!EventSystem.current.IsPointerOverGameObject())
                {
					if (Input.GetMouseButtonUp(0) && !_isMoving && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
					{
						GameObject itemTarget = GetMouseItemWorld();
						if (itemTarget != null)
						{
							SystemEventController.Instance.DispatchSystemEvent(EventSubStateIdleResponseToGoPosition, itemTarget);
						}
						else
						{
							Vector3 positionToGo = GetMouseFloorWorldPosition();
							SystemEventController.Instance.DispatchSystemEvent(EventSubStateIdleResponseToGoPosition, positionToGo);
						}
					}
				}
			}

			if (RunCameraMovement())
			{
				if (_enableWorldSelection)
                {
					Vector3 positionMouse = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y);
					if (Vector2.Distance(_anchorMouse, positionMouse) < 5)
					{
						GameObject selecteItemGO = GetMouseItemWorld();
						_itemWorldSelected = ApplicationController.Instance.LevelView.GetItem(selecteItemGO);
						if (_itemWorldSelected != null)
						{
							if (_requestHumanToGo && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                            {
								if (ApplicationController.Instance.SelectedHuman != null)
								{
									if (_itemWorldSelected.Name.Equals(ApplicationController.Instance.SelectedHuman.NameHuman))
									{
										_requestHumanToGo = false;
										SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject);
										SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);
										return;
									}
								}
							}
							else
                            {
								if (ApplicationController.Instance.SelectedHuman != null)
								{
									if (_itemWorldSelected.Name.Equals(ApplicationController.Instance.SelectedHuman.NameHuman))
									{
										SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject);
										return;
									}
								}
								SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDSelectionObject, selecteItemGO, _itemWorldSelected, true);
								if (selecteItemGO.GetComponent<HumanView>() != null)
								{
									if (ApplicationController.Instance.HumanPlayer != null)
									{
										if (ApplicationController.Instance.HumanPlayer.NameHuman.Equals(selecteItemGO.GetComponent<HumanView>().NameHuman))
										{
											_requestHumanToGo = true;
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}