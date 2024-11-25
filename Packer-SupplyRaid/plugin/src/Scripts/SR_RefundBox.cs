using UnityEngine;
using FistVR;

namespace SupplyRaid
{
    public class SR_RefundBox : MonoBehaviour
    {
        public void RefundPoints(Transform spawnPoint)
        {
            int count = 0;
            if (SR_Manager.profile.freeBuyMenu)
                return;

            SR_Manager.PlayConfirmSFX();

            while (SR_Manager.instance.Points > 0)
            {
                string item = SR_Global.GetHighestValueCashMoney(SR_Manager.instance.Points);

                //Error Check
                if (item == "")
                    break;

                SR_Manager.instance.Points -= SR_Global.GetRoundValue(item);
                FVRObject mainObject;
                IM.OD.TryGetValue(item, out mainObject);
                SR_Manager.instance.StartCoroutine(SR_Global.WaitandCreate(mainObject.GetGameObject(), count * 0.25f, spawnPoint));
                count++;
            }
        }
    }
}
