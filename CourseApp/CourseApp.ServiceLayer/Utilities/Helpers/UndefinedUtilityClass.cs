using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseApp.BusinessLayer.Utilities.Helpers
{
    internal class UndefinedUtilityClass
    {
        public static object Create()
        {
            // test metodu
            Console.WriteLine("UndefinedUtilityClass.Create()");
            return new { Status = "OK" };
        }
    }
}
