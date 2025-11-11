using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using OpenScripts2;
using UnityEngine;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class GameObjectDictionaryMoveToAddon : MonoBehaviour, IPartPhysicalObjectRequirement
    {
        private FVRPhysicalObject _physicalObject;
        private GameObject _gameObjectFromDictionary;
        private Transform _origParent;
        private Vector3 _origLocalPosition;
        private Quaternion _origLocalRotation;

        public string GameObjectDictionaryIDToMove;
        public GameObject GameObjectToMoveTo;

        public FVRPhysicalObject PhysicalObject
        { 
            set
            {
                if (value != null)
                {
                    _physicalObject = value;

                    if (_physicalObject.TryGetComponentsInChildren(out GameObjectDictionary[] gameObjectDictionarys))
                    {
                        foreach (var gameObjectDictionary in gameObjectDictionarys)
                        {
                            if (gameObjectDictionary.TryGetGameObject(GameObjectDictionaryIDToMove, out _gameObjectFromDictionary) == true)
                            {
                                _origParent = _gameObjectFromDictionary.transform.parent;
                                _origLocalPosition = _gameObjectFromDictionary.transform.localPosition;
                                _origLocalRotation = _gameObjectFromDictionary.transform.localRotation;
                                _gameObjectFromDictionary.transform.parent = GameObjectToMoveTo.transform.parent;
                                _gameObjectFromDictionary.transform.localPosition = GameObjectToMoveTo.transform.localPosition;
                                _gameObjectFromDictionary.transform.localRotation = GameObjectToMoveTo.transform.localRotation;
                                break;
                            }
                        }
                        if (_gameObjectFromDictionary == null)
                        {
                            ModularWorkshopManager.LogWarning($"GameObjectDictionaryMoveToAddon on {gameObject.name} could not find GameObject with ID {GameObjectDictionaryIDToMove} in any GameObjectDictionaries of the main modular object!");
                        }
                    }
                    else
                    {
                        ModularWorkshopManager.LogWarning($"GameObjectDictionaryMoveToAddon on {gameObject.name} requires the main modular object or its children to have a GameObjectDictionary component, but it didn't find one!");
                    }
                }
                else
                {
                    if (_gameObjectFromDictionary != null)
                    {
                        _gameObjectFromDictionary.transform.parent = _origParent;
                        _gameObjectFromDictionary.transform.localPosition = _origLocalPosition;
                        _gameObjectFromDictionary.transform.localRotation = _origLocalRotation;
                    }
                }
            }
        }
    }
}