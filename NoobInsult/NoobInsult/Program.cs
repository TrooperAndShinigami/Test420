using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace NoobInsult
{
    class Program
    {
        public static Menu Menu;
        private static string[] messages;
        private static Obj_AI_Hero player;
        private static double timeDead;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }
        private static void OnGameLoad(EventArgs args)
        {
            try
            {
                messages = new[]
                         {
                             "/all yaktola !", "/all Kerim da Koonet Tanget", "/all Dumme luder ko", "/all Airi bebzezat ekhtak", "/all Zuig mijn lul", "/all Du arschgefickter Hurensohn!", "/all Imak ibtintak", "/all Fuck ye dain?",
                             "/all Din kuksugar hora", "/all Atını siken kovboy", "/all Ciğerini Sikeyim", "/all Götüne koyayım",
                             "/all Teri maa ke handle mein pandle marun.", "/all Vai a farti fottere, puttana!", "/all Kerim bimzha, heez", "/all Cuzão", "/all Brûle en enfer", "/all J'ai envie de chier", "/all Je t'emmerde!",
                             "/all skata k'aposkata", "/all Σκατά στα μουτρα σου", "/all Su gamo ti mana", "/all ade sto diaolo", "/all In a couple weeks, you will almost certainly be porking an ugly ugly man in an unoccupied grocery store for crack, you despicable shitfalcon.", " /all Jævla fitte kuk", "/all Din kuksugar hora", "/all Din variga grisfitta", "/all Inte ens din mamma gillar dig.", "/all Jävla fan", "/all Pappas smutsiga lilla hora", "/all Baishunfu", "/all Buchi Korosuzo Konoyaro!", "/all Damare Konoyarou!", "/all Nameruna", "/all shi-nay", "/all Usero yo", "/all Amcik Hosafi", "/all Amýný götünden sikerim.", "/all Ananin ami", "/all sikimin kurma kolu", "/all tam bir surtuksun,kaltaksin", "/all yedi ceddini siktigimin oglu", "/all dalyarak", "/all bok ye", "/allamina koyayim ", "/all a.q", "/all Siktir lan"
                         };
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
            player = ObjectManager.Player;

            Menu = new Menu("Insult", "Insult", true);
            var menu = new Menu("Settings", "Settings");
            Menu.AddSubMenu(menu);
            menu.AddItem(new MenuItem("messages", "Say something when dead").SetValue(true));
            menu.AddItem(new MenuItem("messagesspam", "Activate for spam when dead lol").SetValue(false));
            menu.AddItem(new MenuItem("insult", "Spam key 420 enjoy kek").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Menu.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
        }
        private static void OnUpdate(EventArgs args)
        {
            if (Menu.Item("insult").GetValue<KeyBind>().Active)
            {
                var r = new Random();
                Game.Say(messages[r.Next(0, 46)]);
            }
            if (Menu.Item("messages").GetValue<bool>())
            {
                Messages();
            }
        }
        private static void Messages()
        {
            if (player.IsDead)
            {
                if (Menu.Item("messagesspam").GetValue<bool>())
                {
                    var r = new Random();
                    Game.Say(messages[r.Next(0, 46)]);
                }
                if (!Menu.Item("messagesspam").GetValue<bool>() && Game.Time - timeDead > 80)
                {
                        var r = new Random();
                        Game.Say(messages[r.Next(0, 46)]);
                        timeDead = Game.Time;                  
                }
            }
        }
    }
}
