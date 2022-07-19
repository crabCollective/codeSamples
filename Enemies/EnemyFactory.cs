using System.Collections;
using System.Collections.Generic;
using Scripts.Core.Pooling;
using Scripts.GameObjects.Bullets;
using UnityEngine;

namespace Scripts.GameObjects.Enemies
{
    /// <summary>
    /// Class made for enemies creation. Using array of spawn points (GameObjects put in world space in Unity) to determine enemy start position
    /// </summary>
    public class EnemyFactory : MonoObjectFactory<EnemyController>
    {
        [SerializeField]
        private int _numOfAttempts = 20;
        [SerializeField]
        private EnemySpawnPoint[] _spawnPoints;
        [SerializeField]
        private BulletFactory _bulletFactory;

        /// <summary>
        /// In this handler, we can handle various enemy events (onEnemyDeath etc.) and bind for example some effects to them
        /// </summary>
        [SerializeField]
        private EnemyEventsHandler _enemiesHandler;


        /// <summary>
        /// We have overriden base class method for pooled item creation in order to bind event handler on each created item
        /// </summary>
        /// <param name="objectToCreate"></param>
        /// <param name="objectParent"></param>
        /// <returns>New enemy object</returns>
        protected override EnemyController CreatePooledItem(EnemyController objectToCreate, Transform objectParent)
        {
            EnemyController enemy = base.CreatePooledItem(objectToCreate, objectParent);
            //bind events to persons
            if (!System.Object.ReferenceEquals(enemy, null))
            {
                _enemiesHandler.BindEventsToActions(enemy);
            }

            return enemy;
        }

        /// <summary>
        /// Try to spawn enemy from random spawn point. Limited by the number of attempts.
        /// </summary>
        /// <param name="typeOfEnemy">Enemy to spawn</param>
        /// <returns>Enemy when successfull or null when not</returns>
        public EnemyController SpawnFromRandomPoint(EnemyType typeOfEnemy)
        {
            EnemyController enemy = null;
            int spawnAttempts = 0;
            while (System.Object.ReferenceEquals(enemy, null) && spawnAttempts <= _numOfAttempts)
            {
                enemy = SpawnFromPoint(Random.Range(0, _spawnPoints.Length), typeOfEnemy);
                spawnAttempts++;
            }

            return enemy;
        }

        /// <summary>
        /// Try to spawn enemy from spawn point
        /// </summary>
        /// <param name="pointIndex">Which spawnpoint we are using?</param>
        /// <param name="typeOfEnemy">Enemy to spawn</param>
        /// <returns>Enemy when successfull or null when not</returns>
        private EnemyController SpawnFromPoint(int pointIndex, EnemyType typeOfEnemy)
        {
            if (_spawnPoints[pointIndex].IsOcuppied)
                return null;

            EnemyController enemy = base.Get(typeOfEnemy);
            if (!System.Object.ReferenceEquals(enemy, null))
            {
                enemy.Init(_spawnPoints[pointIndex].transform.position, _bulletFactory);
                _spawnPoints[pointIndex].SetOccupied();

                return enemy;
            }
            else Debug.LogError(gameObject.name + ": Trying to spawn enemy in point, but cannot get enemy controller!");

            return null;
        }
    }
}
