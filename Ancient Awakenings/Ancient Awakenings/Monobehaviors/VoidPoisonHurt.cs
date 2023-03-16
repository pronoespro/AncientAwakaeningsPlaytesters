using UnityEngine;

namespace Ancient_Awakenings_SoulNail_charm.Monobehaviors
{
    public class VoidPoisonHurt:MonoBehaviour
    {

        public float poisonTime = 2;
        public float poisonFrecuency = 4;

        private float timer;
        private int timesDamaged;
        private HealthManager hp;

        private float dmg;
        private int ogDmg;

        private void Start()
        {
            hp = GetComponent<HealthManager>();
            if (hp.gameObject.name.Contains("Ghost Warrior Hu"))
            {
                ogDmg = Mathf.Max(PlayerData.instance.nailDamage, 1);
            }
            else
            {
                ogDmg = Mathf.Max(PlayerData.instance.nailDamage / 10, 1);
            }
            dmg = ogDmg;
        }

        public void Reapply()
        {
            timer = 0;
            timesDamaged = 0;

            poisonTime = Mathf.Min(poisonTime *1.5f, 2*1.5f*1.5f);
            poisonFrecuency = Mathf.Min(poisonFrecuency * 1.75f,4*1.75f*1.75f);
            dmg = Mathf.Min(dmg * 1.25f, ogDmg * 1.75f);
        }

        public void Update()
        {

            if (hp.GetIsDead() || hp.hp<2 || (hp.hasSpecialDeath && hp.hp<3))
            {
                Destroy(this);
                return;
            }

            timer += Time.deltaTime;

            if (timesDamaged< poisonFrecuency/poisonTime*timer)
            {
                HitInstance hit = new HitInstance();

                hit.AttackType =AttackTypes.Spell;
                hit.DamageDealt = (int)dmg;
                hit.IgnoreInvulnerable = true;
                hit.Source = gameObject;

                hp.Hit(hit);
                hp.hp -= hit.DamageDealt;
                if (hp.hp <= 0)
                {
                    hp.Die(0,AttackTypes.Spell,true);
                }

                timesDamaged++;
                AncientAwakeningsMod.Instance.Log("Poison hit by "+dmg.ToString());
            }
            if (timer > poisonTime + Time.deltaTime)
            {
                Destroy(this);
                AncientAwakeningsMod.Instance.Log("Poison Ended");
            }
        }

    }
}
