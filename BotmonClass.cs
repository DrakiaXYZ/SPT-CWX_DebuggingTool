using Comfort.Common;
using EFT;
using EFT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using CWX_DebuggingTool.Models;

namespace CWX_DebuggingTool
{
    public sealed class BotmonClass : MonoBehaviour, IDisposable
    {
        private static BotmonClass _instance;

        private GUIContent _guiContent;
        private GUIStyle _textStyle;
        private Player _player;
        private Dictionary<string, List<Player>> _zoneAndPlayers = new Dictionary<string, List<Player>>();
        private Dictionary<string, BotRoleAndDiffClass> _playerRoleAndDiff = new Dictionary<string, BotRoleAndDiffClass>();
        private List<BotZone> _zones;
        private GameWorld _gameWorld;
        private IBotGame _botGame;
        private Rect _rect;
        private string _content = "";
        private Vector2 _guiSize;
        private float _distance;

        private BotmonClass()
        {

        }

        public int Mode { get; set; }

        public static BotmonClass Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BotmonClass();
                }

                return _instance;
            }
        }

        public void Dispose()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            var gameobj = gameWorld.GetComponent<BotmonClass>();
            Destroy(gameobj);
            _instance = null;
            GC.SuppressFinalize(this);
        }

        ~BotmonClass()
        {
            ConsoleScreen.Log("BotMonitor Disabled on game end");
        }

        public void Awake()
        {
            // Get GameWorld Instance
            _gameWorld = Singleton<GameWorld>.Instance;

            // Get BotGame Instance
            _botGame = Singleton<IBotGame>.Instance;

            // Get Player from GameWorld
            _player = _gameWorld.MainPlayer;

            // Make new rect to use for GUI
            _rect = new Rect(0, 60, 0, 0);

            // Get all BotZones
            _zones = LocationScene.GetAllObjects<BotZone>().ToList();

            // Set up the Dictionary
            foreach (var botZone in _zones)
            {
                _zoneAndPlayers.Add(botZone.name, new List<Player>());
            }

            // Add existing Bots to list
            if (_gameWorld.AllPlayers.Count > 1)
            {
                foreach (var player in _gameWorld.AllPlayers)
                {
                    if (player.IsYourPlayer) continue;

                    _playerRoleAndDiff.Add(player.ProfileId, GetBotRoleAndDiffClass(player.Profile.Info));

                    var theirZone = player.AIData.BotOwner.BotsGroup.BotZone.NameZone;

                    _zoneAndPlayers[theirZone].Add(player);
                }
            }

            // Sub to Event to get and add Bot when they spawn
            _botGame.BotsController.BotSpawner.OnBotCreated += owner =>
            {
                var player = owner.GetPlayer;
                _zoneAndPlayers[player.AIData.BotOwner.BotsGroup.BotZone.NameZone].Add(player);
                _playerRoleAndDiff.Add(player.ProfileId, GetBotRoleAndDiffClass(player.Profile.Info));
            };
        }

        public BotRoleAndDiffClass GetBotRoleAndDiffClass(InfoClass info)
        {
            var settings = info.GetType().GetField("Settings", BindingFlags.Public | BindingFlags.Instance).GetValue(info);

            var role = settings.GetType().GetField("Role", BindingFlags.Instance | BindingFlags.Public).GetValue(settings).ToString();
            var diff = settings.GetType().GetField("BotDifficulty", BindingFlags.Instance | BindingFlags.Public).GetValue(settings).ToString();

            return new BotRoleAndDiffClass(string.IsNullOrEmpty(role) ? "" : role, string.IsNullOrEmpty(diff) ? "" : diff);
        }

        public void OnGUI()
        {
            // set basics on GUI
            if (_textStyle == null)
            {
                _textStyle = new GUIStyle(GUI.skin.box);
                _textStyle.alignment = TextAnchor.MiddleLeft;
                _textStyle.fontSize = 20;
                _textStyle.margin = new RectOffset(3, 3, 3, 3);
            }

            // new GUI Content
            if (_guiContent == null)
            {
                _guiContent = new GUIContent();
            }

            // If Mode Greater than or equal to 1 show total
            if (Mode >= 1)
            {
                _content = string.Empty;
                _content += $"Total = {_gameWorld.AllPlayers.Count - 1}\n";
            }

            // If Mode Greater than or equal to 2 show total for each zone
            if (Mode >= 2 && _zoneAndPlayers != null)
            {
                foreach (var zone in _zoneAndPlayers)
                {
                    if (_zoneAndPlayers[zone.Key].FindAll(x => x.HealthController.IsAlive).Count <= 0) continue;

                    _content += $"{zone.Key} = {_zoneAndPlayers[zone.Key].FindAll(x => x.HealthController.IsAlive).Count}\n";

                    // If Mode Greater than or equal to 3 show Bots individually also
                    if (Mode < 3) continue;

                    foreach (var player in _zoneAndPlayers[zone.Key].Where(player => player.HealthController.IsAlive))
                    {
                        _distance = Vector3.Distance(player.Position, _player.Position);
                        _content += $"> [{_distance:n2}m] [{_playerRoleAndDiff.First(x => x.Key == player.ProfileId).Value.Role}] " +
                                    $"[{player.Profile.Side}] [{_playerRoleAndDiff.First(x => x.Key == player.ProfileId).Value.Difficulty}] {player.Profile.Nickname}\n";
                    }
                }
            }

            _guiContent.text = _content;

            _guiSize = _textStyle.CalcSize(_guiContent);

            _rect.x = Screen.width - _guiSize.x - 5f;
            _rect.width = _guiSize.x;
            _rect.height = _guiSize.y;

            GUI.Box(_rect, _guiContent, _textStyle);
        }
    }
}