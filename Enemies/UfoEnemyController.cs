using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.GameObjects.Bullets;
using Scripts.Utils;
using UnityEngine;

namespace Scripts.GameObjects.Enemies
{
    /// <summary>
    /// Enemy UFO class which is using precomputed movement using DoTween tweening engine
    /// </summary>
    public class UfoEnemyController : EnemyController
    {
        private bool _isShooting = true;

        [SerializeField]
        private RangedFloat _movingDistanceX;
        [SerializeField]
        private RangedFloat _movingDistanceY;
        [SerializeField]
        private RangedFloat _movingDuration;
        [SerializeField]
        private RangedFloat _goalPointRangeX;
        [SerializeField]
        private RangedFloat _goalPointRangeY;
        [SerializeField]
        private RangedFloat _maxTimeOnScene;
        [SerializeField]
        private Ease _movementEasing = Ease.Linear;

        private Tween _currentTween;
        private Coroutine _shootCoroutine;
        private float _distanceX;
        private float _distanceY;
        private float _duration;
        private float _timeToBeAlive;
        private float _currentTimeAlive;
        private Vector2 _firstGoalPos;

        void Update()
        {
            //when being on patrol, count the time being on the scene and eventually fly away
            if (_isShooting)
            {
                _currentTimeAlive += Time.deltaTime;
                if (_currentTimeAlive >= _timeToBeAlive)
                {
                    StopShootingCoroutine();
                    _isShooting = false;
                    FlyAway();
                }
            }
        }

        /// <summary>
        /// Assign start pos to the enemy and precompute movement constants
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="bFactory"></param>
        public override void Init(Vector2 startPos, BulletFactory bFactory)
        {
            base.Init(startPos, bFactory);
            _isShooting = false;
            _distanceX = Random.Range(_movingDistanceX.min, _movingDistanceX.max);
            _distanceY = Random.Range(_movingDistanceY.min, _movingDistanceY.max);
            _duration = Random.Range(_movingDuration.min, _movingDuration.max);
            _firstGoalPos = new Vector2(Random.Range(_goalPointRangeX.min, _goalPointRangeX.max), Random.Range(_goalPointRangeY.min, _goalPointRangeY.max));
            _timeToBeAlive = Random.Range(_maxTimeOnScene.min, _maxTimeOnScene.max);
            _currentTimeAlive = 0;

            GoToFirstPoint();
        }

        /// <summary>
        /// Enemy first goes straight to the first point in game area and then starts patrol sequence
        /// </summary>
        private void GoToFirstPoint()
        {
            _currentTween = _rb.DOMove(_firstGoalPos, 2f).SetEase(_movementEasing).OnComplete(StartPatrolSequence);
        }

        /// <summary>
        /// Patrol sequence programming - everything done through DoTween, Unity tweening framework. No real AI used.
        /// DoTween reference: http://dotween.demigiant.com/documentation.php
        /// </summary>
        private void StartPatrolSequence()
        {
            _currentTween = DOTween.Sequence();
            ((Sequence)_currentTween).Append(
                _rb.DOMove(new Vector2(_distanceX / 2, _distanceY), _duration / 2).SetRelative().SetEase(_movementEasing));
            ((Sequence)_currentTween).Append(
                _rb.DOMove(new Vector2(_distanceX / 2, -_distanceY), _duration / 2).SetRelative().SetEase(_movementEasing));
            ((Sequence)_currentTween).Append(
               _rb.DOMove(new Vector2(-_distanceX / 2, -_distanceY), _duration / 2).SetRelative().SetEase(_movementEasing));
            ((Sequence)_currentTween).Append(
                _rb.DOMove(new Vector2(-_distanceX / 2, _distanceY), _duration / 2).SetRelative().SetEase(_movementEasing));
            ((Sequence)_currentTween).SetLoops(-1, LoopType.Restart);

            _isShooting = true;
            _shootCoroutine = StartCoroutine(DoTheShooting());
        }

        /// <summary>
        /// After certain time enemy will fly away and return itself to the pool
        /// </summary>
        private void FlyAway() {
            if (_currentTween != null)
                _currentTween.Kill();
            
            _currentTween = _rb.DOMoveY(4f, 2f).SetRelative().SetEase(_movementEasing).OnComplete(ReturnToPool);
        }

        /// <summary>
        /// Shoot in certain intervals - coroutine
        /// </summary>
        private IEnumerator DoTheShooting()
        {
            while (_isShooting)
            {
                yield return new WaitForSeconds(_shootInterval);
                ShootTheBullet();
            }
        }

        private void StopShootingCoroutine()
        {
            if (_shootCoroutine != null)
            {
                StopCoroutine(_shootCoroutine);
                _shootCoroutine = null;
            }
        }

        public override void OnPoolRelease()
        {
            base.OnPoolRelease();
            if (_currentTween.IsActive())
                _currentTween.Kill();

            StopShootingCoroutine();
            _currentTween = null;
        }
    }
}
