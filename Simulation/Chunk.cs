using System.Numerics;
using Particles;
using static Raylib_cs.Raylib;
using static Raylib_cs.Color;
using static Raylib_cs.Raymath;

public class Chunk{
	public int width,height;
	public int width_world,height_world;
	
	public Vector2 pos{get; private set;}
	Particle[,] particles;
	List<Tuple<int,int>> changes;//Destination, Source
	public int chunk_timer;
	World world;

	//Dirty rect. Should be only read from (apart from when swapping)
	int m_minX,m_minY,m_maxX,m_maxY;
	//Working dirty rect. Should only be written to.
	int m_minXw,m_minYw,m_maxXw,m_maxYw;

	

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
				if(x<0||x>=width||y<0||y>=height){
					Console.WriteLine("Out of bounds");
					//Get chunk at x,y
					Chunk c=world.GetChunkAt(x,y);
					Console.WriteLine("Chunk at "+x+","+y+" is "+c.pos.X+","+c.pos.Y);
					continue;
				}

				particles[x,y].Update();
			}
		}
		chunk_timer=world.worldTimer+1;
	}
	//Update the dirty rectange to encompass the given coordinates
	public void KeepAlive(int x, int y){
		m_minXw=Math.Clamp(Math.Min(x-4,m_minXw),0,width-1);
		m_minYw=Math.Clamp(Math.Min(y-4,m_minYw),0,height-1);
		m_maxXw=Math.Clamp(Math.Max(x+4,m_maxXw),0,width-1);
		m_maxYw=Math.Clamp(Math.Max(y+4,m_maxYw),0,height-1);
	}
	
	//Move a cell
	public void Move(int x, int y,int xto, int yto){		
		//Return if not in bounds
		if(x<0||x>=width||y<0||y>=height){
			Console.WriteLine("Out of bounds");
			return;
		}
		//Convert to 1D index
		int index1=y*width+x;
		int index2=yto*width+xto;
		changes.Add(
			new Tuple<int,int>(index2,index1)
		);
	}

	//Commit the particle movements to the particle array
	public void CommitCells(){

		//Remove moves that aren't valid (destination is filled)
		for(int i=0; i<changes.Count; i++){
			//Convert to 2D coordinates
			int dest_x=changes[i].Item1%width;
			int dest_y=changes[i].Item1/width;
			int src_x=changes[i].Item2%width;
			int src_y=changes[i].Item2/width;
			//Check if out of bounds, return if so


			Particle destination = particles[dest_x,dest_y];
			if(destination.name!="Air"){
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

				//Convert destination and source to 2D coordinates
				int dest_x=changes[rand].Item1%width;
				int dest_y=changes[rand].Item1/width;
				int src_x=changes[rand].Item2%width;
				int src_y=changes[rand].Item2/width;

				//Move the particle
				Set(dest_x,dest_y,particles[src_x,src_y]);
				Set(src_x,src_y,new Air(src_x,src_y,world));
				i_prev=i+1;
			}
		}
		changes.Clear();
	}

	//Set a particle in the simulation
	public void Set(int x,int y,Particle p){
		//Make sure x and y are in range
		if(x<0||x>=width||y<0||y>=height){
			Console.WriteLine("Out of bounds");
			return;
		}
		KeepAlive(x,y);
		//Convert x y to world coordinates
		

		//Update the dirty rect to encompass the new particle
		int xw=x+(int)pos.X*width;
		int yw=y+(int)pos.Y*height;
		particles[x,y]=p;
		p.x=xw;
		p.y=yw;

		//Wake up particles around the particle that just got set
		for(int i=-4;i<5;i++){
			for(int j=-4;j<5;j++){
				//Convert i,j to world coordinates
				int xw2=xw+i;
				int yw2=yw+j;
				world.KeepAlive(xw2,yw2);
			}
		}
		p.chunk=this;
		p.sleep_timer=0;

		DrawRectangle(
			xw * world.resize_factor, 
			yw * world.resize_factor, 
			world.resize_factor, 
			world.resize_factor, 
			p.GetColor()
		);
	}
	public Particle Get(int x,int y){
		if(x<0||x>=width||y<0||y>=height){
			Console.WriteLine("Out of bounds");
			return new Sand(x,y,world);
		}
		return particles[x,y];
	}

	public void DebugDraw(){
		//Visualize direty rect with DrawRectangleLines
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
			BLUE
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