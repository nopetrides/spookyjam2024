using Bang;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using HelloMurder.Components;
using HelloMurder.Core;
using HelloMurder.Messages;
using HelloMurder.Messages.LoreMessages;
using Murder.Core.Graphics;
using Murder.Services;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace HelloMurder.Systems.Interactions
{
    public enum LoreScreenSide
    {
        Left = 0,
        Right = 1,
    }

    [Filter(typeof(LoreInteractableComponent))]
    [Messager(typeof(LoreMessage))]
    internal class LoreSystem : IMessagerSystem, IMurderRenderSystem
    {
        private string _lore = "";
        private LoreScreenSide _side;

        public void Draw(RenderContext render, Context context)
        {
            if (string.IsNullOrEmpty(_lore)) 
            { 
                return;
            }

            int font = (int)MurderFonts.PixelFont;

            float offset = 0f;
            if (_side == LoreScreenSide.Left) 
            {
                offset = render.Camera.Width * 3f / 5f;
            }



            RenderServices.DrawText(
                render.UiBatch,
                font,
                _lore,
                new Vector2(x: render.Camera.Width / 5f + offset, y: 10f),
                maxWidth: render.Camera.Width / 3,
                _lore.Length,
                new DrawInfo(0.1f)
                {
                    Origin = new Vector2(.5f, 0),
                    Color = Color.Gray,//Palette.Colors[1],
                    Shadow = Palette.Colors[3],
                    Sort = 0
                });


            RenderServices.DrawRectangle(
                render.UiBatch,
                new Murder.Core.Geometry.Rectangle(5f + offset, 5f, render.Camera.Width / 3f + 10f, render.Camera.Height),
                Color.Black,
                0);
        }

        public void OnMessage(World world, Entity entity, IMessage message)
        {
            var loreMessage = (LoreMessage)message;
            if (loreMessage.state == LoreMessage.LoreState.Open)
            {
                var loreComponent = entity.GetComponent<LoreInteractableComponent>();
                _lore = loreComponent.LoreSnippet;
                _side = loreComponent.ScreenSide;
                return;
            }
            _lore = "";
        }
    }
}
