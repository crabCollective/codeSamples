using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.GameObjects.Enemies
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        public float OccupiedTimeAfterSpawn = 1f;
        private bool _isOccupied;
        public bool IsOcuppied
        {
            get { return _isOccupied; }
            private set { _isOccupied = value; }
        }

        public void SetOccupied() {
            StartCoroutine(SetOccupiedAndUnset());
        }

        private IEnumerator SetOccupiedAndUnset() {
            _isOccupied = true;
            yield return new WaitForSeconds(OccupiedTimeAfterSpawn);

            _isOccupied = false;
        }
    }
}
