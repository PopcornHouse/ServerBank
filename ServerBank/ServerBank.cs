using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Wolfje.Plugins.SEconomy;


namespace ServerBank
{
    [ApiVersion(2, 0)]
    public class ServerBank : TerrariaPlugin
    {
        public DBManager manager;

        #region Info
        public override string Name { get { return "ServerBank"; } }
        public override string Author { get { return "Bippity"; } }
        public override string Description { get { return "It's basically death insurance!"; } }
        public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        public ServerBank(Main game) : base(game)
        {
            Order = 1;
            TShock.Initialized += Start;
        }
        #endregion

        #region Initialize/Start
        private void Start()
        {
            manager = new DBManager();
			manager.setupDb();
        }

        public override void Initialize()
        {
            #region Commands
            Commands.ChatCommands.Add(new Command("serverbank.player", Bank, "coins", "points"));
            #endregion
            throw new NotImplementedException();
        }
        #endregion

		private void Bank(CommandArgs args)
		{
			var account = SEconomyPlugin.Instance.GetBankAccount(args.Player.User.Name);
			BankItem bankAccount = new BankItem();
			List<BankItem> bankList = new List<BankItem>();
			Money balance;

			if (args.Parameters.Count < 2)
			{
				args.Player.SendValidBankUsage();
				return;
			}

			string subcmd = args.Parameters[0].ToLower();
			switch (subcmd)
			{
				#region balance
				case "balance":
				case "bal":
					//Display the player's balance
					return;
				#endregion

				#region deposit
				case "deposit":
					//Deposit the specified amount. Max balance = 10 Platinum ServerCoins (10,000,000)
					return;
				#endregion

				#region withdraw
				case "withdraw":
					//Withdraw specified amount. Include a fee. Simulate a savings account
					return;
				#endregion

				default:
					args.Player.SendValidBankUsage();
					return;
			}
		}
    }
}
