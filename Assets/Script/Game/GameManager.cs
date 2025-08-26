using AsyncGameObjectsDependency.Core;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using ZeroMessenger;

namespace Script
{
    public class GameManager : ADMonoBehaviour
    {
        private PlayerCharacterController _playerCharacterController;
        public bool IsGameStarted = false;
        protected override void ADOnFilledDependencies()
        {
            base.ADOnFilledDependencies();
            
            MessageBroker<OnFinishedBuildingMaze>.Default.Subscribe(async x =>
            {
                await UniTask.Yield();
                IsGameStarted = true;
            }).AddTo(this);

            MessageBroker<OnResetingGame>.Default.Subscribe(async x =>
            {
                await UniTask.Yield();
                await UniTask.Yield();

                MessageBroker<OnStartedGame>.Default.Publish(new OnStartedGame());
            }).AddTo(this);
            
            DiContainer.Register(this);
        }

        public void RegisterPlayerCharacter(PlayerCharacterController playerCharacterController)
        {
            _playerCharacterController = playerCharacterController;
        }
        public void Win()
        {
            if (IsGameStarted)
            {
                IsGameStarted = false;
                _playerCharacterController.CancelMovement();
                MessageBroker<IsWinGame>.Default.Publish(new IsWinGame() {IsWin = true});
            }
        }

        public void Lose()
        {
            if (IsGameStarted)
            {
                IsGameStarted = false;
                _playerCharacterController.CancelMovement();
                MessageBroker<IsWinGame>.Default.Publish(new IsWinGame() { IsWin = false });
            }
        }

        public void HittedTrap()
        {
            _playerCharacterController.PlayerHittedTrapBehaviour();
        }
        
    }
}

public struct IsWinGame
{
    public bool IsWin;
}