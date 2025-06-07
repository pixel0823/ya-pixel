using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    private float PlayerMaxHP = 100;
    private float PlayerDamage = 10;

    private float PlayerCurrentHP;



    void Awake()
    {
        PlayerCurrentHP = PlayerMaxHP;
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void GetDamage(float MonsterDamage)
    {
        PlayerCurrentHP -= MonsterDamage;
        // 주인공 맞는 모션

        if (PlayerCurrentHP <= 0)
        {
            //죽는 모션 넣을거임
        }
    }

    public void Attack(float MonsterCurrentHP)
    {
        MonsterCurrentHP -= PlayerDamage;
    }
}
