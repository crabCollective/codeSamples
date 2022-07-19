using System.Collections;
using System.Collections.Generic;
using Scripts.Core.Pooling;
using Scripts.GameObjects.Enemies;
using Scripts.ScriptableObjects;
using Subtegral.WeightedRandom;
using UnityEngine;

namespace Assets.Scripts.GameObjects.Enemies
{
    [System.Serializable]
    public class WeightSetting<T> {
        public T Type;
        public float Weight;
    }

    [System.Serializable]
    public class EnemiesWeight
    {
        [Tooltip("How often should generator check number of enemies on the scene?")]
        public float EnemiesCheckInterval;
        public int MaxNumOfEnemiesOnScene;
        [Range(0,1)]
        public float EnemyGenProbability;
        public List<WeightSetting<EnemyType>> EnemiesWeights;
    }

    /// <summary>
    /// Basic enemy generator - based on current enemy setup, it just runs and checks in intervals whether 
    /// there are enough enemies on the scene.
    /// </summary>
    public class EnemyGenerator : MonoBehaviour
    {
        [Tooltip("Use this bool for quick dis/enabling from inspector")]
        public bool GeneratorEnabled = true;

        [Space(5)]
        [SerializeField]
        private EnemyFactory _enemyFactory;

        private List<EnemyController> _enemyList = new List<EnemyController>();
        public List<EnemyController> EnemyList { get => _enemyList; set => _enemyList = value; }

        private EnemiesWeight _currentEnemiesSetup;
        private Coroutine _spawnCoroutine = null;

        /// <summary>
        /// We are using weighted random technique to generate enemies in user-defined weights
        /// </summary>
        private WeightedRandom<EnemyType> _randomEnemyBag = new WeightedRandom<EnemyType>();

        /// <summary>
        /// Sets given enemy setup - this should be called from game session handler
        /// </summary>
        /// <param name="_currentSetup"></param>
        public void SetCurrentSpawnSetup(EnemiesWeight _currentSetup) {
            _currentEnemiesSetup = _currentSetup;

            _randomEnemyBag.Clear();
            foreach (WeightSetting<EnemyType> enemy in _currentEnemiesSetup.EnemiesWeights) {
                _randomEnemyBag.Add(enemy.Type, enemy.Weight);
            }
        }

        public void StartSpawning() {
            StopSpawning();
            if (_currentEnemiesSetup.EnemiesWeights.Count <= 0)
                return;

            _spawnCoroutine = StartCoroutine(SpawnCoroutine());
        }

        public void StopSpawning() {
            if (_spawnCoroutine != null)
                StopCoroutine(_spawnCoroutine);
        }

        /// <summary>
        /// Main spawn coroutine - it tries to spawn enemies each X seconds
        /// </summary>
        private IEnumerator SpawnCoroutine() {
            while (true) {
                yield return new WaitForSeconds(_currentEnemiesSetup.EnemiesCheckInterval);
                SpawnNeededEnemies();
            }
        }

        /// <summary>
        /// Main spawn method - checking how many enemies are on the scene and try to spawn some
        /// </summary>
        private void SpawnNeededEnemies() {
            if (!GeneratorEnabled)
                return;

            int numOfEnemiesToSpawn = _currentEnemiesSetup.MaxNumOfEnemiesOnScene - _enemyList.Count;
            for (int i = 0; i < numOfEnemiesToSpawn; i++)
            {
                if (Random.Range(0,1f) <= _currentEnemiesSetup.EnemyGenProbability) {
                    EnemyType enemyType = _randomEnemyBag.Next();
                    SpawnEnemy(enemyType);
                }
            }
        }

        /// <summary>
        /// Spawns enemy from factory
        /// </summary>
        /// <param name="typeOfEnemy">Type of enemy to spawn</param>
        public void SpawnEnemy(EnemyType typeOfEnemy)
        {
            EnemyController enemy = _enemyFactory.SpawnFromRandomPoint(typeOfEnemy);
            if (enemy != null)
            {
                enemy.onPoolReturning += OnEnemyDisappear;
                _enemyList.Add(enemy);
            }
            else Debug.LogError("Trying to spawn enemy, but it is null!" + typeOfEnemy);
        }

        private void OnEnemyDisappear(IPoolable enemy)
        {
            enemy.onPoolReturning -= OnEnemyDisappear;
            _enemyList.Remove((EnemyController)enemy);
        }

        public void RemoveAllEnemies() {
            foreach (EnemyController enemy in _enemyList) {
                enemy.onPoolReturning -= OnEnemyDisappear;
                enemy.ReturnToPool();
            }

            _enemyList.Clear();
        }
    }
}
