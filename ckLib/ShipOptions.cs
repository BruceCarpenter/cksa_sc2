/// <summary>
/// Summary description for ShipOptions
/// </summary>
namespace ckLib
{
    public class ShipType
    {
        public enum Options
        {
            Free = 1, // Gift cards or whatever
            SmallFlat = 2,
            Alone = 100 // This helps when calculating exact shipping costs.
        }

        public Options Type { get; set; }
        public int Limit { get; set; }

        public ShipType()
        {
        }

        public ShipType(Options o, int l)
        {
            Type = o;
            Limit = l;
        }

        public void SetType(int t)
        {
            Type = (Options)t;
        }
    }

    public class ShipOptions
    {
        public List<ShipType> ValidShipType { get; set; }

        public bool ShipUps()
        {
            return true;
            //return CanShip(ShipType.Options.Standard);
        }

        public bool ShipFree()
        {
            return CanShip(ShipType.Options.Free);
        }

        public bool ShipSmallFlat()
        {
            return CanShip(ShipType.Options.SmallFlat);
        }

        public bool ShipAlone()
        {
            foreach (var shipType in ValidShipType)
            {
                if (shipType.Type == ShipType.Options.Alone)
                    return true;
            }

            return false;
        }

        private bool CanShip(ShipType.Options optionToTest)
        {
            foreach (var s in ValidShipType)
            {
                if (s.Type == optionToTest)
                {
                    return true;
                }
            }

            return false;
        }
    }
}