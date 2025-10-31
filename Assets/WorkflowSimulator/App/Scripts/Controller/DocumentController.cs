using yourvrexperience.Utils;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace yourvrexperience.WorkDay
{
	public class DocumentController : MonoBehaviour
	{
		public const string EventDocumentControllerUpdateTaskDocs = "EventDocumentControllerUpdateTaskDocs";
		public const string EventDocumentControllerUpdateMeetingDocs = "EventDocumentControllerUpdateMeetingDocs";

		private static DocumentController _instance;

		public static DocumentController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(DocumentController)) as DocumentController;
				}
				return _instance;
			}
		}


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

		private (List<DocumentData>,List<(int, string)>) GetChangedDocuments(List<DocumentData> docs)
        {
			List<DocumentData> changedDocuments = new List<DocumentData>();
			List<(int, string)> documentSummaries = new List<(int, string)>();
			foreach (DocumentData doc in docs)
			{
				if (doc.IsChanged)
				{
					changedDocuments.Add(doc);
#if UNITY_EDITOR
					documentSummaries.Add((doc.Id, Utilities.RandomCodeGeneration(10)));
#endif
				}
			}
			return (changedDocuments, documentSummaries);
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(ScreenDocumentsDataView.EventScreenDocumentsDataViewUpdateGlobalData))
			{
				List<DocumentData> finalDocuments = (List<DocumentData>)parameters[0];
				WorkDayData.Instance.CurrentProject.SetDocuments(finalDocuments.ToArray());

				// ++AI++ SUMMARIZE GLOBAL DOCUMENTS CHANGED 
				var (documentsChanged, documentSummaries) = GetChangedDocuments(finalDocuments);
				if (documentsChanged.Count > 0)
                {
					AICommandsController.Instance.AddNewAICommand(new AICommandSummarizeDocs(), true, documentsChanged, "");
				}
				else
                {
					WorkDayData.Instance.CurrentProject.UpdateGlobalDocuments();
				}
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(EventDocumentControllerUpdateTaskDocs))
			{
				TaskItemData task = (TaskItemData)parameters[0];
				task.Data = (DocumentData[])parameters[1];
				foreach (DocumentData docTask in task.Data)
                {
					docTask.TaskID = task.UID;
                }
				
				if ((task.Data == null) || (task.Data.Length == 0))
				{
					UIEventController.Instance.DispatchUIEvent(ScreenTaskView.EventScreenTaskViewReloadData);
					UIEventController.Instance.DispatchUIEvent(ScreenMeetingView.EventScreenMeetingViewReloadData);
				}
				else
                {
					// ++AI++ SUMMARIZE LOCAL (FOR TASK) DOCUMENTS CHANGED
					var (documentsChanged, documentSummaries) = GetChangedDocuments(task.Data.ToList<DocumentData>());
					if (documentsChanged.Count > 0)
					{
						AICommandsController.Instance.AddNewAICommand(new AICommandSummarizeDocs(), true, documentsChanged, "");
					}
				}
			}
			if (nameEvent.Equals(EventDocumentControllerUpdateMeetingDocs))
			{
				MeetingData meeting = (MeetingData)parameters[0];
				meeting.Data = (DocumentData[])parameters[1];
				
				if ((meeting.Data == null) || (meeting.Data.Length == 0))
				{
					UIEventController.Instance.DispatchUIEvent(ScreenMeetingView.EventScreenMeetingViewReloadData);
				}
				else
				{
					// ++AI++ SUMMARIZE LOCAL (FOR TASK) DOCUMENTS CHANGED 
					var (documentsChanged, documentSummaries) = GetChangedDocuments(meeting.GetData());
					if (documentsChanged.Count > 0)
					{
						AICommandsController.Instance.AddNewAICommand(new AICommandSummarizeDocs(), true, documentsChanged, "");
					}
				}
			}
			if (nameEvent.Equals(ApplicationController.EventMainControllerReleaseGameResources))
			{
			}
			if (nameEvent.Equals(SystemEventController.EventSystemEventControllerReleaseAllResources))
			{
				_instance = null;
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