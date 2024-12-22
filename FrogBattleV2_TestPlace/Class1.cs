using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattleV2_TestPlace
{
    internal class MyClass
    {
        public string Name { get; set; }
        public MyClass(string name)
        {
            Name = name;
        }
        public string actiones()
        {
            return $"My name is {this.Name}!";
        }
    }
    internal class MyClass2 : MyClass
    {
        public Func<string> Action { get; set; }
        public MyClass2(string name, Func<string> action) : base(name)
        {
            Action = action;
        }
    }
}
