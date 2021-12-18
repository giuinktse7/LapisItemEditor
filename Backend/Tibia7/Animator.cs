using System;

namespace Backend.Tibia7
{
    public class FrameDuration
    {
        private static readonly Random Random = new Random();
        public FrameDuration(int minimum, int maximum)
        {
            this.SetTo(minimum, maximum);
        }

        public FrameDuration(uint minimum, uint maximum)
        {
            this.SetTo((int)minimum, (int)maximum);
        }

        public FrameDuration(ThingCategory category)
        {
            switch (category)
            {
                case ThingCategory.Item:
                    this.SetTo(500, 500);
                    break;
                case ThingCategory.Outfit:
                    this.SetTo(300, 300);
                    break;

                case ThingCategory.Effect:
                    this.SetTo(100, 100);
                    break;
                default:
                    this.SetTo(0, 0);
                    break;
            }
        }

        public int Minimum { get; private set; }
        public int Maximum { get; private set; }

        public int Duration
        {
            get
            {
                if (this.Minimum == this.Maximum)
                {
                    return this.Minimum;
                }

                return (this.Minimum + Random.Next(0, this.Maximum - this.Minimum));
            }
        }

        public FrameDuration SetTo(int minimum, int maximum)
        {
            if (minimum > maximum)
            {
                throw new ArgumentException("The minimum value may not be greater than the maximum value.");
            }

            this.Minimum = minimum;
            this.Maximum = maximum;
            return this;
        }

        public FrameDuration Clone()
        {
            return new FrameDuration(this.Minimum, this.Maximum);
        }
    }

    public enum AnimationMode : byte
    {
        None = 0,
        Asynchronous = 1,
        Synchronous = 2
    }

    public enum FrameMode : short
    {
        Automatic = -1,
        Random = 0xFE,
        Asynchronous = 0xFF
    }

    public enum AnimationDirection : byte
    {
        Forward = 0,
        Backward = 1
    }

    public class Animator
    {
        private static readonly Random Random = new Random();

        private int frames;
        private int startFrame;
        private int loopCount;
        private AnimationMode mode;
        private FrameDuration[] durations;
        private long lastTime;
        private int currentFrameDuration;
        private int currentFrame;
        private int currentLoop;
        private AnimationDirection currentDirection;

        public Animator(int frames, int startFrame, int loopCount, AnimationMode mode, FrameDuration[] durations)
        {
            this.frames = frames;
            this.startFrame = startFrame;
            this.loopCount = loopCount;
            this.mode = mode;
            this.durations = durations;
            this.Frame = (int)FrameMode.Automatic;
        }

        public Animator(FrameGroup frameGroup)
        {
            this.frames = frameGroup.Frames;
            this.startFrame = frameGroup.StartFrame;
            this.loopCount = frameGroup.LoopCount;
            this.mode = frameGroup.AnimationMode;
            this.durations = frameGroup.FrameDurations;
            this.Frame = (int)FrameMode.Automatic;
        }

        public int Frame
        {
            get
            {
                return this.currentFrame;
            }

            set
            {
                if (this.currentFrame != value)
                {
                    if (this.mode == AnimationMode.Asynchronous)
                    {
                        if (value == (ushort)FrameMode.Asynchronous)
                        {
                            this.currentFrame = 0;
                        }
                        else if (value == (ushort)FrameMode.Random)
                        {
                            this.currentFrame = Random.Next(0, this.frames);
                        }
                        else if (value >= 0 && value < this.frames)
                        {
                            this.currentFrame = value;
                        }
                        else
                        {
                            this.currentFrame = this.GetStartFrame();
                        }

                        this.IsComplete = false;
                        this.lastTime = GlobalClock.ElapsedMilliseconds;
                        this.currentFrameDuration = this.durations[this.currentFrame].Duration;
                    }
                    else
                    {
                        this.CalculateSynchronous();
                    }
                }
            }
        }

        public bool IsComplete { get; private set; }

        public void Update(long timestamp)
        {
            if (timestamp != this.lastTime && !this.IsComplete)
            {
                int elapsed = (int)(timestamp - this.lastTime);
                if (elapsed >= this.currentFrameDuration)
                {
                    int frame = loopCount < 0 ? this.GetPingPongFrame() : this.GetLoopFrame();
                    if (this.currentFrame != frame)
                    {
                        int duration = this.durations[frame].Duration - (elapsed - this.currentFrameDuration);
                        if (duration < 0 && this.mode == AnimationMode.Synchronous)
                        {
                            this.CalculateSynchronous();
                        }
                        else
                        {
                            this.currentFrame = frame;
                            this.currentFrameDuration = duration < 0 ? 0 : duration;
                        }
                    }
                    else
                    {
                        this.IsComplete = true;
                    }
                }
                else
                {
                    this.currentFrameDuration = this.currentFrameDuration - elapsed;
                }

                this.lastTime = timestamp;
            }
        }

        public int GetStartFrame()
        {
            if (this.startFrame > -1)
            {
                return this.startFrame;
            }

            return Random.Next(0, this.frames);
        }

        private int GetLoopFrame()
        {
            int nextFrame = (this.currentFrame + 1);
            if (nextFrame < this.frames)
            {
                return nextFrame;
            }

            if (this.loopCount == 0)
            {
                return 0;
            }

            if (this.currentLoop < (loopCount - 1))
            {
                this.currentLoop++;
                return 0;
            }

            return this.currentFrame;
        }

        private int GetPingPongFrame()
        {
            int count = this.currentDirection == AnimationDirection.Forward ? 1 : -1;
            int nextFrame = this.currentFrame + count;
            if (this.currentFrame + count < 0 || nextFrame >= frames)
            {
                this.currentDirection = this.currentDirection == AnimationDirection.Forward ? AnimationDirection.Backward : AnimationDirection.Forward;
                count *= -1;
            }

            return this.currentFrame + count;
        }

        private void CalculateSynchronous()
        {
            int totalDuration = 0;
            for (int i = 0; i < this.frames; i++)
            {
                totalDuration += durations[i].Duration;
            }

            long time = GlobalClock.ElapsedMilliseconds;
            long elapsed = time % totalDuration;
            long totalTime = 0;

            for (int i = 0; i < this.frames; i++)
            {
                long duration = this.durations[i].Duration;
                if (elapsed >= totalTime && elapsed < totalTime + duration)
                {
                    this.currentFrame = i;
                    long timeDiff = elapsed - totalTime;
                    this.currentFrameDuration = (int)(duration - timeDiff);
                    break;
                }

                totalTime += duration;
            }

            this.lastTime = time;
        }
    }
}