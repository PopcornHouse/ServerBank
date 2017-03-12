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
        public const int MAXBALANCE = 10000;
        public const double INTEREST_RATE = .005;

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
        }
        #endregion

		private void Bank(CommandArgs args)
		{
            if (args.Player == null)
            {
                return;
            }
            var account = SEconomyPlugin.Instance.GetBankAccount(args.Player.User.Name);
			BankItem bankAccount = new BankItem();
			List<BankItem> bankList = new List<BankItem>();
			Money balance;

			if (args.Parameters.Count < 2)
			{
				args.Player.SendValidBankUsage();
				return;
			}

            bankList = manager.GetBankItem(args.Player);
            if (bankList.Count < 1)
            {
                //Create New Account
                manager.CreateAccount(args.Player);
                bankList = manager.GetBankItem(args.Player);
            }
            bankAccount = bankList.ElementAt(0);

			string subcmd = args.Parameters[0].ToLower();
			switch (subcmd)
			{
				#region balance
				case "balance":
				case "bal":
                    //Display the player's balance
                    balance = manager.GetBalance(bankAccount);
                    args.Player.SendSuccessMessage("[ServerBank] Balance: {0}", balance);
					return;
				#endregion

				#region deposit
				case "deposit":
                    //Deposit the specified amount. Max balance = 10 Platinum ServerCoins (10,000,000)
                    Money deposit = -1;
                    if (args.Parameters.Count > 2 || !Money.TryParse(args.Parameters[1], out deposit))
                    {
                        args.Player.SendErrorMessage("[ServerBank] Invalid Desposit Amount!");
                        return;
                        //Index 1 = price
                    }
                    else if((int)deposit > MAXBALANCE)
                    {
                        args.Player.SendErrorMessage("[ServerBank] Maximum Desposit Amount is 10 Platinum ServerCoins!");
                        return;
                    }
                    else if((int)deposit <= 0)
                    {
                        args.Player.SendErrorMessage("[ServerBank] Invalid Desposit Amount!");
                        return;
                    }

                    if (manager.DepositBal(bankAccount, deposit))
                    {
                        balance = manager.GetBalance(bankAccount);
                        args.Player.SendSuccessMessage("[ServerBank] Success! You have deposited: {0}", deposit);
                        args.Player.SendSuccessMessage("[ServerBank] Balance {0}", balance);
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[ServerBank] An Error has Occured!");
                        return;
                    }

                    return;
				#endregion

				#region withdraw
				case "withdraw":
                    //Withdraw specified amount. Include a fee. Simulate a savings account
                    Money withdraw = -1;
                    if (args.Parameters.Count > 2 || !Money.TryParse(args.Parameters[1], out withdraw))
                    {
                        args.Player.SendErrorMessage("[ServerBank] Invalid Withdraw Amount!");
                        return;
                        //Index 1 = price
                    }

                    balance = manager.GetBalance(bankAccount);

                    if((int)withdraw <= 0)
                    {
                        args.Player.SendErrorMessage("[ServerBank] Invalid Withdraw Amount!");
                        return;
                    }
                    else if((int)withdraw == (int)balance)//No Interest when withdraw amount == balance
                    {
                        if (manager.WithdrawBal(bankAccount, withdraw, 0))
                        {
                            balance = manager.GetBalance(bankAccount);
                            args.Player.SendSuccessMessage("[ServerBank] Success! You have withdrawed: {0}", withdraw);
                            args.Player.SendSuccessMessage("[ServerBank] Balance {0}", balance);
                        }
                        else
                        {
                            args.Player.SendErrorMessage("[ServerBank] An Error has Occured!");
                            return;
                        }
                    }
                    else if((int)(withdraw * (1 + INTEREST_RATE)) > (int)balance)
                    {
                        args.Player.SendErrorMessage("[ServerBank] Withdraw Amount Exceeds Balance With Interest");
                        //args.Player.SendErrorMessage("[ServerBank] Withdraw Entire Balance for No Interest");
                        return;
                    }

                    if(manager.WithdrawBal(bankAccount, withdraw, INTEREST_RATE))
                    {
                        balance = manager.GetBalance(bankAccount);
                        args.Player.SendSuccessMessage("[ServerBank] Success! You have withdrawed with interest: {0}", (int)(withdraw * (1 + INTEREST_RATE)));
                        args.Player.SendSuccessMessage("[ServerBank] Balance {0}", balance);
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[ServerBank] An Error has Occured!");
                        return;
                    }

                    return;
				#endregion

				default:
					args.Player.SendValidBankUsage();
					return;
			}
		}
    }
}
