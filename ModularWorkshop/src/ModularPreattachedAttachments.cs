using UnityEngine;
using FistVR;
using System.Collections;
using OpenScripts2;

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
				FVRFireArmAttachmentMount AttachmentMount = mounts[MountIndex];
				foreach (var attachment in Attachments)
				{
					attachment.SetParentage(AttachmentMount.transform);
					attachment.AttachToMount(AttachmentMount, false);
					if (attachment is Suppressor suppressor)
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

                foreach (var attachment in Attachments)
                {
                    attachment.SetParentage(null);
                }
            }

			Destroy(this);
		}
	}
}