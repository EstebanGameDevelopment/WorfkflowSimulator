using yourvrexperience.Utils;
using UnityEngine;
using static yourvrexperience.WorkDay.RunStateRun;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
	public class LevelEditionController : MonoBehaviour
	{
		private static LevelEditionController _instance;

		public static LevelEditionController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(LevelEditionController)) as LevelEditionController;
				}
				return _instance;
			}
		}

        [SerializeField] private GameObject PrefabInfoHuman;
        [SerializeField] private GameObject PrefabInfoHours;
        [SerializeField] private GameObject[] PrefabsLines;

        public void Initialize()
		{
			SystemEventController.Instance.Event += OnSystemEvent;
			UIEventController.Instance.Event += OnUIEvent;
		}

        void OnDestroy()
		{
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
        }

        public GameObject CreateLine(int type)
        {
            return Instantiate(PrefabsLines[type]);
        }

        public GameObject CreateInfoHuman()
        {
            return Instantiate(PrefabInfoHuman);
        }

        public GameObject CreateInfoHours()
        {
            return Instantiate(PrefabInfoHours);
        }

        private void OnUIEvent(string nameEvent, object[] parameters)
		{
            if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewActivation))
            {
                if ((bool)parameters[0])
                {
                    ApplicationController.Instance.LevelView.DestroyAllCells();
                    ApplicationController.Instance.LevelView.DestroyAllItems();
                    ApplicationController.Instance.LevelView.DestroyAllAreas();
                    ApplicationController.Instance.LevelView.DestroyWorld();

                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Resize, null, null);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Idle, null, null);
                }
            }
            if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewWork))
            {
                if ((bool)parameters[0])
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Areas, AreaMode.Work, null);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Idle, null, null);
                }
            }
            if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewMeeting))
            {
                if ((bool)parameters[0])
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Areas, AreaMode.Meeting, null);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Idle, null, null);
                }
            }
            if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewKitchen))
            {
                if ((bool)parameters[0])
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Areas, AreaMode.Kitchen, null);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Idle, null, null);
                }
            }
            if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewBathroom))
            {
                if ((bool)parameters[0])
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Areas, AreaMode.Bathroom, null);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Idle, null, null);
                }
            }
            if (nameEvent.Equals(TabEditionResizeView.EventTabEditionResizeViewExit))
            {
                if ((bool)parameters[0])
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Areas, AreaMode.Exit, null);
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Idle, null, null);
                }
            }
            if (nameEvent.Equals(ItemImageCatalog.EventItemImageCatalogSelected))
            {
                int isSelected = (int)parameters[2];
                int idCatalog = (int)parameters[3];
                if (isSelected != -1)
                {
                    AssetDefinitionItem itemSelected = AssetsCatalogData.Instance.GetAssetById(idCatalog);
                    if (itemSelected.IsHuman)
                    {
                        SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Avatar, itemSelected, null);
                    }
                    else
                    {
                        SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Decoration, itemSelected, null);
                    }
                }
                else
                {
                    SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Idle, null, null);
                }
            }
            if (nameEvent.Equals(ItemImageCatalog.EventItemImageCatalogUnSelectAll))
            {
                SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Idle, null, null);
            }
            if (nameEvent.Equals(TabEditionBaseView.EventTabEditionBaseViewActivation))
            {
                SystemEventController.Instance.DispatchSystemEvent(RunStateRun.EventRunStateRunChangeState, SubStateRun.Idle, null, null);
            }
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
		{
            if (nameEvent.Equals(ApplicationController.EventMainControllerReleaseGameResources))
			{
            }
            if (nameEvent.Equals(SystemEventController.EventSystemEventControllerReleaseAllResources))
			{                
                GameObject.Destroy(this.gameObject);
			}
			if (nameEvent.Equals(SystemEventController.EventSystemEventControllerDontDestroyOnLoad))
			{
				if (Instance)
				{
					DontDestroyOnLoad(Instance.gameObject);
				}
			}
		}
	}
}