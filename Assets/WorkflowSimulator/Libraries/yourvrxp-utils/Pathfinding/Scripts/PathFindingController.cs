using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace yourvrexperience.Utils
{

    /******************************************
	* 
	* PathFindingController
	* 
	* Run A* to search a path between to cells of a matrix
	* 
	* @author Esteban Gallardo
	*/
    public class PathFindingController : MonoBehaviour
    {
        // ----------------------------------------------
        // PUBLIC CONSTANTS
        // ----------------------------------------------
        public const bool DEBUG_MATRIX_CONSTRUCTION = false;
        public const bool DEBUG_PATHFINDING = false;
        public const bool DEBUG_DOTPATHS = false;

        public const string TAG_FLOOR = "Floor";
        public const string TAG_PATH = "PATH";

        // CELLS
        public const int CELL_EMPTY = 0;
        public const int CELL_COLLISION = 1;

        // CONSTANTS DIRECTIONS
        public const int DIRECTION_LEFT = 1;
        public const int DIRECTION_RIGHT = 2;
        public const int DIRECTION_UP = 100;
        public const int DIRECTION_DOWN = 200;
        public const int DIRECTION_NONE = -1;

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static PathFindingController _instance;

        public static PathFindingController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(PathFindingController)) as PathFindingController;
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------
        public GameObject PathFindingPrefab;
        public GameObject DotReference;
        public GameObject DotReferenceEmtpy;
        public GameObject DotReferenceWay;

        public bool DebugPathPoints;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private List<PathFindingInstance> pathfindingInstances = new List<PathFindingInstance>();
        private bool _isPrecalculated = false;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool IsPrecalculated
        {
            get { return _isPrecalculated; }
        }

        // ---------------------------------------------------
        /**
		 * Constructor of cPathFinding
		 */
        public void Initialize()
        {
        }

        // ---------------------------------------------------
        /**
		 * Destroy
		 */
        public void Destroy()
        {
            if (_instance == null) return;
            _instance = null;

            // ClearDotPaths();
        }

        // ---------------------------------------------------
        /**
		 * Set the height of the waypoints
		 */
        public void SetWaypointHeight(float waypointHeight, int layer = -1)
        {
            if (layer == -1)
            {
                pathfindingInstances[pathfindingInstances.Count - 1].WaypointHeight = waypointHeight;
            }
            else
            {
                pathfindingInstances[layer].WaypointHeight = waypointHeight;
            }
        }

        // ---------------------------------------------------
        /**
		 * Set the height of the waypoints
		 */
        public void SetPathWaypointHeight(float pathHeight, int layer = -1)
        {
            if (layer == -1)
            {
                pathfindingInstances[pathfindingInstances.Count - 1].PathCheckHeight = pathHeight;
            }
            else
            {
                pathfindingInstances[layer].PathCheckHeight = pathHeight;
            }
        }

        // ---------------------------------------------------
        /**
		 * Get the content of the cell in the asked position
		 */
        public bool CheckOutsideBoard(float x, float y, float z, int layer = -1)
        {
            if (layer == -1)
            {
                return pathfindingInstances[pathfindingInstances.Count - 1].CheckOutsideBoard(x, y, z);
            }
            else
            {
                return pathfindingInstances[layer].CheckOutsideBoard(x, y, z);
            }
        }        

        // ---------------------------------------------------
        /**
		 * Get the cell of the current position
		 */
        public Vector3 GetCellPositionInMatrix(float x, float y, float z, int layer = -1)
        {
            if (layer == -1)
            {
                return pathfindingInstances[pathfindingInstances.Count - 1].GetCellPositionInMatrix(x, y, z);
            }
            else
            {
                return pathfindingInstances[layer].GetCellPositionInMatrix(x, y, z);
            }
        }

        // ---------------------------------------------------
        /**
		 * Get the content of the cell in the asked position
		 */
        public int GetCellContentByRealPosition(float x, float y, float z, int layer = -1)
        {
            if (layer == -1)
            {
                return pathfindingInstances[pathfindingInstances.Count - 1].GetCellContentByRealPosition(x, y, z);
            }
            else
            {
                return pathfindingInstances[layer].GetCellContentByRealPosition(x, y, z);
            }
        }

        // ---------------------------------------------------
        /**
		 * Will initialize the structure to be able to use it
		 */
        public void AllocateMemoryMatrix(int cols,
                                        int rows,
                                        int layers,
                                        float cellSize,
                                        float xIni,
                                        float yIni,
                                        float zIni,
                                        int[][][] initContent = null)
        {
            GameObject newPathfindingInstance = Instantiate(PathFindingPrefab);
            newPathfindingInstance.GetComponent<PathFindingInstance>().AllocateMemoryMatrix(cols, rows, layers, cellSize, xIni, yIni, zIni, initContent);
            pathfindingInstances.Add(newPathfindingInstance.GetComponent<PathFindingInstance>());
        }

        // ---------------------------------------------------
        /**
		 * Release memory
		 */
        public void DestroyInstances()
        {
            if (pathfindingInstances != null)
            {
                foreach (PathFindingInstance instance in pathfindingInstances)
                {
                    if (instance != null)
                    {
                        instance.ClearMemoryAllocated();
                        instance.Destroy();
                        instance.DestroyDebugMatrixConstruction();
                        GameObject.Destroy(instance.gameObject);
                    }
                }
                pathfindingInstances.Clear();
            }
        }

        // ---------------------------------------------------
        /**
		 * Will dynamically calculate the collisions
		 */
        public void CalculateCollisions(int layerToCheck = 0, params string[] layersToIgnore)
        {
            foreach (PathFindingInstance pathInstance in pathfindingInstances)
            {
                pathInstance.CalculateCollisions(layerToCheck, layersToIgnore);
            }
        }

        // ---------------------------------------------------
        /**
		 * ClearDotPaths
		 */
        public void ClearDotPaths()
        {
            foreach (PathFindingInstance pathInstance in pathfindingInstances)
            {
                pathInstance.ClearDotPaths();
            }
        }

        // ---------------------------------------------------
        /**
		 * CreateSingleDot
		 */
        public GameObject CreateSingleDot(Vector3 position, float size, int type, int layer = -1)
        {
            if (layer == -1)
            {
                return pathfindingInstances[pathfindingInstances.Count - 1].CreateSingleDot(position, size, type);
            }
            else
            {
                return pathfindingInstances[layer].CreateSingleDot(position, size, type);
            }
        }


        // ---------------------------------------------------
        /**
		 * RenderDebugMatrixConstruction
		 */
        public void RenderDebugMatrixConstruction(int layer = -1, float timeToDisplayCollisions = 0)
        {
            if (timeToDisplayCollisions > 0)
            {
                if (layer == -1)
                {
                    // RENDER ALL LAYERS
                    for (int i = 0; i < pathfindingInstances.Count; i++)
                    {
                        pathfindingInstances[i].RenderDebugMatrixConstruction(0, pathfindingInstances.Count - 1 - i, timeToDisplayCollisions);
                    }
                }
                else
                {
                    pathfindingInstances[layer].RenderDebugMatrixConstruction(layer, -1, timeToDisplayCollisions);
                }
            }
        }

        public void DestroyDebugMatrixConstruction(int layer = -1)
        {
            if (pathfindingInstances != null)
            {
                if (layer == -1)
                {
                    // RENDER ALL LAYERS
                    for (int i = 0; i < pathfindingInstances.Count; i++)
                    {
                        pathfindingInstances[i].DestroyDebugMatrixConstruction();
                    }
                }
                else
                {
                    pathfindingInstances[layer].DestroyDebugMatrixConstruction();
                }
            }
        }

        // ---------------------------------------------------
        /**
		 * CheckBlockedPath
		 */
        public bool CheckBlockedPath(Vector3 origin, Vector3 target, float dotSize = 3, params string[] masksToIgnore)
        {
            return (RaycastingTools.GetCollidedObjectBySegmentTargetIgnore(target, origin, masksToIgnore));
        }

        // ---------------------------------------------------
        /**
		* Gets the path between 2 positions
		*/
        public Vector3 GetPath(Vector3 origin,
                                Vector3 destination,
                                List<Vector3> waypoints,
                                int oneLayer,
                                bool raycastFilter,
                                int limitSearch = -1,
                                params string[] masksToIgnore)
        {
            // USE THE LAST PATH
            return pathfindingInstances[pathfindingInstances.Count - 1].GetPath(origin, destination, waypoints, oneLayer, raycastFilter, limitSearch, masksToIgnore);
        }

        // ---------------------------------------------------
        /**
		* Gets the path between 2 positions
		*/
        public Vector3 GetPathLayer(int layer,
                                Vector3 origin,
                                Vector3 destination,
                                List<Vector3> waypoints,
                                int oneLayer,
                                bool raycastFilter,
                                int limitSearch = -1,
                                params string[] masksToIgnore)
        {
            return pathfindingInstances[layer].GetPath(origin, destination, waypoints, oneLayer, raycastFilter, limitSearch, masksToIgnore);
        }

        // ---------------------------------------------------
        /**
		* Check if the position is a free one
		*/
        public Vector3 IsPositionInFreeNode(Vector3 position, int layer = -1)
        {
            if (layer == -1)
            {
                return pathfindingInstances[pathfindingInstances.Count - 1].IsPositionInFreeNode(position);
            }
            else
            {
                return pathfindingInstances[layer].IsPositionInFreeNode(position);
            }
        }

        // ---------------------------------------------------
        /**
		* Gets the path between 2 positions
		*/
        public Vector3 GetClosestFreeNode(Vector3 position, int layer = -1)
        {
            if (layer == -1)
            {
                return pathfindingInstances[pathfindingInstances.Count - 1].GetClosestFreeNode(position);
            }
            else
            {
                return pathfindingInstances[layer].GetClosestFreeNode(position);
            }
        }

        // ---------------------------------------------------
        /**
		* Gets the size of the cell
		*/
        public float GetCellSize(int layer = -1)
        {
            if (layer == -1)
            {
                return pathfindingInstances[pathfindingInstances.Count - 1].CellSize;
            }
            else
            {
                return pathfindingInstances[layer].CellSize;
            }
        }


        // ---------------------------------------------------
        /**
		* GetRandomFreeCellBorder
		*/
        public Vector3 GetRandomFreeCellBorder(int layer = -1)
        {
            if (layer == -1)
            {
                return pathfindingInstances[pathfindingInstances.Count - 1].GetRandomFreeCellBorder();
            }
            else
            {
                return pathfindingInstances[layer].GetRandomFreeCellBorder();
            }
        }

        // ---------------------------------------------------
        /**
		* CheckOutsideBoard
		*/
        public bool CheckOutsideBoard(Vector3 position, int layer = -1)
        {
            if (layer == -1)
            {
                return pathfindingInstances[pathfindingInstances.Count - 1].CheckOutsideBoard(position.x, position.y, position.z);
            }
            else
            {
                return pathfindingInstances[layer].CheckOutsideBoard(position.x, position.y, position.z);
            }
        }

        // ---------------------------------------------------
        /**
		* Precalculate all the paths
		*/
        public void CalculateAll(string filenamePath, int layer = -1, bool raycastFilter = false, params string[] masksToIgnore)
        {
            if (layer == -1)
            {
                pathfindingInstances[pathfindingInstances.Count - 1].CalculateAll(filenamePath, raycastFilter, masksToIgnore);
            }
            else
            {
                pathfindingInstances[layer].CalculateAll(filenamePath, raycastFilter, masksToIgnore);
            }
        }

        // ---------------------------------------------------
        /**
		 * Load data of pathfinding
		*/
        public void LoadFile(string filenamePath, int layer = -1)
        {
            _isPrecalculated = true;
            if (layer == -1)
            {
                pathfindingInstances[pathfindingInstances.Count - 1].LoadFile(filenamePath);
            }
            else
            {
                pathfindingInstances[layer].LoadFile(filenamePath);
            }
        }

        // ---------------------------------------------------
        /**
		 * Load data of pathfinding
		*/
        public void LoadAsset(TextAsset textAsset, int layer = -1)
        {
            _isPrecalculated = true;
            if (layer == -1)
            {
                pathfindingInstances[pathfindingInstances.Count - 1].LoadAsset(textAsset);
            }
            else
            {
                pathfindingInstances[layer].LoadAsset(textAsset);
            }
        }
    }
}