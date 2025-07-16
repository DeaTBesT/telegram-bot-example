namespace FantasyKingdom.Models;
public class RecruitModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Health { get; set; }
    public int HireCost { get; set; }
    public string[] Inventory { get; set; }

    public override string ToString() =>
        $"{Name}\n⚔️Атака: {Attack} 🛡Защита: {Defense} ❤️Здоровье: {Health}\n💰Стоимость найма: {HireCost}";

    public static List<RecruitModel> GenerateNewRecruits(int count = 10)
    {
        var random = new Random();

        return Enumerable.Range(1, count)
            .Select(i => new RecruitModel
            {
                Id = Guid.NewGuid().GetHashCode(),
                Name = GetRandomName(random),
                Attack = random.Next(5, 10),
                Defense = random.Next(3, 8)
            })
            .ToList();
    }

    private static string GetRandomName(Random random)
    {
        string[] names = ["Воин", "Лучник", "Маг", "Лекарь", "Разведчик", "Рыцарь", "Варвар", "Паладин"];
        string[] prefixes = ["Опытный ", "Молодой ", "Старый ", "Храбрый ", "Мудрый ", "Сильный "];
        
        return random.Next(0, 2) == 0 
            ? names[random.Next(names.Length)] 
            : prefixes[random.Next(prefixes.Length)] + names[random.Next(names.Length)];
    }
}
