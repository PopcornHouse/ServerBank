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
            manager.setupDB();
        }

        public override void Initialize()
        {

            PlayerHooks.PlayerPostLogin += OnPostLogin;
            #region Commands
            Commands.ChatCommands.Add(new Command("ss.player", Shop, "shop", "ss"));
            #endregion
            throw new NotImplementedException();
        }
        #endregion
    }
}
