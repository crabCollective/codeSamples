using System.Collections;
using System.Collections.Generic;
using Scripts.Core.UI;
using Scripts.GameObjects.Effects;
using UnityEngine;

namespace Scripts.GameObjects.Enemies
{
    public class EnemyEventsHandler : MonoBehaviour
    {
        public EffectsObjectFactory EffectsFactory;
        public ScoreHandlerBehaviour ScoreHandler;

        public void BindEventsToActions(EnemyController enemy) {
            enemy.onEnemyDeath += OnEnemyDeath;
        }

        /// <summary>
        /// Instantiate explosion and add score after enemy is killed
        /// </summary>
        /// <param name="enemy"></param>
        private void OnEnemyDeath(EnemyController enemy) {
            EffectObjectController explosion = EffectsFactory.Get(enemy.ExplosionType);
            explosion.Init(enemy.transform.position);

            if (ScoreHandler != null)
                ScoreHandler.Increase(enemy.ScoreForKill);
        }
    }
}
