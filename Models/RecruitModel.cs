using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
