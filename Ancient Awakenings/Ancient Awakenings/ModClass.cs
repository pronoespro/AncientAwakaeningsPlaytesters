using Ancient_Awakenings_SoulNail_charm.Auxiliar;
using Ancient_Awakenings_SoulNail_charm.Auxiliar.UI;
using Ancient_Awakenings_SoulNail_charm.Monobehaviors;
using Modding;
using Modding.Menu;
using Modding.Menu.Components;
using Modding.Menu.Config;
using Satchel.BetterMenus;
using SFCore;
using SFCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ancient_Awakenings_SoulNail_charm
{
    public class AncientAwakeningsMod : Mod, ITogglableMod, IGlobalSettings<AAGlobalSettings>, ILocalSettings<AALocalSettings>, ICustomMenuMod
    {

        #region Startup
        #region Instancing
        public AncientAwakeningsMod() : base("Ancien Awakenings") { }

        internal static AncientAwakeningsMod Instance;

        public override string GetVersion()
        {
            return "v0.0.0.1";
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");

            Instance = this;


            LoadLanguage();
            LoadBundles();

            LoadUI();
            //On.HeroController.Update += HeroController_Update;

            SetupCharms();
            StartSettings();

            InitializeAnimationStuff();

            InitializeData();

            Log("Initialized");
        }

        public void Unload()
        {
            //On.HeroController.Update -= HeroController_Update;
            UnloadAnimationStuff();
            UnloadCharms();
            UnloadSettings();
            UnloadBundles();
            UnloadUI();

            UnloadLanguage();
            Instance = null;
        }
        #endregion

        #region Language

        public void LoadLanguage()
        {
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        }

        public void UnloadLanguage()
        {
            ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
        }

        private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {

            if (AALanguageHelper.GeneralLanguage.ContainsKey(key))
            {
                return AALanguageHelper.GeneralLanguage[key];
            }

            if (key.StartsWith("CHARM_"))
            {
                int charmNum;
                if (int.TryParse(key.Split('_')[2], out charmNum))
                {
                    string charmPart = key.Split('_')[1];
                    if (charmIDs.Contains(charmNum))
                    {
                        string selCharm = GetCharmName(charmIDs.IndexOf(charmNum));

                        if (selCharm != "NULL" && AALanguageHelper.CharmStuff.ContainsKey(selCharm + "_" + charmPart))
                        {
                            return AALanguageHelper.CharmStuff[selCharm + "_" + charmPart];
                        }
                    }
                }
            }

            return orig;
        }

        #endregion

        #region bundles

        public Dictionary<string, AssetBundle> AttackBundles;
        private List<string> attacksToLoad = new List<string> { "charmattacks" };

        public Dictionary<string, AssetBundle> UIBundles;
        private List<string> uiToLoad = new List<string> { "ui" };

        private Dictionary<string, AssetBundle> SpriteBundles;
        private List<string> spritesToLoad = new List<string> { "charmsprites", "charmupgrades", "knightanimations" };

        private void LoadBundles()
        {

            AttackBundles = new Dictionary<string, AssetBundle>();
            UIBundles = new Dictionary<string, AssetBundle>();
            SpriteBundles = new Dictionary<string, AssetBundle>();


            Assembly asm = Assembly.GetExecutingAssembly();
            Log("Searching for Levels");
            foreach (string res in asm.GetManifestResourceNames())
            {
                using (Stream s = asm.GetManifestResourceStream(res))
                {
                    if (s == null)
                    {
                        continue;
                    }
                    Log("Found asset");

                    byte[] buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                    s.Dispose();
                    string bundleName = Path.GetExtension(res).Substring(1);


                    if (attacksToLoad.Contains(bundleName))
                    {
                        Log("Found attack " + bundleName);
                        AttackBundles.Add(bundleName, AssetBundle.LoadFromMemory(buffer));
                    } else if (uiToLoad.Contains(bundleName))
                    {
                        Log("Found attack " + bundleName);
                        UIBundles.Add(bundleName, AssetBundle.LoadFromMemory(buffer));
                    }
                    else if (spritesToLoad.Contains(bundleName))
                    {
                        Log("Found attack " + bundleName);
                        SpriteBundles.Add(bundleName, AssetBundle.LoadFromMemory(buffer));
                    }
                    else
                    {
                        continue;
                    }

                }
            }
        }

        private void UnloadBundles()
        {
            AttackBundles = null;
            UIBundles = null;
        }

        #endregion
        #endregion

        #region Midgame

        #region Animation
        //---------------------
        //---Animation Stuff---
        //---------------------
        private Dictionary<string, tk2dFullAnimationData> knightAnimations;
        public tk2dSpriteAnimator heroAnimator;
        public AAPlayerAnimator playerAnim;
        private tk2dSpriteCollectionData colection;

        #region Setup
        public void InitializeAnimationStuff()
        {

            //Setup
            On.HeroController.Awake += SetupOnlyOnce;
            On.HeroController.Start += PlayerAnimatorSetup;
        }

        public void UnloadAnimationStuff()
        {
            On.HeroController.Awake -= SetupOnlyOnce;
            On.HeroController.Start -= PlayerAnimatorSetup;
        }

        private void SetupOnlyOnce(On.HeroController.orig_Awake orig, HeroController self)
        {
            if (knightAnimations == null)
            {
                knightAnimations = new Dictionary<string, tk2dFullAnimationData>();
                if (SpriteBundles.ContainsKey("knightanimations") && SpriteBundles["knightanimations"].Contains("defender roll"))
                {
                    Texture2D tex = SpriteBundles["knightanimations"].LoadAsset<Texture2D>("defender roll");
                    colection = tk2dAuxiliarHelper.CreateCollectionSimple(tex, HeroController.instance.gameObject, new Vector2(tex.width / 2 / 4, tex.height / 4), 4);

                    knightAnimations.Add("defender roll",
                        new tk2dFullAnimationData(colection,
                        new tk2dAnimationDataForCreation[] {
                            new tk2dAnimationDataForCreation("defender roll", null, new int[] { 0, 1, 2, 3, 0, 1}) }
                        ));

                    Log("Made the Defender's Roll, now he hatting");
                }
            }

            orig(self);

            foreach (string key in knightAnimations.Keys)
            {

                heroAnimator = HeroController.instance.gameObject.GetComponent<tk2dSpriteAnimator>();
                List<tk2dSpriteAnimationClip> list2 = heroAnimator.Library.clips.ToList<tk2dSpriteAnimationClip>();

                foreach (tk2dSpriteAnimationClip item in tk2dAuxiliarHelper.MakeAnimations(colection, knightAnimations[key].animationData))
                {
                    list2.Add(item);
                }
                heroAnimator.Library.clips = list2.ToArray();

            }

            On.HeroController.Awake -= SetupOnlyOnce;
        }


        private void PlayerAnimatorSetup(On.HeroController.orig_Start orig, HeroController self)
        {
            if (playerAnim == null)
            {
                playerAnim = HeroController.instance.gameObject.AddComponent<AAPlayerAnimator>();
                playerAnim.heroAnimator = heroAnimator;
            }

            orig(self);
        }

        #endregion


        #endregion

        #region UI

        public Canvas uiPanel;

        public static bool IsGamePaused()
        {
            return GameManager.instance.isPaused || GameManager.instance.inventoryFSM.GetBoolVariable("Open").Value;
        }

        private void LoadUI()
        {
            CreateCanvas();

            On.SceneManager.Update += SceneManager_Update;
        }

        private void SceneManager_Update(On.SceneManager.orig_Update orig, SceneManager self)
        {
            CreateCanvas();
            orig(self);
        }

        public void CreateCanvas()
        {
            if (uiPanel == null)
            {
                if (UIBundles.ContainsKey("ui"))
                {
                    AssetBundle _bundle = UIBundles["ui"];
                    if (_bundle != null)
                    {
                        uiPanel = GameObject.Instantiate(_bundle.LoadAsset<GameObject>("Canvas")).GetComponent<Canvas>();
                        Log("Loaded Canvas");

                        uiPanel.gameObject.AddComponent<AncientUI>();

                        GameObject.DontDestroyOnLoad(uiPanel);
                    }
                }
            }
        }

        private void UnloadUI() {
            if (uiPanel != null) {
                GameObject.Destroy(uiPanel);
            }
        }

        #endregion

        #region charms

        #region variables
        //------------
        //---Charms---
        //------------
        #region CHARM LIST
        public List<AACharmBase> charmList = new List<AACharmBase>() {
            new AACharmBase(name:"Soul Chakram",sprite:"Soul Chakram",cost:1),
            new AACharmBase(name:"Godswrath",sprite:"Godswrath",cost:2),
            new AACharmBase(name:"Broj’s Rebellion",sprite:"Lifeblood's empowering",cost:1),
            new AAUpgradeableCharm(name:"Unbreakable Defense",sprite:"Unbreakable Defense",cost:1),
            new AACharmBase(name:"Soul Samurai",sprite:"Soul Samurai",cost:1),
            new AACharmBase(name:"Crystal Hunter's Dash",sprite:"crystal dash projectile charm",cost:1),
            new AACharmBase(name:"Defender's Roll",sprite:"Defender's Roll",cost:1),
            new AACharmBase(name:"Soul Basin",sprite:"Soul Basin",cost:3),
            new AAUpgradeableCharm(name:"Unbreakable Stability",sprite:"Unbreakable Stability",cost:1),
            new AACharmBase(name:"Dance of Nails",sprite:"Dance of Nails",cost:1),

            new AACharmBase(name:"Nosk's Toxicity",sprite:"Nosk's Toxicity",cost:1),
            new AACharmBase(name:"Chained Pike",sprite:"Chained Pike",cost:1),
            new AACharmBase(name:"Grappler grip",sprite:"Grappler grip",cost:1),
            new AACharmBase(name:"Demigod Tamer",sprite:"Demigod Tamer",cost:2),
            new AACharmBase(name:"Sibling's Strength", sprite: "Sibling's Strength", cost:2),
            new AACharmBase(name:"Void Scratch",sprite:"Void scratch",cost:1),
            new AACharmBase(name:"Void Rain",sprite:"Void Rain",cost:1),
            new AACharmBase(name:"Radiance's Opposite",sprite:"Radiance's Opposite",cost:1,true),
            new AACharmBase(name:"Void Soul",sprite:"Void Soul",cost:2,true),
            new AACharmBase(name:"The Friends we Made Along the Way",sprite:"The Friends we Made Along the Way",cost:4,true)
        };
        #endregion
        public List<int> charmIDs = new List<int>();
        public List<Sprite> charmSprites = new List<Sprite>();
        public PlayerCollisionDamage playerCollideDamage;
        public Dictionary<string, GameObject> charmCreations;
        #region Spell preparing
        public string[] spellPrepare = new string[] { "voidSoulSpell","voidDiveSpell","voidShriekSpell",
            "radianceSoulSpell","radianceDiveSpell","radianceShriekSpell",
            "samuraiBladeEnergy","chakramSoul","noskShriekSpell"};
        #endregion

        //---------------------
        //---Soul Nail Charm---
        //---------------------
        private int soulNailHitCount;
        private int maxSoulHitCount = 5;

        public bool radianceDeffeated;
        private HitDataList hitDatas;
        private float refreshHitTimer = 0.5f;
        private string[] invincibleEnemies = new string[] { "Zombie Beam Miner" };

        //-----------------
        //---Void Powers---
        //-----------------
        private float knightVoidAmmount;
        private float knightVoidMax = 10f;
        private readonly float baseKnightVoidMax = 10f;
        private float voidFromAttacks = 0.1f;
        private float voidFromHurt = 0.2f;

        //--------------------------------
        //---Ultimate Friendship Attack---
        //--------------------------------
        private float voidPercentToFriendship = 1f;

        //---------------------
        //---Ultimate Spells---
        //---------------------
        private float voidForSpells = 1f;
        
        //---------------------
        //---Defender's Roll---
        //---------------------
        public bool isRolling;
        private float ogWalkSpeed, ogRunSpeed;

        //----------------
        //---Soul Basin---
        //----------------
        private float basin_soulGainTime = 2f;
        private int basin_soulGainAmmount = 4;
        private float basin_curSoulGainTimer;

        //-------------------------
        //---Radiance's Opposite---
        //-------------------------
        private float voidForRadiantSpells = 1.5f;


        #endregion

        #region Setup
        private void SetupCharms()
        {
            charmCreations = new Dictionary<string, GameObject>();
            PrepareSpells();

            for (int i = 0; i < charmList.Count; i++)
            {
                charmList[i].gotCharm = true;
                if (SpriteBundles.ContainsKey("charmsprites"))
                {
                    if (charmList[i].charmSprite != null && charmList[i].charmSprite != "")
                    {
                        charmSprites.Add(SpriteBundles["charmsprites"].LoadAsset<Sprite>(charmList[i].charmSprite));
                        Log("Charm " + charmList[i].GetName() + " loaded.");
                    }
                    else
                    {
                        Log("Charm failed to load");
                    }
                }
            }

            charmIDs = CharmHelper.AddSprites(charmSprites.ToArray());

            //Inventory
            ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
            ModHooks.SetPlayerBoolHook += OnSetPlayerBoolHook;
            ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;

            //Actual use
            On.HealthManager.Hit += EnemyGetsHit;
            On.HealthManager.Update += EnemyHealthUpdate;
            On.HeroController.TakeDamage += HeroTakesDamage;
            On.HeroController.NailParry += HeroController_NailParry;
            On.HeroController.AddHealth += CharmAfterHealing;
            On.HeroController.Update += HeroCharmUpdate;

            //Rolling
            On.HeroController.Awake += HeroController_Awake;
            On.HeroController.Start += HeroStartValuesForRoll;
            On.HeroController.Jump += RollSuperJump;
            On.HeroController.Update += HealthManager_Update;

            On.HeroController.CanAttack += RollCancelAttack;
            On.HeroController.CanCast += RollCancelCast;
            On.HeroController.CanDoubleJump += RollCancelDoubleJump;
            On.HeroController.CanDash += RollCancelDash;
            On.HeroController.CanWallJump += HeroController_CanWallJump;

        }

        private void UnloadCharms()
        {
            //Inventory
            ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
            ModHooks.SetPlayerBoolHook -= OnSetPlayerBoolHook;
            ModHooks.GetPlayerIntHook -= ModHooks_GetPlayerIntHook;

            //Actual use
            On.HealthManager.Hit -= EnemyGetsHit;
            On.HealthManager.Update -= EnemyHealthUpdate;
            On.HeroController.TakeDamage -= HeroTakesDamage;
            On.HeroController.NailParry -= HeroController_NailParry;

            //Rolling
            On.HeroController.Awake -= HeroController_Awake;
            On.HeroController.Start -= HeroStartValuesForRoll;
            On.HeroController.Jump -= RollSuperJump;
            On.HeroController.Update -= HealthManager_Update;

            On.HeroController.CanAttack -= RollCancelAttack;
            On.HeroController.CanCast -= RollCancelCast;
            On.HeroController.CanDoubleJump -= RollCancelDoubleJump;
            On.HeroController.CanDash -= RollCancelDash;

            UnloadSpells();
        }
        #endregion

        #region ModHooks
        #region Inventory ModHooks
        private bool ModHooks_GetPlayerBoolHook(string name, bool orig)
        {

            if (name.StartsWith("gotCharm_"))
            {
                int charmNum = int.Parse(name.Split('_')[1]);
                if (charmIDs.Contains(charmNum))
                {
                    return charmList[charmIDs.IndexOf(charmNum)].gotCharm;
                }
            }
            if (name.StartsWith("newCharm_"))
            {
                int charmNum = int.Parse(name.Split('_')[1]);
                if (charmIDs.Contains(charmNum))
                {
                    return charmList[charmIDs.IndexOf(charmNum)].newCharm;
                }
            }
            if (name.StartsWith("equippedCharm_"))
            {
                int charmNum = int.Parse(name.Split('_')[1]);
                if (charmIDs.Contains(charmNum))
                {
                    return charmList[charmIDs.IndexOf(charmNum)].equippedCharm;
                }
            }

            return orig;
        }

        private bool OnSetPlayerBoolHook(string target, bool orig)
        {
            if (target.StartsWith("gotCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (charmIDs.Contains(charmNum))
                {
                    LocalSaveData.charmsGot[charmIDs.IndexOf(charmNum)] = orig;
                    charmList[charmIDs.IndexOf(charmNum)].gotCharm = orig;
                    return orig;
                }
            }
            if (target.StartsWith("newCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (charmIDs.Contains(charmNum))
                {
                    LocalSaveData.charmsNew[charmIDs.IndexOf(charmNum)] = orig;
                    charmList[charmIDs.IndexOf(charmNum)].newCharm = orig;
                    return orig;
                }
            }
            if (target.StartsWith("equippedCharm_"))
            {
                int charmNum = int.Parse(target.Split('_')[1]);
                if (charmIDs.Contains(charmNum))
                {
                    LocalSaveData.charmsEquipped[charmIDs.IndexOf(charmNum)] = orig;
                    charmList[charmIDs.IndexOf(charmNum)].equippedCharm = orig;
                    return orig;
                }
            }
            return orig;
        }

        private int ModHooks_GetPlayerIntHook(string name, int orig)
        {
            if (name.StartsWith("charmCost_"))
            {

                int charmNum = int.Parse(name.Split('_')[1]);
                if (charmIDs.Contains(charmNum))
                {
                    return charmList[charmIDs.IndexOf(charmNum)].charmCost;
                }
            }
            return orig;
        }
        #endregion

        #region Use ModHooks
        private void EnemyGetsHit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
        {
            if (hitDatas == null)
            {
                hitDatas = new HitDataList();
            }

            if (self != HeroController.instance
                && hitInstance.AttackType == AttackTypes.Nail && !hitDatas.Contains(hitInstance.Source.transform, self.transform)
                && (!self.IsInvincible || IsInvincibleEnemy(self.gameObject.name)))
            {

                hitDatas.Add(new HitData(hitInstance.Source.transform, self.transform, refreshHitTimer));

                //Soul Nail charm
                if (GetCharmEquipped(14))
                {
                    soulNailHitCount++;
                    if (soulNailHitCount % maxSoulHitCount == 0)
                    {
                        CreateSoulNailProj(self.gameObject.transform.position);
                    }
                }
                //Poison charm
                if (GetCharmEquipped(15))
                {
                    VoidPoisonHurt poison = self.gameObject.GetComponent<VoidPoisonHurt>();
                    if (poison != null)
                    {
                        Log("Reapplied poison");
                        poison.Reapply();
                    }
                    else
                    {
                        Log("Applied poison");
                        self.gameObject.AddComponent<VoidPoisonHurt>();
                    }
                }
                //Void obtaining
                if (VoidUnlocked())
                {
                    AddVoid(voidFromAttacks);
                }
            }

            orig(self, hitInstance);
        }

        private void EnemyHealthUpdate(On.HealthManager.orig_Update orig, HealthManager self)
        {
            if (hitDatas != null)
            {
                hitDatas.Update();
            }
            orig(self);
        }

        private void HeroTakesDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
        {

            if (hazardType == (int)GlobalEnums.HazardType.NON_HAZARD)
            {
                AddVoid(voidFromHurt);
            }

            orig(self, go, damageSide, damageAmount, hazardType);
        }

        private void HeroController_NailParry(On.HeroController.orig_NailParry orig, HeroController self)
        {
            if (GetCharmEquipped(6))
            {
                ChangeAnimation("defender roll", 0.2f, () => HeroController.instance.SetDamageMode(1), () => HeroController.instance.SetDamageMode(0));
            }
            orig(self);
        }

        private void CharmAfterHealing(On.HeroController.orig_AddHealth orig, HeroController self, int amount)
        {
            if (GetCharmEquipped(6))
            {
                SetRolling(true);

                float rollTime = 0.75f;

                if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_34))) {
                    rollTime *= 3;
                }
                if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_7))) {
                    rollTime *= 0.5f;
                }

                ChangeAnimation("defender roll", rollTime, () => SetRolling(true), () => SetRolling(false), 0.5f);
            }
            orig(self, amount);
        }

        private void HeroCharmUpdate(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            //Soul Basin
            if (GetCharmEquipped(7))
            {
                SoulBasinGenerateSoul();
            }
        }


        #region Rolling Stuff

        private void HeroController_Awake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig(self);
            playerCollideDamage = self.transform.Find("HeroBox").gameObject.AddComponent<PlayerCollisionDamage>();
        }

        private bool RollCancelDash(On.HeroController.orig_CanDash orig, HeroController self)
        {
            if (isRolling)
            {
                return false;
            }
            return orig(self);
        }

        private bool RollCancelDoubleJump(On.HeroController.orig_CanDoubleJump orig, HeroController self)
        {
            if (isRolling)
            {
                return false;
            }

            return orig(self);
        }

        private bool HeroController_CanWallJump(On.HeroController.orig_CanWallJump orig, HeroController self)
        {
            if (isRolling)
            {
                return false;
            }

            return orig(self);
        }

        private bool RollCancelCast(On.HeroController.orig_CanCast orig, HeroController self)
        {
            if (isRolling)
            {
                return false;
            }

            return orig(self);
        }

        private bool RollCancelAttack(On.HeroController.orig_CanAttack orig, HeroController self)
        {

            if (isRolling)
            {
                return false;
            }

            return orig(self);
        }

        private void RollSuperJump(On.HeroController.orig_Jump orig, HeroController self)
        {
            orig(self);

            if (isRolling)
            {
                self.GetComponent<Rigidbody2D>().velocity *= 1.1f;
            }
        }

        private void HeroStartValuesForRoll(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);
            ogWalkSpeed = self.WALK_SPEED;
            ogRunSpeed = self.RUN_SPEED;
        }

        private void HealthManager_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            if (isRolling)
            {
                self.WALK_SPEED = ogWalkSpeed * 2.5f;
                self.RUN_SPEED = ogRunSpeed * 2.5f;
            }
            else
            {
                self.WALK_SPEED = ogWalkSpeed;
                self.RUN_SPEED = ogRunSpeed;
            }
            orig(self);
        }
        #endregion

        #endregion
        #endregion

        #region Methods

        #region SpellCreate

        public void PrepareSpells()
        {
            if (AttackBundles != null && AttackBundles.ContainsKey("charmattacks"))
            {
                AssetBundle bundle = AttackBundles["charmattacks"];

                foreach (string spell in spellPrepare)
                {
                    if (bundle.Contains(spell))
                    {
                        charmCreations.Add(spell, GameObject.Instantiate(bundle.LoadAsset<GameObject>(spell)));
                    }
                }
            }
        }

        public void CreateSpell(string spellName)
        {
            if (charmCreations.ContainsKey(spellName))
            {
                charmCreations[spellName].SetActive(true);
            }
            else
            {
                if (AttackBundles != null && AttackBundles.ContainsKey("charmattacks"))
                {
                    AssetBundle bundle = AttackBundles["charmattacks"];

                    if (bundle.Contains(spellName))
                    {
                        charmCreations.Add(spellName, GameObject.Instantiate(bundle.LoadAsset<GameObject>(spellName)));
                    }
                }
            }
        }

        public void UnloadSpells()
        {
            foreach(string key in charmCreations.Keys)
            {
                GameObject.Destroy(charmCreations[key]);
            }
            charmCreations.Clear();
            charmCreations = null;
        }

        #endregion

        #region Charm General
        private string GetCharmName(int charmNum)
        {
            if (charmNum < charmList.Count)
            {
                return charmList[charmNum].GetName();
            }
            return "NULL";
        }

        public static bool GetCharmEquipped(int charmNum)
        {

            if (charmNum < Instance.charmList.Count)
            {
                return Instance.charmList[charmNum].equippedCharm;
            }

            return false;
        }

        private bool IsInvincibleEnemy(string name)
        {

            foreach (string s in invincibleEnemies)
            {
                if (name.Contains(s))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Void General

        public bool EnoughVoidToUse()
        {

            if (GetCharmEquipped(18) && UseVoidAmmount(voidForSpells, false))
            {
                return true;
            }
            if (GetCharmEquipped(19) && UseVoidPercentage(voidPercentToFriendship, false))
            {
                return true;
            }

            return false;
        }

        public void ResetMaxVoidAmmount()
        {
            knightVoidMax = baseKnightVoidMax;
        }

        public void UpgradeVoidMax(float multiplier)
        {
            knightVoidMax *= multiplier;
        }

        public bool AddVoid(float ammount)
        {
            if (knightVoidAmmount < knightVoidMax)
            {
                knightVoidAmmount = Mathf.Min(knightVoidAmmount + ammount, knightVoidMax);
                LocalSaveData.curVoidAmmount = knightVoidAmmount;
                return true;
            }
            return false;
        }

        public bool UseVoidAmmount(float ammount, bool use = true)
        {
            if (ammount <= knightVoidAmmount)
            {
                knightVoidAmmount -= (use ? ammount : 0);
                return true;
            }
            return false;
        }

        public bool UseVoidPercentage(float percentage, bool use = true)
        {
            if (knightVoidAmmount >= knightVoidMax * percentage)
            {
                knightVoidAmmount -= use ? (knightVoidMax * percentage) : 0;
                return true;
            }
            return false;
        }

        public Vector2 GetVoidProperties()
        {
            return new Vector2(knightVoidAmmount, knightVoidMax);
        }

        public static bool VoidUnlocked()
        {
            if (Instance != null && HeroController.instance != null)
            {
                for (int i = 0; i < Instance.charmList.Count; i++)
                {
                    if (Instance.charmList[i].equippedCharm && Instance.charmList[i].unlocksVoid)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region Soul Nail
        public Vector2 GetSoulNailHit()
        {
            return new Vector2(soulNailHitCount, maxSoulHitCount);
        }

        private void CreateSoulNailProj(Vector3 pos)
        {
            if (!charmCreations.ContainsKey("soulNail") || charmCreations["soulNail"] == null)
            {
                if (AttackBundles.ContainsKey("charmattacks"))
                {
                    AssetBundle atk = AttackBundles["charmattacks"];
                    if (atk != null && atk.Contains("SoulNailProjectile"))
                    {
                        charmCreations.Add("soulNail", GameObject.Instantiate(atk.LoadAsset<GameObject>("SoulNailProjectile"), pos, Quaternion.identity));
                        SoulNail_proj nailProj = charmCreations["soulNail"].AddComponent<SoulNail_proj>();
                        Transform child = charmCreations["soulNail"].transform.GetChild(0).GetChild(1);
                        CameraShake shake = charmCreations["soulNail"].AddComponent<CameraShake>();
                        shake.ShakeSingle(CameraShakeCues.SmallShake);

                        ProjCollision col = child.gameObject.AddComponent<ProjCollision>();
                        col.proj = nailProj;

                        col.gameObject.AddComponent<NonBouncer>();

                        DamageEnemies dmg = child.gameObject.AddComponent<DamageEnemies>();

                        dmg.attackType = AttackTypes.Spell;
                        dmg.damageDealt = PlayerData.instance.nailDamage;
                        dmg.ignoreInvuln = true;

                        Log("Created Soul Nail Projectile");
                    }
                    else
                    {
                        Log("Attack not found");
                    }
                }
                else
                {
                    Log("Bundle not found");
                }
            }
            else
            {
                charmCreations["soulNail"].SetActive(true);
                charmCreations["soulNail"].transform.position = pos;
                charmCreations["soulNail"].GetComponent<SoulNail_proj>().Restart();
                CameraShake shake = charmCreations["soulNail"].GetComponent<CameraShake>();
                shake.ShakeSingle(CameraShakeCues.SmallShake);

                Log("Created Soul Nail Projectile");
            }
        }
        #endregion

        #region Defender's Roll

        public void SetRolling(bool rolling)
        {
            isRolling = rolling;
            if (rolling)
            {
                HeroController.instance.SetDamageMode(1);
                playerCollideDamage.Enable(PlayerData.instance.GetInt(nameof(PlayerData.instance.nailDamage)) / 4);
            }
            else
            {
                HeroController.instance.SetDamageMode(0);
                playerCollideDamage.Disable();
            }
        }
        #endregion

        #region Soul Basin
        public void SoulBasinGenerateSoul()
        {
            basin_curSoulGainTimer += Time.deltaTime;

            float basinTimer = basin_soulGainTime;
            if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_20)))
            {
                basinTimer *= 0.75f;
            }

            while (basin_curSoulGainTimer > basinTimer)
            {
                basin_curSoulGainTimer -= basinTimer;

                int soulGain = basin_soulGainAmmount;

                if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_34)))
                {
                    soulGain = (int)(soulGain * 1.5f);
                }
                HeroController.instance.AddMPCharge(soulGain);
            }
        }
        #endregion
        #endregion

        #region Knight Animation Change

        public void ChangeAnimation(string animationName, float maxTimer = -1, UnityAction startAction = null, UnityAction endAction = null, float delay = 0)
        {
            if (playerAnim == null)
            {
                playerAnim = HeroController.instance.gameObject.AddComponent<AAPlayerAnimator>();
                playerAnim.heroAnimator = heroAnimator;
            }

            if (startAction != null)
            {
                playerAnim._OnFinished.AddListener(startAction);
            }

            if (endAction != null)
            {
                playerAnim._OnFinished.AddListener(endAction);
            }

            playerAnim.ChangeAnimation(animationName, maxTimer, false, delay);
        }

        #endregion

        #endregion

        #region Uninfected Crossroads
        /*
        private void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            radianceDeffeated = PlayerData.instance.GetBool(nameof(PlayerData.instance.killedFinalBoss));

            if (radianceDeffeated)
            {
                PlayerData.instance.SetBool(nameof(PlayerData.instance.crossroadsInfected), false);
                PlayerData.instance.crossroadsInfected = false;
            }

            orig(self);
        }*/
        #endregion

        #endregion

        #region Data management
        #region Settings

        //Unused
        public bool zotePowerEnabled;

        public bool ToggleButtonInsideMenu => true;

        public void StartSettings()
        {
            On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        }

        private void UnloadSettings()
        {
            On.HealthManager.TakeDamage -= HealthManager_TakeDamage;

        }
        private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (zotePowerEnabled)
            {
                return;
            }
            orig(self, hitInstance);
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            return AAMenuSettings.GetMenu(modListMenu, toggleDelegates);
        }

        #endregion

        #region SaveData

        #region Global
        public static AAGlobalSettings GlobalSaveData { get; set; } = new AAGlobalSettings();
        public void OnLoadGlobal(AAGlobalSettings s) => GlobalSaveData = s;
        public AAGlobalSettings OnSaveGlobal() => GlobalSaveData;
        #endregion

        #region Local
        public AALocalSettings LocalSaveData { get; set; } = new AALocalSettings();

        public void OnLoadLocal(AALocalSettings s) => LoadData(s);
        public AALocalSettings OnSaveLocal() => LocalSaveData;
        #endregion
        public void InitializeData()
        {
            ModHooks.SavegameLoadHook += slot =>
            {
                LocalSaveData = new AALocalSettings();
                LocalSaveData.charmsEquipped = new bool[charmList.Count];
                LocalSaveData.curVoidAmmount = 0;
            };
        }

        public void LoadData(AALocalSettings s)
        {
            if (s != null)
            {
                Log("Loading data...");
                LocalSaveData = s;

                if (LocalSaveData != null)
                {
                    if (LocalSaveData.charmsEquipped != null)
                    {
                        for (int i = 0; i < LocalSaveData.charmsEquipped.Length && i<charmList.Count; i++)
                        {
                            charmList[i].equippedCharm = LocalSaveData.charmsEquipped[i];
                        }
                    }
                    if (LocalSaveData.charmsGot != null)
                    {
                        for (int i = 0; i < LocalSaveData.charmsGot.Length && i < charmList.Count; i++)
                        {
                            charmList[i].gotCharm = LocalSaveData.charmsGot[i];
                        }
                    }
                    if (LocalSaveData.charmsNew != null)
                    {
                        for (int i = 0; i < LocalSaveData.charmsNew.Length && i < charmList.Count; i++)
                        {
                            charmList[i].newCharm = LocalSaveData.charmsNew[i];
                        }
                    }
                }

                knightVoidAmmount = LocalSaveData.curVoidAmmount;
            }
        }
        #endregion
        #endregion

    }
    #region Savedata

    public class AAGlobalSettings
    {
        public bool pacifistModeEnabled;
    }

    public class AALocalSettings
    {
        public bool[] charmsGot;
        public bool[] charmsNew;
        public bool[] charmsEquipped;

        public float curVoidAmmount;
    }
    #endregion
}