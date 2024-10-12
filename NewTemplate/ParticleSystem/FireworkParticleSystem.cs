using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NewTemplate.ParticleSystem
{
    public class FireworkParticleSystem : ParticleSystem
    {
        Color color;

        Color[] colors = new Color[]
        {
            Color.Fuchsia,
            Color.Red,
            Color.Crimson,
            Color.Green,
            Color.LightCoral,
            Color.CadetBlue,
            Color.Pink,
            Color.Purple,
            Color.Lavender
        };
        public FireworkParticleSystem(Game game, int maxExplosions) : base(game, maxExplosions * 25)
        {
        }

        protected override void InitializeConstants()
        {
            textureFilename = "circle";
            minNumParticles = 20;
            maxNumParticles = 25;

            blendState = BlendState.Additive;
            DrawOrder = AdditiveBlendDrawOrder;
        }

        protected override void InitializeParticle(ref Particle p, Vector2 where)
        {
            var velocity = RandomHelper.NextDirection() * RandomHelper.NextFloat(40, 400);

            var lifetime = RandomHelper.NextFloat(0.5f, 1.0f);

            var acceleration = -velocity / lifetime;

            var rotation = RandomHelper.NextFloat(0, MathHelper.TwoPi);

            var angularVelocity = RandomHelper.NextFloat(MathHelper.PiOver4, MathHelper.PiOver4);

            var scale = RandomHelper.NextFloat(4, 6);

            p.Initialize(where, velocity, acceleration, color, lifetime: lifetime, rotation: rotation, angularVelocity: angularVelocity);
        }

        protected override void UpdateParticle(ref Particle particle, float dt)
        {
            base.UpdateParticle(ref particle, dt);

            float normalizedLifetime = particle.TimeSinceStart / particle.Lifetime;

            particle.Scale = .25f + .25f * normalizedLifetime;
        }

        public void PlaceExplosion(Vector2 where)
        {
            color = colors[RandomHelper.Next(colors.Length)];
            AddParticles(where);
        }
    }
}
