using ImGuiNET;
using Murder;
using Murder.Core.Sounds;
using Murder.Editor.ImGuiExtended;
using Murder.Services;
using Murder.Utilities;
using HelloMurder.Core.Sounds;

namespace HelloMurder.Editor.Utilities
{
    internal static class SearchBoxExtensions
    {
        public static SoundEventId? SearchFmodSounds(string id, SoundEventId initial)
        {
            string selected = "Select a sound";

            if (!string.IsNullOrEmpty(initial.Path))
            {
                selected = initial.EditorName;
            }

            SearchBox.SearchBoxSettings<SoundEventId> searchSettings = new SearchBox.SearchBoxSettings<SoundEventId>(selected);

            if (Game.Instance.SoundPlayer is not FmodSoundPlayer player)
            {
                return initial;
            }

            Lazy<Dictionary<string, SoundEventId>> candidates = new(CollectionHelper.ToStringDictionary(
                player.ListAllEvents().Select(v => v.Id), v => v.EditorName, v => v));

            if (ImGuiHelpers.IconButton('', $"play_sound_{id}"))
            {
                if (Game.Preferences.SoundVolume == 0 || Game.Preferences.MusicVolume == 0)
                {
                    // Make sure all our sounds are on here, so there is no confusion.
                    Game.Preferences.SetSoundVolume(1);
                    Game.Preferences.SetMusicVolume(1);
                }

                SoundServices.StopAll(fadeOut: false);
                _ = SoundServices.Play(initial, properties:SoundProperties.Persist);
            }

            ImGui.SameLine();

            if (ImGuiHelpers.IconButton('\uf04c', $"stop_sound_{id}"))
            {
                SoundServices.StopAll(fadeOut: false);
            }

            ImGui.SameLine();

            if (SearchBox.Search(
                id: $"search_sound##{id}", settings: searchSettings, values: candidates, flags: SearchBoxFlags.None, result: out SoundEventId chosen))
            {
                return chosen;
            }

            return default;
        }

        public static SoundEventId? SearchFmodBuses(string id, SoundEventId initial)
        {
            string selected = "Select a bus";

            if (!string.IsNullOrEmpty(initial.Path))
            {
                selected = initial.EditorName;
            }

            SearchBox.SearchBoxSettings<SoundEventId> searchSettings = new SearchBox.SearchBoxSettings<SoundEventId>(selected);

            if (Game.Instance.SoundPlayer is not FmodSoundPlayer player)
            {
                return initial;
            }

            Lazy<Dictionary<string, SoundEventId>> candidates = new(CollectionHelper.ToStringDictionary(
                player.ListAllBuses(), v => v.Path, v => v.Id));

            if (SearchBox.Search(
                id: $"search_sound##{id}", settings: searchSettings, values: candidates, flags: SearchBoxFlags.None, result: out SoundEventId chosen))
            {
                return chosen;
            }

            return default;
        }

        public static ParameterId? SearchFmodLocalParameters(string id, ParameterId initial)
        {
            string selected = "\uf002 Select local parameter";

            if (!string.IsNullOrEmpty(initial.EditorName))
            {
                selected = initial.EditorName;
            }

            SearchBox.SearchBoxSettings<ParameterId> searchSettings = new SearchBox.SearchBoxSettings<ParameterId>(selected);

            if (Game.Instance.SoundPlayer is not FmodSoundPlayer player)
            {
                return initial;
            }

            Lazy<Dictionary<string, ParameterId>> candidates = new(CollectionHelper.ToStringDictionary(
                player.ListAllLocalParameters(), v => v.EditorName, v => v));

            if (SearchBox.Search(
                id: $"search_local_sound##{id}", settings: searchSettings, values: candidates, flags: SearchBoxFlags.None, out ParameterId chosen))
            {
                return chosen;
            }

            return default;
        }

        public static ParameterId? SearchFmodGlobalParameters(string id, ParameterId initial)
        {
            string selected = "\uf002 Select global parameter";

            if (!string.IsNullOrEmpty(initial.Name))
            {
                selected = initial.Name;
            }

            SearchBox.SearchBoxSettings<ParameterId> searchSettings = new SearchBox.SearchBoxSettings<ParameterId>(selected);

            if (Game.Instance.SoundPlayer is not FmodSoundPlayer player)
            {
                return initial;
            }

            Lazy<Dictionary<string, ParameterId>> candidates = new(CollectionHelper.ToStringDictionary(
                player.ListAllGlobalParameters(), v => v.Name!, v => v));

            if (SearchBox.Search(
                id: $"search_global_sound##{id}", settings: searchSettings, values: candidates, flags: SearchBoxFlags.None, out ParameterId chosen))
            {
                return chosen;
            }

            return default;
        }

        public static ParameterId? SearchFmodParameters(string id, ParameterId initial)
        {
            string selected = "\uf002 Select global or local parameter";

            if (!string.IsNullOrEmpty(initial.EditorName))
            {
                selected = initial.EditorName;
            }

            SearchBox.SearchBoxSettings<ParameterId> searchSettings = new SearchBox.SearchBoxSettings<ParameterId>(selected);

            if (Game.Instance.SoundPlayer is not FmodSoundPlayer player)
            {
                return initial;
            }

            Lazy<Dictionary<string, ParameterId>> candidates = new(CollectionHelper.ToStringDictionary(
                player.ListAllParameters(), v => v.EditorName!, v => v));

            if (SearchBox.Search(
                id: $"search_all_sound##{id}", settings: searchSettings, values: candidates, flags: SearchBoxFlags.None, out ParameterId chosen))
            {
                return chosen;
            }

            return default;
        }
    }
}
