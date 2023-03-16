using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Ancient_Awakenings_SoulNail_charm.Monobehaviors
{
    [RequireComponent(typeof(Canvas))]
    public class AncientUI : MonoBehaviour
    {

        private Canvas canvas;

        private void Start()
        {
            canvas = GetComponent<Canvas>();

            CanvasGroup _soulnaildisplay = transform.GetChild(0).GetComponent<CanvasGroup>();
            if (_soulnaildisplay != null)
            {
                _soulnaildisplay.alpha = 0;
            }
        }

        private void Update()
        {

            if (HeroController.instance != null)
            {
                if (canvas != null)
                {

                    canvas.transform.SetAsFirstSibling();

                    CanvasGroup selectedGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
                    if (selectedGroup != null)
                    {
                        if (AncientAwakeningsMod.GetCharmEquipped(14) && !PlayerData.instance.atBench && !AncientAwakeningsMod.IsGamePaused())
                        {
                            selectedGroup.alpha = 1;

                            Slider hitMetter = selectedGroup.transform.GetChild(0).GetComponent<Slider>();
                            if (hitMetter != null)
                            {
                                Vector2 hits = AncientAwakeningsMod.Instance.GetSoulNailHit();
                                hitMetter.maxValue = hits.y - 1;
                                hitMetter.value = hits.x % hits.y;
                            }
                        }
                        else
                        {
                            selectedGroup.alpha = 0;
                        }
                    }

                    selectedGroup = transform.GetChild(1).GetComponent<CanvasGroup>();
                    if (selectedGroup != null)
                    {
                        if (AncientAwakeningsMod.VoidUnlocked() && !AncientAwakeningsMod.IsGamePaused())
                        {
                            selectedGroup.alpha =Mathf.Lerp(selectedGroup.alpha, AncientAwakeningsMod.Instance.EnoughVoidToUse()? 1f:0.5f,Time.deltaTime*10);

                            Slider voidMetter = selectedGroup.transform.GetChild(0).GetComponent<Slider>();
                            if (voidMetter != null)
                            {
                                Vector2 voidValues = AncientAwakeningsMod.Instance.GetVoidProperties();
                                voidMetter.value = voidValues.x;
                                voidMetter.maxValue = voidValues.y;
                            }
                        }
                        else
                        {
                            selectedGroup.alpha = Mathf.Lerp(selectedGroup.alpha, 0,Time.deltaTime*5);
                        }
                    }

                }
            }
            else
            {
                CanvasGroup selectedGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
                if (selectedGroup != null)
                {
                    selectedGroup.alpha = 0;
                }
                selectedGroup = transform.GetChild(1).GetComponent<CanvasGroup>();
                if (selectedGroup != null)
                {
                    selectedGroup.alpha = 0;
                }
            }
        }

    }
}
