using Raylib_cs;
namespace Particles{
    public class Water : Particle{
        int[] randdir=new int[2]{-1,1};
        bool canMove=false; //Can this particle move to any valid spots?

        //Constructor
        public Water(int x, int y, World world) : base(x, y, world,"Water",null){
            properties.mass=0.1f;
            properties.density=0.1f;
        }
        public override void Update()
        {
            base.Update();            

            int dir_x=World.RandomElement<int>(randdir);
            //Set canMove based on if the particle can move to any of the spots around it
            canMove = CheckDown()||CheckDownRight()||CheckDownLeft()||CheckLeft()||CheckRight();
            if(canMove){
                world.KeepAlive(x,y);
                if(CheckDown()){
                    world.Move(x,y,x,y+1);
                }
                else if(CheckRelative(dir_x,1)){
                    world.Move(x,y,x+dir_x,y+1);
                }
                if(CheckRelative(dir_x,0)){
                    world.Move(x,y,x+dir_x,y+0);
                }
            }


        }

        public override Color GetColor()
        {
            return new Color(96, 116, 161,255);
        }
    }
}