using BepInEx;
using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.UI;

namespace CWX_DebuggingTool
{
    [BepInPlugin("com.cwx.debuggingtool", "cwx-debuggingtool", "1.1.0")]
    public class DebuggingTool : BaseUnityPlugin
    {
        private void Awake()
        {
            ConsoleScreen.Processor.RegisterCommandGroup<DebuggingTool>();
        }

        [ConsoleCommand("BotMonitor")]
        public static void BotMonitorConsoleCommand([ConsoleArgument("", "Options: 0 = off, 1 = Total bots, 2 = 1+Total bots per Zone, 3 = 2+Each bot")] int value )
        {
            switch (value)
            {
                case 0:
                    DisableBotMonitor();
                    ConsoleScreen.Log("BotMonitor disabled");
                    break;
                case 1:
                    EnableBotMonitor(1);
                    ConsoleScreen.Log("BotMonitor enabled with only Total");
                    break;
                case 2:
                    EnableBotMonitor(2);
                    ConsoleScreen.Log("BotMonitor enabled with Total and per zone Total");
                    break;
                case 3:
                    EnableBotMonitor(3);
                    ConsoleScreen.Log("BotMonitor enabled with Total, per zone Total and each bot");
                    break;
                default:
                    // fail to load, or wrong option used
                    ConsoleScreen.LogError("Wrong Option used, please use 0, 1, 2 or 3");
                    break;
            }
        }

        public static void DisableBotMonitor()
        {
            BotmonClass.Instance.Dispose();
        }

        public static void EnableBotMonitor(int option)
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            var btmInstance = gameWorld.gameObject.AddComponent<BotmonClass>();

            btmInstance.Mode = option;
        }
    }
}