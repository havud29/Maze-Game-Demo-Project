using System;
using AsyncGameObjectsDependency.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script
{
    public class ThornTrap : ADMonoBehaviour
    {
        [ADDependencyInject] private GameManager _gameManager;
        
        [SerializeField] private Transform _thornTrapRoot;

        public bool IsDeathTrigger = false;

        protected override async void ADOnFilledDependencies()
        {
            base.ADOnFilledDependencies();
            
            IsDeathTrigger = false;
            
            await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(0f,2f)));
            
            Sequence seq = DOTween.Sequence();
            seq.Append(_thornTrapRoot.DOMoveY(-1, 1))
                .Insert(0.75f, DOTween.Sequence().AppendCallback(OnReachedBottom))
                .AppendInterval(1)
                .Append(_thornTrapRoot.DOMoveY(0, 1))
                .AppendCallback(OnReachedTop)
                .AppendInterval(1)
                .SetLoops(-1);
        }


        private void OnReachedBottom()
        {
            IsDeathTrigger = false;
        }

        private void OnReachedTop()
        {
            IsDeathTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && IsDeathTrigger)
            {
                _gameManager.HittedTrap();
            }
        }
    }
}