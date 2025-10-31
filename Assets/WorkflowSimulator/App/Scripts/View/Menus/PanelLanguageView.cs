using TMPro;
using UnityEngine;
using UnityEngine.UI;
using yourvrexperience.Utils;

namespace yourvrexperience.WorkDay
{
	public class PanelLanguageView : MonoBehaviour
	{
		[SerializeField] private ToggleGroup toggleLanguages;
		[SerializeField] private Toggle toggleEnglish;
		[SerializeField] private Toggle toggleSpanish;
		[SerializeField] private Toggle toggleGerman;
		[SerializeField] private Toggle toggleFrench;
		[SerializeField] private Toggle toggleItalian;
		[SerializeField] private Toggle toggleRussian;
		[SerializeField] private Toggle toggleCatalan;
		[SerializeField] private TextMeshProUGUI titleScreen;

		void Start()
		{
			if (titleScreen != null) titleScreen.text = LanguageController.Instance.GetText("word.language");

			if (LanguageController.Instance.CodeLanguage.Equals(LanguageController.CodeLanguageEnglish))
			{
				toggleEnglish.isOn = true;
			}
			if (LanguageController.Instance.CodeLanguage.Equals(LanguageController.CodeLanguageSpanish))
			{
				toggleSpanish.isOn = true;
			}
			if (LanguageController.Instance.CodeLanguage.Equals(LanguageController.CodeLanguageGerman))
			{
				toggleGerman.isOn = true;
			}
			if (LanguageController.Instance.CodeLanguage.Equals(LanguageController.CodeLanguageFrench))
			{
				toggleFrench.isOn = true;
			}
			if (LanguageController.Instance.CodeLanguage.Equals(LanguageController.CodeLanguageItalian))
			{
				toggleItalian.isOn = true;
			}
			if (LanguageController.Instance.CodeLanguage.Equals(LanguageController.CodeLanguageCatalan))
			{
				toggleCatalan.isOn = true;
			}
			if (LanguageController.Instance.CodeLanguage.Equals(LanguageController.CodeLanguageRussian))
			{
				toggleRussian.isOn = true;
			}

			UpdateTexts();

			toggleEnglish.onValueChanged.AddListener(OnLanguageEnglish);
			toggleSpanish.onValueChanged.AddListener(OnLanguageSpanish);
			toggleGerman.onValueChanged.AddListener(OnLanguageGerman);
			toggleFrench.onValueChanged.AddListener(OnLanguageFrench);
			toggleItalian.onValueChanged.AddListener(OnLanguageItalian);
			toggleRussian.onValueChanged.AddListener(OnLanguageRussian);
			toggleCatalan.onValueChanged.AddListener(OnLanguageCatalan);
		}


		private void OnLanguageSpanish(bool value)
		{
			LanguageController.Instance.ChangeLanguage(LanguageController.CodeLanguageSpanish);
		}

		private void OnLanguageEnglish(bool value)
		{
			LanguageController.Instance.ChangeLanguage(LanguageController.CodeLanguageEnglish);
		}

		private void OnLanguageGerman(bool value)
		{
			LanguageController.Instance.ChangeLanguage(LanguageController.CodeLanguageGerman);
		}

		private void OnLanguageFrench(bool value)
		{
			LanguageController.Instance.ChangeLanguage(LanguageController.CodeLanguageFrench);
		}

		private void OnLanguageRussian(bool value)
		{
			LanguageController.Instance.ChangeLanguage(LanguageController.CodeLanguageRussian);
		}

		private void OnLanguageItalian(bool value)
		{
			LanguageController.Instance.ChangeLanguage(LanguageController.CodeLanguageItalian);
		}

		private void OnLanguageCatalan(bool value)
		{
			LanguageController.Instance.ChangeLanguage(LanguageController.CodeLanguageCatalan);
		}

		private void UpdateTexts()
		{
			if (titleScreen != null) titleScreen.text = LanguageController.Instance.GetText("word.language");
			toggleEnglish.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("language.english");
			toggleSpanish.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("language.spanish");
			toggleGerman.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("language.german");
			toggleFrench.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("language.french");
			toggleItalian.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("language.italian");
			toggleRussian.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("language.russian");
			toggleCatalan.transform.GetComponentInChildren<TextMeshProUGUI>().text = LanguageController.Instance.GetText("language.catalan");
		}
	}

}