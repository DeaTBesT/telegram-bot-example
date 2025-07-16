using FantasyKingdom.Enums;

namespace FantasyKingdom.Models;

public class UserData
{
    public UserAction UserState { get; set; }
    public int Coins { get; set; }
    public TavernPanel TavernInfo { get; set; }

    public class TavernPanel
    {
        public bool IsUpdate { get; set; }
        public int PageIndex { get; set; } = 0;
        public List<RecruitModel> AvaiableRecruits { get; set; }
    }

    public class UserInventory
    {
        public int PageIndex { get; set; } = 0;
        public List<RecruitModel> HiredRecruits { get; set; }
    }
}