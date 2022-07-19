using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Core;
using Scripts.Core.Pooling;
using Scripts.GameObjects.Bullets;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts.Tilemaps
{
    public enum PoolableTilemapType
    {
        TILEMAP_CUSTOMIZABLE,//default tilemap
        TILEMAP_HOLE_SMALL,
        TILEMAP_HOLE_BIG
    }

    /// <summary>
    /// Class representing poolable tilemap (chunk of the terrain) used in conjunction with TilemapGeneratorController
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class PoolableTilemapController : MonoBehaviour, IPoolable
    {
        [SerializeField]
        private PoolableTilemapType _tilemapType = PoolableTilemapType.TILEMAP_CUSTOMIZABLE;
        public Enum Type { get => _tilemapType; set => _tilemapType = (PoolableTilemapType)value; }

        public event Action<IPoolable> onPoolReturning;

        /// <summary>
        /// Actual assigned Unity tilemap
        /// </summary>
        private Tilemap _assignedTilemap;
        public Tilemap AssignedTilemap { get => _assignedTilemap; private set => _assignedTilemap = value; }

        /// <summary>
        /// 2D composite collider applied on the tilemap when the generation is finished
        /// </summary>
        private CompositeCollider2D _assignedCollider;

        [SerializeField]
        private float _width = 0;
        public float Width { get => _width; private set => _width = value; }

        private void Awake()
        {
            _assignedTilemap = GetComponent<Tilemap>();
            if (_assignedTilemap == null)
                Debug.LogError(gameObject.name + ": no assigned tilemap found, check it!");

            //only customizable tile has assigned collider
            _assignedCollider = GetComponent<CompositeCollider2D>();
        }

        public void OnPoolGet()
        {
        }

        public void OnPoolRelease()
        {
            if (_tilemapType == PoolableTilemapType.TILEMAP_CUSTOMIZABLE)
                _assignedTilemap.ClearAllTiles();
        }

        public void ReturnToPool()
        {
            onPoolReturning?.Invoke(this);
        }

        public void SetWidth(float width)
        {
            _width = width;
        }

        public void EnableCollider()
        {
            _assignedCollider.GenerateGeometry();
        }
    }
}
