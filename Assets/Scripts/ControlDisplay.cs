using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlDisplay : MonoBehaviour
{
    private MovesetSO[] moveset;
    private PlayerInput input;
    private TextMeshProUGUI text;
    string readyText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = PlayerManager.Instance.input;
        moveset = PlayerManager.Instance.combat.moveset.ToArray();
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        readyText = "Move List : <br>";
        bool onAir = PlayerManager.Instance.onAir;
        foreach (MovesetSO move in moveset)
        {
            if (onAir)
            {
                if (!move.isAirAttack)
                {
                    continue;
                }
                // show only air moves
                string key = "";
                string inputDisplay = "";
                foreach (Combo combo in move.comboList)
                {
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
                            {
                                inputDisplay += "<sprite name=" + key + ">";
                            }
                            else
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
                            {
                                inputDisplay += "Hold <sprite name=" + key + ">";
                            }
                            else
                            {
                                inputDisplay += "Hold <sprite name=" + key + ">, ";
                            }
                            break;
                    }
                }
                inputDisplay += " - " + move.movesetName;
                readyText += inputDisplay + "<br>";
            }
            else
            {
                if (move.isAirAttack)
                {
                    continue;
                }
                // show only ground moves
                string key = "";
                string inputDisplay = "";
                foreach (Combo combo in move.comboList)
                {
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
                            {
                                inputDisplay += "<sprite name=" + key + ">";
                            }
                            else
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
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
                            if (move.comboList[move.comboList.Length - 1] == combo)
                            {
                                inputDisplay += "Hold <sprite name=" + key + ">";
                            }
                            else
                            {
                                inputDisplay += "Hold <sprite name=" + key + ">, ";
                            }
                            break;
                    }
                }
                inputDisplay += " - " + move.movesetName;
                readyText += inputDisplay + "<br>";
            }
        }
        text.text = readyText;
    }
}
