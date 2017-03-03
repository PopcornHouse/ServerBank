using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBank
{
    public class BankItem
    {
        public string player;
        public int balance;

        public BankItem(string player = "", int balance = 0)
        {
            this.player = player;
            this.balance = balance;
        }
    }
}
