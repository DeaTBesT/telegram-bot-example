namespace FantasyKingdom.Models;
public class MissionModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Difficulty { get; set; }
    public string Zone { get; set; }

    public static List<MissionModel> GenerateNewMissions(int count = 10)
    {
        var random = new Random();

        return Enumerable.Range(1, count)
            .Select(i => new MissionModel
            {
                Id = Guid.NewGuid().GetHashCode(),
                Title = GetRandomName(random),
                Difficulty = random.Next(1, 10),
                Zone = GetRandomZone(random)
            })
            .ToList();
    }

    private static string GetRandomName(Random random)
    {
        string[] missionTypes = ["Поиск", "Спасение", "Уничтожение", "Исследование", "Защита", "Доставка"];
        string[] targets = ["артефакта", "деревни", "монстра", "храма", "короля", "магического кристалла"];
        string[] locations = ["в горах", "в лесу", "в подземелье", "в пустыне", "на болотах", "в разрушенном городе"];

        return $"{missionTypes[random.Next(missionTypes.Length)]} {targets[random.Next(targets.Length)]} {locations[random.Next(locations.Length)]}";
    }

    private static string GetRandomZone(Random random)
    {
        string[] zones = [
            "Королевские земли",
            "Темный лес",
            "Горные ущелья",
            "Пустоши",
            "Зачарованные болота",
            "Древние руины",
            "Проклятые земли",
            "Огненные горы"
        ];

        return zones[random.Next(zones.Length)];
    }
}