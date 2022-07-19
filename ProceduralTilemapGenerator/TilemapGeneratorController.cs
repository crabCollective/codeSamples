using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.Tilemaps
{
    /// <summary>
    /// Generates one tilemap representing chunk in the road. Using Perlin noise for height determination.
    /// </summary>
    public class TilemapGeneratorController : MonoBehaviour
    {
        [Header("Size and shape settings")]
        [SerializeField]
        private int _minRandomChunkWidth = 60;
        [SerializeField]
        private int _maxRandomChunkWidth = 100;
        
        [SerializeField]
        [Range(0, 100)]
        [Tooltip("Height value determining height of Perlin-noise generated top")]
        private int _heightValue = 40;
        [SerializeField]
        [Tooltip("Height value which will be added to the top")]
        private int _addedHeight = 30;
        [SerializeField]
        [Range(0, 100)]
        private float Smoothness = 50f;

        [Header("Tilemap settings")]
        [SerializeField]
        private TilemapFactory _tilemapFactory;
        [SerializeField]
        public Tile BasicTile;

        [Header("Optimization")]
        [SerializeField]
        private int _numOfColumnsPaintedInOneFrame = 3;
        private float _seed;
        private int _lastColumnHeight = 0;

        /// <summary>
        /// Generates random tilemap, either using quick or slow generation
        /// </summary>
        /// <param name="localPosition">Position in local space</param>
        /// <param name="isInitGeneration">Are we generating on init?</param>
        /// <returns>PoolableTilemapController object</returns>
        public PoolableTilemapController GenerateRandomTilemap(Vector3 localPosition, bool isInitGeneration)
        {
            _seed = UnityEngine.Random.Range(-1000000, 1000000);
            PoolableTilemapController tmFromPool = _tilemapFactory.Get(PoolableTilemapType.TILEMAP_CUSTOMIZABLE);
            if (tmFromPool == null)
                Debug.LogError(gameObject.name + ": Trying to get tilemap from pool, but it is null!");

            tmFromPool.transform.localPosition = localPosition;

            int chunkWidth = isInitGeneration ? _maxRandomChunkWidth : UnityEngine.Random.Range(_minRandomChunkWidth, _maxRandomChunkWidth+1);
            tmFromPool.SetWidth(BasicTile.sprite.bounds.size.x * chunkWidth);
            //on init we want it to be generated quick - do not split generation between multiple frames
            if (isInitGeneration)
                tmFromPool = GenerateRandomTerrainQuick(tmFromPool, chunkWidth);
            else StartCoroutine(GenerateRandomTerrainSlow(tmFromPool, chunkWidth));

            return tmFromPool;
        }

        /// <summary>
        /// Coroutine responsible for performant generation of the tilemap. Tile generation is splitted into multiple frames to ensure less stuttering.
        /// </summary>
        /// <param name="tmFromPool">Tilemap from pool</param>
        /// <param name="width">Chosen chunk width</param>
        /// <returns></returns>
        private IEnumerator GenerateRandomTerrainSlow(PoolableTilemapController tmFromPool, int width) {

            int columnsPainted = 0;
            for (int x = 0; x < width; x++)
            {
                int height = Mathf.RoundToInt(_heightValue * Mathf.PerlinNoise(x / Smoothness, _seed));
                for (int y = 0; y < _addedHeight + height; y++)
                {
                    _lastColumnHeight = _addedHeight + height;
                    tmFromPool.AssignedTilemap.SetTile(new Vector3Int(x, y, 0), BasicTile);
                }

                columnsPainted++;
                if (columnsPainted >= _numOfColumnsPaintedInOneFrame) {
                    columnsPainted = 0;
                    yield return null;
                }
            }
            
            tmFromPool.EnableCollider();
        }

        /// <summary>
        /// Use this method for immediate generation of random tile. Wider tiles might suffer from stuttering
        /// </summary>
        /// <param name="tmFromPool">Tilemap from pool</param>
        /// <param name="width">Chosen chunk width</param>
        /// <returns></returns>
        private PoolableTilemapController GenerateRandomTerrainQuick(PoolableTilemapController tmFromPool, int width) 
        {
            for (int x = 0; x < width; x++)
            {
                int height = Mathf.RoundToInt(_heightValue * Mathf.PerlinNoise(x / Smoothness, _seed));
                for (int y = 0; y < _addedHeight + height; y++)
                {
                    _lastColumnHeight = _addedHeight + height;
                    tmFromPool.AssignedTilemap.SetTile(new Vector3Int(x, y, 0), BasicTile);
                }
            }
            
            //generate collider in coroutine one frame later, otherwise collider is not generated
            StartCoroutine(EnableColliderDelayed(tmFromPool));

            return tmFromPool;
        }

        private IEnumerator EnableColliderDelayed(PoolableTilemapController tmFromPool) {
            yield return null;
            tmFromPool.EnableCollider();
        }
    }
}
