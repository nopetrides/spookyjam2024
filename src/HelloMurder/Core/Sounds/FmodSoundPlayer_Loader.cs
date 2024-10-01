using Murder.Core.Sounds;
using Murder.Data;
using Murder.Diagnostics;
using Murder.Serialization;
using HelloMurder.Core.Sounds.Fmod;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace HelloMurder.Core.Sounds
{
    /// <summary>
    /// This loads library into the process. I guess this is XPlat??
    /// </summary>
    public partial class FmodSoundPlayer
    {
        private readonly object _banksLock = new();

        private ImmutableArray<EventDescription>? _cacheEventDescriptions = default;
        private ImmutableDictionary<SoundEventId, EventDescription>? _cacheEventDescriptionDictionary = default;

        private ImmutableHashSet<Bus>? _cacheBuses = default;

        private ImmutableArray<ParameterId>? _cacheParameters = default;
        private ImmutableArray<ParameterId>? _cacheLocalParameters = default;
        private readonly Dictionary<ParameterId, ImmutableArray<ParameterLabel>> _cacheParameterInfo = new();

        private const string DESKTOP = "Desktop";

        /// <summary>
        /// Apparently, fmod will return ERR_INTERNAL if we load any banks before the main one.
        /// So we filter this one out afterwards. Anything that has "master" in the name will be considered a priority.
        /// </summary>
        private const string MAIN_BANK = "Master";

        private void LoadFmodAssemblies()
        {
            string platform = "pc";

            string fmodPath = Path.Join(_resourcesFullPath, "fmod", platform);
            if (!Directory.Exists(fmodPath))
            {
                GameLogger.Error("Unable to find the library for fmod. Sounds will not be loaded.");
                return;
            }

            // This resolves the assembly when using the logger.
            NativeLibrary.SetDllImportResolver(typeof(HelloMurderGame).Assembly,
                (name, assembly, dllImportSearchPath) =>
                {
                    name = Path.GetFileNameWithoutExtension(name);
                    if (dllImportSearchPath is null)
                    {
                        dllImportSearchPath = DllImportSearchPath.ApplicationDirectory;
                    }

                    return NativeLibrary.Load(Path.Join(fmodPath, GetLibraryName(name)));
                });
        }

        private string GetLibraryName(string name, bool loadLogOnDebug = true)
        {
            bool isLoggingEnabled = loadLogOnDebug;

#if !DEBUG
            isLoggingEnabled = false;
#endif

            if (OperatingSystem.IsWindows())
            {
                return isLoggingEnabled ? $"{name}L.dll" : $"{name}.dll";
            }
            else if (OperatingSystem.IsLinux())
            {
                return isLoggingEnabled ? $"lib{name}L.so" : $"lib{name}.so";
            }
            else if (OperatingSystem.IsMacOS())
            {
                return isLoggingEnabled ? $"lib{name}L.dylib" : $"lib{name}.dylib";
            }

            // TODO: Support consoles?
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// This loads any DPS plugin libraries that will be dynamically loaded
        /// by the banks.
        /// </summary>
        /// <param name="coreSystem">
        /// Core system for fmod, returned by <see cref="FMOD.Studio.System.GetCoreSystem(out FMOD.System)"/>.
        /// </param>
        private void LoadPluginModules(PackedSoundData data)
        {
            Debug.Assert(_studio is not null);

            string resourcesPath = Path.Join(_resourcesFullPath, _bankRelativeToResourcesPath);
            string extension = FileHelper.ExtensionForOperatingSystem();

            ImmutableArray<string> pluginPaths = data.Plugins;
            foreach (string pluginPath in pluginPaths)
            {
                if (!pluginPath.EndsWith(extension))
                {
                    continue;
                }

                string fullPath = Path.Join(resourcesPath, FileHelper.EscapePath(pluginPath));
                _studio.LoadPluginModule(pluginPath);
            }
        }

        /// <summary>
        /// Fetch and load all the banks from <paramref name="data"/>.
        /// </summary>
        public async Task FetchBanksAsync(PackedSoundData? data)
        {
            lock (_banksLock)
            {
                Debug.Assert(_studio is not null);

                if (_banks.Count > 0)
                {
                    // Banks were already loaded. In that case, start by unloading and starting fresh.
                    UnloadContent();
                }
            }

            if (data is not null)
            {
                // Load any plugin modules that will be loaded in *.banks.
                LoadPluginModules(data);
            }

            ImmutableDictionary<string, List<string>> sounds = data?.Banks ?? FetchAllBanks();

            List<string> orderedBankPaths = sounds[DESKTOP];

            string resourcesPath = Path.Join(_resourcesFullPath, _bankRelativeToResourcesPath);

            Dictionary<SoundEventId, Bank> builder = new();
            foreach (string relativeBankPath in orderedBankPaths)
            {
                string fullBankPath = Path.Join(resourcesPath, FileHelper.EscapePath(relativeBankPath));
                if (!File.Exists(fullBankPath))
                {
                    continue;
                }

                Bank bank = await _studio.LoadBankAsync(fullBankPath);

                // bank.LoadSampleData();
                builder.Add(bank.Id, bank);
            }

            lock (_banksLock)
            {
                _banks = builder;
            }

            // Clean the cache once the new banks have been loaded.
            ClearCache();
        }

        public ImmutableDictionary<string, List<string>> FetchAllBanks()
        {
            string path = Path.Join(_resourcesFullPath, _bankRelativeToResourcesPath);
            if (!Directory.Exists(path))
            {
                // Skip loading sounds.
                return ImmutableDictionary<string, List<string>>.Empty;
            }

            IEnumerable<string> paths = Directory.EnumerateFiles(path, "*.bank", SearchOption.AllDirectories)
                .OrderByDescending(p => p.Contains(MAIN_BANK, StringComparison.InvariantCultureIgnoreCase));

            var builder = ImmutableDictionary.CreateBuilder<string, List<string>>();
            foreach (string p in paths)
            {
                string key = DESKTOP;

                if (!builder.TryGetValue(key, out List<string>? l))
                {
                    l = [];
                    builder[key] = l;
                }

                string trimPath = Path.GetRelativePath(relativeTo: path, p);
                l.Add(trimPath);
            }

            return builder.ToImmutable();
        }

        public ImmutableArray<string> FetchAllPlugins()
        {
            string fmodPluginPath = Path.Join(_resourcesFullPath, "fmod", "plugins");
            if (!Directory.Exists(fmodPluginPath))
            {
                // No plugin path found, simply skip.
                return [];
            }

            return Directory.EnumerateFiles(fmodPluginPath, "*", SearchOption.AllDirectories).ToImmutableArray();
        }

        [MemberNotNullWhen(true, nameof(_cacheEventDescriptions))]
        [MemberNotNullWhen(true, nameof(_cacheEventDescriptionDictionary))]
        private bool InitializeEvents()
        {
            lock (_banksLock)
            {
                if (_banks.Count == 0)
                {
                    return false;
                }

                if (_cacheEventDescriptions is null)
                {
                    var builder = ImmutableArray.CreateBuilder<EventDescription>();

                    foreach (Bank bank in _banks.Values)
                    {
                        builder.AddRange(bank.FetchEvents());
                    }

                    _cacheEventDescriptions = builder.ToImmutable();
                }

                if (_cacheEventDescriptionDictionary is null)
                {
                    var dictionaryBuilder = ImmutableDictionary.CreateBuilder<SoundEventId, EventDescription>();

                    foreach (EventDescription e in _cacheEventDescriptions)
                    {
                        dictionaryBuilder[e.Id] = e;
                    }

                    _cacheEventDescriptionDictionary = dictionaryBuilder.ToImmutable();
                }

                return true;
            }
        }

        /// <summary>
        /// This fetches all the events currently available in the loaded banks by the 
        /// sound player.
        /// </summary>
        public ImmutableArray<EventDescription> ListAllEvents()
        {
            if (_cacheEventDescriptions is null && !InitializeEvents())
            {
                // Likely it has not been loaded yet.
                return ImmutableArray<EventDescription>.Empty;
            }

            return _cacheEventDescriptions.Value;
        }

        public bool HasEvent(SoundEventId sound)
        {
            if (_cacheEventDescriptionDictionary is null && !InitializeEvents())
            {
                // Likely it has not been loaded yet.
                return false;
            }

            return _cacheEventDescriptionDictionary.ContainsKey(sound);
        }

        public bool HasBus(SoundEventId busSoundEventId)
        {
            ImmutableHashSet<Bus>? buses = _cacheBuses;
            if (buses is null)
            {
                buses = ListAllBuses();
            }

            foreach (Bus b in buses)
            {
                if (busSoundEventId.Equals(b.Id))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This fetches all the events currently available in the loaded banks by the 
        /// sound player.
        /// </summary>
        public ImmutableHashSet<Bus> ListAllBuses()
        {
            if (_cacheBuses is null)
            {
                var builder = ImmutableHashSet.CreateBuilder<Bus>(BusComparer.Default);
                foreach (Bank bank in _banks.Values)
                {
                    foreach (Bus bus in bank.FetchBuses())
                    {
                        builder.Add(bus);
                    }
                }

                _cacheBuses = builder.ToImmutable();
            }

            return _cacheBuses;
        }

        public ImmutableArray<ParameterId> ListAllParameters()
        {
            ImmutableArray<ParameterId> global = ListAllGlobalParameters();
            ImmutableArray<ParameterId> locals = ListAllLocalParameters();

            return global.AddRange(locals);
        }

        /// <summary>
        /// This fetches all the global events currently available in the loaded banks by the 
        /// sound player.
        /// </summary>
        public ImmutableArray<ParameterId> ListAllGlobalParameters()
        {
            if (_cacheParameters is null)
            {
                var builder = ImmutableArray.CreateBuilder<ParameterId>();

                HashSet<ParameterId> listedParameters = new();

                ImmutableArray<EventDescription> events = ListAllEvents();
                foreach (EventDescription e in events)
                {
                    ImmutableArray<ParameterId> eventParameters = e.FetchParameters(withFlags: ParameterFlags.Global);
                    builder.AddRange(eventParameters);

                    foreach (ParameterId parameter in eventParameters)
                    {
                        listedParameters.Add(parameter);
                    }
                }

                if (_studio?.FetchParameters() is ImmutableArray<ParameterId> studioParameters)
                {
                    foreach (ParameterId parameter in studioParameters)
                    {
                        if (listedParameters.Contains(parameter))
                        {
                            // Skip global parameters from events.
                            continue;
                        }

                        builder.Add(parameter);
                    }
                }

                _cacheParameters = builder.ToImmutable();
            }

            return _cacheParameters ?? ImmutableArray<ParameterId>.Empty;
        }

        public FMOD.Studio.PARAMETER_DESCRIPTION? FetchParameterDescription(ParameterId id)
        {
            if (_cacheEventDescriptionDictionary is null && !InitializeEvents())
            {
                // Likely it has not been loaded yet.
                return null;
            }

            if (id.Owner is not SoundEventId sound)
            {
                return _studio?.FetchParameterDescription(id);
            }

            if (!_cacheEventDescriptionDictionary.TryGetValue(sound, out EventDescription? @event))
            {
                return null;
            }

            return @event.FetchParameterDescription(id);
        }

        public ImmutableArray<ParameterLabel> ListAllParameterLabels(ParameterId id)
        {
            if (_cacheParameterInfo.TryGetValue(id, out ImmutableArray<ParameterLabel> result))
            {
                return result;
            }

            result = ImmutableArray<ParameterLabel>.Empty;

            ImmutableArray<EventDescription> events = ListAllEvents();
            foreach (EventDescription e in events)
            {
                result = e.FetchLabelsForParameter(id);
                if (result.Length != 0)
                {
                    // Found it!
                    break;
                }
            }

            _cacheParameterInfo[id] = result;
            return result;
        }

        public ImmutableArray<ParameterId> ListAllLocalParameters()
        {
            if (_cacheLocalParameters is null)
            {
                var builder = ImmutableArray.CreateBuilder<ParameterId>();

                ImmutableArray<EventDescription> events = ListAllEvents();
                foreach (EventDescription e in events)
                {
                    builder.AddRange(e.FetchParameters(withoutFlags: ParameterFlags.Global));
                }

                _cacheLocalParameters = builder.ToImmutable();
            }

            return _cacheLocalParameters ?? ImmutableArray<ParameterId>.Empty;
        }

        public IEnumerable<SoundIdentifier> ListAllActiveInstances()
        {
            return _instances.Keys;
        }

        public Dictionary<SoundLayer, HashSet<SoundIdentifier>> ListAllSoundLayers()
        {
            return _layers;
        }

        public Dictionary<SoundIdentifier, Vector3> GetAllSpatialEvents()
        {
#if DEBUG
            return _spatialEvents;
#else
            return [];
#endif
        }
    }
}
