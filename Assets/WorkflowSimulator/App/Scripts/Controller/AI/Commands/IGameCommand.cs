using UnityEditor;
using UnityEngine;

namespace yourvrexperience.WorkDay
{
	public interface IGameCommand
	{		
		bool RequestDestruction { get; set; }
		bool Prioritary { get; }
		MeetingData Meeting { get; }
		string Member { get; }
		string Name { get; }

		void Initialize(params object[] parameters);
		bool IsBlocking();
		bool IsCompleted();
		void Destroy();
		void RunAction();
		void Run();
	}
}