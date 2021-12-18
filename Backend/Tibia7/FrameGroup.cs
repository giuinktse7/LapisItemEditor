using System;

namespace Backend.Tibia7
{
    public enum FrameGroupType : byte
    {
        Default = 0,
        Walking = 1
    }

    public class FrameGroup
    {
        #region Public Properties

        public byte Width { get; set; }

        public byte Height { get; set; }

        public byte ExactSize { get; set; }

        public byte Layers { get; set; }

        public byte PatternX { get; set; }

        public byte PatternY { get; set; }

        public byte PatternZ { get; set; }

        public byte Frames { get; set; }

        public uint[] SpriteIDs { get; set; } = Array.Empty<uint>();

        public bool IsAnimation { get; set; }

        public AnimationMode AnimationMode { get; set; }

        public int LoopCount { get; set; }

        public sbyte StartFrame { get; set; }

        public FrameDuration[] FrameDurations { get; set; } = Array.Empty<FrameDuration>();

        #endregion

        #region Public Methods

        public int GetTotalSprites()
        {
            return this.Width * this.Height * this.PatternX * this.PatternY * this.PatternZ * this.Frames * this.Layers;
        }

        public int GetSpriteIndex(int width, int height, int layers, int patternX, int patternY, int patternZ, int frames)
        {
            return ((((((frames % this.Frames) * this.PatternZ + patternZ) * this.PatternY + patternY) * this.PatternX + patternX) * this.Layers + layers) * this.Height + height) * this.Width + width;
        }

        public int GetTextureIndex(int layer, int patternX, int patternY, int patternZ, int frame)
        {
            return (((frame % this.Frames * this.PatternZ + patternZ) * this.PatternY + patternY) * this.PatternX + patternX) * this.Layers + layer;
        }

        public FrameGroup Clone()
        {
            FrameGroup group = new FrameGroup();
            group.Width = this.Width;
            group.Height = this.Height;
            group.Layers = this.Layers;
            group.Frames = this.Frames;
            group.PatternX = this.PatternX;
            group.PatternY = this.PatternY;
            group.PatternZ = this.PatternZ;
            group.ExactSize = this.ExactSize;
            group.SpriteIDs = (uint[])this.SpriteIDs.Clone();
            group.AnimationMode = this.AnimationMode;
            group.LoopCount = this.LoopCount;
            group.StartFrame = this.StartFrame;

            if (this.Frames > 1)
            {
                group.IsAnimation = true;
                group.FrameDurations = new FrameDuration[this.Frames];

                for (int i = 0; i < this.Frames; i++)
                {
                    group.FrameDurations[i] = this.FrameDurations[i].Clone();
                }
            }

            return group;
        }

        #endregion

        #region Public Static Methods

        public static FrameGroup Create()
        {
            FrameGroup group = new FrameGroup();
            group.Width = 1;
            group.Height = 1;
            group.Layers = 1;
            group.Frames = 1;
            group.PatternX = 1;
            group.PatternY = 1;
            group.PatternZ = 1;
            group.ExactSize = 32;
            group.SpriteIDs = new uint[1];
            group.IsAnimation = false;
            group.AnimationMode = AnimationMode.None;
            group.LoopCount = 0;
            group.StartFrame = 0;
            group.FrameDurations = null;
            return group;
        }

        #endregion
    }
}