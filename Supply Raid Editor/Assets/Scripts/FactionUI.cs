using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Supply_Raid_Editor
{
    public class FactionUI : MonoBehaviour
    {
        public static FactionUI instance;
        [SerializeField] GameObject infomationPanel;
        public Image thumbnail;

        [Header("-INFOMATION-")]
        [SerializeField] InputField factionName;
        [SerializeField] InputField description;
        [SerializeField] InputField category;

        [SerializeField] InputField teamID;

        //--------------------------------------------
        //LEVEL
        //--------------------------------------------
        [Header("-LEVEL-")]
        [SerializeField] GameObject levelPanel;
        [SerializeField] GameObject levelGroupPanel;
        [SerializeField] GameObject levelsPrefab;
        [SerializeField] Text levelsTitle;
        [SerializeField] Text groupTitle;
        private List<GenericButton> levelButtons = new List<GenericButton>();

        private int levelIndex = -1;
        private bool levelEndless = false;

        [SerializeField] InputField levelName;
        [SerializeField] InputField levelEnemiesTotal;
        [SerializeField] Toggle levelInfiniteEnemies;
        [SerializeField] Toggle levelInfiniteSquadEnemies;
        [SerializeField] InputField levelEnemySpawnTimer;
        [SerializeField] InputField levelSquadDelayTimer;


        public GameObject listPrefab;
        //Boss
        [SerializeField] InputField levelBossCount;
        [SerializeField] Transform levelBossContent;
        public List<PointsUI> bossList = new List<PointsUI>();

        //GUARDS
        [SerializeField] InputField levelGuardCount;
        [SerializeField] Transform levelGuardContent;
        public List<PointsUI> guardList = new List<PointsUI>();

        //SNIPERS
        [SerializeField] InputField levelSniperCount;
        [SerializeField] Transform levelSniperContent;
        public List<PointsUI> sniperList = new List<PointsUI>();

        //PATROL
        [SerializeField] InputField levelMinPatrolSize;
        [SerializeField] InputField levelMaxPatrolSize;
        [SerializeField] Transform levelPatrolContent;
        public List<PointsUI> patrolList = new List<PointsUI>();

        //SQUAD
        [SerializeField] Dropdown levelSquadTeamRandomized;

        [SerializeField] InputField levelSquadCount;
        [SerializeField] InputField levelSquadSizeMin;
        [SerializeField] InputField levelSquadSizeMax;

        [SerializeField] Dropdown levelSquadBehaviour;
        [SerializeField] Transform levelSquadContent;

        public List<PointsUI> squadList = new List<PointsUI>();

        private void Awake()
        {
            instance = this;
        }

        public void OpenPage(int i)
        {
            levelPanel.SetActive(false);
            infomationPanel.SetActive(false);

            SaveLevel(levelIndex);

            switch (i)
            {
                default:
                case 0:
                    infomationPanel.SetActive(true);
                    break;
                case 1:
                    levelsTitle.text = "Levels:";
                    levelPanel.SetActive(true);
                    levelEndless = false;
                    OpenLevels();
                    break;
                case 2:
                    levelsTitle.text = "Endless:";
                    levelPanel.SetActive(true);
                    levelEndless = true;
                    OpenLevels();
                    break;
            }
        }

        public void AddLevel()
        {
            FactionLevel addLevel = new FactionLevel();

            addLevel.name = " ";

            addLevel.bossPool = new SosigPool();
            addLevel.sniperPool = new SosigPool();
            addLevel.guardPool = new SosigPool();
            addLevel.patrolPool = new SosigPool();
            addLevel.squadPool = new SosigPool();
            //addLevel.enemiesTotal = 5;
            //addLevel. 

            if (levelEndless)
                DataManager.Faction().endless.Add(addLevel);
            else
                DataManager.Faction().levels.Add(addLevel);

            OpenLevels();
        }

        public void OpenLevels()
        {
            levelIndex = -1;
            //Clear all old levels
            for (int i = 0; i < levelButtons.Count; i++)
            {
                Destroy(levelButtons[i].gameObject);
            }

            levelButtons.Clear();

            //Spawn in new levels
            List<FactionLevel> levels = levelEndless ? DataManager.Faction().endless : DataManager.Faction().levels;

            for (int i = 0; i < levels.Count; i++)
            {
                GenericButton btn = Instantiate(levelsPrefab, levelsPrefab.transform.parent).GetComponent<GenericButton>();
                btn.gameObject.SetActive(true);
                btn.index = i;
                btn.toggle = levelEndless;
                btn.go.GetComponent<Text>().text = i.ToString();
                btn.text.text = levels[i].name;
                levelButtons.Add(btn);
            }

            //Disable Levels Panel (Right Side)
            levelGroupPanel.SetActive(false);
        }


        public void UpdateFaction()
        {
            SR_SosigFaction item = DataManager.Faction();

            item.name = factionName.text;
            item.description = description.text;
            item.category = category.text;
            item.teamID = (TeamEnum)int.Parse(teamID.text);
        }

        public void UpdateUI()
        {
            SR_SosigFaction item = DataManager.Faction();

            factionName.SetTextWithoutNotify(item.name);
            description.SetTextWithoutNotify(item.description);
            category.SetTextWithoutNotify(item.category);
            teamID.SetTextWithoutNotify(item.teamID.ToString());

            infomationPanel.SetActive(false);
            levelPanel.SetActive(false);
            levelGroupPanel.SetActive(false);
            levelIndex = -1;
        }

        //Open Level
        public void OpenLevel(int i)
        {
            //Debug.Log("Open " + levelEndless);
            SaveLevel(levelIndex);

            levelGroupPanel.SetActive(true);
            levelIndex = i;
            levelEndless = false;
            LoadLevelData(DataManager.Faction().levels[i]);
        }

        //Open Endless Level
        public void OpenEndlessLevel(int i)
        {
            //Debug.Log("end Open " + levelEndless);
            SaveLevel(levelIndex);

            levelGroupPanel.SetActive(true);
            levelIndex = i;
            levelEndless = true;
            LoadLevelData(DataManager.Faction().endless[i]);
        }

        void LoadLevelData(FactionLevel level)
        {
            if (level == null)
            {
                DataManager.LogError("Faction level was not loaded!");
                return;
            }

            //Clear old
            for (int i = 0; i < bossList.Count; i++)
            {
                Destroy(bossList[i].gameObject);
            }
            bossList.Clear();

            for (int i = 0; i < sniperList.Count; i++)
            {
                Destroy(sniperList[i].gameObject);
            }
            sniperList.Clear();

            for (int i = 0; i < guardList.Count; i++)
            {
                Destroy(guardList[i].gameObject);
            }
            guardList.Clear();

            for (int i = 0; i < patrolList.Count; i++)
            {
                Destroy(patrolList[i].gameObject);
            }
            patrolList.Clear();

            for (int i = 0; i < squadList.Count; i++)
            {
                Destroy(squadList[i].gameObject);
            }
            squadList.Clear();

            UpdateGroupName();
            levelName.SetTextWithoutNotify(level.name);
            levelEnemiesTotal.SetTextWithoutNotify(level.enemiesTotal.ToString());
            levelInfiniteEnemies.SetIsOnWithoutNotify(level.infiniteEnemies);
            levelInfiniteSquadEnemies.SetIsOnWithoutNotify(level.infiniteSquadEnemies);
            levelEnemySpawnTimer.SetTextWithoutNotify(level.enemySpawnTimer.ToString());
            levelSquadDelayTimer.SetTextWithoutNotify(level.squadDelayTimer.ToString());

            //Boss
            levelBossCount.SetTextWithoutNotify(level.bossCount.ToString());

            if (level.bossPool == null)
                level.bossPool = new SosigPool();

            if (level.bossPool.sosigEnemyID == null)
                level.bossPool.sosigEnemyID = new int[0];

            for (int i = 0; i < level.bossPool.sosigEnemyID.Length; i++)
            {
                NewPoolItem((int)PoolEnum.Boss);
                bossList[i].inputField.SetTextWithoutNotify(((int)level.bossPool.sosigEnemyID[i]).ToString());
            }

            //Snipers
            levelSniperCount.SetTextWithoutNotify(level.sniperCount.ToString());

            if (level.sniperPool == null)
                level.sniperPool = new SosigPool();

            if (level.sniperPool.sosigEnemyID == null)
                level.sniperPool.sosigEnemyID = new int[0];

            for (int i = 0; i < level.sniperPool.sosigEnemyID.Length; i++)
            {
                NewPoolItem((int)PoolEnum.Sniper);
                sniperList[i].inputField.SetTextWithoutNotify(((int)level.sniperPool.sosigEnemyID[i]).ToString());
            }

            //Guards
            levelGuardCount.SetTextWithoutNotify(level.guardCount.ToString());

            if (level.guardPool == null)
                level.guardPool = new SosigPool();

            if (level.guardPool.sosigEnemyID == null)
                level.guardPool.sosigEnemyID = new int[0];

            for (int i = 0; i < level.guardPool.sosigEnemyID.Length; i++)
            {
                NewPoolItem((int)PoolEnum.Guard);
                guardList[i].inputField.SetTextWithoutNotify(((int)level.guardPool.sosigEnemyID[i]).ToString());
            }

            //Patrol
            levelMinPatrolSize.SetTextWithoutNotify(level.minPatrolSize.ToString());
            levelMaxPatrolSize.SetTextWithoutNotify(level.maxPatrolSize.ToString());

            if (level.patrolPool == null)
                level.patrolPool = new SosigPool();
            if (level.patrolPool.sosigEnemyID == null)
                level.patrolPool.sosigEnemyID = new int[0];

            for (int i = 0; i < level.patrolPool.sosigEnemyID.Length; i++)
            {
                NewPoolItem((int)PoolEnum.Patrol);
                patrolList[i].inputField.SetTextWithoutNotify(((int)level.patrolPool.sosigEnemyID[i]).ToString());
            }

            //Squad
            levelSquadCount.SetTextWithoutNotify(level.squadCount.ToString());

            if (level.squadPool == null)
                level.squadPool = new SosigPool();
            if (level.squadPool.sosigEnemyID == null)
                level.squadPool.sosigEnemyID = new int[0];

            for (int i = 0; i < level.squadPool.sosigEnemyID.Length; i++)
            {
                NewPoolItem((int)PoolEnum.Squad);
                squadList[i].inputField.SetTextWithoutNotify(((int)level.squadPool.sosigEnemyID[i]).ToString());
            }

            //Squad
            levelSquadTeamRandomized.SetValueWithoutNotify((int)level.squadTeamRandomized);
            levelSquadSizeMin.SetTextWithoutNotify(level.squadSizeMin.ToString());
            levelSquadSizeMax.SetTextWithoutNotify(level.squadSizeMax.ToString());

            levelSquadBehaviour.SetValueWithoutNotify((int)level.squadBehaviour);

            levelPanel.SetActive(true);
        }

        public void UpdateGroupName()
        {
            if (levelEndless)
                groupTitle.text = levelIndex + ": " + DataManager.Faction().endless[levelIndex].name;
            else
                groupTitle.text = levelIndex + ": " + DataManager.Faction().levels[levelIndex].name;
        }

        public void UpdateOpenLevel()
        {
            SaveLevel(levelIndex);
        }

        public void SaveLevel(int i)
        {
            if (i < 0 
                || (levelEndless ? DataManager.Faction().endless.Count : DataManager.Faction().levels.Count) <= 0)
                return;

            //Debug.Log("Save " + levelEndless);
            FactionLevel level;
            
            if(levelEndless)
                level = DataManager.Faction().endless[i];
            else
                level = DataManager.Faction().levels[i];

            level.name = levelName.text;
            level.enemiesTotal = int.Parse(levelEnemiesTotal.text);
            level.infiniteEnemies = levelInfiniteEnemies.isOn;
            level.infiniteSquadEnemies = levelInfiniteSquadEnemies.isOn;
            level.enemySpawnTimer = float.Parse(levelEnemySpawnTimer.text);
            level.squadDelayTimer = float.Parse(levelSquadDelayTimer.text);

            //Boss
            level.bossCount = int.Parse(levelBossCount.text);
            level.bossPool = new SosigPool();
            level.bossPool.sosigEnemyID = new int[bossList.Count];
            for (int x = 0; x < bossList.Count; x++)
            {
                if(bossList[x].inputField.text != "")
                    level.bossPool.sosigEnemyID[x] = int.Parse(bossList[x].inputField.text);
            }

            //Guards
            level.guardCount = int.Parse(levelGuardCount.text);
            if(level.guardPool == null)
                level.guardPool = new SosigPool();

            level.guardPool.sosigEnemyID = new int[guardList.Count];
            for (int x = 0; x < guardList.Count; x++)
            {
                if (guardList[x].inputField.text != "")
                    level.guardPool.sosigEnemyID[x] = int.Parse(guardList[x].inputField.text);
            }

            //Snipers
            level.sniperCount = int.Parse(levelSniperCount.text);
            level.sniperPool = new SosigPool();
            level.sniperPool.sosigEnemyID = new int[sniperList.Count];
            for (int x = 0; x < sniperList.Count; x++)
            {
                if (sniperList[x].inputField.text != "")
                    level.sniperPool.sosigEnemyID[x] = int.Parse(sniperList[x].inputField.text);
            }

            //Patrol
            level.minPatrolSize = int.Parse(levelMinPatrolSize.text);
            level.maxPatrolSize = int.Parse(levelMaxPatrolSize.text);
            level.patrolPool = new SosigPool();
            level.patrolPool.sosigEnemyID = new int[patrolList.Count];
            for (int x = 0; x < patrolList.Count; x++)
            {
                if (patrolList[x].inputField.text != "")
                    level.patrolPool.sosigEnemyID[x] = int.Parse(patrolList[x].inputField.text);
            }

            //Squad

            level.squadCount = int.Parse(levelSquadCount.text);
            level.squadPool = new SosigPool();
            level.squadPool.sosigEnemyID = new int[squadList.Count];
            for (int x = 0; x < squadList.Count; x++)
            {
                if (squadList[x].inputField.text != "")
                    level.squadPool.sosigEnemyID[x] = int.Parse(squadList[x].inputField.text);
            }

            level.squadTeamRandomized = (TeamSquadEnum)levelSquadTeamRandomized.value;
            level.squadSizeMin = int.Parse(levelSquadSizeMin.text);
            level.squadSizeMax = int.Parse(levelSquadSizeMax.text);

            level.squadBehaviour = (SquadBehaviour)levelSquadBehaviour.value;

            if (levelEndless)
                DataManager.Faction().endless[levelIndex] = level;
            else
                DataManager.Faction().levels[levelIndex] = level;

            UpdateGroupNames();
        }

        void UpdateGroupNames()
        {
            int count;
            if (levelEndless)
                count = DataManager.Faction().endless.Count;
            else
                count = DataManager.Faction().levels.Count;

            for (int i = 0; i < count; i++)
            {
                if (levelEndless)
                    levelButtons[i].text.text = DataManager.Faction().endless[i].name;
                else
                    levelButtons[i].text.text = DataManager.Faction().levels[i].name;
            }
        }

        public enum PoolEnum
        {
            Boss = 0,
            Guard = 1,
            Sniper = 2,
            Patrol = 3,
            Squad = 4,

        }

        //POOLS
        /*
        public void AddPoolItem(PoolEnum i)
        {

            NewPoolItem(PoolEnum.Boss);
            NewPoolItem(PoolEnum.Guard);
            NewPoolItem(PoolEnum.Sniper);
            NewPoolItem(PoolEnum.Patrol);
        }
        */

        public void NewPoolItem(int id)
        {
            Transform content = null;

            List<PointsUI> list = GetListByID((int)id);
            switch ((PoolEnum)id)
            {
                case PoolEnum.Boss:
                    content = levelBossContent;
                    break;
                case PoolEnum.Guard:
                    content = levelGuardContent;
                    break;
                case PoolEnum.Sniper:
                    content = levelSniperContent;
                    break;
                case PoolEnum.Patrol:
                    content = levelPatrolContent;
                    break;
                case PoolEnum.Squad:
                    content = levelSquadContent;
                    break;
            }

            PointsUI point = Instantiate(listPrefab, content).GetComponent<PointsUI>();
            point.gameObject.SetActive(true);
            point.id = (int)id;

            if (list.Count > 0)
            {
                //Duplicate last pool item for speed
                point.inputField.SetTextWithoutNotify(list[list.Count - 1].inputField.text);
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

        public void RemovePoolItem(PointsUI item)
        {
            List<PointsUI> list = GetListByID(item.id);

            if (list.Count <= 0)
            {
                Debug.Log("No List found Error");
            }

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

        public void CreateNewFaction()
        {
            //FILL WITH DEFAULT VALUES
            DataManager.Faction().name = "New Faction";
            DataManager.Faction().description = "A short description of this sosig faction";
            DataManager.Faction().category = "Standard";
            DataManager.Faction().teamID = TeamEnum.Team1;

            DataManager.Faction().levels = new List<FactionLevel>();
            DataManager.Faction().endless = new List<FactionLevel>();

            UpdateUI();
        }

        // LOADING

        public void LoadFaction()
        {
            UpdateUI();
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