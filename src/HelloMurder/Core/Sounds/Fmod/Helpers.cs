using FMOD.Studio;
using Murder;
using Murder.Core.Sounds;
using Murder.Diagnostics;
using Murder.Helpers;
using Murder.Utilities;
using System.Numerics;

namespace HelloMurder.Core.Sounds.Fmod
{
    public static class FmodHelpers
    {
        internal static bool Check(FMOD.RESULT r, string? message = null)
        {
            if (r != FMOD.RESULT.OK)
            {
                GameLogger.Fail(message ?? "Error on fmod operation.");
                return false;
            }

            return true;
        }

        internal static FMOD.GUID ToFmodGuid(this SoundEventId id) =>
            new FMOD.GUID { Data1 = id.Data1, Data2 = id.Data2, Data3 = id.Data3, Data4 = id.Data4 };

        internal static SoundEventId ToSoundId(this FMOD.GUID id) =>
            new SoundEventId { Data1 = id.Data1, Data2 = id.Data2, Data3 = id.Data3, Data4 = id.Data4 };

        internal static FMOD.Studio.PARAMETER_ID ToFmodId(this ParameterId id) =>
            new FMOD.Studio.PARAMETER_ID { Data1 = id.Data1, Data2 = id.Data2 };

        public static ParameterId ToParameterId(this FMOD.Studio.PARAMETER_DESCRIPTION description, SoundEventId? owner = null) =>
            new ParameterId { Data1 = description.Id.Data1, Data2 = description.Id.Data2, Name = description.Name, Owner = owner, IsGlobal = description.Flags.HasFlag(PARAMETER_FLAGS.GLOBAL) };

        /// <summary>
        /// [WARNING] Does this make sense!? I am not sure. It depends on how FMOD 3D works.
        /// </summary>
        public static Vector3 ToVector(this FMOD.VECTOR v) => new(x: v.x, y: v.z, z: -v.y);

        /// <summary>
        /// Use XZY coordinates when translating this to 3D.
        /// </summary>
        public static FMOD.VECTOR ToFmodVector(this Vector3 v) => new() { x = v.X, y = v.Z, z = -v.Y };

        /// <summary>
        /// Use XZ coordinates when translating this to 2D.
        /// </summary>
        public static FMOD.VECTOR ToFmodVector(this Vector2 v) => new() { x = v.X, y = 0, z = -v.Y };

        /// <summary>
        /// Use XZ coordinates when translating this to 2D.
        /// </summary>
        public static Vector2 ToVector2(this FMOD.VECTOR v) => new(x: v.x, y: v.z);

        public static FMOD.ATTRIBUTES_3D ToFmodAttributes(this SoundSpatialAttributes attributes) =>
            new FMOD.ATTRIBUTES_3D
            {
                // Position in world space used for panning and attenuation.
                position = (attributes.Position / Game.Grid.HalfCellSize).ToFmodVector(), // I think that the current unit is too huge for fmod. I tried multiplying it by .1?
                // Velocity in world space used for doppler. Distance units per second.
                velocity = attributes.Velocity.ToFmodVector(),
                // Forwards orientation, must be of unit length (1.0) and perpendicular to up.
                forward = attributes.Direction.ToVector().Normalized().ToFmodVector(),
                // Upwards orientation, must be of unit length (1.0) and perpendicular to forward.
                // Since this is not 3D, leave it as the default value.
                up = new FMOD.VECTOR() { y = 1 }
            };

        /// <summary>
        /// Default valid attributes 3D parameter.
        /// </summary>
        public static FMOD.ATTRIBUTES_3D Default =>
            new FMOD.ATTRIBUTES_3D
            {
                // Forwards orientation, must be of unit length (1.0) and perpendicular to up.
                forward = new FMOD.VECTOR() { z = 1 },
                // Upwards orientation, must be of unit length (1.0) and perpendicular to forward.
                up = new FMOD.VECTOR() { y = 1 }
            };

        public static bool HasParameterFlag(this PARAMETER_FLAGS input, ParameterFlags flags)
        {
            return ((int)input & (int)flags) == (int)flags;
        }

        public static bool HasAnyParameterFlag(this PARAMETER_FLAGS input, ParameterFlags flags)
        {
            return ((int)input & (int)flags) != 0;
        }
    }
}
