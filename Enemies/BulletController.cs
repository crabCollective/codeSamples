using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Core;
using Scripts.Core.Pooling;
using UnityEngine;

namespace Scripts.GameObjects.Bullets
{
    public enum BulletType
    {
        BULLET_FRONT_CANNON = 0,
        BULLET_TOP_CANNON = 1,
        BULLET_ENEMY_TANK = 2,
        BULLET_UFO = 3
    }

    /// <summary>
    /// This is base class for the bullet implementing very basic behaviour.
    /// Create inherited classes for more complex behaviours
    /// </summary>
    public class BulletController : MonoBehaviour, IPoolable
    {
        [SerializeField]
        private BulletType _bulletType = BulletType.BULLET_FRONT_CANNON;
        public Enum Type { get => _bulletType; set => _bulletType = (BulletType)value; }

        [SerializeField]
        private float _speed;

        [SerializeField]
        private bool _hasTimeToLive = false;

        [SerializeField]
        private float _timeToLive = 2f;
        private float _timeAlive = 0f;

        protected Vector3 _shootDirection;

        public event Action<IPoolable> onPoolReturning;
        public event Action<BulletController> onExplosionShow;

        public virtual void Shoot(Vector3 startPos, Vector3 direction)
        {
            _timeAlive = 0;

            _shootDirection = direction;
            transform.SetPositionAndRotation(startPos, transform.rotation);
        }

        protected virtual void Update()
        {
            //bullet movement is not using physics, no rigidbody is attached to the bullet object
            transform.Translate(_shootDirection * _speed * Time.deltaTime, Space.World);
            if (_hasTimeToLive) {
                _timeAlive += Time.deltaTime;
                if (_timeAlive >= _timeToLive)
                    DestroyBullet(true);
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            //Determine if we want to see explosion on bullet death - no explosion when hitting out-of-screen boundaries
            if (other.gameObject.layer == IntConstants.LAYER_GROUND || other.gameObject.layer  == IntConstants.LAYER_PLAYER_BULLETS
                 || other.gameObject.layer  == IntConstants.LAYER_ENEMY_BULLETS)
                DestroyBullet(true);
            else if (other.gameObject.layer == IntConstants.LAYER_BOUNDARIES)
                DestroyBullet(false);
        }
        
        public void DestroyBullet(bool generateExplosion)
        {
            if (generateExplosion)
                onExplosionShow?.Invoke(this);

            ReturnToPool();
        }

        public void OnPoolGet()
        {
        }

        public void OnPoolRelease()
        {
        }

        public void ReturnToPool()
        {
            onPoolReturning?.Invoke(this);
        }
    }
}
