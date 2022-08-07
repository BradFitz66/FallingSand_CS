using System.Numerics;
using Particles;
using static Raylib_cs.Raylib;
using static Raylib_cs.Color;
using static Raylib_cs.Raymath;

public class Chunk{
	public int width,height;
	public int width_world,height_world;
	
	public Vector2 pos{get; private set;}
	public Particle[,] particles;
	List<Tuple<int,int>> changes;//Destination, Source
	public float sleep_timer=0;
	public float sleep_time{get; private set;}=5;
	World world;

	//Dirty rect. Should be only read from (apart from when swapping)
	public int m_minX,m_minY,m_maxX,m_maxY;
	//Working dirty rect. Should only be written to.
	public int m_minXw,m_minYw,m_maxXw,m_maxYw;

	
	

	public Chunk(int size,int pos_x, int pos_y,World world){
		width=size;
		height=size;
		width_world=size*world.resize_factor;
		height_world=size*world.resize_factor;
		pos=new Vector2(pos_x,pos_y);
		particles=new Particle[width,height];
		//Fill with air
		for(int x=0;x<width;x++){
			for(int y=0;y<height;y++){
				particles[x,y]=new Air(x,y,world);
			}
		}
		changes=new List<Tuple<int,int>>();
		this.world=world;
	}

	public void Remove(int x, int y){
		particles[x,y]=new Air(x,y,world);
		//Remove from changes list
		for(int i=0;i<changes.Count;i++){
			//convert item2 to 2D coordinates
			int x2=changes[i].Item2%width;
			int y2=changes[i].Item2/width;
			if(x2==x&&y2==y){
				changes.RemoveAt(i);
				break;
			}
		}
	}

	public void Add(int x, int y, Particle p){
		particles[x,y]=p;
		//Add to changes list
		changes.Add(new Tuple<int,int>(x,y));
	}

	public void Update(){
		//Iterate through the elements within the bounds of the rectangle
		for(int x=m_minX;x<=m_maxX;x++){
			for(int y=m_minY;y<=m_maxY;y++){
				//Continue if outside bounds of chunk
				if((x<0||x>=width||y<0||y>=height) || particles[x,y].sleep_timer>=particles[x,y].sleep_time){
					continue;
				}

				particles[x,y].Update();
			}
		}

		sleep_timer+=1*GetFrameTime();
	}
	//Update the dirty rectange to encompass the given coordinates
	public void KeepAlive(int x, int y){
		//Make sure within bounds
		if(!(x<0||x>=width||y<0||y>=height)){
			particles[x,y].sleep_timer=0;
		}
		sleep_timer=0;
		m_minXw=Math.Clamp(Math.Min(x-4,m_minXw),0,width-1);
		m_minYw=Math.Clamp(Math.Min(y-4,m_minYw),0,height-1);
		m_maxXw=Math.Clamp(Math.Max(x+4,m_maxXw),0,width-1);
		m_maxYw=Math.Clamp(Math.Max(y+4,m_maxYw),0,height-1);
		
	}
	
	//Move a cell
	public void Move(int x, int y,int xto, int yto){		
		//Return if not in bounds
		if(x<0||x>=width||y<0||y>=height){
			return;
		}
		//Convert to 1D index
		int index1=x+width*y;
		int index2=xto+width*yto;
		changes.Add(
			new Tuple<int,int>(index2,index1)
		);
	}

	//Commit the particle movements to the particle array
	public void CommitCells(){

		//Remove moves that aren't valid (destination is filled with a particle that has a higher density than the moving particle)
		for(int i=0; i<changes.Count; i++){
			int dest_x=changes[i].Item1%width;
			int dest_y=changes[i].Item1/width;
			int src_x=changes[i].Item2%width;
			int src_y=changes[i].Item2/width;
			Particle source=particles[src_x,src_y];

			Particle destination = particles[dest_x,dest_y];
			if(destination.properties.density>=source.properties.density){
				changes[i]=changes[changes.Count-1];
				changes.RemoveAt(changes.Count-1);
				i--;
			}
		}
		//Sort the changes by destination index
		changes.Sort((a,b)=>b.Item1.CompareTo(a.Item1));

		int i_prev=0;
		changes.Add(new Tuple<int, int>(-1,-1));//Catch final move
		//Move the particles
		for (int i=0; i<changes.Count-1; i++){
			if(changes[i+1].Item1!=changes[i].Item1){
				//Choose a random particle to move
				int rand = i_prev+World.RandomNumber(i-i_prev);

				int dest_x=changes[rand].Item1%width;
				int dest_y=(changes[rand].Item1/width)%height;
				int src_x=changes[rand].Item2%width;
				int src_y=(changes[rand].Item2/width)%height;

				Particle particle_source=particles[src_x,src_y];
				Particle particle_destination=particles[dest_x,dest_y];

				//World coordinates of dest and src
				Vector2 dest_w = world.toWorldCoorinates(this,dest_x, dest_y);
				Vector2 src_w =  world.toWorldCoorinates(this,src_x,  src_y );

				//Commit the particle's movement
				world.Set((int)dest_w.X,(int)dest_w.Y,particle_source);
				world.Set((int)src_w.X,(int)src_w.Y,particle_destination);
				if(particle_destination.properties.density==0.1f && particle_source.properties.density==1f){
					Console.WriteLine("Particle dest:{0}",particle_destination);
				}
				i_prev=i+1;
			}
		}
		changes.Clear();
	}
	public bool InBounds(int x, int y){
		return x>=0&&x<width&&y>=0&&y<height;
	}
	//Set a particle in the simulation
	public void Set(int x,int y,Particle p){
		if(x<0||x>=width||y<0||y>=height){
			return;
		}
		sleep_timer=0;
		if(y==height && p.properties.density==0.1f){
			Console.WriteLine("????");
		}
		Vector2 pos_w = world.toWorldCoorinates(this,x,y);
		p.x=(int)pos_w.X;
		p.y=(int)pos_w.Y;
		particles[x,y]=p;
		p.chunk=this;
		p.sleep_timer=0;

		world.KeepAlive((int)pos_w.X,(int)pos_w.Y);

		//Wake up particles around the particle that just got set
		for(int i=-1;i<2;i++){
			for(int j=-1;j<2;j++){
				int xw2=(int)pos_w.X+i;
				int yw2=(int)pos_w.Y+j;

				if(!(i<0||i>=width||j<0||j>=height)){
					particles[i,j].sleep_timer=0; 
				}
				world.KeepAlive(xw2,yw2);
			}
		}
		DrawRectangle(
			(int)pos_w.X * world.resize_factor, 
			(int)pos_w.Y * world.resize_factor, 
			world.resize_factor, 
			world.resize_factor, 
			p.GetColor()
		);
	}
	public Particle Get(int x,int y){
		if(x<0||x>=width||y<0||y>=height){
			return new Sand(x,y,world);
		}
		return particles[x,y];
	}

	public void DebugDraw(){
		//Visualize dirty rect with DrawRectangleLines
		DrawRectangleLines(
			(m_minX*world.resize_factor) +((int)pos.X*width_world),
			(m_minY*world.resize_factor) +((int)pos.Y*width_world),
			(m_maxX-m_minX+1)*world.resize_factor,
			(m_maxY-m_minY+1)*world.resize_factor,
			RED
		);

		//Draw chunk with rect outline
		DrawRectangleLines(
			((int)pos.X)*width_world,
			((int)pos.Y)*width_world,
			width_world,
			height_world,
			sleep_timer>sleep_time?RED:BLUE
		);
	}

	public void UpdateDirtRects(){
		//Swap dirty rects
		m_minX=m_minXw;
		m_minY=m_minYw;
		m_maxX=m_maxXw;
		m_maxY=m_maxYw;

		m_minXw=width;
		m_minYw=height;
		m_maxXw=-1;
		m_maxYw=-1;
	}
}