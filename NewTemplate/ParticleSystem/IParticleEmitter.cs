using Microsoft.Xna.Framework;

namespace NewTemplate.ParticleSystem
{
    public interface IParticleEmitter
    {
        public Vector2 Position { get; set; }

        public Vector2 Velocity { get; set; }
    }
}
