using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class Exchange_Order
{

    public bool buyOrSell = false;
    public int quantity = 100;
    public string username;
    public double price;

    public Exchange_Order()
    {

    }

    public Exchange_Order(byte[] body)
    {
        string message = Encoding.UTF8.GetString(body);
        string[] words = message.Split(' ');

        bool buyOrSell = false;
        if (words[0] == "False")
        {
            buyOrSell = false;
        }
        else
        {
            buyOrSell = true;
        }
        int quantity = int.Parse(words[1]);
        string username = words[2];
        double price = double.Parse(words[3]);

        this.buyOrSell = buyOrSell;
        this.quantity = quantity;
        this.username = username;
        this.price = price;

    }

    public Exchange_Order(bool buyorSell, int quantity, string username, double price)
    {
        this.buyOrSell = buyorSell;
        this.quantity = quantity;
        this.username = username;
        this.price = price;
    }


    public byte[] newMessage()
    {
        string allData = buyOrSell.ToString() + " " + quantity + " " + username + " " + price;
        byte[] encoded_message = Encoding.UTF8.GetBytes(allData);
        return encoded_message;

    }


}
