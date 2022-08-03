//Rect struct for defining bounds of stuff

public struct Rect{
    public int x;
    public int y;
    public int width;
    public int height;
    
    public Rect(int x, int y, int width, int height){
        this.x=x;
        this.y=y;
        this.width=width;
        this.height=height;
    }
    
    public bool Contains(int x, int y){
        return x>=this.x&&x<this.x+this.width&&y>=this.y&&y<this.y+this.height;
    }
}