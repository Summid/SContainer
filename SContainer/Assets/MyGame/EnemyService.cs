using UnityEngine;

namespace MyGame
{
    public class EnemyService
    {
        public static int EnemyCount;
        public int count;
        
        public void Attack()
        {
            Debug.Log($"Enemy{this.count} attack");
        }

        public EnemyService()
        {
            this.count = EnemyCount++;
        }
    }
}