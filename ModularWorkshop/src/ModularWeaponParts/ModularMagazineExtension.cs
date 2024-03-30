using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using System.Linq;
using HarmonyLib;

namespace ModularWorkshop
{
    public class ModularMagazineExtension : ModularWeaponPart
    {
        [Header("Magazine Extension Config")]
        [Tooltip("This part will add (or subtract, with a negative number) this number of rounds to the capacity of magazine first found in the object hierarchy. (This is not the number of total rounds, but additional rounds. A true magazine extension!)")]
        public int AdditionalNumberOfRoundsInMagazine = 0;

        protected FVRFireArmMagazine _magazine;

        private static readonly List<FVRFireArmMagazine> _existingMagazineExtensions = new();

        public override void Awake()
        {
            base.Awake();

            if (transform.TryGetComponentInParent(out _magazine)) _existingMagazineExtensions.Add(_magazine);
        }

        public override void OnDestroy()
        {
            _magazine = transform.GetComponentInParent<FVRFireArmMagazine>();
            _existingMagazineExtensions.Remove(_magazine);

            base.OnDestroy();
        }


        public override void EnablePart()
        {
            base.EnablePart();

            if (transform.TryGetComponentInParent(out _magazine))
            {
                int newCapacity = _magazine.m_capacity + AdditionalNumberOfRoundsInMagazine;
                if (_magazine.m_numRounds > newCapacity)
                {
                    int difference = _magazine.m_numRounds - newCapacity;

                    for (int i = 0; i < difference; i++)
                    {
                        _magazine.RemoveRound();
                    }
                }

                if (_magazine.LoadedRounds != null && _magazine.LoadedRounds.Length < newCapacity)
                {
                    int difference = newCapacity - _magazine.LoadedRounds.Length;
                    Array.Resize(ref _magazine.LoadedRounds, newCapacity);

                    for (int i = _magazine.LoadedRounds.Length - difference; i < _magazine.LoadedRounds.Length; i++)
                    {
                        _magazine.LoadedRounds[i] = new();
                    }
                }
                else if (_magazine.LoadedRounds != null && _magazine.m_numRounds < newCapacity)
                {
                    for (int i = _magazine.m_numRounds; i < newCapacity; i++)
                    {
                        if (_magazine.LoadedRounds[i] == null) _magazine.LoadedRounds[i] = new();
                    }
                }
                else if (_magazine.LoadedRounds == null)
                {
                    _magazine.LoadedRounds = new FVRLoadedRound[newCapacity];

                    for (int i = 0; i < _magazine.LoadedRounds.Length; i++)
                    {
                        _magazine.LoadedRounds[i] = new();
                    }
                }


                _magazine.m_capacity = newCapacity;
            }
            else if (_magazine == null)
            {
                 ModularWorkshopManager.LogWarning(this, "Magazine not found! ModularMagazineExtension disabled!");
            }
        }

        public override void DisablePart()
        {
            base.DisablePart();

            if (transform.TryGetComponentInParent(out _magazine))
            {
                int newCapacity = _magazine.m_capacity - AdditionalNumberOfRoundsInMagazine;
                if (_magazine.m_numRounds > newCapacity)
                {
                    int difference = _magazine.m_numRounds - newCapacity;

                    for (int i = 0; i < difference; i++)
                    {
                        _magazine.RemoveRound();
                    }
                }

                if (_magazine.LoadedRounds != null && _magazine.LoadedRounds.Length < newCapacity)
                {
                    int difference = newCapacity - _magazine.LoadedRounds.Length;
                    Array.Resize(ref _magazine.LoadedRounds, newCapacity);

                    for (int i = _magazine.LoadedRounds.Length - difference; i < _magazine.LoadedRounds.Length; i++)
                    {
                        _magazine.LoadedRounds[i] = new();
                    }
                }
                else if (_magazine.LoadedRounds != null && _magazine.m_numRounds < newCapacity)
                {
                    for (int i = _magazine.m_numRounds; i < newCapacity; i++)
                    {
                        if (_magazine.LoadedRounds[i] == null) _magazine.LoadedRounds[i] = new();
                    }
                }
                else if (_magazine.LoadedRounds == null)
                {
                    _magazine.LoadedRounds = new FVRLoadedRound[newCapacity];

                    for (int i = 0; i < _magazine.LoadedRounds.Length; i++)
                    {
                        _magazine.LoadedRounds[i] = new();
                    }
                }

                _magazine.m_capacity = newCapacity;
            }
            else if (_magazine == null)
            {
                ModularWorkshopManager.LogWarning(this, "Magazine not found! ModularMagazineExtension couldn't be disabled!");
            }
        }

        [HarmonyPatch(typeof(FVRPhysicalObject), nameof(FVRPhysicalObject.DuplicateFromSpawnLock))]
        [HarmonyPostfix]
        public static void DuplicateFromSpawnLockPatch(FVRPhysicalObject __instance, ref GameObject __result)
        {
            if (_existingMagazineExtensions.Contains(__instance as FVRFireArmMagazine))
            {
                FVRFireArmMagazine mag = __instance.GetComponent<FVRFireArmMagazine>();
                FVRFireArmMagazine copyMag = __result.GetComponent<FVRFireArmMagazine>();
                for (int i = 0; i < Mathf.Min(mag.LoadedRounds.Length, copyMag.LoadedRounds.Length); i++)
                {
                    if (copyMag.LoadedRounds[i] == null) copyMag.LoadedRounds[i] = new();
                }
            }
        }
    }
}