using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinqToTTreeInterfacesLib;
using System.Threading.Tasks;

namespace TMVAUtilities
{
    public class FutureConsole
    {
        private static List<IFutureValue<string>> _strings = new List<IFutureValue<string>>();

        public static void FutureWrite(IFutureValue<string> str)
        {
            _strings.Add(str);
        }

        public static void Emit()
        {
            foreach (var item in _strings)
            {
                Console.WriteLine($"{item.Value}");
            }
        }
    }
}
