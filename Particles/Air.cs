using Raylib_cs;
namespace Particles{
    public class Air : Particle{

        //Constructor
        public Air(int x, int y, World world) : base(x, y, world,"Air",null){
            properties.mass=0f;
            properties.density=0f;
        }

        public override void Update()
        {
            base.Update();
        }

        public override Color GetColor()
        {
            return new Color(0, 0, 0, 255);
        }
    }
}