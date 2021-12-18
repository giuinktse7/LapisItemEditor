using Backend.Tibia11;

namespace Backend
{
    public class AppearanceData
    {
        public AppearanceArray Objects { get; } = new AppearanceArray(offset: 100);
        public AppearanceArray Outfits { get; } = new AppearanceArray();
        public AppearanceArray Effects { get; } = new AppearanceArray();
        public AppearanceArray Missiles { get; } = new AppearanceArray();
    }
}
