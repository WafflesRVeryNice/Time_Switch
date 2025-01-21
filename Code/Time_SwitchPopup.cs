using System;
using Microsoft.Xna.Framework.Input;
using Celeste;
using On.Celeste;
using On.Celeste.Mod;
using IL.Celeste;
using static MonoMod.InlineRT.MonoModRule;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using static Celeste.Mod.Time_Switch.Time_SwitchModuleSettings;
using Celeste.Mod.Entities;
using static Celeste.Mod.Time_Switch.Time_Switch;
using System.Collections;
using System.Linq;


namespace Celeste.Mod.Time_Switch;


//+++Stolen from Speedrun tool+++ https://github.com/DemoJameson/Celeste.SpeedrunTool/blob/master/SpeedrunTool/Source/Message/Tooltip.cs#L55
public class Popup : Entity
{
    private const int Padding = 25;
    private readonly string message;
    private float alpha;
    private float unEasedAlpha;
    private readonly float duration;

    private Popup(string message, float duration = 1f)
    {
        this.message = message;
        this.duration = duration;
        Vector2 messageSize = ActiveFont.Measure(message);
        Position = new(Padding, Engine.Height - messageSize.Y - Padding / 2f);
        Tag = Tags.HUD | Tags.Global | Tags.FrozenUpdate | Tags.PauseUpdate | Tags.TransitionUpdate;
        Add(new Coroutine(FadeIn()));
    }

    private IEnumerator FadeIn()
    {
        while (alpha < 1f)
        {
            unEasedAlpha = Calc.Approach(unEasedAlpha, 1f, Engine.RawDeltaTime * 5f);
            alpha = Ease.SineOut(unEasedAlpha);
            yield return null;
        }

        yield return FadeOut();
    }

    private IEnumerator FadeOut()
    {
        yield return duration;
        while (alpha > 0f)
        {
            unEasedAlpha = Calc.Approach(unEasedAlpha, 0f, Engine.RawDeltaTime * 5f);
            alpha = Ease.SineIn(unEasedAlpha);
            yield return null;
        }

        RemoveSelf();
    }

    public override void Render()
    {
        base.Render();
        ActiveFont.DrawOutline(message, Position, Vector2.Zero, Vector2.One, Color.White * alpha, 2,
            Color.Black * alpha * alpha * alpha);
    }

    public static void Show(string message, float duration = 1f)
    {
        if (Engine.Scene is { } scene)
        {
            if (!scene.Tracker.Entities.TryGetValue(typeof(Popup), out var tooltips))
            {
                tooltips = scene.Entities.FindAll<Popup>().Cast<Entity>().ToList();
            }
            tooltips.ForEach(entity => entity.RemoveSelf());
            scene.Add(new Popup(message, duration));
        }
    }
}
//+++
