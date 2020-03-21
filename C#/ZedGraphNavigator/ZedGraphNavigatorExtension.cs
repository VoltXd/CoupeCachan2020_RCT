using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZedGraphNavigatorDll
{
    public partial class ZedGraphNavigator
    {
        public void SavePicture(string dirPath)
        {
            Bitmap imageToSave = new Bitmap(this.zedGraphControl.GraphPane.GetImage());
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(dirPath + ".png", FileMode.Create, FileAccess.ReadWrite))
                {
                    imageToSave.Save(memory, ImageFormat.Png);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
