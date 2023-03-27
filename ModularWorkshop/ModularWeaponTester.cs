using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FistVR;
using System.Linq;
using OpenScripts2;

namespace ModularWorkshop
{
    public class ModularWeaponTester : OpenScripts2_BasePlugin
    {
        public FVRPhysicalObject MainObject;

        public Transform TextFrame;
        public Text BarrelText;
        public Text HandguardText;
        public Text StockText;

        private FVRPhysicalObject _curObject;
        private IModularWeapon _modularWeapon;
        private IModularWeapon _lastModularWeapon;

        private int _selectedPart = 0;

        private LayerMask _layerMask;

        private Dictionary<GameObject, Text> _modularPartTexts = new();

        public void Awake()
        {
            BarrelText.text = "No Barrels detected";
            HandguardText.text = "No Handguards detected";
            StockText.text = "No Stocks detected";
            _layerMask = LayerMask.GetMask("Interactable");
        }

        public void Update()
        {
            Collider[] colliders = Physics.OverlapBox(transform.TransformPoint(GetComponent<BoxCollider>().center), GetComponent<BoxCollider>().size / 2f,transform.rotation, _layerMask);

            foreach (Collider collider in colliders)
            {
                _curObject = collider.GetComponent<FVRPhysicalObject>();

                if (_curObject != null && _curObject is IModularWeapon) _modularWeapon = _curObject as IModularWeapon;
            }

            if (_modularWeapon != _lastModularWeapon)
            {
                foreach (var TextGameObject in _modularPartTexts)
                {
                    Destroy(TextGameObject.Key);
                }

                _modularPartTexts.Clear();

                if (_modularWeapon != null)
                {
                    for (int i = 0; i < _modularWeapon.GetModularWeaponPartsAttachmentPoints.Length; i++)
                    {
                        GameObject textGO = Instantiate(StockText.gameObject, StockText.transform.position, StockText.transform.rotation, StockText.transform.parent);
                        textGO.transform.localPosition = textGO.transform.localPosition + new Vector3(0, 240f * (i + 1), 0);

                        _modularPartTexts.Add(textGO, textGO.GetComponent<Text>());
                    }

                    UpdateDisplay();
                }
                else
                {
                    BarrelText.text = "No Barrels detected";
                    HandguardText.text = "No Handguards detected";
                    StockText.text = "No Stocks detected";
                }
            }

            if (_modularWeapon != null)
            {
                FVRViveHand hand = MainObject.m_hand;
                if (hand != null)
                {
                    if (TouchpadDirPressed(hand,Vector2.up))
                    {
                        _selectedPart++;
                        UpdateDisplay();
                    }                    
                    if (TouchpadDirPressed(hand, Vector2.right))
                    {
                        UpdateSelectedPart();
                        UpdateDisplay();
                    }
                }
            }

            _lastModularWeapon = _modularWeapon;
        }

        public void UpdateSelectedPart()
        {
            if (_selectedPart >= 3 + _modularPartTexts.Count) _selectedPart = 0;
            int index;
            switch (_selectedPart)
            {
                case 0:
                    if (_modularWeapon.ModularBarrelPrefabs.Length == 0)
                    {
                        _selectedPart++;
                        UpdateSelectedPart();
                        break;
                    }

                    index = _modularWeapon.GetSelectedModularBarrel;
                    index++;
                    if (index >= _modularWeapon.ModularBarrelPrefabs.Length) index = 0;

                    _modularWeapon.ConfigureModularBarrel(index);
                    break;
                case 1:
                    if (_modularWeapon.ModularHandguardPrefabs.Length == 0)
                    {
                        _selectedPart++;
                        UpdateSelectedPart();
                        break;
                    }

                    index = _modularWeapon.GetSelectedModularHandguard;
                    index++;
                    if (index >= _modularWeapon.ModularHandguardPrefabs.Length) index = 0;

                    _modularWeapon.ConfigureModularHandguard(index);
                    break;
                case 2:
                    if (_modularWeapon.ModularStockPrefabs.Length == 0)
                    {
                        _selectedPart++;
                        UpdateSelectedPart();
                        break;
                    }

                    index = _modularWeapon.GetSelectedModularStock;
                    index++;
                    if (index >= _modularWeapon.ModularStockPrefabs.Length) index = 0;

                    _modularWeapon.ConfigureModularStock(index);
                    break;
            }
            if (_selectedPart >= 3)
            {
                string groupName = _modularWeapon.GetModularWeaponPartsAttachmentPoints[_selectedPart - 3].GroupName;
                if (_modularWeapon.GetModularWeaponPartsDictionary[groupName].Count == 0)
                {
                    _selectedPart++;
                    UpdateSelectedPart();
                }
                else
                {
                    index = _modularWeapon.GetModularWeaponPartsAttachmentPoints[_selectedPart - 3].SelectedModularWeaponPart;
                    index++;
                    if (index >= _modularWeapon.GetModularWeaponPartsDictionary[groupName].Count) index = 0;

                    _modularWeapon.ConfigureModularWeaponPart(_modularWeapon.GetModularWeaponPartsAttachmentPoints[_selectedPart - 3], index);
                }
            }
        }

        public void UpdateDisplay()
        {
            if (_modularWeapon.ModularBarrelPrefabs.Length != 0) BarrelText.text = _modularWeapon.ModularBarrelPrefabs[_modularWeapon.GetSelectedModularBarrel].GetComponent<ModularBarrel>().Name;
            if (_modularWeapon.ModularHandguardPrefabs.Length != 0) HandguardText.text = _modularWeapon.ModularHandguardPrefabs[_modularWeapon.GetSelectedModularHandguard].GetComponent<ModularHandguard>().Name;
            if (_modularWeapon.ModularStockPrefabs.Length != 0) StockText.text = _modularWeapon.ModularStockPrefabs[_modularWeapon.GetSelectedModularStock].GetComponent<ModularStock>().Name;

            switch( _selectedPart )
            {
                case 0:
                    TextFrame.localPosition = BarrelText.transform.localPosition;
                    break;
                case 1:
                    TextFrame.localPosition = HandguardText.transform.localPosition;
                    break;
                case 2:
                    TextFrame.localPosition = StockText.transform.localPosition;
                    break;
            }
            if (_selectedPart >= 3)
            {
                TextFrame.localPosition = _modularPartTexts.ElementAt(_selectedPart - 3).Key.transform.localPosition;
            }

            for (int i = 0; i < _modularPartTexts.Count; i++)
            {
                string groupName = _modularWeapon.GetModularWeaponPartsAttachmentPoints[i].GroupName;
                List<GameObject> prefabs = _modularWeapon.GetModularWeaponPartsDictionary[groupName];
                _modularPartTexts.ElementAt(i).Value.text = prefabs[_modularWeapon.GetModularWeaponPartsAttachmentPoints[i].SelectedModularWeaponPart].GetComponent<ModularWeaponPart>().Name;
            }
        }
    }
}
