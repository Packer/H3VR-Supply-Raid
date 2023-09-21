
using UnityEngine;
using UnityEngine.UI;

namespace SupplyRaid
{

	public class SR_HelpMenu : MonoBehaviour
    {
        public static SR_HelpMenu instance;
        [SerializeField] Text titleText;
        [SerializeField] Text descriptionText;
        [SerializeField] GameObject canvas;


        void Awake () 
        { 
            instance = this;
        }

        public void SetActive(bool set)
        {
            canvas.SetActive(set);

            if (set)
            {
                titleText.text = "Instructions"; ;
                descriptionText.text = "Instructions goes here";
            }
        }
    }
}