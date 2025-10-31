using System.Collections.Generic;

namespace yourvrexperience.WorkDay
{
	[System.Serializable]
	public class ProjectSlot
	{
		public int Id;
		public int Project;
		public int Level;
		public long Timeout;

		public ProjectSlot(string[] tokens)
		{
			Id = int.Parse(tokens[0]);
			Project = int.Parse(tokens[1]);
			Level = int.Parse(tokens[2]);
			Timeout = long.Parse(tokens[3]);
		}
	}

	[System.Serializable]
	public class CostAIOperation
	{
		public string Operation;
		public string Provider;
		public float Cost;
		public int InputTokens;
		public int OutputTokens;

		public CostAIOperation(string operation, string provider, float cost, int inputTokens, int outputTokens)
		{
			Operation = operation;
			Provider = provider;
			Cost = cost;
			InputTokens = inputTokens;
			OutputTokens = outputTokens;
		}
	}


	[System.Serializable]
	public class StorageUsed
	{
		public const int TOTAL_SPACE_BASIC = 524288;
		public const int TOTAL_SPACE_IMAGES = 268435456; // 250 Megas
	
		public int Data;
		public int Images;
		
		public float PercentageTotal;

		public float PercentageData;
		public float PercentageImages;

		public void Calculate(int level)
		{
			switch (level)
			{
				case 0:
					PercentageData = (float)Data / (float)TOTAL_SPACE_BASIC;
					PercentageTotal = PercentageData;
					break;

				case 1:
					PercentageData = (float)Data / (float)TOTAL_SPACE_IMAGES;
					PercentageImages = (float)Images / (float)TOTAL_SPACE_IMAGES;

					PercentageTotal = PercentageData + PercentageImages;
					break;

				default:
					PercentageData = (float)Data / (float)TOTAL_SPACE_BASIC;
					PercentageTotal = PercentageData;
					break;
			}
		}
	}

	[System.Serializable]
	public class AssetDefinitionItem
	{
		public int Id;
		public string Name;
		public string AssetIcon;
		public string AssetName;
		public int x;
		public int y;
		public bool IsHuman;
		public bool IsChair;
		public bool IsMan;
	}

	[System.Serializable]
	public class AssetDefinitionItemList
	{
		public List<AssetDefinitionItem> items;
	}


	[System.Serializable]
	public class ProjectEntryIndex
	{
		public int Id;
		public int User;
		public int DataId;
		public string Title;
		public string Description;
		public int Category1;
		public int Category2;
		public int Category3;
		public int TimeCreation;

		public ProjectEntryIndex(int id, int user, int dataId, string title, string description, int category1, int category2, int category3, int timeCreation)
		{
			Id = id;
			User = user;
			DataId = dataId;
			Title = title;
			Description = description;
			Category1 = category1;
			Category2 = category2;
			Category3 = category3;
			TimeCreation = timeCreation;
		}

		public ProjectEntryIndex(string[] tokens)
		{
			Id = int.Parse(tokens[0]);
			User = int.Parse(tokens[1]);
			DataId = int.Parse(tokens[2]);
			Title = tokens[3];
			Description = tokens[4];
			Category1 = int.Parse(tokens[5]);
			Category2 = int.Parse(tokens[6]);
			Category3 = int.Parse(tokens[7]);
			TimeCreation = int.Parse(tokens[8]);
		}
	}
}