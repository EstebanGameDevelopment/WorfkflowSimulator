using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class EditionSubStateAreas : EditionSubStateBase, IBasicState
	{
		public const string EventSubStateAreasActivated = "EventSubStateAreasActivated";

		public enum AreaMode { Work = 0, Meeting, Kitchen, Bathroom, Exit }

		private bool _isBuilding = false;
		private Vector3 _anchorPosition;
		private Vector3 _lastAnchor;

		private AreaMode _areaMode;
		private GameObject _areaGO;

		private bool _enableSelect = false;
		private bool _enableAdding = false;
		private bool _enableRemoving = false;

		private AreaData _selectedAreaData = null;

		public EditionSubStateAreas(AreaMode areaMode)
		{
			_areaMode = areaMode;
		}

		public override void Initialize()
		{
			base.Initialize();

			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionUpdateTitle, "Areas");

			UIEventController.Instance.Event += OnUIEvent;

			UIEventController.Instance.DispatchUIEvent(EventSubStateAreasActivated, true);
			UIEventController.Instance.DispatchUIEvent(TabEditionResizeView.EventTabEditionResizeViewSelectArea);
			
			ApplicationController.Instance.LevelView.RenderAreas(_areaMode);

			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionViewActivateCancellation, false);
		}

		public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

			UIEventController.Instance.DispatchUIEvent(EventSubStateAreasActivated, false);
			ApplicationController.Instance.LevelView.HideAreas();
		}

		private void CreateArea(Vector3 startingPosition, Vector3 endingPosition)
        {
			if (_areaGO != null)
			{
				GameObject.Destroy(_areaGO);
			}

			Vector3 centerPosition = (startingPosition + endingPosition) / 2;
			Vector3 sizeArea = startingPosition - endingPosition;

			_areaGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
			_areaGO.transform.position = centerPosition;
			_areaGO.transform.localScale = new Vector3(Mathf.Abs(sizeArea.x), 0.3f, Mathf.Abs(sizeArea.z));
			switch (_areaMode)
            {
				case AreaMode.Work:
					ApplicationController.Instance.ApplyColor(_areaGO.GetComponent<Renderer>(), Color.blue);
					break;
				case AreaMode.Meeting:
                    ApplicationController.Instance.ApplyColor(_areaGO.GetComponent<Renderer>(), Color.magenta);
					break;
				case AreaMode.Kitchen:
                    ApplicationController.Instance.ApplyColor(_areaGO.GetComponent<Renderer>(), Color.cyan);
					break;
				case AreaMode.Bathroom:
                    ApplicationController.Instance.ApplyColor(_areaGO.GetComponent<Renderer>(), Color.green);
					break;
				case AreaMode.Exit:
                    ApplicationController.Instance.ApplyColor(_areaGO.GetComponent<Renderer>(), Color.white);
					break;
			}
			_areaGO.layer = LayerMask.NameToLayer(WorkDayData.LayerArea);
			_areaGO.GetComponent<Collider>().isTrigger = true;
		}

		protected override void OnSystemEvent(string nameEvent, object[] parameters)
		{
			base.OnSystemEvent(nameEvent, parameters);

			if (nameEvent.Equals(EditionSubStateIdle.EventSubStateIdleCancelCurrentSelection))
			{
				_selectedAreaData = null;
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewSelectArea))
			{
				_enableSelect = true;
				_enableAdding = false;
				_enableRemoving = false;
				_isBuilding = false;
			}
			if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewAddArea))
			{
				_enableSelect = false;
				_enableAdding = true;
				_enableRemoving = false;
				_isBuilding = false;
			}
			if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewRemoveArea))
			{
				_enableSelect = false;
				_enableAdding = false;
				_enableRemoving = true;
				_isBuilding = false;
			}
			if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewWork))
			{
				if ((bool)parameters[0])
				{
					_areaMode = AreaMode.Work;
					ApplicationController.Instance.LevelView.RenderAreas(_areaMode);
				}
				else
				{
					ApplicationController.Instance.LevelView.HideAreas();
				}
			}
			if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewMeeting))
			{
				if ((bool)parameters[0])
				{
					_areaMode = AreaMode.Meeting;
					ApplicationController.Instance.LevelView.RenderAreas(_areaMode);
				}
				else
				{
					ApplicationController.Instance.LevelView.HideAreas();
				}
			}
			if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewKitchen))
			{
				if ((bool)parameters[0])
				{
					_areaMode = AreaMode.Kitchen;
					ApplicationController.Instance.LevelView.RenderAreas(_areaMode);
				}
				else
				{
					ApplicationController.Instance.LevelView.HideAreas();
				}
			}
			if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewBathroom))
			{
				if ((bool)parameters[0])
				{
					_areaMode = AreaMode.Bathroom;
					ApplicationController.Instance.LevelView.RenderAreas(_areaMode);
				}
				else
				{
					ApplicationController.Instance.LevelView.HideAreas();
				}
			}
			if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewExit))
			{
				if ((bool)parameters[0])
				{
					_areaMode = AreaMode.Exit;
					ApplicationController.Instance.LevelView.RenderAreas(_areaMode);
				}
				else
				{
					ApplicationController.Instance.LevelView.HideAreas();
				}
			}
		}

		private void RunSelect()
        {
			if (RunCameraMovement())
			{
				Vector3 positionMouse = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y);
				if (Vector2.Distance(_anchorMouse, positionMouse) < 5)
				{
					GameObject areaGO = GetMouseAreaWorld();
					if (areaGO != null)
					{
						AreaData areaData = ApplicationController.Instance.LevelView.GetArea(areaGO);
						if (areaData != null)
						{
							_selectedAreaData = areaData;
							SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDSelectionObject, areaGO, areaData);
						}
					}
				}
			}
		}

		private void RunAdd()
        {
			if (!_isBuilding)
			{
				if (Input.GetMouseButtonDown(0))
				{
					Vector3 position = GetMouseFloorWorldPosition();
					if (position != Vector3.zero)
					{
						_isBuilding = true;
						_anchorPosition = position;
						_lastAnchor = position;
					}
				}
			}
			else
			{
				Vector3 position = GetMouseFloorWorldPosition();
				if (Input.GetMouseButtonUp(0))
				{
					_isBuilding = false;
					if (_areaGO != null)
					{
						GameObject.Destroy(_areaGO);
					}

					string nameArea = _areaMode.ToString() + "_" + ApplicationController.Instance.LevelView.Areas.Count;
					AreaData newArea = new AreaData(nameArea, _anchorPosition, position, (int)_areaMode);
					ApplicationController.Instance.LevelView.AddArea(newArea, true);

					UIEventController.Instance.DispatchUIEvent(TabEditionResizeView.EventTabEditionResizeViewSelectArea);
				}
				else
				{
					if (position != Vector3.zero)
					{
						_lastAnchor = position;
						CreateArea(_anchorPosition, position);
					}
				}
			}
		}

		private void RunRemove()
        {
			if (Input.GetMouseButtonDown(0))
            {
				GameObject areaGO = GetMouseAreaWorld();
				if (areaGO != null)
				{
					ApplicationController.Instance.LevelView.RemoveArea(areaGO);
				}
			}
		}

		public override void Run()
		{
			base.Run();

			if (_enableSelect)
            {
				ApplicationController.Instance.PlayerView.Run();
				RunSelect();				
			}
			if (_enableAdding)
            {
				RunAdd();
			}
			if (_enableRemoving)
			{
				RunRemove();
			}
		}
	}
}