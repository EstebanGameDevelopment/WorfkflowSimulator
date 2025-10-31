using yourvrexperience.Utils;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static yourvrexperience.WorkDay.EditionSubStateAreas;
using VolumetricLines;
using yourvrexperience.VR;

namespace yourvrexperience.WorkDay
{
	public class LevelView : MonoBehaviour
	{
		public const string EventLevelViewStarted = "EventLevelViewStarted";
		public const string EventLevelViewDestroy = "EventLevelViewDestroy";
		public const string EventLevelViewDelayedReset = "EventLevelViewDelayedReset";
		public const string EventLevelViewLinesRequestDestroy = "EventLevelViewLinesRequestDestroy";		

		public enum RelationshipColleague { GOOD = 0, AVERAGE = 1, BAD = 2, NEUTRAL = 3 };

		[SerializeField] private GameObject initialPosition;

		private GameObject _cellsContainer;
		private List<GameObject> _cells;
		private Dictionary<GameObject, AreaData> _areas;

		private GameObject _decorationContainer;
		private GameObject _humansContainer;
		private GameObject _areasContainer;
		private GameObject _worldContainer;
		private GameObject _linesContainer;
		private Dictionary<GameObject, WorldItemData> _items;

		private Vector3 _minimumArea;
		private Vector3 _maximumArea;

		private Vector3 _minimumAreaScroll;
		private Vector3 _maximumAreaScroll;

		private List<GameObject> _connectionLines = new List<GameObject>();
		private bool _editionMode = false;

		private Vector3 _centerPosition;

		public GameObject InitialPosition
		{
			get { return initialPosition; }
		}
		public GameObject CellsContainer
        {
			get { return _cellsContainer; }
		}
		public List<GameObject> Cells
        {
			get { return _cells; }
			set { _cells = value; }
        }
		public Dictionary<GameObject, WorldItemData> Items
		{
			get { return _items; }
			set { _items = value; }
		}
		public Dictionary<GameObject, AreaData> Areas
		{
			get { return _areas; }
			set { _areas = value; }
		}
		public bool EditionMode
        {
			get { return _editionMode; }
        }
		public Vector3 CenterPosition
        {
			get { return _centerPosition; }
        }

		void Start()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;

			_cellsContainer = new GameObject();
			_cellsContainer.name = "CellsContainer";
			_cellsContainer.transform.parent = this.transform;

			_decorationContainer = new GameObject();
			_decorationContainer.name = "DecorationsContainer";
			_decorationContainer.transform.parent = this.transform;

			_humansContainer = new GameObject();
			_humansContainer.name = "HumansContainer";
			_humansContainer.transform.parent = this.transform;

			_areasContainer = new GameObject();
			_areasContainer.name = "AreasContainer";
			_areasContainer.transform.parent = this.transform;

			_worldContainer = new GameObject();
			_worldContainer.name = "CityContainer";
			_worldContainer.transform.parent = this.transform;

			_linesContainer = new GameObject();
			_linesContainer.name = "LinesContainer";
			_linesContainer.transform.parent = this.transform;

			SystemEventController.Instance.DelaySystemEvent(EventLevelViewStarted, 0.1f, this);

			_items = new Dictionary<GameObject, WorldItemData>();
			_areas = new Dictionary<GameObject, AreaData>();

			_editionMode = false;
		}

        void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

			DestroyLines();
		}

		public void CalculatePathfindingInformation()
		{
			int startX = (int)(_maximumArea.x / WorkDayData.SIZE_CELL);
			int startZ = (int)(_maximumArea.z / WorkDayData.SIZE_CELL);
			int endX = (int)(_minimumArea.x / WorkDayData.SIZE_CELL);
			int endZ = (int)(_minimumArea.z / WorkDayData.SIZE_CELL);

			int witdh = Math.Abs(endX - startX) + 3;
			int height = Math.Abs(endZ - startZ) + 3;

			float heightWaypoints = -0.75f;
			PathFindingController.Instance.DestroyInstances();
			PathFindingController.Instance.AllocateMemoryMatrix(witdh, height, 1, WorkDayData.SIZE_CELL, _minimumArea.x - WorkDayData.SIZE_CELL, heightWaypoints, _minimumArea.z - WorkDayData.SIZE_CELL);			
			PathFindingController.Instance.CalculateCollisions(0, new string[3] { WorkDayData.LayerHuman, WorkDayData.LayerChair, WorkDayData.LayerArea });			
			PathFindingController.Instance.ClearDotPaths();
		}

		private void DestroyLines()
		{
			foreach (GameObject line in _connectionLines)
			{
				if (line != null)
				{
					GameObject.Destroy(line);
				}
			}
			_connectionLines.Clear();
		}

		public void CreateLinesForSelection(Vector3 origin, List<(Vector3, int)> destinations)
		{
			DestroyLines();

			foreach ((Vector3, int) item in destinations)
			{
				Vector3 destination = item.Item1;
				RelationshipColleague typeRelationship = (RelationshipColleague)item.Item2;
				GameObject line = LevelEditionController.Instance.CreateLine(item.Item2);
				VolumetricLineBehavior volumeLine = line.GetComponent<VolumetricLineBehavior>();
				volumeLine.StartPos = new Vector3(origin.x, origin.y + 1.2f, origin.z);
				volumeLine.EndPos = new Vector3(destination.x, destination.y + 1.2f, destination.z);
				volumeLine.LightSaberFactor = 0.8f;
				line.transform.parent = _linesContainer.transform;
				_connectionLines.Add(line);

				GameObject sphereState = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				sphereState.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				sphereState.transform.position = volumeLine.EndPos;
				_connectionLines.Add(sphereState);
				sphereState.transform.parent = _linesContainer.transform;
				switch (typeRelationship)
                {
					case RelationshipColleague.GOOD:
						ApplicationController.Instance.ApplyColor(sphereState.transform.GetComponent<Renderer>(), Color.green);
						break;

					case RelationshipColleague.AVERAGE:
                        ApplicationController.Instance.ApplyColor(sphereState.transform.GetComponent<Renderer>(), Color.magenta);
						break;

					case RelationshipColleague.BAD:
                        ApplicationController.Instance.ApplyColor(sphereState.transform.GetComponent<Renderer>(), Color.red);
						break;
                }

				volumeLine.LineWidth = 0.1f;
			}
		}

		public void DestroyAllCells()
		{
			if (_cells == null) return;

			for (int i = 0; i < _cells.Count; i++)
			{
				if (_cells[i] != null)
				{
					GameObject.Destroy(_cells[i]);
				}
			}
			_cells = null;
		}

		private void AddCell(float x, float y, float z)
        {
			GameObject cubeCell = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cubeCell.transform.position = new Vector3(x, y, z);
			cubeCell.transform.localScale = new Vector3(WorkDayData.SIZE_CELL * 0.9f, 0.1f, WorkDayData.SIZE_CELL * 0.9f);
			ApplicationController.Instance.ApplyColor(cubeCell.GetComponent<Renderer>(), Color.gray);
            cubeCell.transform.parent = _cellsContainer.transform;
			cubeCell.layer = LayerMask.NameToLayer(WorkDayData.LayerCell);
			cubeCell.GetComponent<Collider>().isTrigger = true;
			_cells.Add(cubeCell);
		}

		public void CreateCells(Vector3[] positionsCells)
		{
			if (positionsCells == null) return;
			
			_cells = new List<GameObject>();
			float heightCell = 0;
			foreach (Vector3 position in positionsCells)
            {
				heightCell = position.y;
				AddCell(position.x, position.y, position.z);
			}
			RenderWorld();
			EnableCellsVisibility(false);
		}

		public bool CheckPositionInside(Vector3 pos)
		{
			if ((Cells != null) && (Cells.Count > 30))
            {
				return ((_minimumAreaScroll.x / 2 < pos.x) && (_minimumAreaScroll.z / 2 < pos.z) && (_maximumAreaScroll.x / 2 > pos.x) && (_maximumAreaScroll.z / 2 > pos.z));
			}
			else
            {
				return true;
            }			
		}

		private float CalculateDimensionsOffice()
        {
			_minimumArea = new Vector3(100000, 0, 100000);
			_maximumArea = new Vector3(-100000, 0, -100000);
			float heightCell = 0;
			foreach (GameObject cellGo in _cells)
			{
				if (_minimumArea.x > cellGo.transform.position.x) _minimumArea.x = cellGo.transform.position.x;
				if (_minimumArea.z > cellGo.transform.position.z) _minimumArea.z = cellGo.transform.position.z;
				if (_maximumArea.x < cellGo.transform.position.x) _maximumArea.x = cellGo.transform.position.x;
				if (_maximumArea.z < cellGo.transform.position.z) _maximumArea.z = cellGo.transform.position.z;
				_minimumArea.y = cellGo.transform.position.y;
				_maximumArea.y = cellGo.transform.position.y;
				heightCell = cellGo.transform.position.y;
			}

			_minimumArea -= new Vector3(WorkDayData.SIZE_CELL / 2, 0, WorkDayData.SIZE_CELL / 2);
			_maximumArea += new Vector3(WorkDayData.SIZE_CELL / 2, 0, WorkDayData.SIZE_CELL / 2);

			_centerPosition = ApplicationController.Instance.LevelView.GetCenterLevel();

			return heightCell;
		}

		public void RenderWorld()
        {
			float heightCell = CalculateDimensionsOffice();
			float scaleGrid = 4f;

			float shiftHeightFloor = 0.05f;
			Vector3 shiftForest = new Vector3(WorkDayData.SIZE_CELL * 6, 0, WorkDayData.SIZE_CELL * 6);

			// MINIMUM AREA
			GameObject cornerMin = AssetBundleController.Instance.CreateGameObject("AsphaltGrid");
			GameObject cornerForestMin = AssetBundleController.Instance.CreateGameObject("GrassForest");
			cornerMin.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
			cornerForestMin.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
			Vector3 sizeGrid = cornerMin.GetComponent<Renderer>().bounds.size;
			_minimumAreaScroll = _minimumArea;
			_minimumAreaScroll -= sizeGrid/2;
			_minimumAreaScroll.y = heightCell + 0.1f;
			cornerMin.transform.position = _minimumAreaScroll;
			cornerMin.transform.parent = _worldContainer.transform;
			cornerForestMin.transform.position = new Vector3(_minimumAreaScroll.x - shiftForest.x, _minimumAreaScroll.y + shiftHeightFloor, _minimumAreaScroll.z - shiftForest.z);
			cornerForestMin.transform.parent = _worldContainer.transform;

			Vector3 positionGrid = _minimumAreaScroll;
			positionGrid.x += sizeGrid.x;
			int iterations = 0;
			while (iterations < 3)
			{
				positionGrid.y = heightCell + 0.1f;

				GameObject gridSegment = AssetBundleController.Instance.CreateGameObject("AsphaltGrid");
				GameObject gridForest = AssetBundleController.Instance.CreateGameObject("GrassForest");
				gridSegment.transform.position = positionGrid;
				gridSegment.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
				gridSegment.transform.parent = _worldContainer.transform;

				gridForest.transform.position = new Vector3(positionGrid.x, positionGrid.y + shiftHeightFloor, positionGrid.z - shiftForest.z);
				gridForest.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
				gridForest.transform.parent = _worldContainer.transform;

				positionGrid.x += sizeGrid.x;
				iterations++;
			}

			positionGrid = _minimumAreaScroll;
			positionGrid.z += sizeGrid.z;
			iterations = 0;
			while (iterations < 3)
			{
				positionGrid.y = heightCell + 0.1f;

				GameObject gridSegment = AssetBundleController.Instance.CreateGameObject("AsphaltGrid");
				GameObject gridForest = AssetBundleController.Instance.CreateGameObject("GrassForest");
				gridSegment.transform.position = positionGrid;
				gridSegment.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
				gridSegment.transform.parent = _worldContainer.transform;

				gridForest.transform.position = new Vector3(positionGrid.x - shiftForest.x, positionGrid.y + shiftHeightFloor, positionGrid.z);
				gridForest.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
				gridForest.transform.parent = _worldContainer.transform;

				positionGrid.z += sizeGrid.z;
				iterations++;
			}

			// MAXIMUM AREA
			GameObject cornerMax = AssetBundleController.Instance.CreateGameObject("AsphaltGrid");
			GameObject cornerForestMax = AssetBundleController.Instance.CreateGameObject("GrassForest");
			cornerMax.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
			cornerForestMax.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
			_maximumAreaScroll = _maximumArea;
			_maximumAreaScroll += sizeGrid / 2;
			_maximumAreaScroll.y = heightCell + 0.1f;
			cornerMax.transform.position = _maximumAreaScroll;
			cornerMax.transform.parent = _worldContainer.transform;
			cornerForestMax.transform.position = new Vector3(_maximumAreaScroll.x + shiftForest.x, _maximumAreaScroll.y + shiftHeightFloor, _maximumAreaScroll.z + shiftForest.z);
			cornerForestMax.transform.parent = _worldContainer.transform;

			positionGrid = _maximumAreaScroll;
			positionGrid.x -= sizeGrid.x;
			iterations = 0;
			while (iterations < 3)
			{
				positionGrid.y = heightCell + 0.1f;

				GameObject gridSegment = AssetBundleController.Instance.CreateGameObject("AsphaltGrid");
				GameObject gridForest = AssetBundleController.Instance.CreateGameObject("GrassForest");
				gridSegment.transform.position = positionGrid;
				gridSegment.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
				gridSegment.transform.parent = _worldContainer.transform;

				gridForest.transform.position = new Vector3(positionGrid.x, positionGrid.y + shiftHeightFloor, positionGrid.z + shiftForest.x);
				gridForest.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
				gridForest.transform.parent = _worldContainer.transform;

				positionGrid.x -= sizeGrid.x;

				iterations++;
			}

			positionGrid = _maximumAreaScroll;
			positionGrid.z -= sizeGrid.z;
			iterations = 0;
			while (iterations < 3)
			{
				positionGrid.y = heightCell + 0.1f;

				GameObject gridSegment = AssetBundleController.Instance.CreateGameObject("AsphaltGrid");
				GameObject gridForest = AssetBundleController.Instance.CreateGameObject("GrassForest");
				gridSegment.transform.position = positionGrid;
				gridSegment.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
				gridSegment.transform.parent = _worldContainer.transform;

				gridForest.transform.position = new Vector3(positionGrid.x + shiftForest.x, positionGrid.y + shiftHeightFloor, positionGrid.z);
				gridForest.transform.localScale = new Vector3(scaleGrid, scaleGrid, scaleGrid);
				gridForest.transform.parent = _worldContainer.transform;

				positionGrid.z -= sizeGrid.z;

				iterations++;
			}			
		}

		public float CreateCells(Vector3 startPosition, Vector3 endPosition)
		{
			DestroyAllCells();

			_cells = new List<GameObject>();

			Vector3 center = (startPosition + endPosition) / 2;
			Vector3 dimensionsArea = startPosition - endPosition;
			dimensionsArea.x = Mathf.Abs(dimensionsArea.x);
			dimensionsArea.z = Mathf.Abs(dimensionsArea.z);

			float startX = ((startPosition.x < endPosition.x) ? startPosition.x : endPosition.x);
			float startZ = ((startPosition.z < endPosition.z) ? startPosition.z : endPosition.z);

			for (float x = startX; x < startX + dimensionsArea.x; x += WorkDayData.SIZE_CELL)
			{
				for (float z = startZ; z < startZ + dimensionsArea.z; z += WorkDayData.SIZE_CELL)
				{
					AddCell(x, startPosition.y, z);
				}
			}

			return startPosition.y;
		}

		private int GetMaxItemID()
        {
			if (_items == null) return 0;

			int maxID = 0;
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
            {
				int temp = item.Value.Id;
				if (temp > maxID)
                {
					maxID = temp;
                }
			}
			return maxID + 1;
        }

		public List<WorldItemData> GetHumans()
		{
			List<WorldItemData> humans = new List<WorldItemData>();
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value.IsHuman)
                {
					humans.Add(item.Value);
				}
			}
			return humans;
		}

		public List<WorldItemData> GetChairs()
		{
			List<WorldItemData> humans = new List<WorldItemData>();
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value.IsChair)
				{
					humans.Add(item.Value);
				}
			}
			return humans;
		}

		public bool CheckNameBelongToHuman(string name)
		{
			List<WorldItemData> humans = GetHumans();
			foreach (WorldItemData human in humans)
			{
				if (human.Name.Equals(name))
				{
					return true;
				}
			}
			return false;
		}

		public void DestroyAllItems()
		{
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Key != null)
                {
					GameObject.Destroy(item.Key);
				}
			}
			_items.Clear();
			_items = new Dictionary<GameObject, WorldItemData>();
		}

		public void DestroyAllAreas()
		{
			foreach (KeyValuePair<GameObject, AreaData> area in _areas)
			{
				if (area.Key != null)
				{
					GameObject.Destroy(area.Key);
				}
			}
			_areas.Clear();
			_areas = new Dictionary<GameObject, AreaData>();
		}

		public void DestroyWorld()
        {
			if (_worldContainer != null)
            {
				GameObject.Destroy(_worldContainer);
				_worldContainer = null;
			}

			_worldContainer = new GameObject();
			_worldContainer.name = "WorldContainer";
			_worldContainer.transform.parent = this.transform;
		}

		public bool CheckCellUsed(Vector3 positionCell)
        {
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value != null)
				{
					if (item.Value.Cell.Equals(positionCell))
                    {
						return true;
                    }
				}
			}
			return false;
		}

		public Vector3 GetCenterLevel()
		{
			Vector3 center = Vector3.zero;
			foreach (GameObject item in Cells)
			{
				center += item.transform.position;
			}
			return center / Cells.Count;
		}

		public bool CheckCollision(GameObject itemGO)
        {
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Key != null)
				{
					Bounds targetBounds = itemGO.transform.GetComponent<Collider>().bounds;
					targetBounds.center = itemGO.transform.position;
					targetBounds.size *= 0.9f;
					Bounds sourceBounds = item.Key.transform.GetComponent<Collider>().bounds;
					if (sourceBounds.Intersects(targetBounds))
					{
						return true;
					}
				}
			}
			return false;
		}

		public (GameObject, WorldItemData) GetItemByName(string nameItem)
		{
			string finalName = nameItem.ToLower();
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value != null)
				{
					if (item.Value.Name.ToLower().Equals(finalName))
                    {
						return (item.Key, item.Value);
                    }
				}
			}
			return (null, null);
		}

		public (GameObject, WorldItemData) GetItemByOwner(string nameItem)
		{
			string finalName = nameItem.ToLower();
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value != null)
				{
					if (item.Value.Owner != null)
                    {
						if (item.Value.Owner.ToLower().Equals(finalName))
						{
							return (item.Key, item.Value);
						}
					}
				}
			}
			return (null, null);
		}

		public bool CheckFreeRoom(string areaName)
        {
			return (!IsAnyChairBusyForArea(areaName));
		}

		public bool IsAnyChairBusyForArea(string areaName)
		{
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value != null)
				{
					if (item.Value.IsChair)
					{
						ChairView chairTarget = item.Key.GetComponent<ChairView>();
						if (chairTarget != null)
						{
							if (chairTarget.Area != null)
							{
								if (chairTarget.Area.Name != null)
								{
									if (chairTarget.Area.Name.Equals(areaName))
									{
										if (chairTarget.Human != null)
										{
											return true;
										}
									}
								}
							}
						}
					}
				}
			}
			return false;
		}

		public int CountFreeChairForArea(string areaName)
		{
			int totalFreeChair = 0;
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value != null)
				{
					if (item.Value.IsChair)
					{
						ChairView chairTarget = item.Key.GetComponent<ChairView>();
						if (chairTarget != null)
						{
							if (chairTarget.Area != null)
							{
								if (chairTarget.Area.Name != null)
								{
									if (chairTarget.Area.Name.Equals(areaName))
									{
										if (chairTarget.Human == null)
										{
											totalFreeChair++;
										}
									}
								}
							}
						}
					}
				}
			}
			return totalFreeChair;
		}

		public (GameObject, WorldItemData) GetFreeChairByRoomRoomArea(string areaName)
        {
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value != null)
				{
					if (item.Value.IsChair)
					{
						ChairView chairTarget = item.Key.GetComponent<ChairView>();
						if (chairTarget != null)
                        {
							if (chairTarget.Area != null)
							{
								if (chairTarget.Area.Name != null)
                                {
									if (chairTarget.Area.Name.Equals(areaName))
                                    {
										if (chairTarget.Human == null)
                                        {
											return (item.Key, item.Value);
										}		
									}
								}									
							}
						}
					}
				}
			}
			return (null, null);
		}

		public (GameObject, WorldItemData) GetAnyChairByByTypeArea(AreaMode areaMode)
		{
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value != null)
				{
					if (item.Value.IsChair)
					{
						ChairView chairTarget = item.Key.GetComponent<ChairView>();
						if (chairTarget != null)
						{
							if (chairTarget.Area != null)
							{
								if ((AreaMode)chairTarget.Area.Type == areaMode)
								{
									return (item.Key, item.Value);
								}
							}
						}
					}
				}
			}
			return (null, null);
		}

		public AreaData GetAreaByName(string nameArea)
        {
			string finalName = nameArea.ToLower();
			foreach (KeyValuePair<GameObject, AreaData> area in _areas)
			{
				if (area.Value != null)
				{
					if (area.Value.Name.ToLower().Equals(finalName))
					{
						return area.Value;
					}
				}
			}
			return null;
		}

		public AreaData GetAreaByPosition(Vector3 position)
		{
			foreach (KeyValuePair<GameObject, AreaData> area in _areas)
			{
				if (area.Value != null)
				{
					Vector3 centerBounds = (area.Value.StartPosition + area.Value.EndPosition)/2;
					Vector3 sizeBounds = area.Value.StartPosition - area.Value.EndPosition;

					Bounds areaBounds = new Bounds(centerBounds, new Vector3(Mathf.Abs(sizeBounds.x), 10, Mathf.Abs(sizeBounds.z)));

					if (areaBounds.Contains(new Vector3(position.x, 0, position.z)))
					{
						return area.Value;
					}
				}
			}
			return null;
		}		

		public void ReplaceOwner(string previousName, string currentName)
		{
			if (previousName.Length == 0) return;

			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value != null)
				{
					if (item.Value.Owner != null)
                    {
						if (item.Value.Owner.Equals(previousName))
						{
							item.Value.Owner = currentName;
						}
					}
				}
			}
		}

		public bool AddItem(WorldItemData item, bool calculateArea, bool initialization = false)
        {
			AssetDefinitionItem catalogItem = AssetsCatalogData.Instance.GetAssetById(item.CatalogId);
			GameObject itemGO = AssetBundleController.Instance.CreateGameObject(catalogItem.AssetName);
			if (catalogItem.IsChair)
            {
				itemGO.layer = LayerMask.NameToLayer(WorkDayData.LayerChair);
				itemGO.GetComponent<Collider>().isTrigger = true;
				itemGO.AddComponent<ColliderEventDispatcher>();
				ChairView chair = itemGO.AddComponent<ChairView>();
				chair.Initialize(item.Name);
			}
			else
			if (catalogItem.IsHuman)
            {
				itemGO.layer = LayerMask.NameToLayer(WorkDayData.LayerHuman);
				itemGO.AddComponent<ColliderEventDispatcher>();
				itemGO.AddComponent<Rigidbody>();
				HumanView human = itemGO.AddComponent<HumanView>();
				human.Initialize(item);
			}
			else
            {
				itemGO.layer = LayerMask.NameToLayer(WorkDayData.LayerItem);
				itemGO.GetComponent<Collider>().isTrigger = true;
			}
			itemGO.transform.eulerAngles = item.Rotation;
			itemGO.transform.position = item.Position;
			if (!initialization)
            {
				Physics.SyncTransforms();
			}

			if (CheckCollision(itemGO) && !initialization)
			{
				GameObject.Destroy(itemGO);
				return false;
            }
			else
            {
				if (catalogItem.IsHuman)
				{
					itemGO.transform.parent = _humansContainer.transform;
					itemGO.transform.name = item.Name;
				}
				else
				{
					itemGO.transform.parent = _decorationContainer.transform;
				}

				if (item.Id == -1)
				{
					item.Id = GetMaxItemID();
				}

				bool output = _items.TryAdd(itemGO, item);

				if (calculateArea)
                {
					SystemEventController.Instance.DispatchSystemEvent(ChairView.EventChairViewAssignAreaData);
				}

				return output;
			}
		}

		public void ReplaceHumanGender(WorldItemData humanData, GameObject humanGO)
		{
			List<AssetDefinitionItem> humansDefinitions = AssetsCatalogData.Instance.GetItemsByType(true);
			List<AssetDefinitionItem> finalHumansDefinitions = new List<AssetDefinitionItem>();
			foreach (AssetDefinitionItem humanDefinition in humansDefinitions)
            {
				if (humanData.IsMan)
                {
					if (!humanDefinition.IsMan)
                    {
						finalHumansDefinitions.Add(humanDefinition);
					}
				}
				else
                {
					if (humanDefinition.IsMan)
					{
						finalHumansDefinitions.Add(humanDefinition);
					}
				}
			}

			// GET RANDOM MAN/WOMAN
			int finalSelection = UnityEngine.Random.Range(0, finalHumansDefinitions.Count);
			int finalCatalogID = finalHumansDefinitions[finalSelection].Id;
			AssetDefinitionItem itemDefinition = AssetsCatalogData.Instance.GetAssetById(finalCatalogID);

			Vector3 posGO = humanGO.transform.position;
			Vector3 rotGO = humanGO.transform.eulerAngles;
			Vector3 posCell = humanData.Cell;

			DeleteItem(humanGO, false);

			WorldItemData itemInstance = new WorldItemData();
			itemInstance.Copy(humanData);
			itemInstance.Id = humanData.Id;
			itemInstance.CatalogId = itemDefinition.Id;
			itemInstance.IsChair = itemDefinition.IsChair;
			itemInstance.IsHuman = itemDefinition.IsHuman;
			itemInstance.IsMan = itemDefinition.IsMan;

			itemInstance.Position = posGO;
			itemInstance.Rotation = rotGO;
			itemInstance.Cell = posCell;

			AddItem(itemInstance, true, true);
		}

		public bool AddArea(AreaData areaData, bool visible)
		{
			Vector3 centerPosition = (areaData.StartPosition + areaData.EndPosition) / 2;
			Vector3 sizeArea = areaData.StartPosition - areaData.EndPosition;

			GameObject areaGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
			areaGO.transform.position = centerPosition;
			areaGO.transform.localScale = new Vector3(Mathf.Abs(sizeArea.x), 0.3f, Mathf.Abs(sizeArea.z));
			switch ((AreaMode)areaData.Type)
			{
				case AreaMode.Work:
					ApplicationController.Instance.ApplyColor(areaGO.GetComponent<Renderer>(), Color.blue);
					break;
				case AreaMode.Meeting:
                    ApplicationController.Instance.ApplyColor(areaGO.GetComponent<Renderer>(), Color.magenta);
					break;
				case AreaMode.Kitchen:
                    ApplicationController.Instance.ApplyColor(areaGO.GetComponent<Renderer>(), Color.cyan);
					break;
				case AreaMode.Bathroom:
                    ApplicationController.Instance.ApplyColor(areaGO.GetComponent<Renderer>(), Color.green);
					break;
				case AreaMode.Exit:
                    ApplicationController.Instance.ApplyColor(areaGO.GetComponent<Renderer>(), Color.white);
					break;
			}
			areaGO.layer = LayerMask.NameToLayer(WorkDayData.LayerArea);
			areaGO.GetComponent<Collider>().isTrigger = true;
			areaGO.transform.parent = _areasContainer.transform;
			areaGO.SetActive(visible);
			bool success = _areas.TryAdd(areaGO, areaData.Clone());
			SystemEventController.Instance.DispatchSystemEvent(ChairView.EventChairViewAssignAreaData);
			
			return success;
		}

		public bool RemoveArea(GameObject areaGO)
        {
			if (_areas.Remove(areaGO))
			{
				GameObject.Destroy(areaGO);
				return true;
			}
			else
			{
				return false;
			}
		}

		public AreaData GetArea(GameObject areaGO)
		{
			AreaData areaData;
			if (_areas.TryGetValue(areaGO, out areaData))
			{
				return areaData;
			}
			else
			{
				return null;
			}
		}

		public bool CheckHumansInTheirOwnChair()
		{
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value != null)
				{
					if (item.Value.IsHuman && !item.Value.IsClient)
					{
						var (chairGO, chairData) = ApplicationController.Instance.LevelView.GetItemByOwner(item.Value.Name);
						if (chairGO != null)
						{
							if (!item.Key.gameObject.activeSelf)
                            {
								return false;
							}
							else
                            {
								if (item.Key.GetComponent<HumanView>().IsMoving())
								{
									return false;
								}
								else
								{
									ChairView chairHuman = item.Key.GetComponent<HumanView>().CurrentChair;
									if (chairHuman == null)
									{
										return false;
									}
									else
									{
										if (chairHuman.gameObject != chairGO)
										{
											return false;
										}
									}
								}
							}
						}
					}
				}
			}
			return true;
		}

		public void RenderAreas(AreaMode areaMode)
        {
			foreach (KeyValuePair<GameObject, AreaData> area in _areas)
			{
				if (area.Value != null)
				{
					if (areaMode == (AreaMode)area.Value.Type)
					{
						area.Key.SetActive(true);
					}
					else
                    {
						area.Key.SetActive(false);
					}
				}
			}
		}

		public void HideAreas()
		{
			foreach (KeyValuePair<GameObject, AreaData> area in _areas)
			{
				if (area.Key != null)
				{
					area.Key.SetActive(false);
				}
			}
		}

		public WorldItemData GetItem(GameObject itemGO)
		{
			if ((_items == null) || (itemGO == null))
            {
				return null;
            }
			else
            {
				WorldItemData result = null;
				_items.TryGetValue(itemGO, out result);
				return result;
			}
		}

		public bool DeleteItem(GameObject itemGO, bool deleteData)
        {
			WorldItemData itemToDelete = GetItem(itemGO);
			if (itemToDelete == null)
            {
				return false;
			}
			else
            {
				string humanName = "";
				if (itemToDelete.IsHuman)
                {
					humanName = itemToDelete.Name;
                }
				if (_items.Remove(itemGO))
				{
					if (deleteData)
                    {
						if (humanName.Length > 0)
						{
							SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunDeleteHuman, humanName);
						}
					}
					GameObject.Destroy(itemGO);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public bool RotateItem(GameObject itemGO)
		{
			WorldItemData worldItemData = GetItem(itemGO);
			if (worldItemData != null)
			{
				worldItemData.Rotation += new Vector3(0, 90, 0);
				itemGO.transform.eulerAngles = worldItemData.Rotation;
				return true;
			}
			else
			{
				return false;
			}
		}

		public void CreateItems(WorldItemData[] items)
		{
			_items = new Dictionary<GameObject, WorldItemData>();
			if (items != null)
            {
				foreach (WorldItemData item in items)
				{
					AddItem(item, false, true);
				}
			}
		}

		public void CreateAreas(AreaData[] areas)
		{
			_areas = new Dictionary<GameObject, AreaData>();
			if (areas != null)
            {
				foreach (AreaData area in areas)
				{
					AddArea(area, false);
				}
			}
		}

		public void BakeNavMesh()
		{
		}

		public void EnableCellsVisibility(bool enabled)
        {
			if (_cells != null)
			{
                foreach (GameObject cell in _cells)
                {
                    if (cell != null) cell.SetActive(enabled);
                }
            }
        }

		public void SaveData(bool setInitialData)
        {
			if (_cells != null)
            {
				WorkDayData.Instance.CurrentProject.SetCells(_cells.ToArray());
			}			
			if (_items != null)
            {
				if (setInitialData)
                {
					foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
					{
						if (item.Value != null)
						{
							if (item.Value.IsHuman)
							{
								item.Value.InitialPosition = item.Value.Position;
								item.Value.InitialRotation = item.Value.Rotation;								
							}
						}
					}
				}
				WorkDayData.Instance.CurrentProject.SetItems(_items.Values.ToArray());
			}
			else
            {
				WorkDayData.Instance.CurrentProject.SetItems(new WorldItemData[0]);
			}
			if (_areas != null)
            {
				WorkDayData.Instance.CurrentProject.SetAreas(_areas.Values.ToArray());
			}
			else
            {
				WorkDayData.Instance.CurrentProject.SetAreas(new AreaData[0]);
			}

			WorkDayData.Instance.CurrentProject.CameraPosition = ApplicationController.Instance.PlayerView.transform.position;
			WorkDayData.Instance.CurrentProject.CameraRotation = ApplicationController.Instance.PlayerView.transform.eulerAngles;
			WorkDayData.Instance.CurrentProject.ConfigurationCamera = ApplicationController.Instance.PlayerView.CurrentRotation;
		}

		public void ResetLevel()
        {
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value != null)
				{
					if (item.Value.IsHuman)
					{
						item.Value.Position = item.Value.InitialPosition;
						item.Value.Rotation = item.Value.InitialRotation;

						item.Key.transform.position = item.Value.Position;
						item.Key.transform.eulerAngles = item.Value.Rotation;

						item.Key.SetActive(true);

						item.Value.Reset();
					}
				}
			}
		}

		public void LinkHumanInMeetingWithChairs(List<string> assistants)
		{
			foreach (string assistant in assistants)
            {
				var (humanGO, humanData) = GetItemByName(assistant);
				if (humanGO != null)
                {					
					foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
					{
						if (item.Value != null)
						{
							if (item.Value.IsChair)
							{
								if (Vector3.Distance(item.Value.Position, humanData.Position) <= 0.2f)
                                {
									if (item.Key.GetComponent<ChairView>() != null)
                                    {
										if (item.Key.GetComponent<ChairView>().Human == null)
										{
											humanGO.GetComponent<HumanView>().SetChair(item.Key);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public bool HasEveryoneLeft()
		{
			foreach (KeyValuePair<GameObject, WorldItemData> item in _items)
			{
				if (item.Value != null)
				{
					if (item.Value.IsHuman)
					{
						if (item.Key.activeSelf)
                        {
							return false;
                        }
					}
				}
			}
			return true;
		}

		private List<(Vector3,string)> OrderPointsClockwise(List<(Vector3, string)> points)
		{
			// 1. Find the centroid
			Vector3 center = Vector3.zero;
			foreach (var point in points)
			{
				center += point.Item1;
			}
			center /= points.Count;

			// 2. Sort points by angle from center
			return points.OrderBy(p =>Mathf.Atan2(p.Item1.z - center.x, p.Item1.x - center.x)).ToList();
		}

		public void CreateLinesForGroup(List<(Vector3, string)> positions, int taskID, bool includeProgress, bool showTasksAssigned)
		{
			DestroyLines();

			for (int i = 0; i < positions.Count; i++)
			{
				Vector3 destination = positions[i].Item1;
				Vector3 origin = Vector3.zero;
				if (i == 0)
                {
					origin = positions[positions.Count-1].Item1;
				}
				else
                {
					origin = positions[i - 1].Item1;
				}
				string member = positions[i].Item2;
				WorldItemData humanData = WorkDayData.Instance.CurrentProject.GetItemByName(member);

				GameObject line = LevelEditionController.Instance.CreateLine((int)RelationshipColleague.NEUTRAL);
				VolumetricLineBehavior volumeLine = line.GetComponent<VolumetricLineBehavior>();
				volumeLine.StartPos = new Vector3(origin.x, origin.y + 1.2f, origin.z);
				volumeLine.EndPos = new Vector3(destination.x, destination.y + 1.2f, destination.z);
				volumeLine.LightSaberFactor = 0.8f;
				line.transform.parent = _linesContainer.transform;
				_connectionLines.Add(line);

				GroupInfoData groupMember = WorkDayData.Instance.CurrentProject.GetGroupOfMember(member);

				if (taskID != -1)
                {
					TaskProgressData humanActiveTask = humanData.GetActiveTask();
					if (humanActiveTask == null)
                    {
						UIEventController.Instance.DispatchUIEvent(HumanView.EventHumanViewHideWorking, member);
					}
					else
                    {
						if (includeProgress)
						{
							float hoursLogged = humanData.GetLoggedTimeForTask(-1, taskID, "");
							var (taskItemData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskID);

							GameObject cubeProgress = GameObject.CreatePrimitive(PrimitiveType.Cube);
							float sizeProgress = hoursLogged / 10f;
							cubeProgress.transform.localScale = new Vector3(0.2f, sizeProgress, 0.2f);							
							cubeProgress.transform.position = volumeLine.EndPos + new Vector3(0, 0.3f + (sizeProgress/2), 0);
							_connectionLines.Add(cubeProgress);
							cubeProgress.transform.parent = _linesContainer.transform;
							cubeProgress.layer = LayerMask.NameToLayer("Ignore Raycast");

							if (groupMember != null)
							{
								ApplicationController.Instance.ApplyColor(cubeProgress.transform.GetComponent<Renderer>(), groupMember.GetColor());
							}
							else
							{
                                ApplicationController.Instance.ApplyColor(cubeProgress.transform.GetComponent<Renderer>(), Color.white);
							}

							GameObject infoHours = LevelEditionController.Instance.CreateInfoHours();
							infoHours.transform.parent = _linesContainer.transform;
							infoHours.transform.position = cubeProgress.transform.position + new Vector3(0, (sizeProgress / 2) + 0.1f, 0);
							infoHours.transform.localScale = new Vector3(3, 3, 3);
							_connectionLines.Add(infoHours);
							infoHours.GetComponent<HourProgressLabelView>().SetText(taskItemData.Name + " (" + Utilities.CeilDecimal(hoursLogged,1) + "h)");
							infoHours.layer = LayerMask.NameToLayer("Ignore Raycast");
						}

						if (humanActiveTask.TaskUID != taskID)
                        {
							UIEventController.Instance.DispatchUIEvent(HumanView.EventHumanViewHideWorking, member);
						}
						else
                        {
							UIEventController.Instance.DispatchUIEvent(HumanView.EventHumanViewShowWorking, member);
						}
					}
				}

				if (showTasksAssigned)
                {
					List<(TaskItemData, BoardData)> tasksForHuman = WorkDayData.Instance.CurrentProject.GetAllTasksAssignedTo(humanData.Name);
					Vector3 startTasksCubes = volumeLine.EndPos + new Vector3(0, 0.3f, 0);
					for (int k = 0; k < tasksForHuman.Count; k++)
                    {
						var (taskForHuman, boardData) = tasksForHuman[k];
						ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(boardData.ProjectId);
						
						GameObject cubeTask = GameObject.CreatePrimitive(PrimitiveType.Cube);
						float sizeEstimation = (float)taskForHuman.EstimatedTime / 20f;
						cubeTask.transform.localScale = new Vector3(0.2f, sizeEstimation, 0.2f);
						cubeTask.transform.position = startTasksCubes + new Vector3(0, (sizeEstimation / 2), 0);
						_connectionLines.Add(cubeTask);
						cubeTask.transform.parent = _linesContainer.transform;
						ApplicationController.Instance.ApplyColor(cubeTask.transform.GetComponent<Renderer>(), projectInfo.GetColor());
						cubeTask.layer = LayerMask.NameToLayer("Ignore Raycast");
						GameObject infoTask = LevelEditionController.Instance.CreateInfoHours();
						infoTask.transform.parent = _linesContainer.transform;
						Vector3 forwardCamera = CameraXRController.Instance.GameCamera.transform.forward;
						forwardCamera.y = 0;
						infoTask.transform.position = cubeTask.transform.position - CameraXRController.Instance.GameCamera.transform.forward * 0.3f;
						infoTask.transform.localScale = new Vector3(3, 3, 3);
						_connectionLines.Add(infoTask);
						infoTask.GetComponent<HourProgressLabelView>().SetText(taskForHuman.Name + " ("+ taskForHuman.EstimatedTime + "h)");
						infoTask.layer = LayerMask.NameToLayer("Ignore Raycast");

						startTasksCubes = cubeTask.transform.position + new Vector3(0, (sizeEstimation / 2) + 0.2f, 0);
					}
				}

				GameObject sphereState = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				sphereState.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				sphereState.transform.position = volumeLine.EndPos;
				_connectionLines.Add(sphereState);
				sphereState.transform.parent = _linesContainer.transform;

				if (groupMember != null)
                {
					ApplicationController.Instance.ApplyColor(sphereState.transform.GetComponent<Renderer>(), groupMember.GetColor());
				}
				else
                {
                    ApplicationController.Instance.ApplyColor(sphereState.transform.GetComponent<Renderer>(), Color.white);
				}
				volumeLine.LineWidth = 0.1f;
			}
		}

		public enum CodeLevelReady { Ready = 0, NoArea, NoHumans, NoChairs, NoHumansWithChairs, NoExit }

		public CodeLevelReady IsReadyToPlay()
        {
			List<WorldItemData> humans = GetHumans();
			if (Cells == null)
            {
				return CodeLevelReady.NoArea;
			}
			if (Cells.Count == 0)
            {
				return CodeLevelReady.NoArea;
			}
			if (humans.Count == 0)
            {
				return CodeLevelReady.NoHumans;
            }
			List<WorldItemData> chairs = GetChairs();
			if (chairs.Count == 0)
			{
				return CodeLevelReady.NoChairs;
			}
			bool foundChair = false;
			foreach (WorldItemData human in humans)
            {
				foreach (WorldItemData chair in chairs)
                {
					if (chair.Owner != null)
                    {
						if (chair.Owner.Equals(human.Name))
						{
							foundChair = true;
						}
					}
				}
			}
			if (!foundChair)
			{
				return CodeLevelReady.NoHumansWithChairs;
			}
			var (chairGO, chairData) = GetAnyChairByByTypeArea(AreaMode.Exit);
			if (chairGO == null)
			{
				return CodeLevelReady.NoExit;
			}
			return CodeLevelReady.Ready;
        }

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventLevelViewLinesRequestDestroy))
            {				
				DestroyLines();
			}
			if (nameEvent.Equals(TimeHUD.EventTimeHUDUpdateCurrentTime))
            {
				DestroyLines();
				SystemEventController.Instance.DispatchSystemEvent(TimeHUD.EventTimeHUDCancelSelectionObject);
			}
			if (nameEvent.Equals(ScreenPanelEditionView.EventScreenPanelEditionActivation))
			{
				_editionMode = (bool)parameters[0];
				DestroyLines();
				if (_editionMode)
                {
					EnableCellsVisibility(true);
					PathFindingController.Instance.DestroyDebugMatrixConstruction();
					UIEventController.Instance.DelayUIEvent(TimeHUD.EventTimeHUDUpdateEnableInteraction, 0.1f, true);
				}
				else
                {					
					EnableCellsVisibility(false);
					CalculatePathfindingInformation();
				}
			}
			if (nameEvent.Equals(TimeHUD.EventTimeHUDSelectionObject))
			{
				if ((parameters.Length == 0) || _editionMode)
				{
					DestroyLines();
				}
				else
				{
					if (parameters[1] is WorldItemData)
					{
						WorldItemData selectedHuman = (WorldItemData)parameters[1];

						if (parameters.Length > 2)
                        {
							List<(Vector3, int)> destinations = new List<(Vector3, int)>();
							if (selectedHuman.IsHuman)
							{
								var (selectedHumanGO, selectedHumanDT) = GetItemByName(selectedHuman.Name);
								if (selectedHumanGO != null)
								{
									TimeWorkingDataDisplay taskProgress = selectedHuman.GetCurrentTaskProgress(-1);
									if (taskProgress != null)
									{
										var (taskData, boardName) = WorkDayData.Instance.CurrentProject.GetTaskItemDataByUID(taskProgress.TaskUID);
										DestroyLines();

										List<string> members = taskData.GetHumanMembers();
										List<(Vector3, string)> positions = new List<(Vector3, string)>();
										foreach (string member in members)
										{
											var (humanGO, humanData) = GetItemByName(member);
											if (humanGO != null)
											{
												if (humanGO.gameObject.activeSelf)
                                                {
													positions.Add((humanGO.transform.position, member));
												}												
											}
										}
										List<(Vector3, string)> orderedPositions = OrderPointsClockwise(positions);
										CreateLinesForGroup(orderedPositions, taskProgress.TaskUID, false, false);
									}
									else
                                    {
										DestroyLines();
									}
								}
							}
							else
							{
								DestroyLines();
							}
						}
						else
                        {
							DestroyLines();
						}
					}
					else
					{
						DestroyLines();
					}
				}
			}
			if (nameEvent.Equals(ApplicationController.EventMainControllerReleaseGameResources))
			{
				DestroyLines();
				GameObject.Destroy(this.gameObject);
			}
			if (nameEvent.Equals(EventLevelViewDestroy))
			{
				DestroyLines();
				GameObject.Destroy(this.gameObject);
			}
			if (nameEvent.Equals(HumanView.EventHumanViewReachedDestination))
            {
				var (itemGO, itemData) = GetItemByName((string)parameters[1]);
				if (itemData != null)
				{
					itemData.Position = itemGO.transform.position;
					itemData.Rotation = itemGO.transform.eulerAngles;
				}
			}
			if (nameEvent.Equals(CommandGoToAreaChair.EventCommandGoToAreaChairDestinationReached))
            {
				var (itemGO, itemData) = GetItemByName((string)parameters[0]);
				if (itemData != null)
                {
					itemData.Position = itemGO.transform.position;
					itemData.Rotation = itemGO.transform.eulerAngles;
                }
			}
			if (nameEvent.Equals(CommandEnterToOffice.EventCommandEnterToOfficeReachedChair))
			{
				var (itemGO, itemData) = GetItemByName((string)parameters[0]);
				if (itemData != null)
				{
					itemData.Position = itemGO.transform.position;
					itemData.Rotation = itemGO.transform.eulerAngles;
				}
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenCalendarView.EventScreenCalendarViewSetNewDate))
			{
				if (!(bool)parameters[0])
				{
					UIEventController.Instance.DelayUIEvent(EventLevelViewDelayedReset, 0.2f);
				}
			}
			if (nameEvent.Equals(EventLevelViewDelayedReset))
            {
				List<WorldItemData> humans = GetHumans();

				int totalAssholes = (humans.Count / 10);
				for (int i = 0; i < totalAssholes; i++)
				{
					int indexAsshole = UnityEngine.Random.Range(0, humans.Count);
					humans[indexAsshole].IsAsshole = true;
					humans.RemoveAt(indexAsshole);
				}
			}
			if (nameEvent.Equals(EditionSubStateAreas.EventSubStateAreasActivated))
			{
				_cellsContainer.SetActive(!(bool)parameters[0]);
			}
			if (nameEvent.Equals(ItemMeetingHUDView.EventItemMeetingHUDViewSelected)
				|| nameEvent.Equals(ItemMeetingProgressView.EventItemMeetingProgressViewSelected))
            {
				if ((int)parameters[2] != -1)
				{
					MeetingData meeting = (MeetingData)parameters[3];
					DestroyLines();
					List<string> members = meeting.GetHumanMembers();
					List<(Vector3, string)> positions = new List<(Vector3,string)>();
					foreach (string member in members)
                    {
						var (humanGO, humanData) = GetItemByName(member);
						if (humanGO != null)
                        {
							positions.Add((humanGO.transform.position, member));
						}
					}
					List<(Vector3, string)> orderedPositions = OrderPointsClockwise(positions);
					CreateLinesForGroup(orderedPositions, -1, false, false);
				}
			}
			if (nameEvent.Equals(ItemTaskView.EventItemTaskViewHUDSelected))
            {
				if ((int)parameters[2] != -1)
				{
					TaskItemData task = (TaskItemData)parameters[3];
					DestroyLines();

					List<string> members = task.GetHumanMembers();
					List<(Vector3, string)> positions = new List<(Vector3, string)>();
					foreach (string member in members)
					{
						var (humanGO, humanData) = GetItemByName(member);
						if (humanGO != null)
						{
							positions.Add((humanGO.transform.position, member));
						}
					}
					List<(Vector3, string)> orderedPositions = OrderPointsClockwise(positions);
					CreateLinesForGroup(orderedPositions, task.UID, true, false);
				}
			}
			if (nameEvent.Equals(ItemGroupInfoView.EventItemGroupInfoViewSelected))
			{
				if ((int)parameters[2] == -1)
				{
					DestroyLines();
				}
				else
				{
					GroupInfoData selectedGroup = (GroupInfoData)parameters[3];
					if (selectedGroup != null)
					{
						DestroyLines();

						List<(Vector3, string)> positions = new List<(Vector3, string)>();
						foreach (string member in selectedGroup.Members)
						{
							var (humanGO, humanData) = GetItemByName(member);
							if (humanGO != null)
							{
								positions.Add((humanGO.transform.position, member));
							}
						}
						
						List<(Vector3, string)> orderedPositions = OrderPointsClockwise(positions);
						CreateLinesForGroup(orderedPositions, -1, false, true);
					}
				}
			}
		}
	}
}
