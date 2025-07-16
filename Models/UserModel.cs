namespace FantasyKingdom.Models;

public class UserModel
{
    public UserModel()
    {
        
    }
    
    public UserModel(long id, string userName, UserData data)
    {
        Id = id;
        UserName = userName;
        Data = data;
    }
    
    public long Id { get; set; }
    public string UserName { get; set; }
    public UserData Data { get; set; }
    public RecruitModel[] Recruits { get; set; }

    public class TavernPersonsPanel()
    {
        public int PageIndex { get; set; } = 0;
        //public TavernPersonModel[] AvaiableRecruits { get; set; }
    }
}