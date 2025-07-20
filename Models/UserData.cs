using FantasyKingdom.Enums;

namespace FantasyKingdom.Models;

public class UserData
{
    public UserAction UserState { get; set; }
    public int Coins { get; set; }
    public TavernPanel TavernInfo { get; set; }
    public UserInventory InventoryInfo { get; set; }

    public class TavernPanel
    {
        public bool IsUpdate { get; set; }
        public int PageIndex { get; set; } = 0;
        public List<HeroModel> AvaiableRecruits { get; set; }

        public bool TryAddRecruit(HeroModel recruit)
        {
            if (AvaiableRecruits.Contains(recruit))
            {
                return false;
            }

            AvaiableRecruits.Add(recruit);
            return true;
        }

        public bool TryRemoveRecruit(HeroModel recruit)
        {
            if (!AvaiableRecruits.Contains(recruit))
            {
                return false;
            }

            AvaiableRecruits.Remove(recruit);
            return true;
        }
    }

    public class UserInventory
    {
        public int PageIndex { get; set; } = 0;
        public List<HeroModel> HiredHeroes { get; set; }

        public bool TryAddHero(HeroModel recruit)
        {
            if (HiredHeroes.Contains(recruit))
            {
                return false;
            }

            HiredHeroes.Add(recruit);
            return true;
        }

        public bool TryRemoveHero(HeroModel recruit)
        {
            if (!HiredHeroes.Contains(recruit))
            {
                return false;
            }

            HiredHeroes.Remove(recruit);
            return true;
        }
    }
}