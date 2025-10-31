using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class ScreenBoardsView : BaseScreenView, IScreenView
	{
		public const string ScreenName = "ScreenBoardsView";

		public const string EventScreenBoardsViewAIGeneration = "EventScreenBoardsViewAIGeneration";		

		[SerializeField] private TextMeshProUGUI titleScreen;
		[SerializeField] private TextMeshProUGUI titleName;
		[SerializeField] private TextMeshProUGUI titleDescription;
		[SerializeField] private TextMeshProUGUI titleFeedback;

		[SerializeField] private CustomInput inputNameBoard;
		[SerializeField] private CustomInput inputDescriptionBoard;

		[SerializeField] private Button buttonAssign;
		[SerializeField] private Button buttonUnAssign;
		
		[SerializeField] private Button buttonAdd;
		[SerializeField] private Button buttonClear;
		[SerializeField] private Button buttonClose;

		[SerializeField] private SlotManagerView SlotManagerAvailable;
		[SerializeField] private SlotManagerView SlotManagerAssigned;
		[SerializeField] private GameObject PrefabBoard;

		[SerializeField] private Button buttonAIGeneration;

		private HumanView _human;
		private List<string> _boardsNames;
		private WorldItemData _humanData;

		private string _boardNameToAssign = "";
		private string _boardNameToUnAssign = "";

		private string _boardName = "";
		private string _boardDescription = "";

		private BoardData _selecteBoardData = null;
	
		public override void Initialize(params object[] parameters)
		{
			base.Initialize(parameters);

			_human = null;

			buttonClose.onClick.AddListener(OnClose);

			buttonAdd.onClick.AddListener(OnAddBoard);
			buttonClear.onClick.AddListener(OnClearData);

			buttonAssign.onClick.AddListener(OnAssignBoard);
			buttonUnAssign.onClick.AddListener(OnUnAssignBoard);
			buttonAssign.interactable = false;
			buttonUnAssign.interactable = false;

			inputNameBoard.onValueChanged.AddListener(OnNameBoardChanged);
			inputNameBoard.text = "";

			inputDescriptionBoard.onValueChanged.AddListener(OnDescriptionBoardChanged);
			inputDescriptionBoard.text = "";

			buttonAIGeneration.onClick.AddListener(OnAIGeneration);

			titleFeedback.text = "";

			buttonClear.interactable = true;

			LoadBoardsNames();
			LoadBoardsHuman();

			titleScreen.text = LanguageController.Instance.GetText("screen.boards.title");
            titleName.text = LanguageController.Instance.GetText("text.name");
            titleDescription.text = LanguageController.Instance.GetText("text.description");
			buttonAIGeneration.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("screen.boards.ai.board.generation");

            UIEventController.Instance.Event += OnUIEvent;

			SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerAppEnableMovement, false);

			ProjectInfoData projectInfo = WorkDayData.Instance.CurrentProject.GetProject(WorkDayData.Instance.CurrentProject.ProjectInfoSelected);
			if (projectInfo != null)
            {
				_content.GetComponent<Image>().color = projectInfo.GetColor();
				titleFeedback.text = projectInfo.Name;
			}

			if (ApplicationController.Instance.IsPlayMode)
			{
				buttonAIGeneration.interactable = false;
				inputNameBoard.interactable = false;
				inputDescriptionBoard.interactable = false;

				buttonAssign.interactable = false;
				buttonUnAssign.interactable = false;

				buttonAdd.interactable = false;
				buttonClear.interactable = false;
			}
		}

		private void OnAIGeneration()
        {
			AICommandsController.Instance.AddNewAICommand(new AICommandGenerateBoard(), true, EventScreenBoardsViewAIGeneration);
        }

        public override void Destroy()
		{
			base.Destroy();
			if (UIEventController.Instance != null) UIEventController.Instance.Event -= OnUIEvent;

			_human = null;
			_humanData = null;

			SystemEventController.Instance.DispatchSystemEvent(PlayerView.EventPlayerAppEnableMovement, true);
		}

		private void OnClearData()
		{
			_selecteBoardData = null;
			inputNameBoard.text = "";
			inputDescriptionBoard.text = "";
		}

		private void OnNameBoardChanged(string value)
		{
			_boardName = value;

			AllowAddNewBoard();
		}

		private void OnDescriptionBoardChanged(string value)
        {
			_boardDescription = value;

			AllowAddNewBoard();
		}

		private void AllowAddNewBoard()
        {
			bool isInteractable = (_boardName.Length > 2) && (_boardDescription.Length > 2);

			buttonAdd.interactable = isInteractable;
			if (isInteractable)
            {
				if (_selecteBoardData != null)
				{
					_selecteBoardData.BoardName = _boardName;
					_selecteBoardData.Description = _boardDescription;
					buttonAdd.interactable = false;
				}
				else
                {
					BoardData board = WorkDayData.Instance.CurrentProject.GetBoardFor(_boardName, false);
					if (board != null)
					{
						buttonAdd.interactable = false;
					}
				}
			}
		}

		private void LoadBoardsNames()
        {
			_boardsNames = new List<string>();
			List<BoardData> boards = WorkDayData.Instance.CurrentProject.GetAllBoards();
			foreach (BoardData board in boards)
            {
				if (board.ProjectId == WorkDayData.Instance.CurrentProject.ProjectInfoSelected)
                {
					_boardsNames.Add(board.BoardName);
				}
			}

			LoadBoards(SlotManagerAvailable, _boardsNames, true);
		}

		private void LoadBoardsHuman()
		{
			if (_human != null)
            {
				var (humanGO, humanData) = ApplicationController.Instance.LevelView.GetItemByName(_human.NameHuman);
				_humanData = humanData;
				List<string> allBoardsHuman = _humanData.GetBoards();
				List<string> boardsHuman = new List<string>();
				foreach (string board in allBoardsHuman)
				{
					BoardData boardData = WorkDayData.Instance.CurrentProject.GetBoardFor(board);
					if (boardData != null)
					{
						if (boardData.ProjectId == WorkDayData.Instance.CurrentProject.ProjectInfoSelected)
						{
							boardsHuman.Add(board);
						}
					}
				}
				LoadBoards(SlotManagerAssigned, boardsHuman, false);
			}
		}

		private void OnClose()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EventScreenControllerDestroyScreen, this.gameObject);
		}

		private void LoadBoards(SlotManagerView slotManager, List<string> boards, bool enableDelete)
		{
			slotManager.ClearCurrentGameObject(true);
			slotManager.Initialize(0, new List<ItemMultiObjectEntry>(), PrefabBoard);

			for (int i = 0; i < boards.Count; i++)
			{
				slotManager.AddItem(new ItemMultiObjectEntry(slotManager.gameObject, slotManager.Data.Count, boards[i], enableDelete));
			}
		}

		private void OnAddBoard()
		{
			if (_selecteBoardData == null)
            {
				if ((_boardName.Length > 2) && (_boardDescription.Length > 2))
				{
					SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerAddNewBoard, _boardName, _boardDescription);
					inputNameBoard.text = "";
					inputDescriptionBoard.text = "";
				}
			}
		}

		private void OnAssignBoard()
		{
			if (_boardNameToAssign.Length > 0)
			{
				string boardNameToAssign = _boardNameToAssign;
				_boardNameToAssign = "";
				List<string> boardsHuman = _humanData.GetBoards();
				if (!boardsHuman.Contains(boardNameToAssign))
				{
					boardsHuman.Add(boardNameToAssign);
					_humanData.SetBoards(boardsHuman);
					LoadBoardsHuman();
				}
			}
		}

		private void OnUnAssignBoard()
        {
			if (_boardNameToUnAssign.Length > 0)
			{
				string boardNameToUnAssign = _boardNameToUnAssign;
				_boardNameToUnAssign = "";
				List<string> boardsHuman = _humanData.GetBoards();
				if (boardsHuman.Remove(boardNameToUnAssign))
				{
					_humanData.SetBoards(boardsHuman);
					LoadBoardsHuman();
				}
			}
		}

		private void OnUIEvent(string nameEvent, object[] parameters)
		{
			if (nameEvent.Equals(TasksController.EventTasksControllerRefreshBoard))
            {
				LoadBoardsNames();
				LoadBoardsHuman();
			}
			if (nameEvent.Equals(ItemBoardView.EventItemBoardViewEdit))
            {
				string nameBoard = (string)parameters[2];
				ScreenController.Instance.CreateScreen(ScreenTaskManagerView.ScreenName, false, true, nameBoard);
			}
			if (nameEvent.Equals(ItemBoardView.EventItemBoardViewDelete))
			{
				string nameBoard = (string)parameters[2];
				OnClearData();
				SystemEventController.Instance.DispatchSystemEvent(TasksController.EventTasksControllerDeleteBoard, nameBoard);
			}
			if (nameEvent.Equals(ItemBoardView.EventItemBoardViewSelected))
            {
				if ((GameObject)parameters[0] == SlotManagerAvailable.gameObject)
                {
					if ((int)parameters[2] == -1)
                    {
						_boardNameToAssign = "";
						_selecteBoardData = null;
						buttonAssign.interactable = false;
						inputNameBoard.text = "";
						inputDescriptionBoard.text = "";
					}
					else
                    {
						_boardNameToAssign = (string)parameters[3];
						_selecteBoardData = WorkDayData.Instance.CurrentProject.GetBoardFor(_boardNameToAssign);
						_boardName = _selecteBoardData.BoardName;
						_boardDescription = _selecteBoardData.Description;
						inputNameBoard.text = _boardName;
						inputDescriptionBoard.text = _boardDescription;
						buttonAssign.interactable = true;
						buttonUnAssign.interactable = false;
						UIEventController.Instance.DispatchUIEvent(ItemBoardView.EventItemBoardViewUnSelectByParent, SlotManagerAssigned.gameObject);
					}
					if (_human == null)
                    {
						buttonAssign.interactable = false;
						buttonUnAssign.interactable = false;
					}
				}
				if ((GameObject)parameters[0] == SlotManagerAssigned.gameObject)
				{
					if ((int)parameters[2] == -1)
					{
						_boardNameToUnAssign = "";
						buttonUnAssign.interactable = false;
					}
					else
					{
						_boardNameToUnAssign = (string)parameters[3];
						buttonUnAssign.interactable = true;
						buttonAssign.interactable = false;
						UIEventController.Instance.DispatchUIEvent(ItemBoardView.EventItemBoardViewUnSelectByParent, SlotManagerAvailable.gameObject);
					}
				}
			}
		}
	}
}