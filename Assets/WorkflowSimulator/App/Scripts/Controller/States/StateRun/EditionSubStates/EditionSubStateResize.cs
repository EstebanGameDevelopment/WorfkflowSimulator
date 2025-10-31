using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class EditionSubStateResize : EditionSubStateBase, IBasicState
	{
		private bool _isBuilding = false;
		private Vector3 _anchorPosition;
		private Vector3 _lastAnchor;
		
		public override void Initialize()
		{
			base.Initialize();

			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionUpdateTitle, "Resize");

			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionViewActivateCancellation, false);
		}

		public override void Destroy()
		{
			base.Destroy();
		}

		protected override void OnSystemEvent(string nameEvent, object[] parameters)
		{
			base.OnSystemEvent(nameEvent, parameters);
		}
		
		public override void Run()
		{
			base.Run();

			if (!_isBuilding)
			{
				if (Input.mouseScrollDelta.y != 0)
				{
					float zoom = (Input.mouseScrollDelta.y > 0 ? 1 : -1);
					ApplicationController.Instance.PlayerView.transform.position += zoom * ApplicationController.Instance.PlayerView.transform.forward;
				}

				if (Input.GetMouseButtonDown(0))
				{
					Vector3 position = GetMouseFloorWorldPosition();
					if (position != Vector3.zero)
					{
						_isBuilding = true;
						_anchorPosition = position;
						_lastAnchor = position;

						ApplicationController.Instance.LevelView.DestroyAllCells();
						ApplicationController.Instance.LevelView.DestroyAllItems();
						ApplicationController.Instance.LevelView.DestroyAllAreas();
						ApplicationController.Instance.LevelView.DestroyWorld();
					}
				}
			}
			else
			{
				Vector3 position = GetMouseFloorWorldPosition();
				if (Input.GetMouseButtonUp(0))
				{
					_isBuilding = false;
					float heightCell = ApplicationController.Instance.LevelView.CreateCells(_anchorPosition, position);
					ApplicationController.Instance.LevelView.RenderWorld();
				}
				else
				{
					if (position != Vector3.zero)
					{
						if (Vector3.Distance(_lastAnchor, position) > WorkDayData.SIZE_CELL)
                        {
							_lastAnchor = position;
							ApplicationController.Instance.LevelView.CreateCells(_anchorPosition, position);
						}
					}
				}
			}
		}
	}
}