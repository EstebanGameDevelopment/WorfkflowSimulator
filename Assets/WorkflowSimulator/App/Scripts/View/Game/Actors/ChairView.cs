using yourvrexperience.Utils;
using UnityEngine;
using static yourvrexperience.WorkDay.EditionSubStateAreas;

namespace yourvrexperience.WorkDay
{
	public class ChairView : MonoBehaviour
	{
        public const string EventChairViewAssignAreaData = "EventChairViewAssignAreaData";
        public const string EventChairViewRequestInAreaData = "EventChairViewRequestInAreaData";
        public const string EventChairViewReportInAreaData = "EventChairViewReportInAreaData";

        private HumanView _human;
		private AreaData _areaData;
        private string _nameChair;
        private float _timeOpenDoor = 0;

        private ObjectStates _objectState;

        public HumanView Human
        {
            get { return _human; }
        }
        public AreaData Area
        {
            get { return _areaData; }
        }
        public string NameChair
        {
            get { return _nameChair; }
        }

        public void Initialize(string nameChair)
        {
            _human = null;
            _areaData = null;
            _nameChair = nameChair;

            _objectState = this.gameObject.GetComponent<ObjectStates>();
            if (_objectState != null)
            {
                _objectState.CollisionEnterEvent += OnChairEntered;
                _objectState.CollisionExitEvent += OnChairEntered;
            }

            Unity.AI.Navigation.NavMeshModifier modifier = this.gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
            modifier.ignoreFromBuild = true;

            SystemEventController.Instance.Event += OnSystemEvent;
        }

        private void OnDestroy()
        {
            if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
            if (_objectState != null)
            {
                _objectState.CollisionEnterEvent -= OnChairEntered;
                _objectState.CollisionExitEvent -= OnChairEntered;
            }
        }

        private void OnChairEntered(GameObject collider, GameObject other)
        {
            if (!ApplicationController.Instance.LevelView.EditionMode)
            {
                if (other.GetComponent<HumanView>() != null)
                {
                    if (_objectState != null)
                    {
                        _objectState.EnableState(1);
                        _timeOpenDoor = 1;
                    }
                }
            }
        }

        public void SetHuman(HumanView human)
        {
            _human = human;
        }

        private void OnSystemEvent(string nameEvent, object[] parameters)
        {
            if (nameEvent.Equals(EventChairViewRequestInAreaData))
            {
                AreaMode targetMode = (AreaMode)parameters[1];
                if (_areaData != null)
                {
                    if (targetMode == (AreaMode)_areaData.Type)
                    {
                        SystemEventController.Instance.DispatchSystemEvent(EventChairViewReportInAreaData, parameters[0], this);
                    }
                }
            }
            if (nameEvent.Equals(EventChairViewAssignAreaData))
            {
                AreaData areaData = ApplicationController.Instance.LevelView.GetAreaByPosition(this.gameObject.transform.position);
                if ((areaData != null) && (_areaData != areaData))
                {
                    _areaData = areaData;
                }
            }
        }

        public void OpenDoor()
        {
            if (_objectState != null)
            {
                _objectState.EnableState(1);
                _timeOpenDoor = 1;
            }
        }

        private void Update()
        {
            if (_timeOpenDoor > 0)
            {
                _timeOpenDoor -= Time.deltaTime;
                if (_timeOpenDoor < 0)
                {
                    _objectState.EnableState(0);
                }
            }
        }
    }
}
