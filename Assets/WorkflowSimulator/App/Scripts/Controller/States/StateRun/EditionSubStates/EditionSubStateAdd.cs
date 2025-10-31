using UnityEngine;
using yourvrexperience.Utils;
using UnityEngine.AI;

namespace yourvrexperience.WorkDay
{
	public class EditionSubStateAdd : EditionSubStateBase, IBasicState
	{
		protected AssetDefinitionItem _itemDefinition;
		protected GameObject _itemWorld;

		protected bool _building = false;
		protected Vector3 _rotation;
		protected Vector3 _backUpPosition;
		protected float _factorScale = 2;

		public override void Initialize()
		{
			base.Initialize();

			_itemWorld = AssetBundleController.Instance.CreateGameObject(_itemDefinition.AssetName);
			DisableColliders();

			_rotation = Vector3.zero;

			UIEventController.Instance.DispatchUIEvent(ScreenPanelEditionView.EventScreenPanelEditionViewActivateCancellation, true);
		}

		public override void Destroy()
		{
			base.Destroy();

			_itemDefinition = null;
			if (_itemWorld != null)
			{
				GameObject.Destroy(_itemWorld);
				_itemWorld = null;
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

		private void DisableColliders()
        {
			if (_itemWorld.GetComponent<NavMeshAgent>() != null)
			{
				_itemWorld.GetComponent<NavMeshAgent>().enabled = false;
			}
			if (_itemWorld.GetComponent<Collider>() != null)
			{
				_itemWorld.GetComponent<Collider>().isTrigger = true;
			}
		}

		protected override void OnSystemEvent(string nameEvent, object[] parameters)
		{
			base.OnSystemEvent(nameEvent, parameters);
		}

		protected void AddInstanceAndContinue(Vector3 positionCell)
		{
			WorldItemData itemInstance = new WorldItemData();
			itemInstance.Id = -1;			
			itemInstance.Name = _itemDefinition.Name + "_" + ApplicationController.Instance.LevelView.Items.Count;
			itemInstance.CatalogId = _itemDefinition.Id;
			itemInstance.IsChair = _itemDefinition.IsChair;
			itemInstance.IsHuman = _itemDefinition.IsHuman;
			itemInstance.IsMan = _itemDefinition.IsMan;

			itemInstance.Position = _backUpPosition;
			itemInstance.Rotation = _rotation;
			itemInstance.Cell = positionCell;

			GameObject.Destroy(_itemWorld);
			_itemWorld = null;
			ApplicationController.Instance.LevelView.AddItem(itemInstance, true);

			_itemWorld = AssetBundleController.Instance.CreateGameObject(_itemDefinition.AssetName);
			DisableColliders();
			_itemWorld.transform.position = _backUpPosition;
			_itemWorld.transform.eulerAngles = _rotation;
		}

		private void PreparePlacement()
        {
			if (_itemWorld != null)
			{
				GameObject.Destroy(_itemWorld);
				_itemWorld = null;
			}
			_itemWorld = AssetBundleController.Instance.CreateGameObject(_itemDefinition.AssetName);
			DisableColliders();
			_itemWorld.transform.position = _backUpPosition;
			_itemWorld.transform.eulerAngles = _rotation;
		}

		protected void RunPlacement()
		{
			GameObject cellCollided = GetMouseCellWorld();
			if (cellCollided != null)
			{
				if (!_building)
				{
					if (_itemWorld != null)
					{
						Vector3 posFinal = GetFinalPosition(cellCollided.transform.position);						
						_itemWorld.transform.position = posFinal;

						if (Input.mouseScrollDelta.y != 0)
						{
							if (Input.mouseScrollDelta.y > 0)
							{
								_rotation += new Vector3(0, 90, 0);
							}
							else
							{
								_rotation -= new Vector3(0, 90, 0);
							}
							_itemWorld.transform.eulerAngles = _rotation;
						}
						_backUpPosition = _itemWorld.transform.position;
					}
				}
				if (Input.GetMouseButtonDown(0))
				{
					_building = true;

					PreparePlacement();
					AddInstanceAndContinue(cellCollided.transform.position);
				}
				else
				{
					if (_building && (_itemWorld != null))
					{
						if (Input.GetMouseButtonUp(0))
						{
							GameObject.Destroy(_itemWorld);
							_itemWorld = null;
							_building = false;

							PreparePlacement();
						}
						else
						{
							if (!ApplicationController.Instance.LevelView.CheckCellUsed(cellCollided.transform.position))
							{
								Vector3 posFinal = GetFinalPosition(cellCollided.transform.position);
								_itemWorld.transform.position = posFinal;
								_backUpPosition = _itemWorld.transform.position;
								PreparePlacement();
								AddInstanceAndContinue(cellCollided.transform.position);
							}
						}
					}
				}				
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				UIEventController.Instance.DispatchUIEvent(ItemImageCatalog.EventItemImageCatalogUnSelectAll);
			}
		}
	}
}