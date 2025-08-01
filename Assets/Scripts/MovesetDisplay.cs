using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovesetDisplay : MonoBehaviour
{
    public int index;
    public MovesetSO moveset;
    public TextMeshProUGUI movesetName;
    public TextMeshProUGUI movesetDamage;
    public TextMeshProUGUI movesetCombination;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Initialize()
    {
        string isItMultiple;
        if (moveset.comboList.Length > 1)
        {
            isItMultiple = "s)";
        } else
        {
            isItMultiple = ")";
        }
        movesetName.text = (index + 1) + ")     " + moveset.movesetName + " (" + moveset.comboList.Length + " Hit" + isItMultiple;
        float totalDamage = 0;
        string inputDisplay = "";
        foreach (Combo combo in moveset.comboList)
        {
            string key = "";
            totalDamage += combo.attackUsed.damage;
            switch (combo.keyUsed)
            {
                case Combo.attackTypes.TapLightAttack:
                    key = PlayerManager.Instance.input.actions.FindAction("Light Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "<sprite name=" + key + ">";
                    } else
                    {
                        inputDisplay += "<sprite name=" + key + ">, ";
                    }
                    break;
                case Combo.attackTypes.HoldLightAttack:
                    key = PlayerManager.Instance.input.actions.FindAction("Light Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">";
                    }
                    else
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">, ";
                    }
                    break;
                case Combo.attackTypes.TapHeavyAttack:
                    key = PlayerManager.Instance.input.actions.FindAction("Heavy Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "<sprite name=" + key + ">";
                    }
                    else
                    {
                        inputDisplay += "<sprite name=" + key + ">, ";
                    }
                    break;
                case Combo.attackTypes.HoldHeavyAttack:
                    key = PlayerManager.Instance.input.actions.FindAction("Heavy Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">";
                    }
                    else
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">, ";
                    }
                    break;
                case Combo.attackTypes.ModifiedTapLightAttackA:
                    key = PlayerManager.Instance.input.actions.FindAction("Attack Modifier A").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    inputDisplay += "<sprite name=" + key + "> + ";
                    key = PlayerManager.Instance.input.actions.FindAction("Light Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "<sprite name=" + key + ">";
                    }
                    else
                    {
                        inputDisplay += "<sprite name=" + key + ">, ";
                    }
                    break;
                case Combo.attackTypes.ModifiedHoldLightAttackA:
                    key = PlayerManager.Instance.input.actions.FindAction("Attack Modifier A").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    inputDisplay += "<sprite name=" + key + "> + ";
                    key = PlayerManager.Instance.input.actions.FindAction("Light Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">";
                    }
                    else
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">, ";
                    }
                    break;
                case Combo.attackTypes.ModifiedTapLightAttackB:
                    key = PlayerManager.Instance.input.actions.FindAction("Attack Modifier B").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    inputDisplay += "<sprite name=" + key + "> + ";
                    key = PlayerManager.Instance.input.actions.FindAction("Light Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "<sprite name=" + key + ">";
                    }
                    else
                    {
                        inputDisplay += "<sprite name=" + key + ">, ";
                    }
                    break;
                case Combo.attackTypes.ModifiedHoldLightAttackB:
                    key = PlayerManager.Instance.input.actions.FindAction("Attack Modifier B").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    inputDisplay += "<sprite name=" + key + "> + ";
                    key = PlayerManager.Instance.input.actions.FindAction("Light Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">";
                    }
                    else
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">, ";
                    }
                    break;
                case Combo.attackTypes.ModifiedTapHeavyAttackA:
                    key = PlayerManager.Instance.input.actions.FindAction("Attack Modifier A").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    inputDisplay += "<sprite name=" + key + "> + ";
                    key = PlayerManager.Instance.input.actions.FindAction("Heavy Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "<sprite name=" + key + ">";
                    }
                    else
                    {
                        inputDisplay += "<sprite name=" + key + ">, ";
                    }
                    break;
                case Combo.attackTypes.ModifiedHoldHeavyAttackA:
                    key = PlayerManager.Instance.input.actions.FindAction("Attack Modifier A").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    inputDisplay += "<sprite name=" + key + "> + ";
                    key = PlayerManager.Instance.input.actions.FindAction("Heavy Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">";
                    }
                    else
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">, ";
                    }
                    break;
                case Combo.attackTypes.ModifiedTapHeavyAttackB:
                    key = PlayerManager.Instance.input.actions.FindAction("Attack Modifier B").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    inputDisplay += "<sprite name=" + key + "> + ";
                    key = PlayerManager.Instance.input.actions.FindAction("Heavy Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "<sprite name=" + key + ">";
                    }
                    else
                    {
                        inputDisplay += "<sprite name=" + key + ">, ";
                    }
                    break;
                case Combo.attackTypes.ModifiedHoldHeavyAttackB:
                    key = PlayerManager.Instance.input.actions.FindAction("Attack Modifier B").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    inputDisplay += "<sprite name=" + key + "> + ";
                    key = PlayerManager.Instance.input.actions.FindAction("Heavy Attack").GetBindingDisplayString();
                    key = key.Replace("Tap;action.interactions ", "");
                    key = key.Replace("Tap ", "");
                    key = key.Replace("Hold ", "");
                    key = key.Replace("Multi Tap ", "");
                    key = key.Replace("Press ", "");
                    key = key.Replace("Slow Tap ", "");
                    key = key.Replace("or ", "");
                    if (moveset.comboList[moveset.comboList.Length - 1] == combo)
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">";
                    }
                    else
                    {
                        inputDisplay += "Hold <sprite name=" + key + ">, ";
                    }
                    break;
            }
            for (int i = 0; i < combo.attackUsed.addEffects.Length; i++)
            {
                if (combo.attackUsed.addEffects[i].specialEffect == SpecialEffects.Effects.UseUltimate)
                {
                    UltimateSO usedUltimate = PlayerManager.Instance.combat.ultimate;
                    totalDamage += usedUltimate.damage * usedUltimate.movePlayer.Length;
                    break;
                }
            }
        }

        movesetDamage.text = "Min. Damage - <color=red>" + totalDamage + " HP";
        movesetCombination.text = inputDisplay;
    }
}
