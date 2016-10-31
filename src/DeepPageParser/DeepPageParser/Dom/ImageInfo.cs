namespace DeepPageParser.Dom
{
    internal class ImageInfo
    {
        public ImageInfo(WrappedNode rawImage, WrappedNode box, WrappedNode image, WrappedNode c1, WrappedNode c2)
        {
            this.RawImage = rawImage;
            this.Box = box;
            this.Image = image;
            this.Caption1 = c1;
            this.Caption2 = c2;
            this.IsVideo = false;
            this.IsLarge = false;
            this.IsSmall = false;
        }

        public WrappedNode Box { get; set; }

        public WrappedNode Caption1 { get; set; }

        public WrappedNode Caption2 { get; set; }

        public int Height { get; set; }

        public WrappedNode Image { get; set; }

        public bool IsLarge { get; set; }

        public bool IsSmall { get; set; }

        public bool IsVideo { get; set; }

        public WrappedNode RawImage { get; set; }

        public int Width { get; set; }
    }
}

