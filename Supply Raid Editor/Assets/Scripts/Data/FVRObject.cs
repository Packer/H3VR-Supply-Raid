namespace Supply_Raid_Editor
{
    public class FVRObject
    {

        public enum OTagSet
        {
            Real,
            GroundedFictional,
            SciFiFictional,
            Meme,
            MeatFortress,
            Holiday,
            TNH,
            NonCombat
        }

        public enum OTagEra
        {
            None,
            Colonial,
            WildWest,
            TurnOfTheCentury,
            WW1,
            WW2,
            PostWar,
            Modern,
            Futuristic,
            Medieval
        }

        public enum OTagFirearmSize
        {
            None,
            Pocket,
            Pistol,
            Compact,
            Carbine,
            FullSize,
            Bulky,
            Oversize
        }

        public enum OTagFirearmAction
        {
            None,
            BreakAction,
            BoltAction,
            Revolver,
            PumpAction,
            LeverAction,
            Automatic,
            RollingBlock,
            OpenBreach,
            Preloaded,
            SingleActionRevolver
        }

        public enum OTagFirearmFiringMode
        {
            None,
            SemiAuto,
            Burst,
            FullAuto,
            SingleFire
        }

        public enum OTagFirearmFeedOption
        {
            None,
            BreachLoad,
            InternalMag,
            BoxMag,
            StripperClip,
            EnblocClip
        }

        public enum OTagFirearmRoundPower
        {
            None,
            Tiny,
            Pistol,
            Shotgun,
            Intermediate,
            FullPower,
            AntiMaterial,
            Ordnance,
            Exotic,
            Fire
        }

        public enum OTagFirearmMount
        {
            None,
            Picatinny,
            Russian,
            Muzzle,
            Stock,
            Bespoke
        }


        public enum OTagAttachmentFeature
        {
            None,
            IronSight,
            Magnification,
            Reflex,
            Suppression,
            Stock,
            Laser,
            Illumination,
            Grip,
            Decoration,
            RecoilMitigation,
            BarrelExtension,
            Adapter,
            Bayonet,
            ProjectileWeapon,
            Bipod
        }

        public enum OTagMeleeStyle
        {
            None,
            Tactical,
            Tool,
            Improvised,
            Medieval,
            Shield,
            PowerTool
        }

        public enum OTagMeleeHandedness
        {
            None,
            OneHanded,
            TwoHanded
        }

        public enum OTagPowerupType
        {
            None = -1,
            Health,
            QuadDamage,
            InfiniteAmmo,
            Invincibility,
            GhostMode,
            FarOutMeat,
            MuscleMeat,
            HomeTown,
            SnakeEye,
            Blort,
            Regen,
            Cyclops,
            WheredIGo,
            ChillOut
        }

        public enum OTagThrownType
        {
            None,
            ManualFuse,
            Pinned,
            Strange
        }

        public enum OTagThrownDamageType
        {
            None,
            Kinetic,
            Explosive,
            Fire,
            Utility
        }

        public enum OTagFirearmCountryOfOrigin
        {
            None = 0,
            Fictional = 1,
            UnitedStatesOfAmerica = 10,
            MuricanRemnants = 11,
            BritishEmpire = 20,
            UnitedKingdom = 21,
            CommonwealthOfAustralia = 22,
            KingdomOfFrance = 30,
            FrenchSecondRepublic = 0x1F,
            SecondFrenchEmpire = 0x20,
            FrenchThirdRepublic = 33,
            VichyFrance = 34,
            FrenchFourthRepublic = 35,
            FrenchRepublic = 36,
            GermanEmpire = 40,
            WeimarRepublic = 41,
            GermanReich = 42,
            WestGermany = 43,
            GermanDemocraticRepublic = 44,
            FederalRepublicOfGermany = 45,
            TsardomOfRussia = 50,
            RussianEmpire = 51,
            UnionOfSovietSocialistRepublics = 52,
            RussianFederation = 53,
            KingdomOfBelgium = 60,
            KingdomOfItaly = 70,
            ItalianRepublic = 71,
            SwedishEmpire = 90,
            UnitedKingdomsOfSwedenAndNorway = 91,
            KingdomOfSweden = 92,
            KingdomOfNorway = 100,
            KingdomOfFinland = 110,
            RepublicOfFinland = 111,
            Czechoslovakia = 120,
            CzechRepublic = 121,
            Ukraine = 130,
            SwissConfederation = 140,
            FirstSpanishRepublic = 150,
            SecondSpanishRepublic = 151,
            SpanishState = 152,
            KingdomOfSpain = 153,
            AustrianEmpire = 160,
            AustroHungarianEmpire = 161,
            RepublicOfAustria = 162,
            FirstHungarianRepublic = 170,
            HungarianRepublic = 171,
            KingdomOfHungary = 172,
            HungarianPeoplesRepublic = 173,
            RepublicOfCroatia = 190,
            RepublicOfKorea = 200,
            DemocraticRepublicOfVietnam = 210,
            StateOfIsrael = 220,
            FederativeRepublicOfBrazil = 230,
            EmpireOfJapan = 240,
            RepublicOfSouthAfrica = 250,
            GovernmentOfTheRepublicOfPolandInExile = 262,
            RepublicOfPoland = 263,
            PeoplesRepublicOfChina = 270,
            FormerYugoslavicRepublicOfMacedonia = 280,
            Yugoslavia = 281
        }

        public enum ObjectCategory
        {
            Uncategorized = 0,
            Firearm = 1,
            Magazine = 2,
            Clip = 3,
            Cartridge = 4,
            Attachment = 5,
            SpeedLoader = 6,
            Thrown = 7,
            MeleeWeapon = 10,
            Explosive = 20,
            Powerup = 25,
            Target = 30,
            Prop = 0x1F,
            Furniture = 0x20,
            Tool = 40,
            Toy = 41,
            Firework = 42,
            Ornament = 43,
            Loot = 50,
            VFX = 51,
            SosigClothing = 60
        }
    }
}