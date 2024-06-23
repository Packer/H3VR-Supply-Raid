using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class FactionUI : MonoBehaviour
    {
        public static FactionUI instance;

        [SerializeField] InputField factionName;
        [SerializeField] InputField description;
        [SerializeField] InputField category;

        [SerializeField] Dropdown teamID;

        //--------------------------------------------
        //LEVEL
        //--------------------------------------------

        [SerializeField] InputField levelName;
        [SerializeField] InputField levelEnemiesTotal;
        [SerializeField] Toggle levelInfiniteEnemies;
        [SerializeField] Toggle levelInfiniteSquadEnemies;
        [SerializeField] InputField levelEnemySpawnTimer;
        [SerializeField] InputField levelSquadDelayTimer;


        public GameObject listPrefab;
        //Boss
        [SerializeField] InputField levelBossCount;
        [SerializeField] List<InputField> levelBossPool;
        [SerializeField] Transform levelBossContent;

        public List<PointsUI> bossList = new List<PointsUI>();

        //GUARDS
        [SerializeField] InputField levelGuardCount;
        [SerializeField] List<InputField> levelGuardPool;

        public List<PointsUI> guardList = new List<PointsUI>();

        //SNIPERS
        [SerializeField] InputField levelSniperCount;
        [SerializeField] List<InputField> levelSniperPool;

        public List<PointsUI> sniperList = new List<PointsUI>();

        //PATROL
        [SerializeField] InputField levelMinPatrolSize;
        [SerializeField] List<InputField> levelPatrolPool;


        public List<PointsUI> patrolList = new List<PointsUI>();

        //SQUAD
        [SerializeField] Dropdown levelSquadTeamRandomized;

        [SerializeField] InputField levelSquadCount;
        [SerializeField] InputField levelSquadSizeMin;
        [SerializeField] InputField levelSquadSizeMax;

        [SerializeField] Dropdown levelSquadBehaviour;
        [SerializeField] List<InputField> levelSquadPool;

        public List<PointsUI> squadList = new List<PointsUI>();

        private void Awake()
        {
            instance = this;
        }


        public void UpdateFaction()
        {
            SR_SosigFaction item = DataManager.instance.faction;

            item.name = factionName.text;
            item.description = description.text;
            item.category = category.text;
            item.teamID = (TeamEnum)teamID.value;
        }


        //Open Level
        public void OpenLevel(int i)
        {
            
        }


        public void SaveLevel(int i)
        {
            
        }

        //POOLS

        public void NewPoolItem(int id, Transform content)
        {

            PointsUI point = Instantiate(listPrefab, content).GetComponent<PointsUI>();
            point.gameObject.SetActive(true);
            point.id = id;
            List<PointsUI> list = GetListByID(id);

            if (list.Count > 0)
            {
                //Duplicate last pool item for speed
                point.inputField.text = list[list.Count - 1].inputField.text;
            }

            //Add Last
            list.Add(point);

            UpdateFaction();
        }

        public List<PointsUI> GetListByID(int id)
        {
            List<PointsUI> list;

            switch (id)
            {
                case 0:
                default:
                    list = bossList;
                    break;
                case 1:
                    list = guardList;
                    break;
                case 2:
                    list = sniperList;
                    break;
                case 3:
                    list = patrolList;
                    break;
                case 4:
                    list = squadList;
                    break;
            }
            return list;
        }

        public void RemovePoolItem(int id, PointsUI item)
        {
            List<PointsUI> list = GetListByID(id);

            if (list.Count <= 0)
                return;

            if (list.Contains(item))
                list.Remove(item);

            Destroy(item.gameObject);

            UpdateFaction();
        }

        public void RemoveBossPool(PointsUI tab)
        {
            if (bossList.Count <= 0)
                return;

            if (bossList.Contains(tab))
                bossList.Remove(tab);

            Destroy(tab.gameObject);

            UpdateFaction();

            //Reopen Level
        }

        public void UpdateUI()
        {
            
        }

        public void CreateNewFaction()
        {
            LoadFaction(new SR_SosigFaction());
        }

        // LOADING

        public void LoadFaction(SR_SosigFaction faction)
        {
            
        }

        // SAVING

        public void TrySaveFaction()
        {
            if (DataManager.Faction() == null)
                return;
            //MenuManager.instance.PopupWarning("SaveCharacter", "This will overwrite the character file, are you sure?");
            SaveFaction();
        }

        ///Save to Json
        public void SaveFaction()
        {
            if (DataManager.Faction() == null)
                return;

            //Final update to make sure settings are correct
            UpdateFaction();

            //SAVE CHARACTER
            string json = JsonUtility.ToJson(DataManager.Faction(), true);
            DataManager.instance.OnSaveDialogue(JSONTypeEnum.Faction, json, DataManager.Faction().name);
        }
    }
}