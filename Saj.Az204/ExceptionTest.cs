﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saj.Az204
{
    internal class ExceptionTest
    {
        public void Test()
        {
            int[] array1 = { 0, 0 };
            int[] array2 = { 0, 0 };

            try
            {
                Array.Copy(array1, array2, -1);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Error: {0}", e);
                throw;
            }
            finally
            {
                Console.WriteLine("This statement is always executed.");
            }
        }
    }
}
