using Godot;
using System;

public partial class GlassPane : Area2D
{
    private AnimationPlayer animationPlayer;
    private bool isPlayerOnSelf;
    private Sprite2D sprite;
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("Sprite2D/AnimationPlayer");
        sprite = GetNode<Sprite2D>("Sprite2D");
        sprite.SelfModulate = Color.Color8(255,255,255,0);
    }
    private void FadeIn()
    {
        if (animationPlayer.IsPlaying())
        {
            double t = animationPlayer.CurrentAnimationPosition;
            t = t * (animationPlayer.GetAnimation("fade_in").Length / animationPlayer.GetAnimation("fade_out").Length);
            animationPlayer.Play("fade_in");
            animationPlayer.Seek(t, true);
        }
        else
        {
        animationPlayer.Play("fade_in");
        }
        

    }
    private void FadeOut()
    {
        animationPlayer.Play("fade_out");
    }
    
    private void _on_area_entered(Area2D area)
    {
        FadeIn();
    }
    private void _on_area_exited(Area2D area)
    {
        FadeOut();
    }

}
