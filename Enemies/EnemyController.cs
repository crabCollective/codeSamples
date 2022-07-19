using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Core;
using Scripts.Core.Pooling;
using Scripts.GameObjects.Bullets;
using Scripts.GameObjects.Effects;
using Scripts.Utils;
using UnityEngine;

namespace Scripts.GameObjects.Enemies
{
    public enum EnemyType
    {
        ENEMY_UFO = 0,
        ENEMY_TURRET = 1,
        ENEMY_TANK = 2
    }

    /// <summary>
    /// Basic class for enemies of all kind. Only ensures basic functionality. Derive from this for specific implementation.
    /// </summary>
    public class EnemyController : MonoBehaviour, IPoolable
    {
        [SerializeField]
        protected EnemyType _enemyType;
        public Enum Type { get => _enemyType; set => _enemyType = (EnemyType)value; }

        [SerializeField]
        protected BulletType _bulletType;
        public BulletType BulletType { get => _bulletType; set => _bulletType = value; }

        [SerializeField]
        protected EffectObjectType _explosionType;
        public EffectObjectType ExplosionType { get => _explosionType; set => _explosionType = value; }

        protected BulletFactory _bulletFactory;

        [SerializeField]
        protected RangedFloat _shootIntervalRange;

        [SerializeField]
        protected float _shootSpeed = 30;
        [SerializeField]
        private int scoreForKill = 200;
        public int ScoreForKill { get => scoreForKill; set => scoreForKill = value; }

        [SerializeField]
        protected Transform _shootPosition;

        protected float _shootInterval = 0.5f;
        protected Rigidbody2D _rb;

        protected AudioSource _audioSource;

        public event Action<EnemyController> onEnemyDeath;
        public event Action<IPoolable> onPoolReturning;

        protected void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _audioSource = GetComponent<AudioSource>();
        }

        public void OnPoolGet()
        {
        }

        public virtual void OnPoolRelease()
        {
        }

        public void ReturnToPool()
        {
            onPoolReturning?.Invoke(this);
        }

        /// <summary>
        /// Initializes position and assign bullet factory
        /// </summary>
        /// <param name="startPos">Start position</param>
        /// <param name="bFactory">Bullet factory to use</param>
        public virtual void Init(Vector2 startPos, BulletFactory bFactory)
        {
            _bulletFactory = bFactory;
            transform.position = startPos;
            _shootInterval = UnityEngine.Random.Range(_shootIntervalRange.min, _shootIntervalRange.max);
        }

        /// <summary>
        /// When being shot by the player, enemy dies. No health engaged in process.
        /// </summary>
        /// <param name="other">Other object</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == IntConstants.LAYER_PLAYER_BULLETS)
            {
                BulletController playerBullet = other.gameObject.GetComponent<BulletController>();
                playerBullet.DestroyBullet(true);
                Die();
            }
        }

        /// <summary>
        /// Use this method for bullet shooting and play sound on assigned audiosource
        /// </summary>
        protected virtual void ShootTheBullet()
        {
            BulletController bullet = _bulletFactory.Get(_bulletType);
            if (bullet != null)
            {
                bullet.Shoot(_shootPosition.position, Vector2.down);
                _audioSource.Play();
            }
            else Debug.LogError("EnemyHelicopter: cannot get a bullet from pool!");
        }

        public void Die()
        {
            onEnemyDeath?.Invoke(this);
            ReturnToPool();
        }
    }
}
