using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Terraria;
using TShockAPI.DB;
using TShockAPI;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolfje.Plugins.SEconomy;

namespace ServerBank
{
    public class DBManager
    {
        private IDbConnection db;

        public DBManager()
        {
        }

        public void setupDb()
        {
            switch (TShock.Config.StorageType.ToLower())
            {
                case "sqlite":
                    db = new SqliteConnection(String.Format("uri=file://{0},Version=3",
                            Path.Combine(TShock.SavePath, "ServerBank.sqlite")));
                    break;

                case "mysql":
                    try
                    {
                        var host = TShock.Config.MySqlHost.Split(':');
                        db = new MySqlConnection
                        {
                            ConnectionString = String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}",
                            host[0],
                            host.Length == 1 ? "3306" : host[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword
                            )
                        };
                    }
                    catch (MySqlException ex)
                    {
                        TShock.Log.Error(ex.ToString());
                        throw new Exception("MySQL not setup correctly");
                    }
                    break;

                default:
                    throw new Exception("Invalid storage type");
            }

            var sqlCreator = new SqlTableCreator(db,
                db.GetSqlType() == SqlType.Sqlite
                ? (IQueryBuilder)new SqliteQueryCreator()
                : new MysqlQueryCreator());

            var bank = new SqlTable("ServerBank",
                new SqlColumn("PlayerName", MySqlDbType.Text),
                new SqlColumn("Balance", MySqlDbType.Int32)
                );

            sqlCreator.EnsureTableStructure(bank);
        }

        public BankItem GetBankItem(TSPlayer player)
        {
            var matches = new List<BankItem>();
            BankItem account = null;

            using (var reader = db.QueryReader("SELECT * FROM ServerBank WHERE PlayerName = @0", player.User.Name))
            {
                while (reader.Read())
                {
                    matches.Add(new BankItem(
                        reader.Get<string>("PlayerName"),
                        reader.Get<int>("Balance"))
                        );
                }
            }

            if (matches.Count < 1) //Player isn't in db, create account
            {
                db.Query("INSERT INTO ServerBank (PlayerName, Balance) "
                    + "VALUES (@0, @1)", player.User.Name, 0);
                account = GetBankItem(player); //recurse through the function
            }
            else
            {
                account = matches.ElementAt(0);
            }

            return account;
        }

        //Get Balance, returns -1 if name not found
        /*  public int GetBalance(BankItem account)
          {
              int balance = -1;
              var matches = new List<string>();
              using (var reader = db.QueryReader("SELECT * FROM ServerBank WHERE PlayerName = @0", account.player))
              {
                  while (reader.Read())
                  {
                      matches.Add(account.player);
                  }
                  if (matches.Count != 0)
                  {
                      balance = reader.Get<int>("Balance");
                  }
              }
              return balance;

          }*/

        //Deposit 
        public bool DepositBal(BankItem account, int amount)
        {
            var matches = new List<string>();
            using (var reader = db.QueryReader("SELECT * FROM ServerBank WHERE PlayerName = @0", account.player))
            {
                while (reader.Read())
                {
                    matches.Add(account.player);
                }
                if (matches.Count != 0)
                {
                    db.Query("UPDATE ServerBank SET Balance = Balance + @0", amount);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        //Withdraw
        public bool WithdrawBal(BankItem account, Money amount, double INTEREST_RATE)
        {
            var matches = new List<string>();
            using (var reader = db.QueryReader("SELECT * FROM ServerBank WHERE PlayerName = @0", account.player))
            {
                while (reader.Read())
                {
                    matches.Add(account.player);
                }
                if (matches.Count != 0)
                {
                    db.Query("UPDATE ServerBank SET Balance = Balance - @0", (int)(amount * (1 + INTEREST_RATE)));
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

    }
}
