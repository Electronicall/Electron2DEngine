﻿using Electron2D.ECS;
using System.Numerics;

namespace Electron2D.Audio
{
    public class AudioSpatializerSystem : BaseSystem<Component> { }
    public class AudioSpatializer : Component
    {
        public List<AudioInstance> AudioInstances { get; private set; } = new List<AudioInstance>();
        public float MinRange { get; set; } = 100f;
        public float MaxRange { get; set; } = Display.REFERENCE_WINDOW_WIDTH * 0.5f;
        public float PanningSpatializationMultiplier { get; set; } = 1.0f;
        public float VolumeSpatializationMultiplier { get; set; } = 1.0f;
        public bool Is3D { get; }
        public Curve FalloffCurve { get; set;  }
        public float DirectionBasedPanning { get; private set; }
        public float DistanceBasedVolumeMultiplier01 { get; private set; }

        private Transform transform;

        public AudioSpatializer(bool _is3D, AudioInstance _audioInstance, Curve _falloffCurve = null)
        {
            Is3D = _is3D;

            AddAudioInstance(_audioInstance);

            FalloffCurve = _falloffCurve;
            AudioSpatializerSystem.Register(this);
        }

        public AudioSpatializer(bool _is3D, AudioInstance[] _audioInstances, Curve _falloffCurve = null)
        {
            Is3D = _is3D;

            for (int i = 0; i < _audioInstances.Length; i++)
            {
                AddAudioInstance(_audioInstances[i]);
            }

            FalloffCurve = _falloffCurve;
            AudioSpatializerSystem.Register(this);
        }

        public AudioSpatializer(bool _is3D, Curve _falloffCurve = null)
        {
            Is3D = _is3D;

            FalloffCurve = _falloffCurve;
            AudioSpatializerSystem.Register(this);
        }

        public void AddAudioInstance(AudioInstance _audioInstance)
        {
            if(!AudioInstances.Contains(_audioInstance))
            {
                AudioInstances.Add(_audioInstance);
                if(Is3D)
                {
                    _audioInstance.Stream = _audioInstance.AudioClip.GetNewStream(_audioInstance, true);
                }
                _audioInstance.SetSpatializerReference(this);
            }
        }

        public void RemoveAudioInstance(AudioInstance _audioInstance)
        {
            if (AudioInstances.Contains(_audioInstance))
            {
                AudioInstances.Remove(_audioInstance);
                _audioInstance.SetSpatializerReference(null);
            }
        }

        public override void Update()
        {
            CalculateDistanceMultiplier();
            CalculatePanning();

            for (int i = 0; i < AudioInstances.Count; i++)
            {
                AudioInstance instance = AudioInstances[i];
                instance.VolumeMultiplier = DistanceBasedVolumeMultiplier01;
                instance.PanningAdditive = DirectionBasedPanning;
            }
        }

        private void CalculateDistanceMultiplier()
        {
            float rawDistance = Vector2.Distance(AudioSpatialListener.Instance.GetPosition(), transform.Position);
            DistanceBasedVolumeMultiplier01 = 1 - (MathEx.Clamp01((rawDistance / (MaxRange - MinRange)) + (MinRange / MaxRange)) * MathEx.Clamp(VolumeSpatializationMultiplier, 0, 1));
        }

        private void CalculatePanning()
        {
            DirectionBasedPanning = ((transform.Position.X - AudioSpatialListener.Instance.GetPosition().X) / (Display.REFERENCE_WINDOW_WIDTH * 0.5f)) * MathEx.Clamp(PanningSpatializationMultiplier, 0, 10);
        }

        public override void OnAdded()
        {
            transform = GetComponent<Transform>();
        }

        protected override void OnDispose()
        {
            AudioSpatializerSystem.Unregister(this);
        }
    }
}
