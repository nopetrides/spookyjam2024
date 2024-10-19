using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder.Core.Graphics;
using Murder.Core.Geometry;
using Murder.Core;
using Murder.Components;
using HelloMurder.Components;
using Murder.Utilities;
using System.Numerics;
using Murder;

namespace HelloMurder.Systems
{
    [Filter(kind: ContextAccessorKind.Read, typeof(CameraFollowComponent), typeof(IMurderTransformComponent))]
    internal class CameraFollowSystem : IFixedUpdateSystem, IUpdateSystem, IStartupSystem
    {
        bool _firstStarted = false;
        private List<RoomMap> _roomCache = new List<RoomMap>();
        private RoomMap? _currentRoom;

        public void Start(Context context)
        {
            _roomCache = context.World.GetEntitiesWith(typeof(RoomComponent), typeof(TileGridComponent))
                    .Select(r => r.GetComponent<TileGridComponent>())
                    .Select(r => new RoomMap(
                        r.Origin.ToVector2() * Grid.CellSize,
                        new Vector2(r.Origin.X + r.Width, r.Origin.Y + r.Height) * Grid.CellSize))
                    .ToList();
        }

        public void Update(Context context)
        {
            var trackedEntity = context.World.TryGetUniqueEntity<PlayerComponent>();
            if (trackedEntity is null) return;

            var targetPosition = trackedEntity.GetGlobalTransform().Vector2;
            _currentRoom = _roomCache.Where(r => r.Contains(targetPosition)).OrderBy(r => r.Area).FirstOrDefault();
        }

        public void FixedUpdate(Context context)
        {
            if (!context.HasAnyEntity || context.Entity is not Entity cameraman)
                return;

            var cameraFollow = context.Entity.GetCameraFollow();
            if (!cameraFollow.Enabled)
                return;

            var camera = ((MonoWorld)context.World).Camera;
            var cameramanPosition = cameraman.GetGlobalTransform().Vector2;

            Entity? trackedEntity;
            if (context.Entity.HasIdTarget())
            {
                trackedEntity = context.World.TryGetEntity(context.Entity.GetIdTarget().Target);
            }
            else
            {
                trackedEntity = context.World.TryGetUniqueEntity<PlayerComponent>();
            }

            if (trackedEntity is not null)
            {
                Vector2 targetPosition = trackedEntity.GetGlobalTransform().Vector2;

                if (cameraFollow.SecondaryTarget is not null)
                {
                    if (cameraFollow.SecondaryTarget.IsDestroyed)
                    {
                        cameraFollow = new CameraFollowComponent(true);
                        context.Entity.SetCameraFollow(cameraFollow);
                    }
                    else
                    {
                        targetPosition = Vector2.Lerp(targetPosition, cameraFollow.SecondaryTarget.GetGlobalTransform().Vector2, 0.35f);
                    }
                }

                if (!_firstStarted)
                {
                    _firstStarted = true;
                    cameramanPosition = targetPosition;
                    cameraman.SetGlobalTransform(new PositionComponent(cameramanPosition));
                }
                else
                {
                    if (cameraFollow.Style == CameraStyle.Perfect)
                    {
                        cameraman.SetTransform(new PositionComponent(targetPosition));
                        cameramanPosition = targetPosition;
                    }
                    else
                    {
                        Point deadzone = cameraFollow.Style == CameraStyle.DeadZone ? new(24, 24) : Point.Zero;
                        var delta = (cameramanPosition - targetPosition);
                        float lerpAmount = 1 - MathF.Pow(0.1f, Game.FixedDeltaTime);
                        float lerpedX = cameramanPosition.X;
                        float lerpedY = cameramanPosition.Y;
                        if (MathF.Abs(delta.X) > deadzone.X)
                        {
                            lerpedX = Calculator.LerpSnap(cameramanPosition.X, targetPosition.X + deadzone.X * MathF.Sign(delta.X), lerpAmount, 0.2f);
                        }
                        if (MathF.Abs(delta.Y) > deadzone.Y)
                        {
                            lerpedY = Calculator.LerpSnap(cameramanPosition.Y, targetPosition.Y + deadzone.Y * MathF.Sign(delta.Y), lerpAmount, 0.2f);
                        }
                        cameramanPosition = new Vector2(lerpedX, lerpedY);
                        cameraman.SetTransform(new PositionComponent(cameramanPosition));
                    }
                }
            }

            Vector2 finalPosition = cameramanPosition - new Vector2(camera.Width, camera.Height) / 2f;

            if (cameraFollow.Style == CameraStyle.Room && _currentRoom is not null)
                finalPosition = _currentRoom.Clamp(finalPosition, camera.Bounds);
            else if (context.World.TryGetUnique<MapComponent>() is MapComponent map && map.Map != null)
                finalPosition = ClampBounds(map.Width, map.Height, camera, finalPosition);


            camera.Position = Vector2.Lerp(camera.Position, finalPosition, .2f);
        }

        private Vector2 ClampBounds(int width, int height, Camera2D camera, Vector2 position)
        {
            if (position.X < 0) position.X = 0;
            if (position.Y < 0) position.Y = 0;

            var maxWidth = width * Grid.CellSize;
            var maxHeight = height * Grid.CellSize;

            if (position.X + camera.Bounds.Width > maxWidth) position.X = maxWidth - camera.Bounds.Width;
            if (position.Y + camera.Bounds.Height > maxHeight) position.Y = maxHeight - camera.Bounds.Height;

            return position;
        }

        internal class RoomMap(Vector2 startsAt, Vector2 endsAt)
        {
            public readonly Vector2 StartsAt = startsAt;
            public readonly Vector2 EndsAt = endsAt;

            public readonly float Top = Math.Max(startsAt.Y, endsAt.Y);
            public readonly float Left = Math.Min(startsAt.X, endsAt.X);
            public readonly float Right = Math.Max(startsAt.X, endsAt.X);
            public readonly float Bottom = Math.Min(startsAt.Y, endsAt.Y);

            private float? _centerX;
            public float CenterX => _centerX ?? (_centerX = Left + Width / 2f).Value;

            private float? _centerY;
            public float CenterY => _centerY ?? (_centerY = Bottom + Height / 2f).Value;

            private Vector2? _center;
            public Vector2 RoomCenter => _center ?? (_center = new(CenterX, CenterY)).Value;

            private float? _width;
            public float Width => _width ?? (_width = Math.Abs(Right - Left)).Value;

            private float? _height;
            public float Height => _height ?? (_height = Math.Abs(Top - Bottom)).Value;

            private float? _area;
            public float Area => _area ?? (_area = Height * Width).Value;

            public bool Contains(Vector2 point) => Left <= point.X && Right >= point.X && Bottom <= point.Y && Top >= point.Y;

            public Vector2 Clamp(Vector2 point, Rectangle surround)
            {
                var clampedPosition = new Vector2(point.X, point.Y);

                if (Width < surround.Width)
                    clampedPosition.X = CenterX - surround.Width / 2f;
                else
                {
                    if (point.X < Left) clampedPosition.X = Left;
                    if (point.X + surround.Width > Right) clampedPosition.X = Right - surround.Width;
                }

                if (Height < surround.Height)
                    clampedPosition.Y = CenterY - surround.Height / 2f;
                else
                {
                    if (point.Y < Bottom) clampedPosition.Y = Bottom;
                    if (point.Y + surround.Height > Top) clampedPosition.Y = Top - surround.Height;
                }

                return clampedPosition;
            }
        }
    }
}
