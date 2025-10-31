using UnityEngine;

namespace yourvrexperience.WorkDay
{
	public class CommandGoToBase
	{
		protected bool _isCompleted = false;
		protected bool _hasStartedAction = false;
		protected bool _prioritary = false;
		protected bool _requestDestruction = false;
		protected float _timeToStart = 0;
		protected float _timeAcum = 0;
		protected float _timeSecond = 0;
		protected string _member = "";

		public bool Prioritary
        {
			get { return _prioritary; }
			set { _prioritary = value; }
        }
		public string Member 
		{
			get { return _member; }
		}
		public bool RequestDestruction
		{
			get { return _requestDestruction; }
			set { _requestDestruction = value; }
		}

		public virtual void Initialize(params object[] parameters)
		{
			_isCompleted = false;
			_hasStartedAction = false;
			_timeToStart = 0;
			_timeAcum = 0;
			_timeSecond = 0;
			_prioritary = true;
			_member = "";
			_requestDestruction = false;
		}

		public virtual void RunAction()
        {
			
		}

		public virtual void Run()
		{
			if (!_hasStartedAction && !_isCompleted)
            {
				_timeSecond += Time.deltaTime;
				if (_timeSecond >= 1)
                {
					_timeSecond -= 1;
					_timeAcum += (float)ApplicationController.Instance.TimeHUD.IncrementTime.TotalSeconds;
					if (_timeAcum > _timeToStart)
					{
						_hasStartedAction = true;
						RunAction();
					}
				}
			}
		}
    }
}