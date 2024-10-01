using Murder.Core.Sounds;
using Murder.Data;
using Murder.Diagnostics;
using Murder.Serialization;
using HelloMurder.Core.Sounds.Fmod;
using System.Diagnostics;
using System.Numerics;

namespace HelloMurder.Core.Sounds
{
    /// <summary>
    /// This is the sound player, which relies on fmod.
    /// The latest version tested was "2.02.11"
    /// </summary>
    public partial class FmodSoundPlayer : ISoundPlayer, IDisposable
    {
        public record struct SoundIdentifier
        {
            public readonly SoundEventId Id;
            public readonly int EntityId = -1;

            public SoundIdentifier(SoundEventId id, int entityId) =>
                (Id, EntityId) = (id, entityId);
        }

        private readonly static string _bankRelativeToResourcesPath = Path.Join("sounds");

        private Studio? _studio;

        /// <summary>
        /// Collection of sounds per world.
        /// </summary>
        private readonly Dictionary<SoundLayer, HashSet<SoundIdentifier>> _layers = [];

        private Dictionary<SoundEventId, Bank> _banks = new();

        private readonly Dictionary<SoundEventId, Bus> _buses = new();
        private readonly Dictionary<SoundEventId, EventDescription> _events = new();

        private readonly Dictionary<SoundIdentifier, EventInstance> _instances = new();

#if DEBUG
        /// <summary>
        /// Only used for debugging!
        /// </summary>
        private readonly Dictionary<SoundIdentifier, Vector3> _spatialEvents = [];
#endif

        private bool _initialized = false;

        private string _resourcesFullPath = string.Empty;

        public void Initialize(string resourcesPath)
        {
            if (_initialized)
            {
                // This was likely called from a refresh call.
                // We simply need to make sure we are refreshing the cache.
                _ = ReloadAsync();
                return;
            }

            _resourcesFullPath = FileHelper.GetPath(resourcesPath);

            LoadFmodAssemblies();
            InitializeFmod();

            _initialized = true;
        }

        public async Task LoadContentAsync(PackedSoundData? packedData)
        {
            await FetchBanksAsync(packedData);
        }

        public async Task ReloadAsync()
        {
            UnloadContent();

            // There will be no data in a reload.
            await FetchBanksAsync(data: null);
        }

        /// <summary>
        /// This will load and initialize the fmod library so we are ready once the game starts.
        /// </summary>
        private void InitializeFmod()
        {
            // *apparently*, there is a requirement to call the core API before calling the studio API?
            // this seems to break Linux scenarios, but i haven't seen this yet.
            FMOD.Memory.GetStats(out int currentAllocated, out int maxAllocated);

            FMOD.RESULT result = FMOD.Studio.System.Create(out FMOD.Studio.System studio);
            if (!FmodHelpers.Check(result, "Unable to create the fmod factory system!")) return;

            result = studio.GetCoreSystem(out FMOD.System core);
            if (!FmodHelpers.Check(result, "Unable to acquire the core system?")) return;

            _studio = new(studio);

            FMOD.Studio.INITFLAGS studioInitFlags = FMOD.Studio.INITFLAGS.NORMAL;

#if DEBUG
            studioInitFlags |= FMOD.Studio.INITFLAGS.LIVEUPDATE;
#endif

            result = studio.Initialize(
                maxchannels: 256,
                studioflags: studioInitFlags,
                flags: FMOD.INITFLAGS.CHANNEL_LOWPASS | FMOD.INITFLAGS.CHANNEL_DISTANCEFILTER,
                extradriverdata: IntPtr.Zero);

            FmodHelpers.Check(result, "Unable to initialize fmod?");

            // okay, *this has to be called last*. I am not sure why, but things started exploding
            // (non-stream files would not play) if this was not the case <_<
            core.SetDSPBufferSize(bufferlength: 4, numbuffers: 32);
        }

        internal EventInstance? FetchOrCreateInstance(SoundEventId id)
        {
            if (!_events.TryGetValue(id, out EventDescription? description))
            {
                Debug.Assert(_studio is not null);

                description = _studio.GetEvent(id);
                if (description is not null)
                {
                    _events.Add(id, description);
                }
            }

            return description?.CreateInstance();
        }

        public void UpdateListener(SoundSpatialAttributes attributes)
        {
            if (_studio is null)
            {
                return;
            }

            bool success = _studio.SetListenerAttributes(attributes);
            GameLogger.Verify(success, $"Unable to update listener attributes?");
        }

        public bool UpdateEvent(SoundEventId id, int entityId, SoundSpatialAttributes attributes)
        {
            if (_studio is null)
            {
                return false;
            }

            SoundIdentifier soundIdentifier = new(id, entityId);
            if (!_instances.TryGetValue(soundIdentifier, out EventInstance? instance))
            {
                return false;
            }

#if DEBUG
            _spatialEvents[soundIdentifier] = attributes.Position;
#endif

            bool success = instance.Set3DAttributes(attributes);
            GameLogger.Verify(success, $"Unable to update event source attributes?");

            return success;
        }

        public void Update()
        {
            if (_studio is null)
            {
                return;
            }

            if (!_studio.ListenerInitialized)
            {
                _studio.InitializeListener();
            }

            _studio?.Update();
        }

        public ValueTask PlayEvent(SoundEventId id, PlayEventInfo info)
        {
            if (info.Properties.HasFlag(SoundProperties.Persist))
            {
                return PlayPersistedEvent(id, info);
            }

            // Otherwise, this will be played and immediately released.
            using EventInstance? scopedInstance = FetchOrCreateInstance(id);

            bool success;
            if (scopedInstance is not null && info.Attributes is not null)
            {
                success = scopedInstance.Set3DAttributes(info.Attributes.Value);
                GameLogger.Verify(success, $"Failed to set spatial sounds for {id.EditorName}.");
            }

            success = scopedInstance?.Start() ?? false;
            GameLogger.Verify(success, $"Failed to play event {id.EditorName}. Did the event id got updated?");

            return default;
        }

        private ValueTask PlayPersistedEvent(SoundEventId soundEventId, PlayEventInfo info)
        {
            SoundIdentifier id = new(soundEventId, info.EntityId);
            if (info.Properties.HasFlag(SoundProperties.SkipIfAlreadyPlaying) && _instances.ContainsKey(id))
            {
                // The sound is currently playing already and it is the same as the last one.
                // So we'll just skip playing it again.
                return default;
            }

            if (info.Properties.HasFlag(SoundProperties.StopOtherEventsInLayer))
            {
                Stop(info.Layer, fadeOut: true);
            }

            if (info.Layer != SoundLayer.Any)
            {
                if (!_layers.TryGetValue(info.Layer, out HashSet<SoundIdentifier>? layerEvents))
                {
                    layerEvents = [];
                    _layers.Add(info.Layer, layerEvents);
                }

                layerEvents.Add(id);
            }

            if (_instances.ContainsKey(id))
            {
                return default;
            }

            EventInstance? instance = FetchOrCreateInstance(soundEventId);
            if (instance is not null)
            {
                // This won't be released right away, so we will track its instance.
                _instances.Add(id, instance);
            }

            bool success;
            if (instance is not null && info.Attributes is not null)
            {
                success = instance.Set3DAttributes(info.Attributes.Value);
                GameLogger.Verify(success, $"Failed to set spatial sounds for {soundEventId.EditorName}.");
            }

            success = instance?.Start() ?? false;
            GameLogger.Verify(success, $"Failed to play event {soundEventId.EditorName}. Did the event id got updated?");

            return default;
        }

        public bool Resume(SoundLayer layer)
        {
            if (layer == SoundLayer.Any)
            {
                return ResumeAll();
            }

            if (!_layers.TryGetValue(layer, out HashSet<SoundIdentifier>? events))
            {
                return false;
            }

            foreach (SoundIdentifier e in events)
            {
                if (!_instances.TryGetValue(e, out EventInstance? instance))
                {
                    continue;
                }

                instance.Resume();
            }

            return true;
        }

        private bool ResumeAll()
        {
            bool succeeded = false;
            foreach (EventInstance instance in _instances.Values)
            {
                succeeded &= instance.Resume();
            }

            return succeeded;
        }

        public bool Pause(SoundLayer layer)
        {
            if (layer == SoundLayer.Any)
            {
                return PauseAll();
            }

            if (!_layers.TryGetValue(layer, out HashSet<SoundIdentifier>? events))
            {
                return false;
            }

            foreach (SoundIdentifier e in events)
            {
                if (!_instances.TryGetValue(e, out EventInstance? instance))
                {
                    continue;
                }

                instance.Pause();
            }

            return true;
        }

        private bool PauseAll()
        {
            bool succeeded = false;
            foreach (EventInstance instance in _instances.Values)
            {
                succeeded &= instance.Pause();
            }

            return succeeded;
        }

        public bool Stop(SoundLayer layer, bool fadeOut)
        {
            if (layer == SoundLayer.Any)
            {
                StopAll(fadeOut);
                return true;
            }

            if (!_layers.TryGetValue(layer, out HashSet<SoundIdentifier>? events))
            {
                return false;
            }

            foreach (SoundIdentifier e in events)
            {
                Stop(e.Id, e.EntityId, fadeOut);
            }

            events.Clear();
            return true;
        }

        public bool Stop(SoundEventId? soundEventId, int entityId, bool fadeOut)
        {
            if (soundEventId is null)
            {
                return StopAll(fadeOut);
            }

            SoundIdentifier id = new(soundEventId.Value, entityId);

            bool succeeded = false;
            if (_instances.TryGetValue(id, out EventInstance? instance))
            {
                succeeded &= instance.Stop(fadeOut);
                instance.Dispose();

                StopTrackingEventInLayer(id);
            }

#if DEBUG
            _spatialEvents.Remove(id);
#endif

            _instances.Remove(id);
            return succeeded;
        }

        private bool StopTrackingEventInLayer(SoundIdentifier id)
        {
            foreach ((_, var events) in _layers)
            {
                if (events.Remove(id))
                {
                    return true;
                }
            }

            return false;
        }

        private bool StopAll(bool fadeOut)
        {
            bool succeeded = false;
            foreach (EventInstance instance in _instances.Values)
            {
                succeeded &= instance.Stop(fadeOut);
                instance.Dispose();
            }

            _instances.Clear();
            _layers.Clear();

#if DEBUG
            _spatialEvents.Clear();
#endif

            return succeeded;
        }

        public void SetParameter(SoundEventId soundEventId, string name, float value)
        {
            SoundIdentifier id = new(soundEventId, entityId: -1);
            if (_instances.TryGetValue(id, out var instance))
            {
                bool success = instance.SetParameterValue(name, value);
                GameLogger.Verify(success, $"Failed to find parameter {soundEventId.EditorName}.");
            }
            else
            {
                GameLogger.Error($"Missing sound {soundEventId.Path}");
            }
        }

        public void SetParameter(SoundEventId soundEventId, ParameterId parameterId, float value)
        {
            SoundIdentifier id = new(soundEventId, entityId: -1);
            if (_instances.TryGetValue(id, out var instance))
            {
                bool success = instance.SetParameterValue(parameterId.ToFmodId(), value);
                GameLogger.Verify(success, $"Failed to find parameter {soundEventId.EditorName}.");
            }
            else
            {
                GameLogger.Error($"Missing sound {soundEventId.Path}");
            }
        }

        public void SetGlobalParameter(ParameterId parameterId, float value)
        {
            bool result = _studio?.SetParameterValue(parameterId, value) ?? false;
            if (result)
            {
                return;
            }

            // Otherwise, this may be tied to a sound? Even though it's global...?
            if (parameterId.Owner is SoundEventId soundId)
            {
                foreach ((SoundIdentifier id, EventInstance instance) in _instances)
                {
                    // TODO: We might want to be smarter and set the local parameter of the specific entity.
                    if (soundId.Equals(id.Id))
                    {
                        bool success = instance.SetParameterValue(parameterId.ToFmodId(), value);
                        GameLogger.Verify(success, $"Failed to find parameter {parameterId.EditorName}.");
                    }
                }
            }
        }

        public float GetGlobalParameter(ParameterId parameterId)
        {
            return _studio?.GetParameterCurrentValue(parameterId) ?? 0;
        }

        public void SetVolume(SoundEventId? id, float volume)
        {
            if (id is null)
            {
                GameLogger.Fail("Unable to find a null bus id.");
                return;
            }

            lock (_buses)
            {
                if (!_buses.TryGetValue(id.Value, out Bus? bus))
                {
                    bus = _studio?.GetBus(id.Value);
                    if (bus is null)
                    {
                        GameLogger.Fail("Invalid bus name!");
                        return;
                    }

                    _buses.Add(id.Value, bus);
                }

                bus.Volume = volume;
            }
        }

        private void UnloadContent()
        {
            lock (_banksLock)
            {
                ClearCache();

                foreach (Bank bank in _banks.Values)
                {
                    bank.Dispose();
                }

                _banks.Clear();
            }

            foreach (EventDescription @event in _events.Values)
            {
                @event.Dispose();
            }

            foreach (Bus bus in _buses.Values)
            {
                bus.Dispose();
            }

            foreach (EventInstance instance in _instances.Values)
            {
                instance.Dispose();
            }

            _events.Clear();
            _buses.Clear();
            _instances.Clear();
        }

        private void ClearCache()
        {
            _cacheEventDescriptions = null;
            _cacheEventDescriptionDictionary = null;

            _cacheBuses = null;

            _cacheParameters = null;
            _cacheLocalParameters = null;
            _cacheParameterInfo.Clear();
        }

        public void Dispose()
        {
            UnloadContent();

            _studio?.Dispose();
        }
    }
}
