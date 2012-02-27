using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atomia.Provisioning.Modules.Folders
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Folders f = new Folders();
                f.HandleAndProvide(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
