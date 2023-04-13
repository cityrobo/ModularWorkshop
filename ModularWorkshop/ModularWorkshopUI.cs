using FistVR;
using OpenScripts2;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModularWorkshop
{
    public class ModularWorkshopUI : MonoBehaviour
    {
        [Header("Required interface parts.")]
        public OptionsPanel_ButtonSet ButtonSet;
        public GameObject[] PartButtons;
        public Text[] PartTexts;
        public Image[] PartImages;
        public GameObject BackButton;
        public GameObject NextButton;
        [Header("Optional interface parts.")]
        [Tooltip("Optional Field that displays the selected and maximum page index.")]
        public Text PageIndex;
        [Tooltip("Button that allows to show the UI when hidden. Requires ShowButtonText, HideButton and MainCanvas to be set up as well.")]
        public GameObject ShowButton;
        [Tooltip("Text field that shows the part group name on the show button. Requires ShowButton, HideButton and MainCanvas to be set up as well.")]
        public Text ShowButtonText;
        [Tooltip("Button that allows to hide the UI when shown. Requires ShowButton, ShowButtonText and MainCanvas to be set up as well.")]
        public GameObject HideButton;
        [Tooltip("Game object that contains all objects that should be hidden. Requires ShowButton, ShowButtonText and HideButton to be set up as well.")]
        public GameObject MainCanvas;
        [Tooltip("Optional Field that simply displays the name of the part group. Can be the PartsID or the DisplayName, depending on how the PartsDefinition is set up.")]
        public Text DisplayNameText;
        //public GameObject ApplyButton;
        [Header("Sound effects.")]
        public AudioEvent AudEvent_Beep;
        public AudioEvent ApplySound;
        public AudioEvent ShowSound;
        public AudioEvent HideSound;

        [HideInInspector]
        public IModularWeapon ModularWeapon;

        public enum EPartType
        {
            Barrel,
            Handguard,
            Stock,
            MainWeaponGeneralAttachmentPoint,
            SubAttachmentPoint
        }
        [HideInInspector]
        public EPartType PartType;
        [HideInInspector]
        public string ModularPartsGroupID;

        private int _pageIndex;
        private int _selectedButton;
        private int _selectedPart;

        private string[] _partNames;
        private Dictionary<string,GameObject> _partDictionary;
        private Sprite[] _partSprites;

        private bool _isShowingUI = false;

        public void Awake()
        {
            foreach (var button in PartButtons)
            {
                button.SetActive(false);
            }

            if (HideButton == null || ShowButton == null) _isShowingUI = true;
        }

        public void Beep()
        {
            SM.PlayCoreSound(FVRPooledAudioType.Generic, AudEvent_Beep, transform.position);
        }

        //public void Update()
        //{

        //}

        public void PBButton_Apply()
        {
            ModularWeaponPart part;
            switch (PartType)
            {
                case EPartType.Barrel:
                    part = ModularWeapon.ConfigureModularBarrel(_partNames[_selectedPart]);
                    break;
                case EPartType.Handguard:
                    part = ModularWeapon.ConfigureModularHandguard(_partNames[_selectedPart]);
                    break;
                case EPartType.Stock:
                    part = ModularWeapon.ConfigureModularStock(_partNames[_selectedPart]);
                    break;
                case EPartType.MainWeaponGeneralAttachmentPoint:
                    part = ModularWeapon.ConfigureModularWeaponPart(ModularWeapon.ModularWeaponPartsAttachmentPoints.Single(obj => obj.ModularPartsGroupID == ModularPartsGroupID), _partNames[_selectedPart]);
                    break;
                case EPartType.SubAttachmentPoint:
                    part = ModularWeapon.ConfigureModularWeaponPart(ModularWeapon.SubAttachmentPoints.Single(obj => obj.ModularPartsGroupID == ModularPartsGroupID), _partNames[_selectedPart]);
                    break;
                default:
                    part = null;
                    break;
            }
        }

        public void PButton_ShowUI()
        {
            _isShowingUI = true;
            UpdateDisplay();
            SM.PlayCoreSound(FVRPooledAudioType.Generic, ShowSound, transform.position);
        }

        public void PButton_HideUI()
        {
            _isShowingUI = false;
            UpdateDisplay();
            SM.PlayCoreSound(FVRPooledAudioType.Generic, HideSound, transform.position);
        }

        public void PBButton_Next()
        {
            _pageIndex++;
            //_selectedButton--;
            UpdateDisplay();
            Beep();
        }
        public void PBButton_Previous()
        {
            _pageIndex--;
            //_selectedButton++;
            UpdateDisplay();
            Beep();
        }

        public void PButton_Select(int i)
        {
            _selectedButton = i;
            _selectedPart = _selectedButton + _pageIndex;
            UpdateDisplay();
            PBButton_Apply();
            SM.PlayCoreSound(FVRPooledAudioType.Generic, ApplySound, transform.position);
        }

        public void InitializeArrays()
        {
            switch (PartType)
            {
                case EPartType.Barrel:
                    _partDictionary = ModularWeapon.ModularBarrelPrefabsDictionary;
                    _partNames = _partDictionary.Select(prefab => prefab.Key).ToArray();
                    _selectedPart = Array.IndexOf(_partNames, ModularWeapon.SelectedModularBarrel);
                    break;
                case EPartType.Handguard:
                    _partDictionary = ModularWeapon.ModularHandguardPrefabsDictionary;
                    _partNames = _partDictionary.Select(prefab => prefab.Key).ToArray();
                    _selectedPart = Array.IndexOf(_partNames, ModularWeapon.SelectedModularHandguard);
                    break;
                case EPartType.Stock:
                    _partDictionary = ModularWeapon.ModularStockPrefabsDictionary;
                    _partNames = _partDictionary.Select(prefab => prefab.Key).ToArray();
                    _selectedPart = Array.IndexOf(_partNames, ModularWeapon.SelectedModularStock);
                    break;
                case EPartType.MainWeaponGeneralAttachmentPoint:
                    _partDictionary = ModularWorkshopManager.ModularWorkshopDictionary[ModularPartsGroupID].PartsDictionary;
                    _partNames = _partDictionary.Select(prefab => prefab.Key).ToArray();
                    _selectedPart = Array.IndexOf(_partNames, ModularWeapon.ModularWeaponPartsAttachmentPoints.Single(obj => obj.ModularPartsGroupID == ModularPartsGroupID).SelectedModularWeaponPart);
                    break;
                case EPartType.SubAttachmentPoint:
                    _partDictionary = ModularWorkshopManager.ModularWorkshopDictionary[ModularPartsGroupID].PartsDictionary;
                    _partNames = _partDictionary.Select(prefab => prefab.Key).ToArray();
                    _selectedPart = Array.IndexOf(_partNames, ModularWeapon.SubAttachmentPoints.Single(obj => obj.ModularPartsGroupID == ModularPartsGroupID).SelectedModularWeaponPart);
                    break;
            }
            _partSprites = _partDictionary.Select(prefab => prefab.Value.GetComponent<ModularWeaponPart>().Icon).ToArray();

            _pageIndex = Mathf.FloorToInt(_selectedPart / PartButtons.Length);
            if (_pageIndex < 0) _pageIndex = 0;
            _selectedButton = _selectedPart - _pageIndex * PartButtons.Length;
        }

        public void UpdateDisplay()
        {
            if (DisplayNameText != null) DisplayNameText.text = ModularWorkshopManager.ModularWorkshopDictionary[ModularPartsGroupID].DisplayName;

            if (_isShowingUI)
            {
                MainCanvas.SetActive(true);
                ShowButton.SetActive(false);
                HideButton.SetActive(true);

                for (int i = 0; i < PartButtons.Length; i++)
                {
                    if (i + PartButtons.Length * _pageIndex < _partDictionary.Count)
                    {
                        PartButtons[i].SetActive(true);
                        PartTexts[i].text = _partNames[i + PartButtons.Length * _pageIndex];
                        PartImages[i].sprite = _partSprites[i + PartButtons.Length * _pageIndex];
                    }
                    else
                    {
                        PartButtons[i].SetActive(false);
                    }
                }

                if (_pageIndex == 0) BackButton.SetActive(false);
                else BackButton.SetActive(true);

                if (_partDictionary.Count > PartButtons.Length * _pageIndex) NextButton.SetActive(true);
                else NextButton.SetActive(false);

                ButtonSet.SetSelectedButton(_selectedButton);

                if (PageIndex != null) PageIndex.text = $"{1 + _pageIndex}/{Mathf.CeilToInt(1 + (_partDictionary.Count / PartButtons.Length))}";
            }
            else
            {
                MainCanvas.SetActive(false);
                ShowButton.SetActive(true);
                HideButton.SetActive(false);

                ShowButtonText.text = ModularWorkshopManager.ModularWorkshopDictionary[ModularPartsGroupID].DisplayName;
            }
        }
    }
}