using UnityEngine;

public static class FindClosestEnemy // static 클래스로 변경
{
    public static GameObject FindClosestEnemyObject(Vector2 fromPosition)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(fromPosition, enemy.transform.position);
            if (dist < minDist)
            {
                closest = enemy;
                minDist = dist;
            }
        }
        return closest;
    }
}
