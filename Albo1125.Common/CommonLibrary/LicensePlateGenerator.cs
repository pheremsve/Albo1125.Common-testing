
// License Plate Generator Written by Pherem
namespace Albo1125.Common.CommonLibrary
{
    public static class LicensePlateGenerator
    {
        private static readonly ThreadLocal<Random> ThreadSafeRandom = new(() => new Random());
        private static Random Rnd => ThreadSafeRandom.Value;

        public enum PlateFormat
        {
            Default,
            Classic,
            Gov,
            EMS,
            Police,
            SAPlate,
            CivilianShort,
            Motorcycle,
            Diplomat,
            RegionBased,
            USGov,
            LSFD
        }

        public enum RegionCode
        {
            LS,
            BC,
            SB,
            ZC,
            SA
        }

        public static string GenerateLicensePlate(Vehicle vehicle)
        {
            PlateFormat format = GetAdaptivePlateFormat(vehicle);
            return format == PlateFormat.RegionBased
                ? GenerateRegionLicensePlate(GetRegionFromZone(World.GetStreetName(vehicle.Position)))
                : GenerateLicensePlate(format);
        }

        public static PlateFormat GetRandomPlateFormat()
        {
            PlateFormat[] formats = (PlateFormat[])Enum.GetValues(typeof(PlateFormat));
            return formats[Rnd.Next(formats.Length)];
        }

        public static PlateFormat GetRandomCivilianPlateFormat()
        {
            PlateFormat[] civilianFormats =
            {
                PlateFormat.Default,
                PlateFormat.Classic,
                PlateFormat.SAPlate,
                PlateFormat.CivilianShort,
                PlateFormat.RegionBased
            };
            return civilianFormats[Rnd.Next(civilianFormats.Length)];
        }

        public static string GenerateLicensePlate(PlateFormat format)
        {
            switch (format)
            {
                case PlateFormat.Classic:
                    return $"{RandomLetters(3)}{RandomDigits(4)}";
                case PlateFormat.Gov:
                    return $"GOV{RandomDigits(4)}";
                case PlateFormat.EMS:
                    return $"EMS{RandomDigits(3)}";
                case PlateFormat.Police:
                    return $"PD{RandomDigits(2)}UNIT";
                case PlateFormat.SAPlate:
                    return $"SA-{RandomDigits(4)}";
                case PlateFormat.CivilianShort:
                    return RandomLetters(4);
                case PlateFormat.Motorcycle:
                    return $"MOTO{RandomDigits(3)}";
                case PlateFormat.Diplomat:
                    return $"DIPLO{RandomDigits(1)}{RandomLetters(1)}";
                case PlateFormat.USGov:
                    return $"US GOV {RandomDigits(3)}";
                case PlateFormat.LSFD:
                    return $"LSFD-{RandomDigits(3)}";
                case PlateFormat.RegionBased:
                    return GenerateRegionPlate();
                case PlateFormat.Default:
                default:
                    return $"{RandomDigits(2)}{RandomLetters(3)}{RandomDigits(3)}";
            }
        }

        public static string GenerateCivilianPlate()
        {
            return GenerateLicensePlate(GetRandomCivilianPlateFormat());
        }

        private static string GenerateRegionPlate()
        {
            RegionCode region = GetRegionFromZone(World.GetStreetName(Game.LocalPlayer.Character.Position));
            return GenerateRegionLicensePlate(region);
        }

        private static PlateFormat GetAdaptivePlateFormat(Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists())
                return PlateFormat.Default;

            string modelName = vehicle.Model.Name.ToLowerInvariant();

            if (vehicle.Model.IsBike)
                return PlateFormat.Motorcycle;

            if (modelName.Contains("police") || modelName.Contains("sheriff") || modelName.Contains("lspd"))
                return PlateFormat.Police;

            if (modelName.Contains("amb") || modelName.Contains("ems") || modelName.Contains("firetruk"))
                return PlateFormat.EMS;

            if (modelName.Contains("riot") || modelName.Contains("fbi") || modelName.Contains("gov"))
                return PlateFormat.Gov;

            if (modelName.Contains("diplomat") || modelName.Contains("consulate"))
                return PlateFormat.Diplomat;

            return PlateFormat.RegionBased;
        }

        private static string GenerateRegionLicensePlate(RegionCode region)
        {
            return region switch
            {
                RegionCode.LS => $"LS-{RandomLetters(3)}{RandomDigits(3)}",
                RegionCode.BC => $"BC-{RandomDigits(4)}",
                RegionCode.SB => $"SB-{RandomDigits(3)}LSP",
                RegionCode.ZC => $"ZC-{RandomLetters(2)}{RandomDigits(2)}",
                RegionCode.SA => $"{RandomDigits(2)}{RandomLetters(3)}{RandomDigits(3)}",
                _ => $"{RandomDigits(2)}{RandomLetters(3)}{RandomDigits(3)}"
            };
        }

        private static RegionCode GetRegionFromZone(string zoneName)
        {
            zoneName = zoneName?.ToLowerInvariant() ?? "";

            if (zoneName.Contains("sandy") || zoneName.Contains("grapeseed") || zoneName.Contains("alamo") || zoneName.Contains("chiliad"))
                return RegionCode.SB;

            if (zoneName.Contains("paleto") || zoneName.Contains("zancudo") || zoneName.Contains("blaine"))
                return RegionCode.BC;

            if (zoneName.Contains("del") || zoneName.Contains("vine") || zoneName.Contains("mirror") || zoneName.Contains("downt") || zoneName.Contains("vesp") || zoneName.Contains("burton"))
                return RegionCode.LS;

            if (zoneName.Contains("military") || zoneName.Contains("army") || zoneName.Contains("fort"))
                return RegionCode.ZC;

            return RegionCode.SA;
        }

        private static string RandomDigits(int count)
        {
            return new string(Enumerable.Repeat("0123456789", count)
                .Select(s => s[Rnd.Next(s.Length)]).ToArray());
        }

        private static string RandomLetters(int count)
        {
            return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ", count)
                .Select(s => s[Rnd.Next(s.Length)]).ToArray());
        }
    }
}
