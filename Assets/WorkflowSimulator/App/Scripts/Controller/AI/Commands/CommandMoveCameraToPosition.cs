using UnityEngine;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class CommandMoveCameraToPosition : IGameCommand
	{
		private Vector3 _targetPosition;
		private float _timeAcum;
		private float _timeToMove;
		private float _targetZoom;
		private bool _isCompleted = false;

		private Vector3 _anchorPosition;
		private float _anchorZoom;

		public string Name
		{
			get { return "MoveCameraToPosition"; }
		}

		public MeetingData Meeting
		{
			get { return null; }
		}

        public bool Prioritary
        {
			get { return true; }
        }

        public string Member
        {
			get { return null; }
        }

        public bool RequestDestruction
		{
			get { return false; }
			set { }
		}

        public void Initialize(params object[] parameters)
		{
			_targetPosition = (Vector3)parameters[0];
			_timeToMove = (float)parameters[1];
			_targetZoom = (float)parameters[2];

			Vector3 floorRerence = ApplicationController.Instance.PlayerView.RayCastFloor();
			Vector3 positionPlayer = ApplicationController.Instance.PlayerView.transform.position;
			_targetPosition = _targetPosition - floorRerence;

			_timeAcum = 0;
			_anchorZoom = ApplicationController.Instance.PlayerView.transform.position.y;
			_anchorPosition = ApplicationController.Instance.PlayerView.transform.position;
			_targetZoom = _targetZoom - _anchorZoom;

			SystemEventController.Instance.DispatchSystemEvent(LevelView.EventLevelViewLinesRequestDestroy);
		}

		public bool IsBlocking()
		{
			return false;
		}

		public void Destroy()
		{
		}

		public bool IsCompleted()
		{
			return _isCompleted;
		}

		public void RunAction()
		{

		}

		public void Run()
		{
			if (!_isCompleted)
            {
				_timeAcum += Time.deltaTime;
				if (_timeAcum > _timeToMove)
                {
					_isCompleted = true;
                }
				else
                {
					Vector3 currPos = _anchorPosition + new Vector3(_targetPosition.x * (_timeAcum / _timeToMove), 0, _targetPosition.z * (_timeAcum / _timeToMove));
					ApplicationController.Instance.PlayerView.transform.position = currPos;

					float currZoom = _anchorPosition.y + _targetZoom * (_timeAcum / _timeToMove);
					ApplicationController.Instance.PlayerView.transform.position = new Vector3(currPos.x, currZoom, currPos.z);

					ApplicationController.Instance.PlayerView.UpdatePosition();
				}
			}
		}        
    }
}