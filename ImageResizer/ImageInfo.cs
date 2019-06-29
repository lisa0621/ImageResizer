using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageInfo
    {
        public Bitmap ImgPhoto { get; set; }
        public int SrcWidth { get; set; }
        public int SrcHeight { get; set; }
        public int NewWidth { get; set; }
        public int NewHeight { get; set; }
        public string ImgName { get; set; }
    }
}
