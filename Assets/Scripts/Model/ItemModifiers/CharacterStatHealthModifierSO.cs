using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterStatHealthModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        PlayerData health = character.GetComponent<PlayerMovement>().Data;
        if (health != null )
        {
            health.currentHealth += (int)val;
        }
    }
}
