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
        public OptionsPanel_ButtonSet ButtonSet;
        public GameObject[] PartButtons;
        public Text[] PartTexts;
        public Image[] PartImages;
        public GameObject BackButton;
        public GameObject NextButton;
        public GameObject ShowButton;
        public Text ShowButtonText;
        public GameObject HideButton;
        public GameObject MainCanvas;
        public Text PageIndex;
        //public GameObject ApplyButton;

        public AudioEvent AudEvent_Beep;

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
        public string PartID;

        private int _entryIndex;
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
                    part = ModularWeapon.ConfigureModularWeaponPart(ModularWeapon.ModularWeaponPartsAttachmentPoints.Single(obj => obj.PartID == PartID), _partNames[_selectedPart]);
                    break;
                case EPartType.SubAttachmentPoint:
                    part = ModularWeapon.ConfigureModularWeaponPart(ModularWeapon.SubAttachmentPoints.Single(obj => obj.PartID == PartID), _partNames[_selectedPart]);
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
            Beep();
        }

        public void PButton_HideUI()
        {
            _isShowingUI = false;
            UpdateDisplay();
            Beep();
        }

        public void PBButton_Next()
        {
            _entryIndex += PartButtons.Length;
            //_selectedButton--;
            UpdateDisplay();
            Beep();
        }
        public void PBButton_Previous()
        {
            _entryIndex -= PartButtons.Length;
            //_selectedButton++;
            UpdateDisplay();
            Beep();
        }

        public void PButton_Select(int i)
        {
            _selectedButton = i;
            _selectedPart = _selectedButton + _entryIndex;
            UpdateDisplay();
            PBButton_Apply();
            Beep();
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
                    _partDictionary = ModularWorkshopManager.ModularWorkshopDictionary[PartID].PartsDictionary;
                    _partNames = _partDictionary.Select(prefab => prefab.Key).ToArray();
                    _selectedPart = Array.IndexOf(_partNames, ModularWeapon.ModularWeaponPartsAttachmentPoints.Single(obj => obj.PartID == PartID).SelectedModularWeaponPart);
                    break;
                case EPartType.SubAttachmentPoint:
                    _partDictionary = ModularWorkshopManager.ModularWorkshopDictionary[PartID].PartsDictionary;
                    _partNames = _partDictionary.Select(prefab => prefab.Key).ToArray();
                    _selectedPart = Array.IndexOf(_partNames, ModularWeapon.SubAttachmentPoints.Single(obj => obj.PartID == PartID).SelectedModularWeaponPart);
                    break;
            }
            _partSprites = _partDictionary.Select(prefab => prefab.Value.GetComponent<ModularWeaponPart>().Icon).ToArray();

            _entryIndex = _selectedPart - (PartButtons.Length - 1);
            if (_entryIndex < 0) _entryIndex = 0;
            _selectedButton = _selectedPart - _entryIndex;
        }

        public void UpdateDisplay()
        {
            if (_isShowingUI)
            {
                MainCanvas.SetActive(true);
                ShowButton.SetActive(false);
                HideButton.SetActive(true);

                for (int i = _entryIndex; i - _entryIndex < PartButtons.Length; i++)
                {
                    if (i < _partDictionary.Count)
                    {
                        PartButtons[i - _entryIndex].SetActive(true);
                        PartTexts[i - _entryIndex].text = _partNames[i];
                        PartImages[i - _entryIndex].sprite = _partSprites[i];
                    }
                    else
                    {
                        PartButtons[i - _entryIndex].SetActive(false);
                    }
                }

                //for (int i = 0; i < Mathf.Min(_partDictionary.Count, PartButtons.Length); i++)
                //{
                //    PartButtons[i].SetActive(true);

                //    PartTexts[i].text = _partNames[i + _entryIndex];
                //    PartImages[i].sprite = _partSprites[i + _entryIndex];
                //}

                if (_entryIndex == 0) BackButton.SetActive(false);
                else BackButton.SetActive(true);

                if (_partDictionary.Count > PartButtons.Length + _entryIndex) NextButton.SetActive(true);
                else NextButton.SetActive(false);

                ButtonSet.SetSelectedButton(_selectedButton);

                if (PageIndex != null) PageIndex.text = $"{1 + (_entryIndex / PartButtons.Length)}/{Mathf.CeilToInt(1 + (_partDictionary.Count / PartButtons.Length))}";
            }
            else
            {
                MainCanvas.SetActive(false);
                ShowButton.SetActive(true);
                HideButton.SetActive(false);

                ShowButtonText.text = PartID;

                //switch (PartType)
                //{
                //    case EPartType.Barrel:
                //        ShowButtonText.text = "Barrel";
                //        break;
                //    case EPartType.Handguard:
                //        ShowButtonText.text = "Handguard";
                //        break;
                //    case EPartType.Stock:
                //        ShowButtonText.text = "Stock";
                //        break;
                //    case EPartType.MainWeaponGeneralAttachmentPoint:
                //        ShowButtonText.text = PartID;
                //        break;
                //    case EPartType.SubAttachmentPoint:
                //        ShowButtonText.text = PartID;
                //        break;
                //}
            }
        }
    }
}