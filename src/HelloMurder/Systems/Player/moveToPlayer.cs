using Bang.Contexts;
using Bang.Systems;
using HelloMurder.Components;
using Murder.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HelloMurder.Systems.Player
{
    [Filter (typeof(PlayerComponent))]
    internal class moveToPlayer : IFixedUpdateSystem
    {
        public void FixedUpdate(Context context)
        {
            

            if (!context.HasAnyEntity)
                return;
            var player = context.World.TryGetUniqueEntity<PlayerComponent>();   
                
            if (player == null)
            return;

            var playerPos = player.GetGlobalTransform().ToVector2();

            foreach (var enemy in context.Entities)
            {
                var enemyPos = enemy.GetGlobalTransform().ToVector2();
                var dirtoplayer = (playerPos - enemyPos).Normalized();
                enemy.SetAgentImpulse(dirtoplayer);
            }
        }
    }
}
