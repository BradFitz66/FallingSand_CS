using Raylib_cs;
namespace Particles{
    public class Water : Particle{
        int[] randdir=new int[2]{-1,1};
        bool canMove=false; //Can this particle move to any valid spots?
        //Constructor
        public Water(int x, int y, World world) : base(x, y, world,"Water",null){}
        public override void Update()
        {
            base.Update();
            
            int dir_x=World.RandomElement<int>(randdir);
            //Set canMove based on if the particle can move to any of the spots around it
            canMove = CheckDown("Air")||CheckDownRight("Air")||CheckDownLeft("Air");
            if(CheckDown("Air")){
                world.Move(x,y,x,y+1);
            }
            else if(CheckRelative(dir_x,1,"Air")){
                world.Move(x,y,x+dir_x,y+1);
            }
            else{
                //Sometimes the random direction chosen for the sand will not be empty and can cause sand to get stuck.
                //This will make sure that the sand won't get stuck by making the chunk keep it alive until it has no spaces left to move to.
                if(canMove){
                    world.KeepAlive(x,y);
                }
            }
            if(CheckRelative(dir_x,0,"Air")){
                world.Move(x,y,x+dir_x,y+0);
            }


        }

        public override Color GetColor()
        {
            return new Color(96, 116, 161,255);
        }
    }
}