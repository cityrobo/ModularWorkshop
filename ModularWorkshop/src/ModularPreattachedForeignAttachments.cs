using System;
using UnityEngine;
using FistVR;
using System.Collections;
using System.Collections.Generic;
using OpenScripts2;
using System.Net.Mail;

namespace ModularWorkshop
{
	public class ModularPreattachedForeignAttachments : OpenScripts2_BasePlugin
	{
		[Serializable]
		public class ForeignAttachmentSet
		{
			public ForeignAttachmentSet(string primaryItemID, string backupID, Transform attachmentPoint)
			{
				PrimaryItemID = primaryItemID;
				BackupItemID = backupID;
				AttachmentPoint = attachmentPoint;
			}

			public string PrimaryItemID;
			[Tooltip("If your item fails to spawn, it will spawn the backup ID.")]
			public string BackupItemID;
			[Tooltip("Position and Rotation to spawn the Attachment at.")]
			public Transform AttachmentPoint;
		}

        public GameObject ModularWeapon;
		[Tooltip("If left empty it will use the firearms own AttachmentMounts.")]
        public string ModularPartsGroupID;
        public int MountIndex = 0;
        public ForeignAttachmentSet[] ForeignAttachmentSets;

		private readonly List<FVRFireArmAttachment> _spawnedAttachments = new();
        private IModularWeapon _modularWeapon;

#if !DEBUG
        public void Start()
		{
            _modularWeapon = ModularWeapon.GetComponent<IModularWeapon>();
			if (!_modularWeapon.GetModularFVRFireArm.WasUnvaulted)
			{
				SpawnAttachments();
				StartCoroutine(AttachAllToMount());
			}
			else Destroy(this);
		}

		private IEnumerator AttachAllToMount()
		{
			yield return null;
            FVRFireArmAttachmentMount[] mounts = ModularPartsGroupID != string.Empty ? _modularWeapon.AllAttachmentPoints[ModularPartsGroupID].ModularPartPoint.GetComponent<ModularWeaponPart>().AttachmentMounts : _modularWeapon.GetModularFVRFireArm.FireArm.AttachmentMounts.ToArray();
			if (mounts.Length > 0 || mounts.Length <= MountIndex)
			{
				FVRFireArmAttachmentMount AttachmentMount = mounts[MountIndex];
				foreach (var spawnedAttachment in _spawnedAttachments)
				{
					spawnedAttachment.SetParentage(AttachmentMount.transform);
					spawnedAttachment.AttachToMount(AttachmentMount, false);
					if (spawnedAttachment is Suppressor suppressor)
					{
						suppressor.AutoMountWell();
					}
					yield return null;
				}
			}
            else
            {
                if (ModularPartsGroupID == string.Empty) OpenScripts2_BepInExPlugin.LogWarning(this, $"No mounts on Modular Weapon found! Dropping attachment!");
                else OpenScripts2_BepInExPlugin.LogWarning(this, $"No mounts on Modular Weapon Part on point \"{ModularPartsGroupID}\" found! Dropping Attachment!");

                foreach (var attachment in _spawnedAttachments)
                {
                    attachment.SetParentage(null);
                }
            }

            Destroy(this);
        }

		private void SpawnAttachments()
		{
			GameObject spawnedGameObject;
			FVRFireArmAttachment spawnedAttachment;
			FVRObject objectReference;
			foreach (var foreignAttachmentSet in ForeignAttachmentSets)
			{
				spawnedGameObject = null;
				spawnedAttachment = null;
				objectReference = null;
				try
				{
					objectReference = IM.OD[foreignAttachmentSet.PrimaryItemID];
					spawnedGameObject = Instantiate(objectReference.GetGameObject(), foreignAttachmentSet.AttachmentPoint.position, foreignAttachmentSet.AttachmentPoint.rotation);
					spawnedAttachment = spawnedGameObject.GetComponent<FVRFireArmAttachment>();
					_spawnedAttachments.Add(spawnedAttachment);
				}
				catch
				{
					Log($"Item ID {foreignAttachmentSet.PrimaryItemID} not found; attempting to spawn backupID!");
					try
					{
						objectReference = IM.OD[foreignAttachmentSet.BackupItemID];
						spawnedGameObject = Instantiate(objectReference.GetGameObject(), foreignAttachmentSet.AttachmentPoint.position, foreignAttachmentSet.AttachmentPoint.rotation);
						spawnedAttachment = spawnedGameObject.GetComponent<FVRFireArmAttachment>();
						_spawnedAttachments.Add(spawnedAttachment);
					}
					catch
					{
						LogWarning($"Item ID {foreignAttachmentSet.BackupItemID} not found; continuing with next attachment in list!");
					}
				}

				Destroy(foreignAttachmentSet.AttachmentPoint.gameObject);
			}
		}
#endif
	}
}
