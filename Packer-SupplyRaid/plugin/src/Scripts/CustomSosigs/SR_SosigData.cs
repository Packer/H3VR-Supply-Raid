using FistVR;
using UnityEngine;

namespace SupplyRaid
{
    internal class SR_SosigData
    {
        public static MeshRenderer GetSosigMeshRenderer(string title, Transform geoParent)
        {
            for (int i = 0; i < geoParent.childCount; i++)
            {
                if (geoParent.GetChild(i).name == title)
                    return geoParent.GetChild(i).GetComponent<MeshRenderer>();
            }

            return null;
        }

        public static void UpdateSosigLink(SosigLink link, Vector3 bodyScale, Vector3 linkScale, Material sosigMaterial, bool stopSever, MeshRenderer geo)
        {
            if (link == null)
                return;

            CapsuleCollider capsule = (CapsuleCollider)link.C;
            capsule.height *= linkScale.y;
            capsule.radius *= (linkScale.x > linkScale.z ? linkScale.x : linkScale.z);

            CharacterJoint joint = link.J != null ? link.J : link.GetComponent<CharacterJoint>();
            if (joint)
            {
                //Review these, try without this
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = new Vector3(0, joint.anchor.y * bodyScale.y, 0);
                joint.connectedAnchor = new Vector3(0, joint.connectedAnchor.y * bodyScale.y, 0);
            }

            geo.sharedMaterial = sosigMaterial;
            geo.transform.localScale = linkScale;

            //LOL ANTON HARDCODED NONSENSE
            if (stopSever)
                link.m_isJointSevered = true;

            if (link.m_wearables.Count > 0)
            {
                foreach (SosigWearable sosigWearable in link.m_wearables)
                {
                    sosigWearable.gameObject.transform.localScale = geo.transform.localScale;
                }
            }
        }
    }
}
