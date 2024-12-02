using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image normalAttackImage;

    [SerializeField] private Image secondAttackImage;

    [SerializeField] private TextMeshProUGUI secondAttackUses;

    [SerializeField] private Image dashAttackImage;
    [SerializeField] private Image dashAttackUse;

    [SerializeField] private TextMeshProUGUI normalAttackTime;
    [SerializeField] private TextMeshProUGUI secondAttackTime;

    private int maxUsesSecondAttack;
    private int currentUsesSecondAttack;

    private float currentCDNormalAttack;
    private float cdNormalAttack;
    private bool normalCDDone = true;

    private float currentCDSecondAttack;
    private float cdSecondAttack;
    private bool secondCDDone = true;

    [SerializeField] private GameObject bluePlanetPanel;

    public void GameStarted()
    {
        bluePlanetPanel.SetActive(false);
    }

    public void UsedNormalAttack(float cd)
    {
        cdNormalAttack = cd;
        currentCDNormalAttack = cd;
        normalAttackImage.fillAmount =1f;
        
        normalAttackTime.text = currentCDNormalAttack.ToString();
        StartCoroutine(NormalCooldown());
    }

    private IEnumerator NormalCooldown()
    {
        normalCDDone = false;
        while(!normalCDDone)
        {
            currentCDNormalAttack -= 0.05f;
            yield return new WaitForSeconds(0.05f);
            if (currentCDNormalAttack <= 0f)
            {
                normalCDDone = true;
                currentCDNormalAttack = 0f;
            }
            normalAttackTime.text = "" + (Mathf.Round(currentCDNormalAttack*10) / 10); 
            normalAttackImage.fillAmount = currentCDNormalAttack / cdNormalAttack;
        }
        normalAttackTime.text = "";
    }

    public void UsedSecondAttack(float cd)
    {
        cdSecondAttack = cd;
        currentCDSecondAttack = cd;
        secondAttackImage.fillAmount = 1f;
        if (maxUsesSecondAttack >= 0)
        {
            currentUsesSecondAttack--;
            secondAttackUses.text = currentUsesSecondAttack + "/" + maxUsesSecondAttack;
        }

        secondAttackTime.text = currentCDSecondAttack.ToString();
        StartCoroutine(SecondCooldown());
    }

    private IEnumerator SecondCooldown()
    {
        secondCDDone = false;
        while (!secondCDDone)
        {
            currentCDSecondAttack -= 0.05f;
            yield return new WaitForSeconds(0.05f);
            if (currentCDSecondAttack <= 0f)
            {
                secondCDDone = true;
                currentCDSecondAttack = 0f;
            }
            secondAttackTime.text = "" + (Mathf.Round(currentCDSecondAttack * 10) / 10);
            secondAttackImage.fillAmount = currentCDSecondAttack / cdSecondAttack;
        }
        secondAttackTime.text = "";
    }

    public void SetSecondary(int uses)
    {
        maxUsesSecondAttack = currentUsesSecondAttack = uses;
    }

    public void UsedSpecialDash()
    {
        //cambiar sprite de dashattackuse y dashattackimage 

    }

    public void UsingBluePlanet()
    {
        bluePlanetPanel.SetActive(true);
    }

    public void EndedBluePlanet()
    {
        bluePlanetPanel.SetActive(false);
    }



}
