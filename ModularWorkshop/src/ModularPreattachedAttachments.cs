using UnityEngine;
using FistVR;
using System.Collections;
using OpenScripts2;
using System.Runtime.Serialization;

namespace ModularWorkshop
{
	public class ModularPreattachedAttachments : OpenScripts2_BasePlugin
	{
		public GameObject ModularWeapon;
        [Tooltip("If left empty it will use the firearms own AttachmentMounts.")]
        public string ModularPartsGroupID;
		public int MountIndex = 0;
		public FVRFireArmAttachment[] Attachments;

		private IModularWeapon _modularWeapon;

		public void Start()
		{
			_modularWeapon = ModularWeapon.GetComponent<IModularWeapon>();
			if (!_modularWeapon.GetModularFVRFireArm.WasUnvaulted) StartCoroutine("AttachAllToMount");
			else 
			{
				for (int i = 0; i < Attachments.Length; i++)
				{
					Destroy(Attachments[i].gameObject);
				}
				Destroy(this);
			}
		}

		public IEnumerator AttachAllToMount()
		{
			yield return null;

			FVRFireArmAttachmentMount[] mounts = ModularPartsGroupID != string.Empty ? _modularWeapon.AllAttachmentPoints[ModularPartsGroupID].ModularPartPoint.GetComponent<ModularWeaponPart>().AttachmentMounts : _modularWeapon.GetModularFVRFireArm.FireArm.AttachmentMounts.ToArray();
			if (mounts.Length > 0 || mounts.Length <= MountIndex)
			{
				FVRFireArmAttachmentMount attachmentMount = mounts[MountIndex];
				foreach (var attachment in Attachments)
				{
					if (attachment.Type != attachmentMount.Type)
					{
						ModularWorkshopManager.LogWarning(this, $"Incompatible mount type found! Dropping attachment!");
						attachment.transform.SetParent(null);
					}
					else
					{
						attachment.transform.SetParent(attachmentMount.transform);
						attachment.AttachToMount(attachmentMount, false);
						if (attachment is Suppressor suppressor)
						{
							suppressor.AutoMountWell();
						}
					}
                    yield return null;
                }
			}
			else
			{
				if (ModularPartsGroupID == string.Empty) ModularWorkshopManager.LogWarning(this, $"No mounts on Modular Weapon found! Dropping attachment!");
				else ModularWorkshopManager.LogWarning(this, $"No mounts on Modular Weapon Part on point \"{ModularPartsGroupID}\" found! Dropping Attachment!");

                foreach (var attachment in Attachments)
                {
                    attachment.transform.SetParent(null);
                }
            }

			Destroy(this);
		}
	}
}