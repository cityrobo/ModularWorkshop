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
    public class GameObjectBasedManipAddon : MonoBehaviour, IPartPhysicalObjectRequirement
    {
        private FVRPhysicalObject _physicalObject;
        private GameObject _gameObjectFromDictionary;

        public string GameObjectDictionaryID;

        public ManipulateTransforms.TransformManipulationDefinition ManipulationDefinition;

        public FVRPhysicalObject PhysicalObject
        { 
            set
            {
                if (value != null)
                {
                    _physicalObject = value;

                    if (_physicalObject.TryGetComponentsInChildren(out GameObjectDictionary[] gameObjectDictionaris))
                    {
                        foreach (var gameObjectDictionary in gameObjectDictionaris)
                        {
                            if (gameObjectDictionary.TryGetGameObject(GameObjectDictionaryID, out _gameObjectFromDictionary) == true)
                            {
                                ManipulationDefinition.ManipulatedTransform = _gameObjectFromDictionary.transform;
                                ManipulationDefinition.SetLerp(1f);
                                break;
                            }
                        }
                        if (_gameObjectFromDictionary == null)
                        {
                            ModularWorkshopManager.LogWarning($"GameObjectBasedManipAddon on {gameObject.name} could not find GameObject with ID {GameObjectDictionaryID} in any GameObjectDictionaries of the main modular object!");
                        }
                    }
                    else
                    {
                        ModularWorkshopManager.LogWarning($"GameObjectBasedManipAddon on {gameObject.name} requires the main modular object or its children to have a GameObjectDictionary component, but it didn't find one!");
                    }
                }
                else
                {
                    if (_gameObjectFromDictionary != null)
                    {
                        ManipulationDefinition.SetLerp(0f);
                    }
                }
            }
        }
    }
}