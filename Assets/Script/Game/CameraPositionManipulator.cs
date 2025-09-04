using System;
using AsyncGameObjectsDependency.Core;
using Cysharp.Threading.Tasks;
using R3;
using Unity.Cinemachine;
using UnityEngine;
using ZeroMessenger;

namespace Script
{
    public class CameraPositionManipulator : ADMonoBehaviour
    {
        [ADDependencyInject] private MazeBuilder _mazeBuilder;
        
        [SerializeField] private CinemachineBrain _brain;
        
        [SerializeField] private CinemachineCamera menuCamera;
        [SerializeField] private CinemachineCamera startPosCamera;
        [SerializeField] private CinemachineCamera endPosCamera;
        [SerializeField] private CinemachineCamera mainGameCamera;

        protected override void ADOnFilledDependencies()
        {
            base.ADOnFilledDependencies();
            
            MessageBroker<OnFinishedBuildingMaze>.Default.Subscribe(x =>
            {
                StartGameCameraSequence().Forget();
            }).AddTo(this);
            
            MessageBroker<OnClickedOnAWorldCell>.Default.Subscribe(x =>
            {
            
            }).AddTo(this);
        }

        private async UniTask StartGameCameraSequence()
        {
            _brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
            startPosCamera.Target.TrackingTarget = _mazeBuilder.GetStartWorldCell().transform;
            endPosCamera.Target.TrackingTarget = _mazeBuilder.GetEndWorldCell().transform;
            var blendDuration = _brain.DefaultBlend.BlendTime;
            var holdDuration = 1f;
            
            await UniTask.Delay(TimeSpan.FromSeconds(holdDuration));
            
            SwitchCamera(startPosCamera);
            await UniTask.Delay(TimeSpan.FromSeconds(blendDuration));
            await UniTask.Delay(TimeSpan.FromSeconds(holdDuration));

            SwitchCamera(endPosCamera);
            await UniTask.Delay(TimeSpan.FromSeconds(blendDuration));
            await UniTask.Delay(TimeSpan.FromSeconds(holdDuration));

            SwitchCamera(mainGameCamera);
            await UniTask.Delay(TimeSpan.FromSeconds(blendDuration));

            MessageBroker<OnFinishedAnimationSequence>.Default.Publish(new OnFinishedAnimationSequence());
        }
        
        private void SwitchCamera(CinemachineCamera newCamera)
        {
            menuCamera.gameObject.SetActive(false);
            startPosCamera.gameObject.SetActive(false);
            endPosCamera.gameObject.SetActive(false);
            mainGameCamera.gameObject.SetActive(false);
            
            newCamera.gameObject.SetActive(true);
        }
    }
    
}

public struct OnFinishedAnimationSequence
{
    
}