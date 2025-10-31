using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class EditionSubStateMove : EditionSubStateBase, IBasicState
	{
		public const string EventSubStateMoveStarted = "EventSubStateMoveStarted";

		private AssetDefinitionItem _itemDefinition;
		private GameObject _itemWorld;
		private WorldItemData _data;

		private float _factorScale = 2;

		public EditionSubStateMove(AssetDefinitionItem item, WorldItemData data)
        {			
			_itemDefinition = item;
			_data = data;
		}

		public override void Initialize()
		{
			base.Initialize();
			
			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionUpdateTitle, "Move");
			SystemEventController.Instance.DispatchSystemEvent(EventSubStateMoveStarted);

			_itemWorld = AssetBundleController.Instance.CreateGameObject(_itemDefinition.AssetName);
			_itemWorld.GetComponent<Collider>().isTrigger = true;
			_itemWorld.transform.eulerAngles = _data.Rotation;

			if (_itemDefinition.IsHuman)
            {
				_factorScale = 1;
			}
			else
            {
				_factorScale = 2;
			}

			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionViewActivateCancellation, true);
		}

		public override void Destroy()
		{
			base.Destroy();

			_itemDefinition = null;
			_itemWorld = null;
			_data = null;
		}

		protected override void OnSystemEvent(string nameEvent, object[] parameters)
		{
			base.OnSystemEvent(nameEvent, parameters);
		}
		
		private void AddInstance(Vector3 positionCell)
        {
			WorldItemData itemInstance = new WorldItemData();
			itemInstance.Copy(_data);
			itemInstance.Id = -1;
			itemInstance.CatalogId = _itemDefinition.Id;
			itemInstance.IsChair = _itemDefinition.IsChair;
			itemInstance.IsHuman = _itemDefinition.IsHuman;
			itemInstance.IsMan = _itemDefinition.IsMan;

			itemInstance.Position = _itemWorld.transform.position;
			itemInstance.Rotation = _itemWorld.transform.eulerAngles;
			itemInstance.Cell = positionCell;

			GameObject.Destroy(_itemWorld);
			_itemWorld = null;
			
			if (ApplicationController.Instance.LevelView.AddItem(itemInstance, true))
            {
				UIEventController.Instance.DispatchUIEvent(ItemImageCatalog.EventItemImageCatalogUnSelectAll);
			}
			else
            {
				_itemWorld = AssetBundleController.Instance.CreateGameObject(_itemDefinition.AssetName);
				_itemWorld.GetComponent<Collider>().isTrigger = true;
				_itemWorld.transform.eulerAngles = _data.Rotation;
			}
		}

		private Vector3 GetFinalPosition(Vector3 position)
		{
			Vector3 posFinal = new Vector3(position.x, position.y, position.z);
			posFinal.x += _itemWorld.GetComponent<Collider>().bounds.extents.x;
			posFinal.y += _itemWorld.GetComponent<Collider>().bounds.extents.y;
			posFinal.z += _itemWorld.GetComponent<Collider>().bounds.extents.z;

			posFinal.x -= WorkDayData.SIZE_CELL / 2;
			posFinal.z -= WorkDayData.SIZE_CELL / 2;

			return posFinal;
		}

		public override void Run()
		{
			base.Run();

			if (_itemWorld != null)
            {
				GameObject cellCollided = GetMouseCellWorld();
				if (cellCollided != null)
				{
					Vector3 posFinal = GetFinalPosition(cellCollided.transform.position);
					_itemWorld.transform.position = posFinal;

					if (Input.mouseScrollDelta.y != 0)
					{
						if (Input.mouseScrollDelta.y > 0)
						{
							_data.Rotation += new Vector3(0, 90, 0);
						}
						else
						{
							_data.Rotation -= new Vector3(0, 90, 0);
						}
						_itemWorld.transform.eulerAngles = _data.Rotation;
					}

					if (Input.GetMouseButtonDown(0))
					{
						AddInstance(cellCollided.transform.position);
					}
				}
			}
		}
	}
}