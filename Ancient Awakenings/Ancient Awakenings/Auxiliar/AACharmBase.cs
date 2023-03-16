using Ancient_Awakenings_SoulNail_charm.Monobehaviors;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using SFCore;

namespace Ancient_Awakenings_SoulNail_charm.Auxiliar
{

    public class AACharmBase
    {

        public string charmSprite;

        public string charmName;

        public bool gotCharm;
        public bool newCharm;
        public bool equippedCharm;

        public bool unlocksVoid;
        public int charmCost;
        public int charmNum;

        public AACharmBase(string name, string sprite, int cost = 1,bool unlockVoid=false)
        {
            charmName = name;
            charmSprite = sprite;
            charmCost = cost;
            unlocksVoid = unlockVoid;
        }

        public AACharmBase()
        {

        }

        public virtual void EquipCharm()
        {

        }

        public virtual string GetName()
        {
            return charmName;
        }

    }

    public class AAUpgradeableCharm : AACharmBase
    {

        public int curPhase;
        public int maxPhase;
        public string[] phaseNames;

        public AAUpgradeableCharm(string name, string sprite, int cost = 1,int phaseQuantity=2,string[] nameOfPhases=null)
        {
            charmName = name;
            charmSprite = sprite;
            charmCost = cost;

            maxPhase = phaseQuantity;
            phaseNames = nameOfPhases;
        }

        public override string GetName()
        {
            if (phaseNames == null || phaseNames.Length == 0)
            {
                return base.GetName();
            }
            else
            {
                return phaseNames[Math.Min(curPhase, phaseNames.Length)];
            }
        }
    }

}
