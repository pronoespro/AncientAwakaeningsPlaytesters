using UnityEngine;

namespace Ancient_Awakenings_SoulNail_charm.Monobehaviors
{
    public class PlayerCollisionDamage:MonoBehaviour
    {

        public bool damaging;
        public int damage;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (damaging)
            {
                HealthManager hp = collider.GetComponent<HealthManager>();
                if (hp != null)
                {
                    HitInstance dmg = new HitInstance();
                    dmg.AttackType = AttackTypes.SharpShadow;
                    dmg.DamageDealt = damage;
                    dmg.Source = HeroController.instance.gameObject;

                    hp.Hit(dmg);
                }
            }
        }

        public void Enable(int attackDamage)
        {
            damage = attackDamage;
            damaging = true;
        }

        public void Disable()
        {
            damaging = false;
        }

    }
}
