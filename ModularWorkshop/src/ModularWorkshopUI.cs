using FistVR;
using OpenScripts2;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        public GameObject ShowSkinsButton;
        public GameObject HideSkinsButton;
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
        public AudioEvent ApplySkinSound;
        public AudioEvent ShowSound;
        public AudioEvent HideSound;

        [HideInInspector]
        public IModularWeapon ModularWeapon;
        [HideInInspector]
        public ReceiverSkinSystem SkinSystem;

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

        // Skin UI
        private bool _isShowingSkins = false;
        private int _selectedSkin;
        private int _skinPageIndex;

        private string[] _skinNames;
        private string[] _skinDisplayNames;
        private Dictionary<string, ModularWorkshopSkinsDefinition.SkinDefinition> _skinDictionary;
        private Sprite[] _skinSprites;

        private bool _skinOnlyMode = false;
        private bool _receiverSkinMode = false;
        private bool _receiverSkinSystemMode = false;

        public void Awake()
        {
            foreach (var button in PartButtons)
            {
                button.SetActive(false);
            }

            ShowSkinsButton.SetActive(false);
            HideSkinsButton.SetActive(false);

            if (HideButton == null || ShowButton == null) _isShowingUI = true;
        }

        public void Beep()
        {
            SM.PlayCoreSound(FVRPooledAudioType.Generic, AudEvent_Beep, transform.position);
        }

        //public void Update()
        //{

        //}

        public void PBButton_ApplyPart()
        {
            string selectedPart = _partNames[_selectedPart];
            ModularWeaponPart part = PartType switch
            {
                EPartType.Barrel => ModularWeapon.ConfigureModularBarrel(selectedPart),
                EPartType.Handguard => ModularWeapon.ConfigureModularHandguard(selectedPart),
                EPartType.Stock => ModularWeapon.ConfigureModularStock(selectedPart),
                EPartType.MainWeaponGeneralAttachmentPoint => ModularWeapon.ConfigureModularWeaponPart(ModularWeapon.ModularWeaponPartsAttachmentPoints.Single(obj => obj.ModularPartsGroupID == ModularPartsGroupID), selectedPart),
                EPartType.SubAttachmentPoint => ModularWeapon.ConfigureModularWeaponPart(ModularWeapon.SubAttachmentPoints.Single(obj => obj.ModularPartsGroupID == ModularPartsGroupID), selectedPart),
                _ => null,
            };
            SM.PlayCoreSound(FVRPooledAudioType.Generic, ApplySound, transform.position);
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
            if (!_isShowingSkins) _pageIndex++;
            else _skinPageIndex++;
            //_selectedButton--;
            UpdateDisplay();
            Beep();
        }
        public void PBButton_Previous()
        {
            if (!_isShowingSkins) _pageIndex--;
            else _skinPageIndex--;
            //_selectedButton++;
            UpdateDisplay();
            Beep();
        }

        public void PButton_Select(int i)
        {
            _selectedButton = i;
            
            if (!_isShowingSkins)
            {
                _selectedPart = _selectedButton + _pageIndex * PartButtons.Length;
                PBButton_ApplyPart();
            }
            else
            {
                _selectedSkin = _selectedButton + _skinPageIndex * PartButtons.Length;
                PBButton_ApplySkin();
            }

            UpdateDisplay();
        }

        public void PButton_ShowSkins()
        {
            _isShowingSkins = true;

            string partName = _partNames[_selectedPart];
            _skinDictionary = ModularWorkshopManager.ModularWorkshopSkinsDictionary[ModularPartsGroupID + "/" + partName].SkinDictionary;
            _skinNames = _skinDictionary.Keys.ToArray();
            _skinSprites = _skinDictionary.Values.Select(s => s.Icon).ToArray();
            _skinDisplayNames = _skinDictionary.Values.Select(s => s.DisplayName).ToArray();
            string skinName = ModularWeapon.AllAttachmentPoints[ModularPartsGroupID].CurrentSkin;
            _selectedSkin = Array.IndexOf(_skinNames, skinName);
            _skinPageIndex = Mathf.FloorToInt(_selectedSkin / PartButtons.Length);
            _selectedButton = _selectedSkin - _skinPageIndex * PartButtons.Length;
            UpdateDisplay();
            Beep();
        }

        public void PButton_HideSkins()
        {
            _isShowingSkins = false;
            _selectedButton = _selectedPart - _pageIndex * PartButtons.Length;
            UpdateDisplay();
            Beep();
        }

        public void PBButton_ApplySkin()
        {
            if (!_receiverSkinMode) ModularWeapon.ApplySkin(ModularPartsGroupID, _skinNames[_selectedSkin]);
            else if (_receiverSkinMode && !_receiverSkinSystemMode) ModularWeapon.GetModularFVRFireArm.ApplyReceiverSkin(_skinNames[_selectedSkin]);
            else if (_receiverSkinMode && _receiverSkinSystemMode) SkinSystem.ApplyReceiverSkin(_skinNames[_selectedSkin]);
            SM.PlayCoreSound(FVRPooledAudioType.Generic, ApplySkinSound, transform.position);
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

            if (_partDictionary.Count <= 1)
            {
                _skinOnlyMode = true;

                _isShowingSkins = true;

                string partName = _partNames[_selectedPart];
                _skinDictionary = ModularWorkshopManager.ModularWorkshopSkinsDictionary[ModularPartsGroupID + "/" + partName].SkinDictionary;
                _skinNames = _skinDictionary.Keys.ToArray();
                _skinSprites = _skinDictionary.Values.Select(s => s.Icon).ToArray();
                _skinDisplayNames = _skinDictionary.Values.Select(s => s.DisplayName).ToArray();
                string skinName = ModularWeapon.AllAttachmentPoints[ModularPartsGroupID].CurrentSkin;
                _selectedSkin = Array.IndexOf(_skinNames, skinName);
                _skinPageIndex = Mathf.FloorToInt(_selectedSkin / PartButtons.Length);
                _selectedButton = _selectedSkin - _skinPageIndex * PartButtons.Length;
            }
        }

        public void SetupReceiverSkinOnlyMode(bool receiverSkinSystemMode = false)
        {
            _skinOnlyMode = true;
            _receiverSkinMode = true;
            _isShowingSkins = true;
            _receiverSkinSystemMode = receiverSkinSystemMode;

            if (!_receiverSkinSystemMode)
            {
                string skinPath = ModularWeapon.GetModularFVRFireArm.SkinPath;

                _skinDictionary = ModularWorkshopManager.ModularWorkshopSkinsDictionary[skinPath].SkinDictionary;
                _skinNames = _skinDictionary.Keys.ToArray();
                _skinSprites = _skinDictionary.Values.Select(s => s.Icon).ToArray();
                _skinDisplayNames = _skinDictionary.Values.Select(s => s.DisplayName).ToArray();
                string skinName = ModularWeapon.GetModularFVRFireArm.CurrentSelectedReceiverSkinID;
                _selectedSkin = Array.IndexOf(_skinNames, skinName);
                _skinPageIndex = Mathf.FloorToInt(_selectedSkin / PartButtons.Length);
                _selectedButton = _selectedSkin - _skinPageIndex * PartButtons.Length;
            }
            else
            {
                string skinPath = SkinSystem.SkinPath;

                _skinDictionary = ModularWorkshopManager.ModularWorkshopSkinsDictionary[skinPath].SkinDictionary;
                _skinNames = _skinDictionary.Keys.ToArray();
                _skinSprites = _skinDictionary.Values.Select(s => s.Icon).ToArray();
                _skinDisplayNames = _skinDictionary.Values.Select(s => s.DisplayName).ToArray();
                string skinName = SkinSystem.CurrentSelectedReceiverSkinID;
                _selectedSkin = Array.IndexOf(_skinNames, skinName);
                _skinPageIndex = Mathf.FloorToInt(_selectedSkin / PartButtons.Length);
                _selectedButton = _selectedSkin - _skinPageIndex * PartButtons.Length;
            }
        }

        public void UpdateDisplay()
        {
            if (DisplayNameText != null && !_skinOnlyMode) DisplayNameText.text = ModularWorkshopManager.ModularWorkshopDictionary[ModularPartsGroupID].DisplayName;
            else if (DisplayNameText != null && _skinOnlyMode) DisplayNameText.text = "Receiver Skins";

            if (_isShowingUI)
            {
                MainCanvas?.SetActive(true);
                ShowButton?.SetActive(false);
                HideButton?.SetActive(true);

                if (!_isShowingSkins)
                {
                    for (int i = 0; i < PartButtons.Length; i++)
                    {
                        if (i + PartButtons.Length * _pageIndex < _partDictionary.Count)
                        {
                            PartButtons[i].SetActive(true);
                            string partName = _partNames[i + PartButtons.Length * _pageIndex];
                            PartTexts[i].text = partName;
                            PartImages[i].sprite = _partSprites[i + PartButtons.Length * _pageIndex];
                        }
                        else
                        {
                            PartButtons[i].SetActive(false);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < PartButtons.Length; i++)
                    {
                        if (i + PartButtons.Length * _skinPageIndex < _skinDictionary.Count)
                        {
                            PartButtons[i].SetActive(true);

                            PartTexts[i].text = _skinDisplayNames[i + PartButtons.Length * _skinPageIndex];
                            PartImages[i].sprite = _skinSprites[i + PartButtons.Length * _skinPageIndex];
                        }
                        else
                        {
                            PartButtons[i].SetActive(false);
                        }
                    }
                }

                if (!_isShowingSkins && _pageIndex == 0) BackButton.SetActive(false);
                else if (_isShowingSkins && _skinPageIndex == 0) BackButton.SetActive(false);
                else BackButton.SetActive(true);

                if (!_isShowingSkins && _partDictionary.Count > PartButtons.Length * (1 + _pageIndex)) NextButton.SetActive(true);
                else if (_isShowingSkins && _skinDictionary.Count > PartButtons.Length * (1 + _skinPageIndex)) NextButton.SetActive(true);
                else NextButton.SetActive(false);

                ButtonSet.SetSelectedButton(_selectedButton);

                if (!_skinOnlyMode)
                {
                    string currentPartName = _partNames[_selectedPart];
                    if (!_isShowingSkins && ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(ModularPartsGroupID + "/" + currentPartName, out ModularWorkshopSkinsDefinition definition) && definition.SkinDictionary.Count > 1)
                    {
                        ShowSkinsButton.SetActive(true);
                    }
                    else ShowSkinsButton.SetActive(false);
                }

                if (_isShowingSkins && !_skinOnlyMode) HideSkinsButton.SetActive(true);
                else HideSkinsButton.SetActive(false);

                if (PageIndex != null)
                {
                    PageIndex.text = _isShowingSkins
                        ? $"{1 + _skinPageIndex}/{Mathf.CeilToInt(1 + (_skinDictionary.Count / PartButtons.Length))}"
                        : $"{1 + _pageIndex}/{Mathf.CeilToInt(1 + (_partDictionary.Count / PartButtons.Length))}";
                }
            }
            else
            {
                MainCanvas.SetActive(false);
                if (!_skinOnlyMode && _partDictionary.Count > 1) ShowButton.SetActive(true);
                else if (_skinOnlyMode && _skinDictionary.Count > 1) ShowButton.SetActive(true);
                else ShowButton.SetActive(false);
                HideButton.SetActive(false);

                if (!_skinOnlyMode) ShowButtonText.text = ModularWorkshopManager.ModularWorkshopDictionary[ModularPartsGroupID].DisplayName;
                else ShowButtonText.text = "Receiver Skin";
            }
        }
    }
}