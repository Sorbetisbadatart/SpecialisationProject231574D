using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;
    public int unitLevel;
    public int damage;
    public int maxHealth;
    public int currentHealth;

    public bool TakeDamage(int dmg)
    {
        if (IsDead() == false)
        {          
            currentHealth -= dmg;         
        }
        return IsDead();
    }

    private bool IsDead()
    {
        return !(currentHealth > 0); //return true if currHP equal/below 0
    }
}
