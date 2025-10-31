using UnityEngine;
using yourvrexperience.Utils;
using System;
using yourvrexperience.UserManagement;
using yourvrexperience.Networking;
using static yourvrexperience.WorkDay.ApplicationController;

namespace yourvrexperience.WorkDay
{
	public interface IAICommand
	{
		string Name { get; }
		void Request(bool confirmation, params object[] parameters);
		void Destroy();
		void Run();
		bool IsCompleted();
	}
}