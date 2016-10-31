namespace DeepPageParser.Dom
{
    internal class CaptionInfo
    {
        public CaptionInfo()
        {
            this.Conf1 = 0;
            this.Conf2 = 0;
            this.NumberChildren = 0;
            this.Caption1 = null;
            this.Caption2 = null;
        }

        public WrappedNode Caption1 { get; set; }

        public WrappedNode Caption2 { get; set; }

        public int Conf1 { get; set; }

        public int Conf2 { get; set; }

        public int NumberChildren { get; set; }
    }
}

