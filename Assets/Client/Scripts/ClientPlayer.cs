using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ClientPlayer
{
    public int ClientID { get; private set; }
    public string Name { get; private set; }
    public Color Color { get; private set; }
    public List<AbstractBuilding> Buildings { get; private set; } = new List<AbstractBuilding>();

    public ClientPlayer(int clientID, string name, Color color)
    {
        ClientID = clientID;
        Name = name;
        Color = color;
    }
}
