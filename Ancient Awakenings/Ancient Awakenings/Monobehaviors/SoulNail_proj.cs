using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using UnityEngine;

namespace Ancient_Awakenings_SoulNail_charm.Monobehaviors
{
    public class SoulNail_proj:MonoBehaviour
    {

        private float timeToTransform = 1f;
        private float projSpeed = 30;

        private float transformTimer;
        private Animator anim;
        private AudioSource audio;
        private ParticleSystem particles;
        private HealthManager target;
        private float disableTimer;
        private bool targeted;

        private void Start()
        {
            anim = GetComponentInChildren<Animator>();

            audio = GetComponent<AudioSource>();
            particles = transform.GetChild(0).GetChild(0).GetComponentInChildren<ParticleSystem>();
        }

        public void Restart()
        {
            if (transformTimer >= timeToTransform)
            {
                disableTimer = 0;
                anim.SetBool("transformed", false);
                anim.SetFloat("sibilingType", UnityEngine.Random.Range(0, 3));
                transformTimer = 0;
                targeted = false;

                transform.rotation = Quaternion.Euler(Vector3.zero);

                if (audio != null){
                    audio.Play();
                }
                if (particles != null){
                    particles.Play();
                }
            }
        }

        public void Collide()
        {
            if (disableTimer < 3f - Time.deltaTime)
            {
                transformTimer = -0.05f;
                disableTimer = 3f - Time.deltaTime - 0.001f;
            }

        }

        private void OnEnable()
        {
            Restart();
        }

        private void GetTarget()
        {
            foreach(HealthManager health in FindObjectsOfType<HealthManager>())
            {
                if (health.isDead){
                    continue;
                }
                if (target==null || (
                    Vector3.Magnitude(health.transform.position-transform.position)<Vector3.Magnitude(target.transform.position-transform.position)+2f
                    &&
                    Vector3.Dot(Vector3.Normalize(health.transform.position-transform.position),transform.up)>
                    Vector3.Dot(Vector3.Normalize(target.transform.position-transform.position),transform.up))) { 

                    target = health;
                }
                if (health.hasSpecialDeath)
                {
                    target = health;
                    break;
                }
            }
        }

        private void Update()
        {

            if (target == null)
            {
                anim.SetBool("transformed", false);
                transform.rotation = Quaternion.Euler(Vector3.zero);
                GetTarget();
            }

            transformTimer += Time.deltaTime;

            if (transformTimer > timeToTransform || transformTimer<0){

                anim.SetBool("transformed", true);
                transform.position += transform.up * Time.deltaTime*projSpeed;

                if (target != null)
                {
                    if (!targeted)
                    {
                        transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.Normalize(target.transform.position - transform.position));
                        targeted = true;
                    }

                    transform.rotation=Quaternion.LookRotation(Vector3.forward,Vector3.Lerp(transform.up, Vector3.Normalize(target.transform.position - transform.position), Time.deltaTime * 10));
                }
                disableTimer += Time.deltaTime;
                if (disableTimer >= 3f) 
                {
                    gameObject.SetActive(false);
                    transformTimer = timeToTransform;
                }
                if (target == null)
                {
                    disableTimer = 3 - Time.deltaTime;
                    particles.Pause();
                }
            }
            else
            {
                anim.SetBool("transformed", false);
                transform.rotation = Quaternion.Euler(Vector3.zero);
            }
        }

    }
}
