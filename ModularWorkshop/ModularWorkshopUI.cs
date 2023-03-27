using FistVR;
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
        //public GameObject ApplyButton;

        public AudioEvent AudEvent_Beep;

        [HideInInspector]
        public IModularWeapon ModularWeapon;

        public enum EPartType
        {
            Barrel,
            Handguard,
            Stock,
            Custom
        }
        [HideInInspector]
        public EPartType PartType;
        [HideInInspector]
        public string CustomPartTypeGroupName;

        private IModularWeapon _lastModularWeapon;

        private int _entryIndex;
        private int _selectedPart;

        private string[] _partNames;
        private GameObject[] _partPrefabs;
        private Sprite[] _partSprites;

        public void Awake()
        {
            foreach (var item in PartButtons)
            {
                item.SetActive(false);
            }
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
            switch (PartType)
            {
                case EPartType.Barrel:
                    ModularWeapon.ConfigureModularBarrel(_selectedPart);
                    break;
                case EPartType.Handguard:
                    ModularWeapon.ConfigureModularHandguard(_selectedPart);
                    break;
                case EPartType.Stock:
                    ModularWeapon.ConfigureModularStock(_selectedPart);
                    break;
                case EPartType.Custom:
                    ModularWeapon.ConfigureModularWeaponPart(ModularWeapon.GetModularWeaponPartsAttachmentPoints.Single(obj => obj.GroupName == CustomPartTypeGroupName), _selectedPart);
                    break;
            }
        }

        public void PBButton_Next()
        {
            _entryIndex++;
            UpdateDisplay();
            Beep();
        }
        public void PBButton_Previous()
        {
            _entryIndex--;
            UpdateDisplay();
            Beep();
        }

        public void PButton_Select(int i)
        {
            _selectedPart = i + _entryIndex;
            UpdateDisplay();
            Beep();
        }

        public void InitializeArrays()
        {
            switch (PartType)
            {
                case EPartType.Barrel:
                    _partPrefabs = ModularWeapon.ModularBarrelPrefabs;
                    _selectedPart = ModularWeapon.GetSelectedModularBarrel;
                    break;
                case EPartType.Handguard:
                    _partPrefabs = ModularWeapon.ModularHandguardPrefabs;
                    _selectedPart = ModularWeapon.GetSelectedModularHandguard;
                    break;
                case EPartType.Stock:
                    _partPrefabs = ModularWeapon.ModularStockPrefabs;
                    _selectedPart = ModularWeapon.GetSelectedModularStock;
                    break;
                case EPartType.Custom:
                    _partPrefabs = ModularWeapon.GetModularWeaponPartsDictionary[CustomPartTypeGroupName].ToArray();
                    _selectedPart = ModularWeapon.GetModularWeaponPartsAttachmentPoints.Single(obj => obj.GroupName == CustomPartTypeGroupName).SelectedModularWeaponPart;
                    break;
            }
            _partNames = _partPrefabs.Select(prefab => prefab.GetComponent<ModularWeaponPart>().Name).ToArray();
            _partSprites = _partPrefabs.Select(prefab => prefab.GetComponent<ModularWeaponPart>().Icon).ToArray();
        }

        public void UpdateDisplay()
        {
            for (int i = 0; i < Mathf.Min(_partPrefabs.Length, PartButtons.Length); i++)
            {
                PartButtons[i].SetActive(true);

                PartTexts[i].text = _partNames[i + _entryIndex];
                PartImages[i].sprite = _partSprites[i + _entryIndex];
            }

            if (_entryIndex == 0) BackButton.SetActive(false);
            else BackButton.SetActive(true);

            if (_partPrefabs.Length > PartButtons.Length + _entryIndex) NextButton.SetActive(true);
            else NextButton.SetActive(false);

            ButtonSet.SetSelectedButton(_selectedPart);
        }
    }
}