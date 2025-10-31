using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;
#if ENABLE_OCULUS || ENABLE_OPENXR || ENABLE_ULTIMATEXR || ENABLE_NREAL
using yourvrexperience.VR;
#endif

namespace yourvrexperience.WorkDay
{
	public class ScreenListProjectsView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenListProjectsView";

		public const string EventScreenListProjectsViewLoadProjects = "EventScreenListProjectsViewLoadProjects";
		public const string EventScreenListProjectsViewSelectProject = "EventScreenListProjectsViewSelectProject";
		public const string EventScreenListProjectsViewDeleteProject = "EventScreenListProjectsViewDeleteProject";
		public const string EventScreenListProjectsViewClose = "EventScreenListProjectsViewClose";

		public const string SubEventDeleteProjectConfirmation = "SubEventDeleteProjectConfirmation";

		[SerializeField] private GameObject StoryNamePrefab;
		[SerializeField] private GameObject ButtonLinkVideo;
		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI descriptionScreen;
		[SerializeField] private SlotManagerView SlotManagerStories;
		[SerializeField] private Button buttonSelect;
		[SerializeField] private Button buttonDelete;
		[SerializeField] private Button buttonBack;
		[SerializeField] private Sprite[] iconsPackages;

		private ProjectEntryIndex _projectSelected;

		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			buttonBack.onClick.AddListener(OnButtonBack);
			buttonDelete.onClick.AddListener(OnButtonDelete);
			buttonSelect.onClick.AddListener(OnButtonSelectStory);

			buttonDelete.gameObject.SetActive(false);

			titleScreen.text = LanguageController.Instance.GetText("screen.users.offices.title");
			descriptionScreen.text = LanguageController.Instance.GetText("screen.users.offices.description");

			buttonSelect.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.users.offices.select.office");
			buttonSelect.interactable = false;

			UIEventController.Instance.Event += OnUIEvent;
			SystemEventController.Instance.Event += OnSystemEvent;

			LoadProjects((List<ProjectEntryIndex>)parameters[0]);

			ButtonLinkVideo.SetActive(ApplicationController.Instance.IsPlayMode);
        }

		public override void Destroy()
		{
			base.Destroy();

			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;
			if (SystemEventController.Instance != null) SystemEventController.Instance.Event -= OnSystemEvent;
		}

		private void OnButtonBack()
		{			
			UIEventController.Instance.DispatchUIEvent(EventScreenListProjectsViewClose);
		}

		private void OnButtonDelete()
		{
			if (_projectSelected != null)
			{
				string titleWarning = LanguageController.Instance.GetText("text.warning");
				string textAskToExit = LanguageController.Instance.GetText("screen.main.do.you.want.to.delete.office");
				ScreenInformationView.CreateScreenInformation(ScreenInformationView.ScreenConfirmation, this.gameObject, titleWarning, textAskToExit, SubEventDeleteProjectConfirmation);
			}
		}

		private void OnButtonSelectStory()
		{
			if (_projectSelected != null)
			{
				UIEventController.Instance.DispatchUIEvent(EventScreenListProjectsViewSelectProject, _projectSelected);				
			}
		}

		private void LoadProjects(List<ProjectEntryIndex> data)
        {
			List<ProjectEntryIndex> stories = data;
			SlotManagerStories.ClearCurrentGameObject(true);
			List<ItemMultiObjectEntry> itemsUserStories = new List<ItemMultiObjectEntry>();
			if (stories != null)
            {
				for (int i = 0; i < stories.Count; i++)
				{
					itemsUserStories.Add(new ItemMultiObjectEntry(this.gameObject, i, stories[i], iconsPackages));
				}
				SlotManagerStories.Initialize(itemsUserStories.Count, itemsUserStories, StoryNamePrefab);
			}
		}

		private void OnSystemEvent(string nameEvent, object[] parameters)
		{
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(SubEventDeleteProjectConfirmation))
			{
				ScreenInformationResponses userResponse = (ScreenInformationResponses)parameters[1];
				if (userResponse == ScreenInformationResponses.Confirm)
				{					
					SystemEventController.Instance.DispatchSystemEvent(EventScreenListProjectsViewDeleteProject, _projectSelected.Id);
				}
			}			
			if (nameEvent.Equals(EventScreenListProjectsViewLoadProjects))
			{
				LoadProjects((List<ProjectEntryIndex>)parameters[0]);
			}
			if (nameEvent.Equals(ItemProjectEntryIndex.EventItemProjectEntryIndexSelected))
			{
				if ((GameObject)parameters[0] == this.gameObject)
				{
					if ((int)parameters[2] == -1)
					{
						_projectSelected = null;
						buttonSelect.interactable = false;
						buttonDelete.gameObject.SetActive(false);
					}
					else
					{
						_projectSelected = (ProjectEntryIndex)parameters[3];
						buttonSelect.interactable = true;
						buttonDelete.gameObject.SetActive(!ApplicationController.Instance.IsPlayMode);
					}
				}
			}
		}
	}
}