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

		public List<BankItem> GetBankItem(TSPlayer player)
		{
			var bankList = new List<BankItem>();

			using (var reader = db.QueryReader("SELECT * FROM ServerBank WHERE PlayerName = @0", player.User.Name))
			{
				while (reader.Read())
				{
					bankList.Add(new BankItem(
						reader.Get<string>("PlayerName"),
						reader.Get<int>("Balance"))
						);
				}
			}
			return bankList;
		}
    }
}
