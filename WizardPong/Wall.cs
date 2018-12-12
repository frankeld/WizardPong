using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WizardPong
{
    public class Wall
    {

        Rectangle boundingBox;
        Texture2D image;

        public Wall()
        {

        }

        public void LoadContent(ContentManager c, Rectangle box, int imgChoice)
        {
            boundingBox = box;
            if (imgChoice == 0)//default
            {
                image = c.Load<Texture2D>("Main Graphics\\defaultWall");
            }
            else if (imgChoice == 1) //center line
            {
                image = c.Load<Texture2D>("Main Graphics\\centerLine");
            }
            else if (imgChoice == 2) //goal line
            {
                image = c.Load<Texture2D>("Main Graphics\\goalLine");
            }
            else if (imgChoice == 3) //bottom texture
            {
                image = c.Load<Texture2D>("Main Graphics\\bottomWall");
            }
            else if (imgChoice == 4) //top texture
            {
                image = c.Load<Texture2D>("Main Graphics\\topWall");
            }
            else if (imgChoice == 5) //side left textures
            {
                image = c.Load<Texture2D>("Main Graphics\\sideWallLeft");
            }
            else if (imgChoice == 6)//side right textures
            {
                image = c.Load<Texture2D>("Main Graphics\\sideWallRight");
            }

            Game1.walls.Add(this);
        }

        public virtual Rectangle BoundingBox()
        {
            return boundingBox;

        }

        //Walls must have an update function so that portalTrap and forceField work
        public virtual void Update() { }

        public virtual void Draw(SpriteBatch s)
        {

            s.Draw(image, boundingBox, null, Color.White);

        }
    }

}